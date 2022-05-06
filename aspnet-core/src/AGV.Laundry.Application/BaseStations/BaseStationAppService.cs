using AGV.Laundry.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AGV.Laundry.BaseStations
{
    [Authorize(AGVLaundryPermissions.BaseStations.Default)]
    public class BaseStationAppService :
        CrudAppService<
            BaseStation,
            BaseStationDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateBaseStationDto>,
        IBaseStationAppService
    {
        public BaseStationAppService(
            IRepository<BaseStation, Guid> repository)
            : base(repository)
        {
            GetPolicyName = AGVLaundryPermissions.BaseStations.Default;
            GetListPolicyName = AGVLaundryPermissions.BaseStations.Default;
            CreatePolicyName = AGVLaundryPermissions.BaseStations.Create;
            UpdatePolicyName = AGVLaundryPermissions.BaseStations.Edit;
            DeletePolicyName = AGVLaundryPermissions.BaseStations.Create;
        }
    }
}
