using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;
using SWIMS.Models;          
using SWIMS.Models.StoredProcs; 
using SWIMS.Models.Security;     
using Microsoft.AspNetCore.DataProtection;

namespace SWIMS.Services
{
    public class StoredProcedureRunner
    {
        private readonly IConfiguration _config;
        private readonly StoredProcOptions _opts;
        private readonly IDataProtector? _protector;

        public StoredProcedureRunner(
            IConfiguration config,
            IOptions<StoredProcOptions> opts,
            IDataProtectionProvider dp)               
        {
            _config = config;
            _opts = opts.Value;
            _protector = dp?.CreateProtector(DataProtectionPurposes.StoredProcedures);
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

                foreach (var p in parameters.OrderBy(p => p.Key))
                {
                    cmd.Parameters.Add(new SqlParameter
                    {
                        ParameterName = p.Key, // includes @
                        SqlDbType = ToSqlDbType(p.DataType),
                        Direction = ParameterDirection.Input,
                        Value = CoerceValue(p)
                    });
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
            // Option A: use named ConnectionStrings key
            if (!string.IsNullOrWhiteSpace(proc.ConnectionKey))
            {
                var cs = _config.GetConnectionString(proc.ConnectionKey!);
                if (string.IsNullOrWhiteSpace(cs))
                    throw new InvalidOperationException($"ConnectionString '{proc.ConnectionKey}' not found.");
                return cs;
            }

            // Option B: manual connection with safe defaults
            var b = new SqlConnectionStringBuilder
            {
                DataSource = proc.DataSource ?? throw new InvalidOperationException("DataSource required."),
                InitialCatalog = proc.Database ?? throw new InvalidOperationException("Database required."),
                MultipleActiveResultSets = _opts.ManualConnection.MultipleActiveResultSets,
                Encrypt = _opts.ManualConnection.Encrypt,
                TrustServerCertificate = _opts.ManualConnection.TrustServerCertificate,
                ConnectTimeout = Math.Max(1, _opts.ManualConnection.ConnectTimeout)
            };

            // If creds were provided, decrypt (or pass-through if they were plaintext)
            var hasUser = !string.IsNullOrWhiteSpace(proc.DbUserEncrypted);
            var hasPass = !string.IsNullOrWhiteSpace(proc.DbPasswordEncrypted);

            if (hasUser && hasPass)
            {
                b.UserID = DecryptIfNeeded(proc.DbUserEncrypted!);
                b.Password = DecryptIfNeeded(proc.DbPasswordEncrypted!);
                b.IntegratedSecurity = false;
            }
            else
            {
                b.IntegratedSecurity = true; // Windows auth
            }

            return b.ConnectionString;
        }

        private string DecryptIfNeeded(string value)
        {
            if (string.IsNullOrEmpty(value) || _protector == null) return value;
            try
            {
                return _protector.Unprotect(value);
            }
            catch
            {
                // Not protected (legacy/plaintext) or wrong key ring → fall back
                return value;
            }
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
            "text" => SqlDbType.NVarChar,
            _ => SqlDbType.NVarChar
        };
    }
}
