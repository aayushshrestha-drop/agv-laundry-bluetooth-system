using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AGV.Laundry.Configurations
{
    public class Configuration : FullAuditedAggregateRoot<Guid>
    {
        public virtual string Key { get; set; }
        public virtual string Value { get; set; }

        public Configuration() { }
        public Configuration(Guid id, string key, string value) : base(id)
        {
            Key = key;
            Value = value;
        }
    }
}
