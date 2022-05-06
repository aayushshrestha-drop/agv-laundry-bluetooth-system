using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AGV.Laundry.BaseStations
{
    public interface IBaseStationAppService :
         ICrudAppService<
             BaseStationDto,
             Guid,
             PagedAndSortedResultRequestDto,
             CreateUpdateBaseStationDto>
    {
    }
}
