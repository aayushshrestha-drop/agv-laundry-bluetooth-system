﻿using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AuditLogging.MongoDB;
using Volo.Abp.BackgroundJobs.MongoDB;
using Volo.Abp.FeatureManagement.MongoDB;
using Volo.Abp.Identity.MongoDB;
using Volo.Abp.IdentityServer.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.MongoDB;
using Volo.Abp.SettingManagement.MongoDB;
using Volo.Abp.TenantManagement.MongoDB;
using Volo.Abp.Uow;

namespace AGV.Laundry.MongoDB
{
    [DependsOn(
        typeof(AGVLaundryDomainModule),
        typeof(AbpPermissionManagementMongoDbModule),
        typeof(AbpSettingManagementMongoDbModule),
        typeof(AbpIdentityMongoDbModule),
        typeof(AbpIdentityServerMongoDbModule),
        typeof(AbpBackgroundJobsMongoDbModule),
        typeof(AbpAuditLoggingMongoDbModule),
        typeof(AbpTenantManagementMongoDbModule),
        typeof(AbpFeatureManagementMongoDbModule)
        )]
    public class AGVLaundryMongoDbModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddMongoDbContext<AGVLaundryMongoDbContext>(options =>
            {
                options.AddDefaultRepositories();
            });

            Configure<AbpUnitOfWorkDefaultOptions>(options =>
            {
                options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
            });
        }
    }
}
