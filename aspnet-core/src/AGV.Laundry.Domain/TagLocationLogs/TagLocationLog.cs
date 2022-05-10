using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AGV.Laundry.TagLocationLogs
{
    public class TagLocationLog : FullAuditedAggregateRoot<Guid>
    {
        public virtual Guid BSIP { get; set; }
        public virtual Guid TagId { get; set; }
        public virtual TagLocationLogStatus Status { get; set; }

        public TagLocationLog() { }
        public TagLocationLog(Guid id, Guid bsIp, Guid tagId, TagLocationLogStatus status) : base(id)
        {
            BSIP = bsIp;
            TagId = tagId;
            Status = status;
        }
        public enum TagLocationLogStatus
        {
            IN = 1,
            OUT = 2
        }
    }
}
