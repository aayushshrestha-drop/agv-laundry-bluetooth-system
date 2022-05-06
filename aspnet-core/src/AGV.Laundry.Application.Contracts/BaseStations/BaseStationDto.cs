using System;
using Volo.Abp.Application.Dtos;

namespace AGV.Laundry.BaseStations
{
    public class BaseStationDto : AuditedEntityDto<Guid>
    {
        public string SystemName { get; set; }
        public string DisplayName { get; set; }
        public bool Status { get; set; }
        public string BSIP { get; set; }
        public string Hotel { get; set; }
        public string LotNo { get; set; }
    }
}