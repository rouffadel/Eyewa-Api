using static Eyewa.Application.DTOs.Common;
using Eyewa.Domain.Entities;
using Eyewa.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eyewa.Application.Interfaces
{
    public interface IDbExecutorService
    {
        Task<List<Dictionary<string, object>>> ExecuteStoredProcedureAsync(string spName, Dictionary<string, object?> parameters);
        Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object?>? parameters = null);
        Task<Dictionary<string, List<Dictionary<string, object>>>> ExecuteStoredProcedureMultiResultAsync(string spName, Dictionary<string, object?> parameters);
    }
}





