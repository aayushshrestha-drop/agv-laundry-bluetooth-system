using System;
using AGV.Laundry.Enums;
using Volo.Abp.Application.Dtos;

namespace AGV.Laundry.TagBatteries
{
    public class TagBatteryDto
    {
        public string CartId { get; set; }
        public int BattStatus { get; set; }
    }
}