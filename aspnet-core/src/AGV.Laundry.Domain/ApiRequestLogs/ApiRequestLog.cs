using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AGV.Laundry.ApiRequestLogs
{
    public class ApiRequestLog : FullAuditedAggregateRoot<Guid>
    {
        public virtual Guid BSIP { get; set; }
        public virtual Guid TagId { get; set; }
        public virtual string Url { get; set; }
        public virtual string RequestPayload { get; set; }
        public virtual string ResponseBody { get; set; }

        public ApiRequestLog() { }
        public ApiRequestLog(Guid id, Guid bsIp, Guid tagId, string url, string requestPayload, string responseBody) : base(id)
        {
            BSIP = bsIp;
            TagId = tagId;
            Url = url;
            RequestPayload = requestPayload;
            ResponseBody = responseBody;
        }
    }
}
