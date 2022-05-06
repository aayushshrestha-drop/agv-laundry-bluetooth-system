using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AGV.Laundry
{
    [Dependency(ReplaceServices = true)]
    public class AGVLaundryBrandingProvider : DefaultBrandingProvider
    {
        public override string AppName => "AGVLaundry";
    }
}
