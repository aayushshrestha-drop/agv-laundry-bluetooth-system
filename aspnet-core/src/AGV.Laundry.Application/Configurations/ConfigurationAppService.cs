using AGV.Laundry.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AGV.Laundry.Configurations
{
    [Authorize(AGVLaundryPermissions.Configurations.Default)]
    public class ConfigurationAppService :
        CrudAppService<
            Configuration,
            ConfigurationDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateConfigurationDto>,
        IConfigurationAppService
    {
        public ConfigurationAppService(
            IRepository<Configuration, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AGVLaundryPermissions.Configurations.Default;
            GetListPolicyName = AGVLaundryPermissions.Configurations.Default;
            CreatePolicyName = AGVLaundryPermissions.Configurations.Create;
            UpdatePolicyName = AGVLaundryPermissions.Configurations.Edit;
            DeletePolicyName = AGVLaundryPermissions.Configurations.Create;
        }
    }
}
