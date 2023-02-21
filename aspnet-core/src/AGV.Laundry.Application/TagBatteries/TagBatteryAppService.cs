using AGV.Laundry.BaseStations;
using AGV.Laundry.ConfigKeys;
using AGV.Laundry.Configurations;
using AGV.Laundry.TagRssis;
using AGV.Laundry.Tags;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace AGV.Laundry.TagBatteries
{
    public class TagBatteryAppService : ApplicationService,
        ITagBatteryAppService
    {
        IRepository<Tag, Guid> _tagRepository;
        IRepository<TagRssi, Guid> _tagRssiRepository;
        IRepository<Configuration, Guid> _configurationRepository;
        public TagBatteryAppService(
            IRepository<Tag, Guid> tagRepository,
            IRepository<TagRssi, Guid> tagRssiRepository,
            IRepository<Configuration, Guid> configurationRepository)
        {
            _tagRepository = tagRepository;
            _tagRssiRepository = tagRssiRepository;
            _configurationRepository = configurationRepository;
        }
        [AllowAnonymous]
        public async Task<TagBatteryDto> TagBattery([FromBody] TagBatteryRequestDto model)
        {
            if (model == null)
            {
                return new TagBatteryDto();
            }
            if (!string.IsNullOrEmpty(model.cartId))
            {
                var tag = await _tagRepository.FirstOrDefaultAsync(f => f.CartNo.Equals(model.cartId));
                if (tag is not null)
                {
                    var tagRssis = _tagRssiRepository.Where(w => w.TagId.Equals(tag.TagId)).OrderByDescending(o => o.CreationTime);
                    if (tagRssis.Any())
                    {
                        var tagRssi = tagRssis.FirstOrDefault();
                        float TAG_BATTERY_LEVEL_THRESHOLD = 2.0F;
                        var configuration = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(ConfigKeys.Keys.TAG_BATTERY_LEVEL_THRESHOLD));
                        if (configuration is not null) float.TryParse(configuration.Value, out TAG_BATTERY_LEVEL_THRESHOLD);
                        return new TagBatteryDto()
                        {
                            BattStatus = tagRssi.Battery >= TAG_BATTERY_LEVEL_THRESHOLD ? 1 : 0,
                            CartId = model.cartId
                        };
                    }
                }
            }
            return new TagBatteryDto();
        }


        [AllowAnonymous]
        public async Task<TagMasterNodeDto> MasterNode(string tagId)
        {
            var config = await _configurationRepository.FindAsync(w => w.Key.Equals(Keys.PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS));
            int PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS = 5;
            int.TryParse(config.Value, out PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS);

            var timestamp = DateTime.Now.AddSeconds(-PACKET_INTERVAL_FOR_MASTER_NODE_WINDOW_IN_SECONDS);
            var tagRssis = _tagRssiRepository.Where(w => w.TagId.Equals(tagId) && w.CreationTime >= timestamp);
            if (tagRssis.Any())
            {
                var maxRssi = tagRssis.Max(m => m.Rssi);
                var masterNode = tagRssis.Where(w => w.Rssi.Equals(maxRssi)).OrderByDescending(o => o.CreationTime).FirstOrDefault().BaseStationIP;
                return new TagMasterNodeDto()
                {
                    MasterNode = masterNode,
                    TagRssis = tagRssis.OrderByDescending(o => o.CreationTime).Select(s => new TagRssiDto()
                    {
                        BaseStationIP = s.BaseStationIP,
                        Battery = s.Battery,
                        Rssi = s.Rssi,
                        TagId = s.TagId
                    }).ToList()
                };
            }
            return new TagMasterNodeDto();            
        }
    }
}
