using Volo.Abp.Settings;

namespace AGV.Laundry.Settings
{
    public class AGVLaundrySettingDefinitionProvider : SettingDefinitionProvider
    {
        public override void Define(ISettingDefinitionContext context)
        {
            //Define your own settings here. Example:
            //context.Add(new SettingDefinition(AGVLaundrySettings.MySetting1));
        }
    }
}
