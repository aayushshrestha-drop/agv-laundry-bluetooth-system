using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AGV.Laundry.Tags;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AGV.Laundry.TagBatteries
{
    public interface ITagBatteryAppService
    {
        Task<TagBatteryDto> TagBattery(string cartId);
    }
}
