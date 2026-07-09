using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using Eyewa.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Data.Common;

namespace Eyewa.Infrastructure.Services
{
    public class DbExecutorService : IDbExecutorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DbExecutorService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task SetTenantContextAsync(DbConnection conn)
        {
            // Tenant session context disabled per user request
            /*
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId";
                var param = cmd.CreateParameter();
                param.ParameterName = "@TenantId";
                param.Value = tenantId;
                cmd.Parameters.Add(param);
                await cmd.ExecuteNonQueryAsync();
            }
            */
            await Task.CompletedTask;
        }

        public async Task<List<Dictionary<string, object>>> ExecuteStoredProcedureAsync(string spName, Dictionary<string, object?> parameters)
        {
            var conn = _context.Database.GetDbConnection();
            bool wasClosed = conn.State == ConnectionState.Closed;
            if (wasClosed)
            {
                await conn.OpenAsync();
            }
            await SetTenantContextAsync(conn);
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


        public async Task<Dictionary<string, List<Dictionary<string, object>>>> ExecuteStoredProcedureMultiResultAsync(
    string spName,
    Dictionary<string, object?> parameters)
        {
            var conn = _context.Database.GetDbConnection();

            bool wasClosed = conn.State == ConnectionState.Closed;

            if (wasClosed)
                await conn.OpenAsync();

            await SetTenantContextAsync(conn);

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

                var result = new Dictionary<string, List<Dictionary<string, object>>>();

                int tableIndex = 0;

                do
                {
                    var rows = new List<Dictionary<string, object>>();

                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row[reader.GetName(i)] =
                                reader.IsDBNull(i) ? null : reader.GetValue(i);
                        }

                        rows.Add(row);
                    }

                    string tableName = tableIndex == 0
                        ? "table"
                        : $"table{tableIndex}";

                    result.Add(tableName, rows);

                    tableIndex++;

                } while (await reader.NextResultAsync());

                return result;
            }
            finally
            {
                if (wasClosed)
                    await conn.CloseAsync();
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
            await SetTenantContextAsync(conn);
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




