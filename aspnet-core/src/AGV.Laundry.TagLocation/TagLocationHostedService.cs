using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using AGV.Laundry.Tags;
using Newtonsoft.Json;
using System.Linq;
using System;
using AGV.Laundry.ConfigKeys;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR.Client;
using RestSharp;
using AGV.Laundry.TagRssis;
using AGV.Laundry.Configurations;
using AGV.Laundry.BaseStations;
using AGV.Laundry.TagLocationLogs;
using RabbitMQ.Client;
using System.Text;
using System.Net.Http;

namespace AGV.Laundry.TagLocation
{
    public class TagLocationHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TagLocationHostedService> _logger;
        public TagLocationHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration, ILogger<TagLocationHostedService> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var application = AbpApplicationFactory.Create<AGVLaundryTagLocationClientModule>(options => {
                options.Services.ReplaceConfiguration(_configuration);
                options.UseAutofac();
                options.Services.AddLogging(c => c.AddSerilog());
            }))
            {

                application.Initialize();
                var _tagRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<Tag>>();
                var _baseStationRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<BaseStation>>();
                var _tagRssiRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<TagRssi>>();
                var _configurationRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<Configuration>>();

                var _tagLocationLogRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<TagLocationLog>>();

                var tags = _tagRepository.Where(w => w.Status);
                var tagIds = tags.Select(s => s.TagId).ToList();
                var baseStations = await _baseStationRepository.ToListAsync();
                var bsAddresses = baseStations.Select(s => s.BSIP).ToList();

                int packetIntervalForMasterNodeWindowInSeconds = 2;
                var PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS));
                if (PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS != null) int.TryParse(PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS.Value, out packetIntervalForMasterNodeWindowInSeconds);

                int inRssiThreshold = -60;
                var IN_RSSI_THRESHOLD = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.IN_RSSI_THRESHOLD));
                if (IN_RSSI_THRESHOLD != null) int.TryParse(IN_RSSI_THRESHOLD.Value, out inRssiThreshold);

                var time = DateTime.Now.AddSeconds(-packetIntervalForMasterNodeWindowInSeconds);

                var allTagsRssi = _tagRssiRepository.Where(w => 
                w.CreationTime >= time && 
                bsAddresses.Contains(w.BaseStationIP) && tagIds.Contains(w.TagId));

                List<Guid> tagLocationLogIds = new List<Guid>();

                foreach (var item in tags)
                {
                    var tagRssis = allTagsRssi.Where(w => w.TagId.Equals(item.TagId));
                    if (tagRssis.Any())
                    {
                        var highestRssi = tagRssis.Max(m => m.Rssi);
                        if(highestRssi >= inRssiThreshold)
                        {
                            //Check Last Log exists
                            var lastTagLocationLog = _tagLocationLogRepository.Where(w => w.TagId.Equals(item.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                            if(lastTagLocationLog != null)
                            {
                                var maxRssis = tagRssis.Where(w => w.Rssi == highestRssi);
                                var masterNode = baseStations.FirstOrDefault(f => f.BSIP.Equals(maxRssis.FirstOrDefault().BaseStationIP));
                                // if last log exists and STATE is OUT for Cart/Tag, check if lot is offupied & do STATE IN
                                if (lastTagLocationLog.Status == Enums.TagLocationLogStatus.OUT)
                                {
                                    var occupiedLot = _tagLocationLogRepository.Where(w => w.BasestationId.Equals(masterNode.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                                    var cart = tags.FirstOrDefault(f => f.Id.Equals(item.Id)).CartNo;
                                    if (occupiedLot != null)
                                    {
                                        if (occupiedLot.Status == Enums.TagLocationLogStatus.IN)
                                        {
                                            var lot = baseStations.FirstOrDefault(f => f.Id.Equals(occupiedLot.BasestationId)).LotNo;
                                            var occupiedCart = tags.FirstOrDefault(f => f.Id.Equals(occupiedLot.TagId)).CartNo;
                                            _logger.LogInformation($"{lot} is occupied with {occupiedCart}. Cannot insert {cart}.");
                                        }
                                        else
                                        {
                                            var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                            {
                                                BasestationId = masterNode.Id,
                                                TagId = item.Id,
                                                Status = Enums.TagLocationLogStatus.IN
                                            });
                                            _logger.LogInformation($"{cart} inserted to {masterNode.LotNo}.");
                                            tagLocationLogIds.Add(created.Id);
                                        }
                                    }
                                    else
                                    {
                                        var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                        {
                                            BasestationId = masterNode.Id,
                                            TagId = item.Id,
                                            Status = Enums.TagLocationLogStatus.IN
                                        });
                                        _logger.LogInformation($"{cart} inserted to {masterNode.LotNo}.");
                                        tagLocationLogIds.Add(created.Id);
                                    }
                                }
                                // if last log exists and STATE is IN but for different basaestation, do STATE OUT from last basestation and STATE IN for new basestation
                                if (lastTagLocationLog.Status == Enums.TagLocationLogStatus.IN
                                    && lastTagLocationLog.BasestationId != masterNode.Id)
                                {
                                    var lot = baseStations.FirstOrDefault(f => f.Id.Equals(lastTagLocationLog.BasestationId)).LotNo;
                                    var cart = tags.FirstOrDefault(f => f.Id.Equals(item.Id)).CartNo;
                                    //STATE OUT from last basestation
                                    var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                    {
                                        BasestationId = lastTagLocationLog.BasestationId,
                                        TagId = item.Id,
                                        Status = Enums.TagLocationLogStatus.OUT
                                    });
                                    _logger.LogInformation($"{cart} exited from {lot}.");
                                    tagLocationLogIds.Add(created.Id);
                                    //check if lot is occupied & STATE IN from new basestation
                                    var occupiedLot = _tagLocationLogRepository.Where(w => w.BasestationId.Equals(masterNode.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                                    if(occupiedLot != null)
                                    {
                                        if (occupiedLot.Status == Enums.TagLocationLogStatus.IN)
                                        {
                                            var occupiedCart = tags.FirstOrDefault(f => f.Id.Equals(occupiedLot.TagId)).CartNo;
                                            _logger.LogInformation($"{lot} is occupied with {occupiedCart}. Cannot insert {cart}.");
                                        }
                                        else
                                        {
                                            created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                            {
                                                BasestationId = masterNode.Id,
                                                TagId = item.Id,
                                                Status = Enums.TagLocationLogStatus.IN
                                            });
                                            _logger.LogInformation($"{cart} inserted to {masterNode.LotNo}.");
                                            tagLocationLogIds.Add(created.Id);
                                        }
                                    }
                                    else
                                    {
                                        created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                        {
                                            BasestationId = masterNode.Id,
                                            TagId = item.Id,
                                            Status = Enums.TagLocationLogStatus.IN
                                        });
                                        _logger.LogInformation($"{cart} inserted to {masterNode.LotNo}.");
                                        tagLocationLogIds.Add(created.Id);
                                    }
                                }
                            }
                            else
                            {
                                // if last log does not exist, check is lot is occupied and do STATE IN for Cart/Tag
                                var maxRssis = tagRssis.Where(w => w.Rssi == highestRssi);
                                var masterNode = baseStations.FirstOrDefault(f => f.BSIP.Equals(maxRssis.FirstOrDefault().BaseStationIP));

                                var occupiedLot = _tagLocationLogRepository.Where(w => w.BasestationId.Equals(masterNode.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                                var cart = tags.FirstOrDefault(f => f.Id.Equals(item.Id)).CartNo;
                                if (occupiedLot != null)
                                {
                                    if (occupiedLot.Status == Enums.TagLocationLogStatus.IN)
                                    {
                                        var lot = baseStations.FirstOrDefault(f => f.Id.Equals(occupiedLot.BasestationId)).LotNo;                                        
                                        var occupiedCart = tags.FirstOrDefault(f => f.Id.Equals(occupiedLot.TagId)).CartNo;
                                        _logger.LogInformation($"{lot} is occupied with {occupiedCart}. Cannot insert {cart}.");
                                    }
                                    else
                                    {
                                        var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                        {
                                            BasestationId = masterNode.Id,
                                            TagId = item.Id,
                                            Status = Enums.TagLocationLogStatus.IN
                                        });
                                        _logger.LogInformation($"{cart} inserted to {masterNode.LotNo}.");
                                        tagLocationLogIds.Add(created.Id);
                                    }
                                }
                                else
                                {
                                    var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                    {
                                        BasestationId = masterNode.Id,
                                        TagId = item.Id,
                                        Status = Enums.TagLocationLogStatus.IN
                                    });
                                    _logger.LogInformation($"{cart} inserted to {masterNode.LotNo}.");
                                    tagLocationLogIds.Add(created.Id);
                                }
                            }                            
                        }
                        else
                        {
                            var lastTagLocationLog = _tagLocationLogRepository.Where(w => w.TagId.Equals(item.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                            if (lastTagLocationLog != null)
                            {
                                if (lastTagLocationLog.Status == Enums.TagLocationLogStatus.IN)
                                {
                                    var lot = baseStations.FirstOrDefault(f => f.Id.Equals(lastTagLocationLog.BasestationId)).LotNo;
                                    var cart = tags.FirstOrDefault(f => f.Id.Equals(item.Id)).CartNo;
                                    //STATE OUT from last basestation
                                    var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                    {
                                        BasestationId = lastTagLocationLog.BasestationId,
                                        TagId = item.Id,
                                        Status = Enums.TagLocationLogStatus.OUT
                                    });
                                    _logger.LogInformation($"{cart} exited from {lot}.");
                                    tagLocationLogIds.Add(created.Id);
                                }
                            }
                        }
                    }

                    else
                    {
                        var lastTagLocationLog = _tagLocationLogRepository.Where(w => w.TagId.Equals(item.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                        if(lastTagLocationLog != null)
                        {
                            int autoCartExitInSeconds = 60;
                            var NO_PACKET_WAIT_IN_SECONDS_FOR_AUTO_CART_EXIT = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.NO_PACKET_WAIT_IN_SECONDS_FOR_AUTO_CART_EXIT));
                            if (NO_PACKET_WAIT_IN_SECONDS_FOR_AUTO_CART_EXIT != null) int.TryParse(NO_PACKET_WAIT_IN_SECONDS_FOR_AUTO_CART_EXIT.Value, out autoCartExitInSeconds);


                            var compareTime = DateTime.Now.AddSeconds(-autoCartExitInSeconds);
                            if(compareTime >= lastTagLocationLog.CreationTime)
                            {
                                if (lastTagLocationLog.Status == Enums.TagLocationLogStatus.IN)
                                {
                                    var lot = baseStations.FirstOrDefault(f => f.Id.Equals(lastTagLocationLog.BasestationId)).LotNo;
                                    var cart = tags.FirstOrDefault(f => f.Id.Equals(item.Id)).CartNo;
                                    var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                    {
                                        BasestationId = lastTagLocationLog.BasestationId,
                                        TagId = item.Id,
                                        Status = Enums.TagLocationLogStatus.OUT
                                    });
                                    _logger.LogInformation($"{cart} auto exited from {lot}.");
                                    tagLocationLogIds.Add(created.Id);
                                }
                            }
                        }
                    }
                }

                if (tagLocationLogIds.Any())
                {
                    await Task.Factory.StartNew(() => {
                        var factory = new ConnectionFactory()
                        {
                            HostName = _configuration["RabbitMQ:HostName"],
                            Port = _configuration.GetValue<int>("RabbitMQ:Port"),
                            UserName = _configuration["RabbitMQ:UserName"],
                            Password = _configuration["RabbitMQ:Password"]
                        };
                        using (var connection = factory.CreateConnection())
                        using (var channel = connection.CreateModel())
                        {
                            var properties = channel.CreateBasicProperties();
                            properties.Persistent = false;
                            foreach(var item in tagLocationLogIds)
                            {
                                byte[] messagebuffer = Encoding.UTF8.GetBytes(item.ToString());
                                channel.BasicPublish(_configuration["RabbitMQ:EXCHANGE"], "", properties, messagebuffer);
                            }
                        }
                    });
                }

                #region
                //var pendings = _tagLocationLogRepository.Where(w => !w.IsAcknowledged).OrderBy(o => o.CreationTime).ToList();
                //var updatables = new List<TagLocationLog>();
                //if (pendings.Any())
                //{
                //    var API_URL = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.API_URL));
                //    using (var httpClientHandler = new HttpClientHandler())
                //    {
                //        using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(httpClientHandler))
                //        {
                //            client.BaseAddress = new Uri(API_URL.Value);
                //            client.DefaultRequestHeaders.Accept.Clear();
                //            foreach (var item in pendings)
                //            {
                //                var requestPayload = JsonConvert.SerializeObject(item.RequestPayload);
                //                var stringContent = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                //                try
                //                {
                //                    var response = client.PostAsync(API_URL.Value, stringContent).Result;
                //                    if (response.IsSuccessStatusCode)
                //                    {
                //                        var content = response.Content.ReadAsStringAsync();
                //                        item.ResponsePayload = content.Result.ToString();
                //                        item.ResponseStatus = (int)response.StatusCode;
                //                        item.IsAcknowledged = response.IsSuccessStatusCode;
                //                        updatables.Add(item);
                //                        Console.WriteLine($"RESPONSE PAYLOAD: {content.Result.ToString()}");
                //                    }
                //                }
                //                catch
                //                {
                //                    continue;
                //                }
                                
                //            }
                //        }
                //    }
                    
                //}
                //if (updatables.Any())
                //{
                //    await _tagLocationLogRepository.UpdateManyAsync(updatables);
                //}
                #endregion
                application.Shutdown();
                _hostApplicationLifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
