using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Volo.Abp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System;
using Volo.Abp.Domain.Repositories;
using AGV.Laundry.Tags;
using Newtonsoft.Json;
using System.Linq;
using AGV.Laundry.BaseStations;
using AGV.Laundry.TagRssis;
using AGV.Laundry.TagLocationLogs;
using AGV.Laundry.Configurations;
using AGV.Laundry.ConfigKeys;
using static AGV.Laundry.Permissions.AGVLaundryPermissions;
using System.Timers;
using Microsoft.Extensions.Logging;

namespace AGV.Laundry.MqClient
{
    public class MqClientHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MqClientHostedService> _logger;
        public MqClientHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration, ILogger<MqClientHostedService> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var application = AbpApplicationFactory.Create<AGVLaundryMqClientModule>(options => {
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
                var _tagLocationLogRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<TagLocationLog>>();

                var _configurationRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<Configuration>>();

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
                        //properties.Persistent = false;
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += async (model, ea) => {
                            await Task.Yield();
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            var data = JsonConvert.DeserializeObject<DTO>(message);
                            

                            var tag = await _tagRepository.FirstOrDefaultAsync(w => w.Status && w.TagId.Equals(data.tagAddress));


                            if (tag != null && tag.Status)
                            {
                                var basestation = await _baseStationRepository.FirstOrDefaultAsync(w => w.Status && w.BSIP.Equals(data.address));
                                if (basestation != null && basestation.Status)
                                {
                                    Console.WriteLine($"{data.address} - {data.tagAddress} - {data.rssi} - {data.batt}");
                                    _logger.LogInformation($"{data.address} - {data.tagAddress} - {data.batt}");
                                    //insert here
                                    await _tagRssiRepository.InsertAsync(new TagRssi()
                                    {
                                        BaseStationIP = data.address,
                                        TagId = data.tagAddress,
                                        Rssi = data.rssi,
                                        Battery = data.batt
                                    });

                                    var tagLocation = _tagLocationLogRepository.OrderByDescending(o => o.CreationTime).FirstOrDefault(f => f.TagId.Equals(tag.Id));
                                    var tagStatus = tagLocation != null ? tagLocation.Status : Enums.TagLocationLogStatus.OUT;

                                    int inRssiThreshold = -60;
                                    var IN_RSSI_THRESHOLD = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.IN_RSSI_THRESHOLD));
                                    if (IN_RSSI_THRESHOLD != null) int.TryParse(IN_RSSI_THRESHOLD.Value, out inRssiThreshold);

                                    int packetIntervalForMasterNodeWindowInSeconds = 15;
                                    var PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS));
                                    if (PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS != null) int.TryParse(PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS.Value, out packetIntervalForMasterNodeWindowInSeconds);
                                    var time = DateTime.Now.AddSeconds(-packetIntervalForMasterNodeWindowInSeconds);


                                    int THRESHOLD_PACKET_COUNTER_MAX = _configuration.GetValue<int>("APP:THRESHOLD_PACKET_COUNTER_MAX");

                                    if (tagStatus == Enums.TagLocationLogStatus.IN)
                                    {
                                        if(basestation.Id == tagLocation.BasestationId)
                                        {
                                            if(data.rssi < inRssiThreshold)
                                            {
                                                var bsTagRssis = _tagRssiRepository.Where(w => w.BaseStationIP.Equals(basestation.BSIP)
                                                && w.TagId.Equals(tag.TagId)
                                                && w.CreationTime >= time
                                                && w.Rssi < inRssiThreshold)
                                                .OrderByDescending(o => o.CreationTime);

                                                if (bsTagRssis.Count() >= THRESHOLD_PACKET_COUNTER_MAX)
                                                {
                                                    var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                                    {
                                                        BasestationId = basestation.Id,
                                                        TagId = tag.Id,
                                                        Status = Enums.TagLocationLogStatus.OUT
                                                    });
                                                    _logger.LogInformation($"{tag.TagId} exited {basestation.LotNo}. Under threshold");
                                                    Console.WriteLine($"{tag.TagId} exited {basestation.LotNo}. Under threshold");
                                                    byte[] messagebuffer = Encoding.UTF8.GetBytes(created.Id.ToString());
                                                    channel.BasicPublish(_configuration["RabbitMQ:EXCHANGE"], "", properties, messagebuffer);
                                                }
                                            }
                                            
                                        }
                                        else
                                        {
                                            if (data.rssi >= inRssiThreshold)
                                            {
                                                var bsTagRssis = _tagRssiRepository.Where(w => w.BaseStationIP.Equals(basestation.BSIP)
                                                        && w.TagId.Equals(tag.TagId)
                                                        && w.CreationTime >= time
                                                        && w.Rssi >= inRssiThreshold)
                                                        .OrderByDescending(o => o.CreationTime);

                                                
                                                if (bsTagRssis.Count() >= THRESHOLD_PACKET_COUNTER_MAX)
                                                {
                                                    var outMessage = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                                    {
                                                        BasestationId = tagLocation.BasestationId,
                                                        TagId = tag.Id,
                                                        Status = Enums.TagLocationLogStatus.OUT
                                                    });
                                                    var tagLocationBasestation = await _baseStationRepository.FirstOrDefaultAsync(w => w.Status && w.Id.Equals(tagLocation.BasestationId));
                                                    _logger.LogInformation($"{tag.TagId} exited {tagLocationBasestation.LotNo}.");
                                                    byte[] messagebuffer = Encoding.UTF8.GetBytes(outMessage.Id.ToString());
                                                    channel.BasicPublish(_configuration["RabbitMQ:EXCHANGE"], "", properties, messagebuffer);
                                                }
                                            }
                                        }
                                    }
                                    else if(tagStatus == Enums.TagLocationLogStatus.OUT)
                                    {
                                        if(data.rssi >= inRssiThreshold)
                                        {
                                            var bsTagRssis = _tagRssiRepository.Where(w => w.BaseStationIP.Equals(basestation.BSIP) 
                                            && w.TagId.Equals(tag.TagId) 
                                            && w.CreationTime >= time
                                            && w.Rssi >= inRssiThreshold)
                                            .OrderByDescending(o => o.CreationTime);

                                            if (bsTagRssis.Count() >= THRESHOLD_PACKET_COUNTER_MAX)
                                            {
                                                var bsLocation = _tagLocationLogRepository.OrderByDescending(o => o.CreationTime).FirstOrDefault(f => f.BasestationId.Equals(basestation.Id));
                                                var lotIsOccupied = bsLocation != null ? bsLocation.Status == Enums.TagLocationLogStatus.IN ? true : false : false;

                                                if (!lotIsOccupied)
                                                {
                                                    var created = await _tagLocationLogRepository.InsertAsync(new TagLocationLog()
                                                    {
                                                        BasestationId = basestation.Id,
                                                        TagId = tag.Id,
                                                        Status = Enums.TagLocationLogStatus.IN
                                                    });
                                                    _logger.LogInformation($"{tag.CartNo} inserted to {basestation.LotNo}.");
                                                    Console.WriteLine($"{tag.CartNo} inserted to {basestation.LotNo}.");
                                                    byte[] messagebuffer = Encoding.UTF8.GetBytes(created.Id.ToString());
                                                    channel.BasicPublish(_configuration["RabbitMQ:EXCHANGE"], "", properties, messagebuffer);
                                                }

                                                

                                            }


                                        }

                                    }

                                }
                            }
                        };
                        channel.BasicConsume(queue: _configuration["RabbitMQ:QUEUE"],
                                             autoAck: true,
                                             consumer: consumer);

                        Thread.Sleep(Timeout.Infinite);
                    }
                });
                application.Shutdown();
                _hostApplicationLifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
    public class DTO
    {
        public string address { get; set; }
        public string tagAddress { get; set; }
        public int rssi { get; set; }
        public float batt { get; set; }
    }
}
