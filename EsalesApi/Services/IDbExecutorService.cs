using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eyewa_new_api.Services
{
    public interface IDbExecutorService
    {
        Task<List<Dictionary<string, object>>> ExecuteStoredProcedureAsync(string spName, Dictionary<string, object?> parameters);
        Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object?>? parameters = null);
    }
}
