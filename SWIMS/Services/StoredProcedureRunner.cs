using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using SWIMS.Models;

namespace SWIMS.Services
{
    public sealed class StoredProcedureRunner
    {
        private readonly IConfiguration _config;
        private readonly IDataProtector? _protector;

        public StoredProcedureRunner(IConfiguration config, IDataProtectionProvider? dp = null)
        {
            _config = config;
            _protector = dp?.CreateProtector("SWIMS.StoredProcedures");
        }

        public async Task<(DataTable? Table, string? Error)> ExecuteAsync(
            StoredProcess sp,
            IEnumerable<StoredProcessParam> parameters,
            CancellationToken ct = default)
        {
            try
            {
                var connectionString = BuildConnectionString(sp);
                using var conn = new SqlConnection(connectionString);
                using var cmd = new SqlCommand(sp.Name, conn) { CommandType = CommandType.StoredProcedure };

                foreach (var p in parameters)
                {
                    var (sqlType, val) = Coerce(p.DataType, p.Value);
                    cmd.Parameters.Add(new SqlParameter(p.Key, sqlType) { Value = val ?? DBNull.Value });
                }

                await conn.OpenAsync(ct).ConfigureAwait(false);
                await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);

                using var adapter = new SqlDataAdapter(cmd);
                var table = new DataTable();
                adapter.Fill(table);
                return (table.Rows.Count > 0 ? table : null, null);
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        private string BuildConnectionString(StoredProcess sp)
        {
            // Preferred: resolve by ConnectionStrings key
            if (!string.IsNullOrWhiteSpace(sp.ConnectionKey))
            {
                var cs = _config.GetConnectionString(sp.ConnectionKey);
                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException($"Missing ConnectionStrings:{sp.ConnectionKey}");
                return cs!;
            }

            // Fallback: build from parts (keeps module independent from the app's main connection)
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = sp.DataSource ?? throw new InvalidOperationException("DataSource required"),
                InitialCatalog = sp.Database ?? throw new InvalidOperationException("Database required"),
                MultipleActiveResultSets = true,
                TrustServerCertificate = true
            };

            var hasUser = !string.IsNullOrWhiteSpace(sp.DbUserEncrypted) || !string.IsNullOrWhiteSpace(sp.DbPasswordEncrypted);
            if (!hasUser)
            {
                builder.IntegratedSecurity = true; // recommended if your app server has rights
            }
            else
            {
                builder.UserID = Decrypt(sp.DbUserEncrypted);
                builder.Password = Decrypt(sp.DbPasswordEncrypted);
                builder.IntegratedSecurity = false;
            }

            return builder.ConnectionString;
        }

        private string? Decrypt(string? cipher)
        {
            if (string.IsNullOrEmpty(cipher) || _protector is null) return cipher;
            try { return _protector.Unprotect(cipher); } catch { return cipher; }
        }

        private static (SqlDbType, object?) Coerce(string type, string? raw)
        {
            switch (type)
            {
                case "Int": return (SqlDbType.Int, int.TryParse(raw, out var i) ? i : (object?)DBNull.Value);
                case "Float": return (SqlDbType.Float, double.TryParse(raw, out var f) ? f : (object?)DBNull.Value);
                case "Decimal": return (SqlDbType.Decimal, decimal.TryParse(raw, out var d) ? d : (object?)DBNull.Value);
                case "Bit": return (SqlDbType.Bit, bool.TryParse(raw, out var b) ? b : (object?)DBNull.Value);
                case "DateTime": return (SqlDbType.DateTime2, DateTime.TryParse(raw, out var dt) ? dt : (object?)DBNull.Value);
                case "UniqueIdentifier": return (SqlDbType.UniqueIdentifier, Guid.TryParse(raw, out var g) ? g : (object?)DBNull.Value);
                case "Text": return (SqlDbType.NVarChar, raw);
                case "NVarChar":
                default: return (SqlDbType.NVarChar, raw);
            }
        }
    }
}