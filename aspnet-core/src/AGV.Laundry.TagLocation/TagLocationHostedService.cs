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
                var _tagRssiRepository = application
                    .ServiceProvider
                    .GetRequiredService<IRepository<TagRssi>>();
                int HARD_DELETE_SECONDS = _configuration.GetValue<int>("APP:HARD_DELETE_SECONDS");

                var time = DateTime.Now.AddSeconds(-HARD_DELETE_SECONDS);
                if (allTagsRssi.Any())
                {
                    await _tagRssiRepository.HardDeleteAsync(x => x.CreationTime >= time);
                    _logger.LogInformation($"Tag Rssi Purged.");
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
