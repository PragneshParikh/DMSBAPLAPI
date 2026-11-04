using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DMS_BAPL_Utils.Helpers
{
    /// <summary>
    /// Live status of a long-running Excel import (Vehicle Sale Import, Vehicle Dispatch Import).
    /// The page generates a ProgressId, sends it with the import options, and polls
    /// GET api/import-progress/{id} while the import request is still running.
    /// In-memory only (per API instance); entries expire after 2 hours. It never affects the import itself.
    /// </summary>
    public sealed class ImportProgressInfo
    {
        private readonly object _sync = new();

        public string Id { get; init; } = "";
        public string Kind { get; init; } = "";
        public bool DryRun { get; init; }
        public string Phase { get; private set; } = "Starting…";
        /// <summary>current step / number of steps (e.g. 2 of 5)</summary>
        public int Step { get; private set; }
        public int Steps { get; private set; }
        public int RowsRead { get; private set; }
        public int TotalRows { get; private set; }
        /// <summary>"bills" / "chassis" - what Done/Total count</summary>
        public string Unit { get; private set; } = "";
        public int Done { get; private set; }
        public int Total { get; private set; }
        public int BatchNo { get; private set; }
        public int Batches { get; private set; }
        public int Inserted { get; private set; }
        public int Updated { get; private set; }
        public int Failed { get; private set; }
        public bool Finished { get; private set; }
        public bool Success { get; private set; }
        public DateTime StartedAtUtc { get; init; } = DateTime.UtcNow;
        public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;
        public double ElapsedSeconds => Math.Round(((Finished ? UpdatedAtUtc : DateTime.UtcNow) - StartedAtUtc).TotalSeconds, 1);

        private void Touch() => UpdatedAtUtc = DateTime.UtcNow;

        public void SetPhase(string phase, int step, int steps)
        {
            lock (_sync) { Phase = phase; Step = step; Steps = steps; Touch(); }
        }

        public void SetRows(int read, int total)
        {
            lock (_sync) { RowsRead = read; TotalRows = total; Touch(); }
        }

        /// <summary>work counter for the current phase (e.g. 1,200 of 4,500 bills); resets the batch counter</summary>
        public void SetWork(string unit, int done, int total, int batchNo = 0, int batches = 0)
        {
            lock (_sync) { Unit = unit; Done = done; Total = total; BatchNo = batchNo; Batches = batches; Touch(); }
        }

        public void SetCounts(int inserted, int updated, int failed)
        {
            lock (_sync) { Inserted = inserted; Updated = updated; Failed = failed; Touch(); }
        }

        public void Finish(bool success, string phase)
        {
            // on failure keep the step it stopped at, so the panel shows where it failed
            lock (_sync) { Finished = true; Success = success; Phase = phase; if (success) Step = Steps; Touch(); }
        }

        public ImportProgressInfo Snapshot()
        {
            lock (_sync)
            {
                var s = new ImportProgressInfo { Id = Id, Kind = Kind, DryRun = DryRun, StartedAtUtc = StartedAtUtc };
                s.Phase = Phase; s.Step = Step; s.Steps = Steps; s.RowsRead = RowsRead; s.TotalRows = TotalRows;
                s.Unit = Unit; s.Done = Done; s.Total = Total; s.BatchNo = BatchNo; s.Batches = Batches;
                s.Inserted = Inserted; s.Updated = Updated; s.Failed = Failed; s.Finished = Finished; s.Success = Success;
                s.UpdatedAtUtc = UpdatedAtUtc;
                return s;
            }
        }
    }

    public static class ImportProgress
    {
        private static readonly ConcurrentDictionary<string, ImportProgressInfo> Items = new(StringComparer.OrdinalIgnoreCase);
        private static readonly TimeSpan Ttl = TimeSpan.FromHours(2);

        /// <summary>Registers a progress entry; returns null (no tracking) when the caller sent no / an invalid id.</summary>
        public static ImportProgressInfo? Start(string? id, string kind, bool dryRun)
        {
            if (string.IsNullOrWhiteSpace(id) || id.Length > 64 || !id.All(c => char.IsLetterOrDigit(c) || c == '-')) return null;
            foreach (var old in Items.Where(kv => DateTime.UtcNow - kv.Value.UpdatedAtUtc > Ttl).Select(kv => kv.Key).ToList())
                Items.TryRemove(old, out _);
            var p = new ImportProgressInfo { Id = id, Kind = kind, DryRun = dryRun };
            Items[id] = p;
            return p;
        }

        public static ImportProgressInfo? Get(string id) => Items.TryGetValue(id, out var p) ? p.Snapshot() : null;

        /// <summary>Marks a still-running entry as failed (called by the controller when the import throws).</summary>
        public static void Fail(string? id, string phase)
        {
            if (!string.IsNullOrWhiteSpace(id) && Items.TryGetValue(id, out var p) && !p.Finished)
                p.Finish(false, $"{phase} — while: {p.Phase.TrimEnd('…')}");
        }
    }
}
