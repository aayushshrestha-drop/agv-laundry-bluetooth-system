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

                var time = DateTime.Now.AddSeconds(packetIntervalForMasterNodeWindowInSeconds);

                var allTagsRssi = _tagRssiRepository.Where(w => 
                w.CreationTime >= time && 
                bsAddresses.Contains(w.BaseStationIP) && tagIds.Contains(w.TagId));
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
                                if (lastTagLocationLog.Status == TagLocationLog.TagLocationLogStatus.OUT)
                                {
                                    var occupiedLot = _tagLocationLogRepository.Where(w => w.BasestationId.Equals(masterNode.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                                    var cart = tags.FirstOrDefault(f => f.Id.Equals(item.TagId)).CartNo;
                                    if (occupiedLot != null)
                                    {
                                        if (occupiedLot.Status == TagLocationLog.TagLocationLogStatus.IN)
                                        {
                                            var lot = baseStations.FirstOrDefault(f => f.Id.Equals(occupiedLot.BasestationId)).LotNo;
                                            var occupiedCart = tags.FirstOrDefault(f => f.Id.Equals(occupiedLot.TagId)).CartNo;
                                            Console.WriteLine($"{lot} is occupied with {occupiedCart}. Cannot insert {cart}.");
                                        }
                                        else
                                        {
                                            await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                            {
                                                BasestationId = masterNode.Id,
                                                TagId = item.Id,
                                                Status = TagLocationLog.TagLocationLogStatus.IN
                                            });
                                            Console.WriteLine($"{cart} inserted to {masterNode.LotNo}.");
                                        }
                                    }
                                    else
                                    {
                                        await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                        {
                                            BasestationId = masterNode.Id,
                                            TagId = item.Id,
                                            Status = TagLocationLog.TagLocationLogStatus.IN
                                        });
                                        Console.WriteLine($"{cart} inserted to {masterNode.LotNo}.");
                                    }
                                }
                                // if last log exists and STATE is IN but for different basaestation, do STATE OUT from last basestation and STATE IN for new basestation
                                if (lastTagLocationLog.Status == TagLocationLog.TagLocationLogStatus.IN
                                    && lastTagLocationLog.BasestationId != masterNode.Id)
                                {
                                    var lot = baseStations.FirstOrDefault(f => f.Id.Equals(lastTagLocationLog.BasestationId)).LotNo;
                                    var cart = tags.FirstOrDefault(f => f.Id.Equals(item.Id)).CartNo;
                                    //STATE OUT from last basestation
                                    await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                    {
                                        BasestationId = lastTagLocationLog.BasestationId,
                                        TagId = item.Id,
                                        Status = TagLocationLog.TagLocationLogStatus.OUT
                                    });
                                    Console.WriteLine($"{cart} exited from {lot}.");

                                    //check if lot is occupied & STATE IN from new basestation
                                    var occupiedLot = _tagLocationLogRepository.Where(w => w.BasestationId.Equals(masterNode.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                                    if(occupiedLot != null)
                                    {
                                        if (occupiedLot.Status == TagLocationLog.TagLocationLogStatus.IN)
                                        {
                                            var occupiedCart = tags.FirstOrDefault(f => f.Id.Equals(occupiedLot.TagId)).CartNo;
                                            Console.WriteLine($"{lot} is occupied with {occupiedCart}. Cannot insert {cart}.");
                                        }
                                        else
                                        {
                                            await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                            {
                                                BasestationId = masterNode.Id,
                                                TagId = item.Id,
                                                Status = TagLocationLog.TagLocationLogStatus.IN
                                            });
                                            Console.WriteLine($"{cart} inserted to {masterNode.LotNo}.");
                                        }
                                    }
                                    else
                                    {
                                        await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                        {
                                            BasestationId = masterNode.Id,
                                            TagId = item.Id,
                                            Status = TagLocationLog.TagLocationLogStatus.IN
                                        });
                                        Console.WriteLine($"{cart} inserted to {masterNode.LotNo}.");
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
                                    if (occupiedLot.Status == TagLocationLog.TagLocationLogStatus.IN)
                                    {
                                        var lot = baseStations.FirstOrDefault(f => f.Id.Equals(occupiedLot.BasestationId)).LotNo;                                        
                                        var occupiedCart = tags.FirstOrDefault(f => f.Id.Equals(occupiedLot.TagId)).CartNo;
                                        Console.WriteLine($"{lot} is occupied with {occupiedCart}. Cannot insert {cart}.");
                                    }
                                    else
                                    {
                                        await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                        {
                                            BasestationId = masterNode.Id,
                                            TagId = item.Id,
                                            Status = TagLocationLog.TagLocationLogStatus.IN
                                        });
                                        Console.WriteLine($"{cart} inserted to {masterNode.LotNo}.");
                                    }
                                }
                                else
                                {
                                    await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                    {
                                        BasestationId = masterNode.Id,
                                        TagId = item.Id,
                                        Status = TagLocationLog.TagLocationLogStatus.IN
                                    });
                                    Console.WriteLine($"{cart} inserted to {masterNode.LotNo}.");
                                }
                            }                            
                        }                        
                    }

                    else
                    {
                        var lastTagLocationLog = _tagLocationLogRepository.Where(w => w.TagId.Equals(item.Id)).OrderByDescending(o => o.CreationTime).FirstOrDefault();
                        var lot = baseStations.FirstOrDefault(f => f.Id.Equals(lastTagLocationLog.BasestationId)).LotNo;
                        var cart = tags.FirstOrDefault(f => f.Id.Equals(item.Id)).CartNo;
                        if(lastTagLocationLog != null)
                        {
                            if(lastTagLocationLog.Status == TagLocationLog.TagLocationLogStatus.IN)
                            {
                                await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                {
                                    BasestationId = lastTagLocationLog.BasestationId,
                                    TagId = item.Id,
                                    Status = TagLocationLog.TagLocationLogStatus.OUT
                                });
                                Console.WriteLine($"{cart} exited from {lot}.");
                            }
                        }
                    }
                }
                application.Shutdown();
                _hostApplicationLifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
    public class TokenReponseModel
    {
        public string access_token { get; set; }
    }
}
