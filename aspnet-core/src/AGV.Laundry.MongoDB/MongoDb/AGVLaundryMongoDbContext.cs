using MongoDB.Driver;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace AGV.Laundry.MongoDB
{
    [ConnectionStringName("Default")]
    public class AGVLaundryMongoDbContext : AbpMongoDbContext
    {
        /* Add mongo collections here. Example:
         * public IMongoCollection<Question> Questions => Collection<Question>();
         */
        public IMongoCollection<AGV.Laundry.Tags.Tag> Tags => Collection<AGV.Laundry.Tags.Tag>();
        public IMongoCollection<AGV.Laundry.BaseStations.BaseStation> BaseStations => Collection<AGV.Laundry.BaseStations.BaseStation>();
        public IMongoCollection<AGV.Laundry.TagRssis.TagRssi> TagRssis => Collection<AGV.Laundry.TagRssis.TagRssi>();
        public IMongoCollection<AGV.Laundry.TagLocationLogs.TagLocationLog> TagLocationLogs => Collection<AGV.Laundry.TagLocationLogs.TagLocationLog>();

        public IMongoCollection<AGV.Laundry.ApiRequestLogs.ApiRequestLog> ApiRequestLogs => Collection<AGV.Laundry.ApiRequestLogs.ApiRequestLog>();

        public IMongoCollection<AGV.Laundry.Configurations.Configuration> Configurations => Collection<AGV.Laundry.Configurations.Configuration>();
        protected override void CreateModel(IMongoModelBuilder modelBuilder)
        {
            base.CreateModel(modelBuilder);
        }
    }
}
