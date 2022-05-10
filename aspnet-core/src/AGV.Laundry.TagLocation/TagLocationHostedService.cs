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

namespace AGV.Laundry.TagLocation
{
    public class TagLocationHostedService : IHostedService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TagLocationHostedService> _logger;
        HubConnection connection;
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

                var tags = await _tagRepository.ToListAsync();
                foreach (var tag in tags.Where(x => x.Status))
                {
                    try
                    {
                        
                        //await _tagApiService.ProcessTag(dto);
                    }
                    catch (Exception)
                    {
                        //process doesnot break if one tag process fail
                        continue;
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
