using System;
using Volo.Abp.Application.Dtos;

namespace AGV.Laundry.Tags
{
    public class TagDto : AuditedEntityDto<Guid>
    {
        public string SystemName { get; set; }
        public string DisplayName { get; set; }
        public bool Status { get; set; }
        public string TagId { get; set; }
        public string CartNo { get; set; }
    }
}