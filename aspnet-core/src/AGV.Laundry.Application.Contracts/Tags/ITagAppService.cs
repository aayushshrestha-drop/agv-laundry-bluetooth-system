using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AGV.Laundry.Tags
{
    public interface ITagAppService :
         ICrudAppService<
             TagDto,
             Guid,
             PagedAndSortedResultRequestDto,
             CreateUpdateTagDto>
    {
    }
}
