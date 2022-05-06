using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AGV.Laundry.Tags
{
    public class Tag : FullAuditedAggregateRoot<Guid>
    {
        public virtual string SystemName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual bool Status { get; set; }
        public virtual string TagId { get; set; }
        public virtual string CartNo { get; set; }

        public Tag() { }
        public Tag(Guid id, string systemName, string displayName, bool status, string tagId, string cartNo) : base(id)
        {
            SystemName = systemName;
            DisplayName = displayName;
            Status = status;
            TagId = tagId;
            CartNo = cartNo;
        }
    }
}
