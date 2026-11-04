using DMS_BAPL_Utils.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DMS_BAPL_Data.Services.JobCardServiceHistoryImportService
{
    /// <summary>
    /// Read-only access to BAPLDMSvad.dbo.JobCardServiceHistory (imported service history).
    /// Uses the API's DefaultConnection (BAPLDMSvad), same as the import; only SELECTs, all parameterised.
    /// </summary>
    public sealed class JobCardServiceHistoryReadService : IJobCardServiceHistoryReadService
    {
        private const int MaxPageSize = 200;
        private readonly IConfiguration _config;

        public JobCardServiceHistoryReadService(IConfiguration config) { _config = config; }

        private static readonly string Columns =
            "Id, " + string.Join(", ", JcshColumns.All.Select(c => $"[{c.Db}]")) + ", ImportBatchId, ImportedAtUtc, ImportedBy, SourceFileName, SourceRowNo";

        private SqlConnection NewConnection()
        {
            var cs = _config.GetConnectionString(JobCardServiceHistoryImportService.ConnectionName);
            if (string.IsNullOrWhiteSpace(cs))
                throw new InvalidOperationException($"Connection string '{JobCardServiceHistoryImportService.ConnectionName}' is not configured on the API.");
            return new SqlConnection(cs);
        }

        public async Task<JcshHistoryPage> SearchAsync(JcshHistoryQuery q, CancellationToken ct)
        {
            var page = Math.Max(1, q.Page);
            var size = Math.Clamp(q.PageSize <= 0 ? 50 : q.PageSize, 1, MaxPageSize);
            var where = new StringBuilder("WHERE 1 = 1");
            var ps = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(q.ChassisNo)) { where.Append(" AND ChassisNo = @chassis"); ps.Add(new SqlParameter("@chassis", SqlDbType.NVarChar, 30) { Value = q.ChassisNo.Trim() }); }
            if (!string.IsNullOrWhiteSpace(q.RegNo)) { where.Append(" AND RegNo = @reg"); ps.Add(new SqlParameter("@reg", SqlDbType.NVarChar, 30) { Value = q.RegNo.Trim() }); }
            if (!string.IsNullOrWhiteSpace(q.DealerCode)) { where.Append(" AND DealerCode = @dealer"); ps.Add(new SqlParameter("@dealer", SqlDbType.NVarChar, 20) { Value = q.DealerCode.Trim() }); }
            if (q.JobNo.HasValue) { where.Append(" AND JobNo = @jobNo"); ps.Add(new SqlParameter("@jobNo", SqlDbType.Int) { Value = q.JobNo.Value }); }
            if (q.FromDate.HasValue) { where.Append(" AND JobDate >= @from"); ps.Add(new SqlParameter("@from", SqlDbType.Date) { Value = q.FromDate.Value.Date }); }
            if (q.ToDate.HasValue) { where.Append(" AND JobDate <= @to"); ps.Add(new SqlParameter("@to", SqlDbType.Date) { Value = q.ToDate.Value.Date }); }

            var result = new JcshHistoryPage { Page = page, PageSize = size };
            await using var con = NewConnection();
            await con.OpenAsync(ct);

            await using (var cnt = new SqlCommand($"SELECT COUNT_BIG(*) FROM dbo.JobCardServiceHistory {where};", con) { CommandTimeout = 60 })
            {
                cnt.Parameters.AddRange(ps.Select(Clone).ToArray());
                result.Total = (long)(await cnt.ExecuteScalarAsync(ct) ?? 0L);
            }

            var sql = $"SELECT {Columns} FROM dbo.JobCardServiceHistory {where} ORDER BY JobDate DESC, Id DESC OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;";
            await using var cmd = new SqlCommand(sql, con) { CommandTimeout = 60 };
            cmd.Parameters.AddRange(ps.Select(Clone).ToArray());
            cmd.Parameters.Add("@skip", SqlDbType.Int).Value = (page - 1) * size;
            cmd.Parameters.Add("@take", SqlDbType.Int).Value = size;
            result.Rows = await ReadAsync(cmd, ct);
            return result;
        }

        public async Task<List<Dictionary<string, object?>>> ByChassisAsync(string chassisNo, CancellationToken ct)
        {
            await using var con = NewConnection();
            await con.OpenAsync(ct);
            await using var cmd = new SqlCommand($"SELECT TOP (500) {Columns} FROM dbo.JobCardServiceHistory WHERE ChassisNo = @chassis ORDER BY JobDate DESC, Id DESC;", con) { CommandTimeout = 60 };
            cmd.Parameters.Add("@chassis", SqlDbType.NVarChar, 30).Value = chassisNo.Trim();
            return await ReadAsync(cmd, ct);
        }

        private static SqlParameter Clone(SqlParameter p) => new(p.ParameterName, p.SqlDbType, p.Size) { Value = p.Value };

        private static async Task<List<Dictionary<string, object?>>> ReadAsync(SqlCommand cmd, CancellationToken ct)
        {
            var rows = new List<Dictionary<string, object?>>();
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct))
            {
                var row = new Dictionary<string, object?>(rd.FieldCount);
                for (var i = 0; i < rd.FieldCount; i++) row[rd.GetName(i)] = rd.IsDBNull(i) ? null : rd.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }
    }
}
