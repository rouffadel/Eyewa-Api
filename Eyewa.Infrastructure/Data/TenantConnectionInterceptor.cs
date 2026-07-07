using Eyewa.Domain.Entities;
using Eyewa.Application.Interfaces;
using Eyewa.Application.DTOs;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Eyewa.Infrastructure.Data
{
    public class TenantConnectionInterceptor : DbConnectionInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantConnectionInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            SetTenantContext(connection);
            base.ConnectionOpened(connection, eventData);
        }

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            await SetTenantContextAsync(connection);
            await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);
        }

        private void SetTenantContext(DbConnection connection)
        {
            // Tenant session context disabled per user request
            /*
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId";
                var param = cmd.CreateParameter();
                param.ParameterName = "@TenantId";
                param.Value = tenantId;
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();
            }
            */
        }

        private async Task SetTenantContextAsync(DbConnection connection)
        {
            // Tenant session context disabled per user request
            /*
            var tenantId = _httpContextAccessor.HttpContext?.User?.FindFirst("tenantId")?.Value;
            if (!string.IsNullOrEmpty(tenantId))
            {
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@TenantId";
                var param = cmd.CreateParameter();
                param.ParameterName = "@TenantId";
                param.Value = tenantId;
                cmd.Parameters.Add(param);
                await cmd.ExecuteNonQueryAsync();
            }
            */
        }
    }
}




