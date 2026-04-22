using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SWIMS.Data;
using SWIMS.Models.ViewModels;
using SWIMS.Services;
using System.Security.Claims;
using System.Text.Json;
using SWIMS.Services.Elsa;
using SWIMS.Services.Notifications;
using SWIMS.Services.Diagnostics.Auditing;


namespace SWIMS.Controllers
{
    public class StoredProcessesController : Controller
    {
        private readonly SwimsStoredProcsDbContext _db;
        private readonly StoredProcedureRunner _runner;
        private readonly IElsaWorkflowQueue _elsaQueue;
        private readonly IAuditLogger _audit;

        public StoredProcessesController(
            SwimsStoredProcsDbContext db,
            StoredProcedureRunner runner,
            IElsaWorkflowQueue elsaQueue,
            IAuditLogger audit)
        {
            _db = db;
            _runner = runner;
            _elsaQueue = elsaQueue;
            _audit = audit;
        }

        // GET: /StoredProcesses
        public async Task<IActionResult> Index()
        {
            var procs = await _db.StoredProcesses
                                 .AsNoTracking()
                                 .OrderBy(x => x.Name)
                                 .ToListAsync();
            return View(procs);
        }

        // GET: /StoredProcesses/Run/5
        [HttpGet]
        public async Task<IActionResult> Run(int id, int? formId = null, int? orgId = null)
        {
            var sp = await _db.StoredProcesses
                              .Include(x => x.Params)
                              .FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            var vm = new RunStoredProcessViewModel
            {
                ProcessId = sp.Id,
                Name = sp.Name,
                Description = sp.Description,
                ConnectionDisplay = !string.IsNullOrWhiteSpace(sp.ConnectionKey)
                    ? $"Connection: {sp.ConnectionKey}"
                    : $"{sp.DataSource}/{sp.Database}",
                Params = sp.Params
                           .OrderBy(p => p.Key)
                           .Select(p => new RunParamViewModel
                           {
                               Id = p.Id,
                               Key = p.Key,
                               DataType = p.DataType,
                               Value = p.Value
                           })
                           .ToList()
            };
            return View(vm);
        }

        // POST: /StoredProcesses/Run/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Run(int id, RunStoredProcessViewModel model, int? formId = null, int? orgId = null)
        {
            var sp = await _db.StoredProcesses.Include(x => x.Params).FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            // persist edited values
            var map = sp.Params.ToDictionary(p => p.Id);
            foreach (var p in model.Params)
                if (map.TryGetValue(p.Id, out var row)) row.Value = p.Value;
            await _db.SaveChangesAsync();

            // 📝 Audit: Stored procedure execute (prepare)
            var execStartedUtc = DateTime.UtcNow;

            // Do NOT log param values — only counts
            var paramCount = sp.Params?.Count ?? 0;
            var paramFilledCount = sp.Params?.Count(p => !string.IsNullOrWhiteSpace(p.Value)) ?? 0;
            // 📝 Audit: END

            // --- uuid-aware tokenization (unchanged) ---
            var uid = Request.Query["uid"].FirstOrDefault()
                   ?? Request.Form["uid"].FirstOrDefault()
                   ?? Request.Query["uuid"].FirstOrDefault()
                   ?? Request.Query["UID"].FirstOrDefault();

            var ctx = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FormId"] = formId?.ToString() ?? string.Empty,
                ["OrganizationId"] = orgId?.ToString() ?? string.Empty,
                ["FormUUID"] = uid ?? string.Empty,
                ["UserName"] = User?.Identity?.Name ?? "system"
            };
            var tokenizedParams = ApplyTokens(sp.Params, ctx);

            // ✅ Stash context so Export (GET) doesn’t read Request.Form
            TempData["uid"] = uid ?? string.Empty;
            TempData["formId"] = formId?.ToString() ?? string.Empty;
            TempData["orgId"] = orgId?.ToString() ?? string.Empty;
            TempData.Keep();

            var (table, error) = await _runner.ExecuteAsync(sp, tokenizedParams);

            var hasError = !string.IsNullOrWhiteSpace(error);
            string subject;
            string body;
            string? errorSummary = null;

            if (!hasError)
            {
                subject = "Stored procedure executed successfully";
                body = $"Stored process '{sp.Name}' executed successfully.";
            }
            else
            {
                // Take only the first line / prefix of the error, so we don't spam the notif.
                var firstLine = error
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault() ?? "Unknown error";

                // Trim to a reasonable length for the notification.
                errorSummary = firstLine.Length > 160 ? firstLine[..160] + "…" : firstLine;

                subject = "Stored procedure execution failed";
                body = $"Stored process '{sp.Name}' failed: {errorSummary}";
            }

            // 📝 Audit: Stored procedure executed
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);

            var execDurationMs = (DateTime.UtcNow - execStartedUtc).TotalMilliseconds;
            var rowCount = table?.Rows?.Count ?? 0;
            var colCount = table?.Columns?.Count ?? 0;

            await _audit.TryLogAsync(
                action: "StoredProcedureExecuted",
                entity: "StoredProcess",
                entityId: sp.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: null,
                newObj: new
                {
                    processId = sp.Id,
                    processName = sp.Name,
                    success = !hasError,
                    hasError,
                    errorSummary,
                    durationMs = execDurationMs,
                    rowCount,
                    colCount,
                    paramCount,
                    paramFilledCount
                },
                extra: new
                {
                    formId,
                    orgId,
                    uid
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Stored procedure executed (with success/failure info)
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyStoredProcAsync(
                eventKey: SwimsEventKeys.StoredProcedures.Executed,
                subject: subject,
                body: body,
                processId: sp.Id,
                processName: sp.Name,
                url: Url.Action(nameof(Run), new { id, formId, orgId, uid }),
                texts: new
                {
                    actor = new { subject = subject, body = body },
                    routed = new { subject = subject, body = $"{actorName}: {body}" },
                    superadmin = new { subject = subject, body = $"{actorName}: {body}" }
                },
                extraMeta: new { formId, orgId, uid, hasError, errorSummary },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


            // make context available to the RunResult view so Export can include it
            ViewBag.uid = uid;
            ViewBag.formId = formId;
            ViewBag.orgId = orgId;

            return View("RunResult", new RunStoredProcessResultViewModel
            {
                ProcessId = id,
                Name = sp.Name,
                Description = sp.Description,
                Error = error,
                Table = table
            });

        }

        [HttpGet]
        public async Task<IActionResult> Export(int id, string format = "csv", int? formId = null, int? orgId = null)
        {
            var sp = await _db.StoredProcesses.Include(x => x.Params).FirstOrDefaultAsync(x => x.Id == id);
            if (sp is null) return NotFound();

            // 📝 Audit: Actor
            AuditHelpers.TryResolveActor(User, out var actorId, out var actorUsername);
            // 📝 Audit: END

            // ✅ Only read Query on GET; fall back to TempData.Peek
            var uidQ = Request.Query["uid"].FirstOrDefault()
                    ?? Request.Query["uuid"].FirstOrDefault()
                    ?? Request.Query["UID"].FirstOrDefault()
                    ?? (TempData.Peek("uid") as string);

            var formIdStr = formId?.ToString() ?? (TempData.Peek("formId") as string ?? string.Empty);
            var orgIdStr = orgId?.ToString() ?? (TempData.Peek("orgId") as string ?? string.Empty);

            var ctx = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["FormId"] = formIdStr,
                ["OrganizationId"] = orgIdStr,
                ["FormUUID"] = uidQ ?? string.Empty,
                ["UserName"] = User?.Identity?.Name ?? "system"
            };
            var tokenizedParams = ApplyTokens(sp.Params, ctx);

            var (table, error) = await _runner.ExecuteAsync(sp, tokenizedParams);
            if (!string.IsNullOrWhiteSpace(error) || table is null)
            {
                // 📝 Audit: Stored procedure export failed

                string? exportErrorSummary = null;
                var exportError = error ?? "No data returned.";
                var exportFirstLine = exportError
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault() ?? "Unknown error";
                exportErrorSummary = exportFirstLine.Length > 160 ? exportFirstLine[..160] + "…" : exportFirstLine;

                await _audit.TryLogAsync(
                    action: "StoredProcedureExportFailed",
                    entity: "StoredProcess",
                    entityId: sp.Id.ToString(),
                    userId: actorId,
                    username: actorUsername,
                    oldObj: null,
                    newObj: new
                    {
                        processId = sp.Id,
                        processName = sp.Name,
                        format,
                        errorSummary = exportErrorSummary
                    },
                    extra: new
                    {
                        formId = formIdStr,
                        orgId = orgIdStr,
                        uid = uidQ
                    },
                    ct: HttpContext.RequestAborted);
                // 📝 Audit: END

                TempData["Error"] = error ?? "No data returned.";
                return RedirectToAction(nameof(Run), new { id, formId = formIdStr, orgId = orgIdStr, uid = uidQ });
            }

            var normalizedFormat = (format ?? "csv").ToLowerInvariant();
            if (normalizedFormat != "csv" && normalizedFormat != "xlsx" && normalizedFormat != "txt")
                return BadRequest("Unsupported format.");

            // 📝 Audit: Stored procedure export (prepare)
            var rowCount = table.Rows.Count;
            var colCount = table.Columns.Count;
            var fileNameBase = $"{sp.Name.Replace(':', '_').Replace('/', '_')}_{DateTime.UtcNow:yyyyMMdd_HHmmss}";
            var fileName = $"{fileNameBase}.{normalizedFormat}";
            // 📝 Audit: END

            await _audit.TryLogAsync(
                action: "StoredProcedureExported",
                entity: "StoredProcess",
                entityId: sp.Id.ToString(),
                userId: actorId,
                username: actorUsername,
                oldObj: null,
                newObj: new
                {
                    processId = sp.Id,
                    processName = sp.Name,
                    format,
                    rowCount,
                    colCount,
                    fileName
                },
                extra: new
                {
                    formId = formIdStr,
                    orgId = orgIdStr,
                    uid = uidQ
                },
                ct: HttpContext.RequestAborted);
            // 📝 Audit: END

            // 🔔 Notify: Stored procedure export
            var actorName = User?.Identity?.Name ?? "Someone";

            await NotifyStoredProcAsync(
                eventKey: SwimsEventKeys.StoredProcedures.Exported,
                subject: "Stored procedure exported",
                body: $"Data from stored process '{sp.Name}' was exported as {format.ToUpperInvariant()}.",
                processId: sp.Id,
                processName: sp.Name,
                url: Url.Action(nameof(Run), new { id, formId = formIdStr, orgId = orgIdStr, uid = uidQ }),
                texts: new
                {
                    actor = new { subject = "Stored procedure exported", body = $"You exported data from '{sp.Name}' as {format.ToUpperInvariant()}." },
                    routed = new { subject = "Stored procedure exported", body = $"{actorName} exported data from '{sp.Name}' as {format.ToUpperInvariant()}." },
                    superadmin = new { subject = "Stored procedure exported", body = $"{actorName} exported data from '{sp.Name}' as {format.ToUpperInvariant()}." }
                },
                extraMeta: new { formId = formIdStr, orgId = orgIdStr, uid = uidQ, format },
                ct: HttpContext.RequestAborted);

            // 🔔 Notify: END


            switch (normalizedFormat)
            {
                case "xlsx":
                {
                    var xlsx = DataTableToXlxs(table, sp.ExcludeHeadersOnExport);
                    return File(xlsx.ToArray(), "application/octet-stream", fileName);
                }
                case "txt":
                {
                    var txt = DataTableToTxt(table, includeHeaders: !sp.ExcludeHeadersOnExport);
                    return File(System.Text.Encoding.UTF8.GetBytes(txt), "application/octet-stream", fileName);
                }
                default:
                {
                    var csv = DataTableToCsv(table, includeHeaders: !sp.ExcludeHeadersOnExport);
                    return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
                }
            }
        }

        private static string DataTableToCsv(System.Data.DataTable dt, bool includeHeaders = true)
        {
            var sb = new System.Text.StringBuilder();

            // headers
            if (includeHeaders)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(EscapeCsv(dt.Columns[i].ColumnName));
                }
                sb.AppendLine();
            }

            // rows
            foreach (System.Data.DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    var val = row[i]?.ToString() ?? string.Empty;
                    sb.Append(EscapeCsv(val));
                }
                sb.AppendLine();
            }

            return sb.ToString();

            static string EscapeCsv(string s)
            {
                // wrap in quotes if it contains comma, quote, or newline; double the quotes inside
                var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                if (needsQuotes)
                    return $"\"{s.Replace("\"", "\"\"")}\"";
                return s;
            }
        }

        private static MemoryStream DataTableToXlxs(System.Data.DataTable dt, bool includeHeaders)
        {
            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var workSheet = package.Workbook.Worksheets.Add("Sheet1");
                workSheet.Cells["A1"].LoadFromDataTable(dt, true);

                // headers
                if (includeHeaders)
                {
                    workSheet.DeleteRow(1);
                }
                package.Save();
            }
            stream.Position = 0;

            return stream;
        }

        private static string DataTableToTxt(System.Data.DataTable dt, bool includeHeaders = true)
        {
            var sb = new System.Text.StringBuilder();

            // headers
            if (includeHeaders)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    sb.Append(EscapeTxt(dt.Columns[i].ColumnName));
                }
                sb.AppendLine();
            }

            // rows
            foreach (System.Data.DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    var val = row[i]?.ToString() ?? string.Empty;
                    sb.Append(EscapeTxt(val));
                }
                sb.AppendLine();
            }

            return sb.ToString();

            static string EscapeTxt(string s)
            {
                // wrap in quotes if it contains comma, quote, or newline; double the quotes inside
                var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                if (needsQuotes)
                    return $"\"{s.Replace("\"", "\"\"")}\"";
                return s;
            }
        }

        // ------------------- TOKEN HELPERS (tiny, self-contained) -------------------
        private static string ReplaceTokens(string? value, IDictionary<string, string> ctx)
        {
            if (string.IsNullOrEmpty(value)) return value ?? string.Empty;
            foreach (var kv in ctx)
                value = value.Replace("{" + kv.Key + "}", kv.Value, StringComparison.OrdinalIgnoreCase);
            return value;
        }

        private static IEnumerable<SWIMS.Models.StoredProcessParam> ApplyTokens(
            IEnumerable<SWIMS.Models.StoredProcessParam> src,
            IDictionary<string, string> ctx)
        {
            foreach (var p in src)
            {
                // return a transient copy — DB values remain unchanged
                yield return new SWIMS.Models.StoredProcessParam
                {
                    Id = p.Id,
                    StoredProcessId = p.StoredProcessId,
                    Key = p.Key,
                    DataType = p.DataType,
                    Value = ReplaceTokens(p.Value, ctx)
                };
            }
        }
        // ---------------------------------------------------------------------------

        // ---------------------------------------------------------------------------
        // Generic notification helper for stored procedure operations.
        // ---------------------------------------------------------------------------
        private async Task NotifyStoredProcAsync(
    string eventKey,
    string subject,
    string body,
    int? processId = null,
    string? processName = null,
    string? url = null,
    object? texts = null,
    object? extraMeta = null,
    CancellationToken ct = default)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var recipient = !string.IsNullOrWhiteSpace(userIdClaim)
                ? userIdClaim
                : User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(recipient))
                return;

            int? actorUserId = null;
            if (int.TryParse(userIdClaim, out var parsedActorId))
                actorUserId = parsedActorId;

            var actorUserName = User?.Identity?.Name ?? "system";

            var payload = new
            {
                Recipient = recipient,
                Channel = "InApp",
                Subject = subject,
                Body = body,
                MetadataJson = JsonSerializer.Serialize(new
                {
                    type = "System",
                    eventKey,
                    url,
                    metadata = new
                    {
                        actorUserId,
                        actorUserName,
                        processId,
                        processName,
                        texts,
                        extra = extraMeta
                    }
                })
            };

            try
            {
                // 🔔 Notify: Stored procedure event
                await _elsaQueue.EnqueueByNameAsync("Swims.Notifications.DirectInApp", payload, ct);
            }
            catch
            {
                // Never block execution if Elsa is unavailable.
            }
        }



    }
}
