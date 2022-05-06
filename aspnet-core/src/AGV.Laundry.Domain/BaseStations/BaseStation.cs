using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AGV.Laundry.BaseStations
{
    public class BaseStation : FullAuditedAggregateRoot<Guid>
    {
        public virtual string SystemName { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual bool Status { get; set; }
        public virtual string BSIP { get; set; }
        public virtual string Hotel { get; set; }
        public virtual string LotNo { get; set; }

        public BaseStation() { }
        public BaseStation(Guid id, string systemName, string displayName, bool status, string bsip, string hotel, string lotNo) : base(id)
        {
            SystemName = systemName;
            DisplayName = displayName;
            Status = status;
            BSIP = bsip;
            Hotel = hotel;
            LotNo = lotNo;
        }
    }
}
