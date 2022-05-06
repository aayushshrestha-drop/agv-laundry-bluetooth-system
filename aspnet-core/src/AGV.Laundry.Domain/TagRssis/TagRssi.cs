using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AGV.Laundry.TagRssis
{
    public class TagRssi : FullAuditedAggregateRoot<Guid>
    {
        public virtual string TagId { get; set; }
        public virtual string BaseStationIP { get; set; }
        public virtual int Rssi { get; set; }
        public virtual float Battery { get; set; }

        public TagRssi() { }
        public TagRssi(Guid id, string tagId, string baseStationIP, int rssi, int battery) : base(id)
        {
            TagId = tagId;
            BaseStationIP = baseStationIP;
            Rssi = rssi;
            Battery = battery;
        }
    }
}
