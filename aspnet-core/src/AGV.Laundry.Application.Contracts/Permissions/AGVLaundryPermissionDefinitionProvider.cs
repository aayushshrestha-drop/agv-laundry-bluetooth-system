﻿using AGV.Laundry.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace AGV.Laundry.Permissions
{
    public class AGVLaundryPermissionDefinitionProvider : PermissionDefinitionProvider
    {
        public override void Define(IPermissionDefinitionContext context)
        {
            var AGVLaundryGroup = context.AddGroup(AGVLaundryPermissions.GroupName);

            var tagsPermission = AGVLaundryGroup.AddPermission(AGVLaundryPermissions.Tags.Default, L("Permission:Tags"));
            tagsPermission.AddChild(AGVLaundryPermissions.Tags.Create, L("Permission:Tags.Create"));
            tagsPermission.AddChild(AGVLaundryPermissions.Tags.Edit, L("Permission:Tags.Edit"));
            tagsPermission.AddChild(AGVLaundryPermissions.Tags.Delete, L("Permission:Tags.Delete"));
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<AGVLaundryResource>(name);
        }
    }
}
