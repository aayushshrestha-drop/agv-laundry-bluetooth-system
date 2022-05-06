using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AGV.Laundry.Data
{
    /* This is used if database provider does't define
     * ILocationEngineDbSchemaMigrator implementation.
     */
    public class NullAGVLaundryDbSchemaMigrator : IAGVLaundryDbSchemaMigrator, ITransientDependency
    {
        public Task MigrateAsync()
        {
            return Task.CompletedTask;
        }
    }
}