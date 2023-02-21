using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AGV.Laundry.TagRssis;
using AGV.Laundry.Tags;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AGV.Laundry.TagBatteries
{
    public interface ITagBatteryAppService
    {
        Task<TagBatteryDto> TagBattery(TagBatteryRequestDto model);
        Task<TagMasterNodeDto> MasterNode(string tagId);
    }
}
