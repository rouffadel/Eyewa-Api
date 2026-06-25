using Eyewa_new_api.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Eyewa_new_api.Services
{
    public class DbExecutorService : IDbExecutorService
    {
        private readonly ApplicationDbContext _context;

        public DbExecutorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Dictionary<string, object>>> ExecuteStoredProcedureAsync(string spName, Dictionary<string, object?> parameters)
        {
            var conn = _context.Database.GetDbConnection();
            bool wasClosed = conn.State == ConnectionState.Closed;
            if (wasClosed)
            {
                await conn.OpenAsync();
            }
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = spName;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 100000;
                
                foreach (var param in parameters)
                {
                    var sqlParam = cmd.CreateParameter();
                    sqlParam.ParameterName = param.Key;
                    sqlParam.Value = param.Value ?? DBNull.Value;
                    cmd.Parameters.Add(sqlParam);
                }
                
                using var reader = await cmd.ExecuteReaderAsync();
                var result = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var val = reader.GetValue(i);
                        row[name] = val == DBNull.Value ? null : val;
                    }
                    result.Add(row);
                }
                return result;
            }
            finally
            {
                if (wasClosed)
                {
                    await conn.CloseAsync();
                }
            }
        }

        public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string sql, Dictionary<string, object?>? parameters = null)
        {
            var conn = _context.Database.GetDbConnection();
            bool wasClosed = conn.State == ConnectionState.Closed;
            if (wasClosed)
            {
                await conn.OpenAsync();
            }
            try
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 100000;
                
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        var sqlParam = cmd.CreateParameter();
                        sqlParam.ParameterName = param.Key;
                        sqlParam.Value = param.Value ?? DBNull.Value;
                        cmd.Parameters.Add(sqlParam);
                    }
                }
                
                using var reader = await cmd.ExecuteReaderAsync();
                var result = new List<Dictionary<string, object>>();
                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        var name = reader.GetName(i);
                        var val = reader.GetValue(i);
                        row[name] = val == DBNull.Value ? null : val;
                    }
                    result.Add(row);
                }
                return result;
            }
            finally
            {
                if (wasClosed)
                {
                    await conn.CloseAsync();
                }
            }
        }
    }
}
