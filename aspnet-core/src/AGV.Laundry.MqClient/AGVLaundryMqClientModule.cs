using AGV.Laundry.MongoDB;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace AGV.Laundry.MqClient
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AGVLaundryMongoDbModule),
        typeof(AGVLaundryApplicationContractsModule)
        )]
    public class AGVLaundryMqClientModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        }
    }
}
