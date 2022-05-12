﻿using System.Threading;
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
using AGV.Laundry.TagLocationLogs;
using AGV.Laundry.Configurations;
using AGV.Laundry.ConfigKeys;
using AGV.Laundry.TagLocationHttps;

namespace AGV.Laundry.TagLocation.HttpClient
{
    public class TagLocationHttpClientHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _configuration;

        public TagLocationHttpClientHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var application = AbpApplicationFactory.Create<AGVLaundryTagLocationHttpClientModule>(options => {
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
                var _tagLocationLogRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<TagLocationLog>>();
                var _configurationRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<Configuration>>();
                var API_URL = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.API_URL));
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
                            Guid tagLocationLogId = Guid.Empty;
                            Guid.TryParse(message, out tagLocationLogId);
                            if(tagLocationLogId != Guid.Empty)
                            {
                                var tagLocationLog = _tagLocationLogRepository.FirstOrDefault(f => f.Id.Equals(tagLocationLogId));
                                if(tagLocationLog != null)
                                {
                                    
                                    var cart = _tagRepository.FirstOrDefault(f => f.Id.Equals(tagLocationLog.TagId));
                                    var baseStation = _baseStationRepository.FirstOrDefault(f => f.Id.Equals(tagLocationLog.BasestationId));
                                    var requestBody = new TagLocationHttpDto();
                                    requestBody.cartId = cart.TagId;
                                    requestBody.cartNo = cart.CartNo;
                                    requestBody.lotNo = baseStation.LotNo;
                                    requestBody.state = ((Enums.TagLocationLogStatus)tagLocationLog.Status).ToString();
                                    Console.WriteLine($"URL: {API_URL}");
                                    Console.WriteLine($"PAYLOAD: {JsonConvert.SerializeObject(requestBody)}");
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
