﻿namespace AGV.Laundry.Permissions
{
    public static class AGVLaundryPermissions
    {
        public const string GroupName = "AGVLaundry";

        //Add your own permission names. Example:
        //public const string MyPermission1 = GroupName + ".MyPermission1";

        public static class Tags
        {
            public const string Default = GroupName + ".Tags";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
        public static class BaseStations
        {
            public const string Default = GroupName + ".BaseStations";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
        public static class TagBatteries
        {
            public const string Default = GroupName + ".TagBatteries";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
        public static class TagRssis
        {
            public const string Default = GroupName + ".TagRssis";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
        public static class Trackers
        {
            public const string Default = GroupName + ".Trackers";
        }
        public static class Configurations
        {
            public const string Default = GroupName + ".Configurations";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
        public static class TagLocations
        {
            public const string Default = GroupName + ".TagLocations";
            public const string Create = Default + ".Create";
            public const string Edit = Default + ".Edit";
            public const string Delete = Default + ".Delete";
        }
    }
}