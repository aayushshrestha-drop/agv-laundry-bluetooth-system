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

namespace AGV.Laundry.MqClient
{
    public class MqClientHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _configuration;

        public MqClientHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var application = AbpApplicationFactory.Create<LocationEngineMqClientModule>(options => {
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
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) => {
                            var body = ea.Body.ToArray();
                            var message = Encoding.UTF8.GetString(body);
                            var data = JsonConvert.DeserializeObject<DTO>(message);
                            var isValidTag = _tagRepository.Where(w => w.Status).Any(w => w.TagId.Equals(data.tagAddress));
                            var isValidBasestation = _baseStationRepository.Where(w => w.Status).Any(w => w.BSIP.Equals(data.address));
                            if (isValidTag && isValidBasestation)
                            {
                                Console.WriteLine("{0} {1} {2}", data.address, data.tagAddress, data.rssi);
                                //insert here
                                _tagRssiRepository.InsertAsync(new TagRssi()
                                {
                                    BaseStationIP = data.address,
                                    TagId = data.tagAddress,
                                    Rssi = data.rssi,
                                    Battery = data.batt
                                });
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
