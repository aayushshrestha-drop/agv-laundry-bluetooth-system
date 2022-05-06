using AGV.Laundry.MongoDB;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.Modularity;

namespace AGV.Laundry.TagLocation
{
    [DependsOn(
        typeof(AbpAutofacModule),
        typeof(AGVLaundryMongoDbModule),
        typeof(AGVLaundryApplicationContractsModule)
        )]
    public class AGVLaundryTagLocationClientModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBackgroundJobOptions>(options => options.IsJobExecutionEnabled = false);
        }
    }
}
