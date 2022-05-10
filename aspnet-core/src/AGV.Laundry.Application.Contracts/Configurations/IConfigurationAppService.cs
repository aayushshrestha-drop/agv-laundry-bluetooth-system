using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AGV.Laundry.Configurations
{
    public interface IConfigurationAppService :
         ICrudAppService<
             ConfigurationDto,
             Guid,
             PagedAndSortedResultRequestDto,
             CreateUpdateConfigurationDto>
    {
    }
}
