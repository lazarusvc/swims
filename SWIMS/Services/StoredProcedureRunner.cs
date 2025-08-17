using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SWIMS.Models;
using System;
using System.Data;

namespace SWIMS.Services
{
    public class StoredProcedureRunner
    {
        private readonly IConfiguration _config;
        private readonly StoredProcOptions _opts;

        public StoredProcedureRunner(IConfiguration config, IOptions<StoredProcOptions> opts)
        {
            _config = config;
            _opts = opts.Value;
        }

        public async Task<(DataTable? Table, string? Error)> ExecuteAsync(
            StoredProcess proc, IEnumerable<StoredProcessParam> parameters,
            CancellationToken ct = default)
        {
            try
            {
                var connString = BuildConnectionString(proc);
                using var conn = new SqlConnection(connString);
                await conn.OpenAsync(ct);

                using var cmd = new SqlCommand(proc.Name, conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    CommandTimeout = Math.Max(1, _opts.DefaultCommandTimeoutSeconds)
                };

                // bind parameters by name
                foreach (var p in parameters.OrderBy(p => p.Key))
                {
                    var sqlParam = new SqlParameter
                    {
                        ParameterName = p.Key, // must include @
                        SqlDbType = ToSqlDbType(p.DataType),
                        Direction = ParameterDirection.Input,
                        Value = CoerceValue(p)
                    };
                    cmd.Parameters.Add(sqlParam);
                }

                var dt = new DataTable();
                using var rdr = await cmd.ExecuteReaderAsync(ct);
                dt.Load(rdr);
                return (dt, null);
            }
            catch (OperationCanceledException)
            {
                return (null, "Execution cancelled or timed out.");
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        private string BuildConnectionString(StoredProcess proc)
        {
            // Option A: ConnectionStrings key
            if (!string.IsNullOrWhiteSpace(proc.ConnectionKey))
            {
                var cs = _config.GetConnectionString(proc.ConnectionKey!);
                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException($"ConnectionString '{proc.ConnectionKey}' not found.");
                return cs;
            }

            // Option B: Manual DataSource/Database (+ optional SQL login)
            var b = new SqlConnectionStringBuilder
            {
                DataSource = proc.DataSource ?? throw new InvalidOperationException("DataSource required."),
                InitialCatalog = proc.Database ?? throw new InvalidOperationException("Database required."),
                MultipleActiveResultSets = _opts.ManualConnection.MultipleActiveResultSets,
                Encrypt = _opts.ManualConnection.Encrypt,
                TrustServerCertificate = _opts.ManualConnection.TrustServerCertificate,
                ConnectTimeout = Math.Max(1, _opts.ManualConnection.ConnectTimeout)
            };

            if (!string.IsNullOrWhiteSpace(proc.DbUserEncrypted) && !string.IsNullOrWhiteSpace(proc.DbPasswordEncrypted))
            {
                // DbUser/DbPassword are already stored encrypted by the admin controller.
                // Decrypt here if you encrypt at rest; if you stored plaintext, assign directly.
                var user = DecryptIfNeeded(proc.DbUserEncrypted!);
                var pass = DecryptIfNeeded(proc.DbPasswordEncrypted!);
                b.UserID = user;
                b.Password = pass;
                b.IntegratedSecurity = false;
            }
            else
            {
                b.IntegratedSecurity = true; // Windows auth
            }

            return b.ConnectionString;
        }

        private static object CoerceValue(StoredProcessParam p)
        {
            if (p.Value is null) return DBNull.Value;

            return p.DataType?.ToLowerInvariant() switch
            {
                "int" => int.TryParse(p.Value, out var i) ? i : DBNull.Value,
                "float" => double.TryParse(p.Value, out var d) ? d : DBNull.Value,
                "decimal" => decimal.TryParse(p.Value, out var m) ? m : DBNull.Value,
                "bit" => bool.TryParse(p.Value, out var b) ? b : (p.Value == "1" ? true : p.Value == "0" ? false : DBNull.Value),
                "datetime" => DateTime.TryParse(p.Value, out var dt) ? dt : DBNull.Value,
                "uniqueidentifier" => Guid.TryParse(p.Value, out var g) ? g : DBNull.Value,
                "text" or "nvarchar" or _ => p.Value
            };
        }

        private static SqlDbType ToSqlDbType(string? dataType) => (dataType ?? "NVarChar").ToLowerInvariant() switch
        {
            "int" => SqlDbType.Int,
            "float" => SqlDbType.Float,
            "decimal" => SqlDbType.Decimal,
            "bit" => SqlDbType.Bit,
            "datetime" => SqlDbType.DateTime2,
            "uniqueidentifier" => SqlDbType.UniqueIdentifier,
            "text" => SqlDbType.NVarChar,    // treated as NVARCHAR for input
            _ => SqlDbType.NVarChar
        };

        // If you protected DbUser/DbPassword, plug your decryptor here. If they’re plaintext, just return the value.
        private static string DecryptIfNeeded(string s) => s;
    }
}