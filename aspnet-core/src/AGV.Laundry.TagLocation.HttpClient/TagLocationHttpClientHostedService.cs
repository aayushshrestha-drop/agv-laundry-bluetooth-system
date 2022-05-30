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
using AGV.Laundry.TagLocationLogs;
using AGV.Laundry.Configurations;
using AGV.Laundry.ConfigKeys;
using AGV.Laundry.TagLocationHttps;
using System.Net.Http;

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
                            if (tagLocationLogId != Guid.Empty)
                            {
                                var tagLocationLog = _tagLocationLogRepository.FirstOrDefault(f => f.Id.Equals(tagLocationLogId));
                                if (tagLocationLog != null)
                                {

                                    var cart = _tagRepository.FirstOrDefault(f => f.Id.Equals(tagLocationLog.TagId));
                                    var baseStation = _baseStationRepository.FirstOrDefault(f => f.Id.Equals(tagLocationLog.BasestationId));
                                    var requestModel = new TagLocationHttpRequestDto();
                                    requestModel.cartId = cart.TagId;
                                    requestModel.cartNo = cart.CartNo;
                                    requestModel.lotNo = baseStation.LotNo;
                                    requestModel.state = ((Enums.TagLocationLogStatus)tagLocationLog.Status).ToString();

                                    var requestPayload = JsonConvert.SerializeObject(requestModel);
                                    Console.WriteLine($"URL: {API_URL.Value}");
                                    Console.WriteLine($"REQUEST PAYLOAD: {requestPayload}");
                                    tagLocationLog.Url = API_URL.Value;
                                    tagLocationLog.RequestPayload = requestPayload;
                                    try
                                    {
                                        using (var httpClientHandler = new HttpClientHandler())
                                        {
                                            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(httpClientHandler))
                                            {
                                                client.BaseAddress = new Uri(API_URL.Value);
                                                client.DefaultRequestHeaders.Accept.Clear();
                                                var stringContent = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                                                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                                                var response = client.PostAsync(API_URL.Value, stringContent).Result;
                                                Console.WriteLine($"Is Success Status Code: {response.IsSuccessStatusCode}");
                                                if (response.IsSuccessStatusCode)
                                                {
                                                    var content = response.Content.ReadAsStringAsync();
                                                    tagLocationLog.ResponsePayload = content.Result.ToString();
                                                    tagLocationLog.ResponseStatus = (int)response.StatusCode;
                                                    tagLocationLog.IsAcknowledged = response.IsSuccessStatusCode;
                                                    Console.WriteLine($"RESPONSE PAYLOAD: {content.Result.ToString()}");
                                                }
                                                else
                                                {
                                                    var content = response.Content.ReadAsStringAsync();
                                                    tagLocationLog.ResponsePayload = content.Result.ToString();
                                                    tagLocationLog.ResponseStatus = (int)response.StatusCode;
                                                    tagLocationLog.IsAcknowledged = response.IsSuccessStatusCode;
                                                    Console.WriteLine($"RESPONSE PAYLOAD: {content.Result.ToString()}");
                                                }
                                            }
                                        }
                                    }
                                    catch(Exception ex)
                                    {
                                        Console.WriteLine($"EXCEPTION: {ex.ToString()}");
                                        tagLocationLog.IsAcknowledged = false;
                                    }
                                    finally
                                    {
                                        _tagLocationLogRepository.UpdateAsync(tagLocationLog);
                                        Console.WriteLine($"ACKNOWLEDGED: {tagLocationLog.IsAcknowledged}");
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
