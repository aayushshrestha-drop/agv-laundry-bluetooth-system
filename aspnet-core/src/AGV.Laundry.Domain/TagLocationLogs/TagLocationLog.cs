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
        public virtual Guid BasestationId { get; set; }
        public virtual Guid TagId { get; set; }
        public virtual TagLocationLogStatus Status { get; set; }

        public TagLocationLog() { }
        public TagLocationLog(Guid id, Guid basestationId, Guid tagId, TagLocationLogStatus status) : base(id)
        {
            BasestationId = basestationId;
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
