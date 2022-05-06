using System;
using System.Collections.Generic;
using System.Text;
using AGV.Laundry.Localization;
using Volo.Abp.Application.Services;

namespace AGV.Laundry
{
    /* Inherit your application services from this class.
     */
    public abstract class AGVLaundryAppService : ApplicationService
    {
        protected AGVLaundryAppService()
        {
            LocalizationResource = typeof(AGVLaundryResource);
        }
    }
}
