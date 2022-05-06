using AGV.Laundry.Tags;
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
        protected override void CreateModel(IMongoModelBuilder modelBuilder)
        {
            base.CreateModel(modelBuilder);
        }
    }
}
