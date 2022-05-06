using System.Threading.Tasks;

namespace AGV.Laundry.Data
{
    public interface IAGVLaundryDbSchemaMigrator
    {
        Task MigrateAsync();
    }
}
