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
using AGV.Laundry.TagLocationHttps;

namespace AGV.Laundry.TagLocation.Consolidator
{
    public class TagLocationConsolidatorHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TagLocationConsolidatorHostedService> _logger;
        public TagLocationConsolidatorHostedService(IHostApplicationLifetime hostApplicationLifetime, IConfiguration configuration, ILogger<TagLocationConsolidatorHostedService> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var application = AbpApplicationFactory.Create<AGVLaundryTagLocationConsolidatorClientModule>(options => {
                options.Services.ReplaceConfiguration(_configuration);
                options.UseAutofac();
                options.Services.AddLogging(c => c.AddSerilog());
            }))
            {

                application.Initialize();

                #region
                var _tagLocationLogRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<TagLocationLog>>();
                var _configurationRepository = application
                   .ServiceProvider
                   .GetRequiredService<IRepository<Configuration>>();
                var pendings = _tagLocationLogRepository.Where(w => !w.IsAcknowledged).OrderBy(o => o.CreationTime).ToList();
                var updatables = new List<TagLocationLog>();
                if (pendings.Any())
                {
                    var API_URL = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(Keys.API_URL));
                    using (var httpClientHandler = new HttpClientHandler())
                    {
                        using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient(httpClientHandler))
                        {
                            client.BaseAddress = new Uri(API_URL.Value);
                            client.DefaultRequestHeaders.Accept.Clear();
                            foreach (var item in pendings)
                            {
                                var requestPayload = JsonConvert.SerializeObject(item.RequestPayload);
                                var stringContent = new StringContent(requestPayload, Encoding.UTF8, "application/json");
                                try
                                {
                                    var response = client.PostAsync(API_URL.Value, stringContent).Result;
                                    if (response.IsSuccessStatusCode)
                                    {
                                        var content = response.Content.ReadAsStringAsync();
                                        var responseDto = JsonConvert.DeserializeObject<TagLocationHttpResponseDto>(content.Result);
                                        item.ResponsePayload = content.Result.ToString();
                                        item.ResponseStatus = (int)response.StatusCode;
                                        item.IsAcknowledged = responseDto.success;
                                        updatables.Add(item);
                                        Console.WriteLine($"RESPONSE PAYLOAD: {content.Result.ToString()}");
                                    }
                                }
                                catch
                                {
                                    continue;
                                }

                            }
                        }
                    }
                }
                if (updatables.Any())
                {
                    await _tagLocationLogRepository.UpdateManyAsync(updatables);
                }
                #endregion
                application.Shutdown();
                _hostApplicationLifetime.StopApplication();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
