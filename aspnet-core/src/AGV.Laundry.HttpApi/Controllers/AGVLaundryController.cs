using AGV.Laundry.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace AGV.Laundry.Controllers
{
    /* Inherit your controllers from this class.
     */
    public abstract class AGVLaundryController : AbpController
    {
        protected AGVLaundryController()
        {
            LocalizationResource = typeof(AGVLaundryResource);
        }
    }
}