using AGV.Laundry.Configurations;
using AGV.Laundry.TagRssis;
using AGV.Laundry.Tags;
using System;
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
            IRepository<Configuration, Guid> configurationRepository
            )
        {
            _tagRepository = tagRepository;
            _tagRssiRepository = tagRssiRepository;
            _configurationRepository = configurationRepository;
        }
        public async Task<TagBatteryDto> TagBattery(string cartId)
        {
            var tag = await _tagRepository.FirstOrDefaultAsync(f => f.CartNo.Trim().ToLower().Equals(cartId.Trim().ToLower()));
            if(tag is not null)
            {
                var tagRssis = _tagRssiRepository.Where(w => w.TagId.Equals(tag.TagId)).OrderByDescending(o => o.CreationTime);
                if (tagRssis.Any())
                {
                    var tagRssi = tagRssis.FirstOrDefault();
                    float TAG_BATTERY_LEVEL_THRESHOLD = 2.0F;
                    var configuration = await _configurationRepository.FirstOrDefaultAsync(f => f.Key.Equals(ConfigKeys.Keys.TAG_BATTERY_LEVEL_THRESHOLD));
                    if(configuration is not null) float.TryParse(configuration.Value, out TAG_BATTERY_LEVEL_THRESHOLD);
                    return new TagBatteryDto()
                    {
                        BattStatus = tagRssi.Battery >= TAG_BATTERY_LEVEL_THRESHOLD ? 1 : 0,
                        CartId = cartId
                    };
                }
            }
            return new TagBatteryDto();
        }
    }
}
