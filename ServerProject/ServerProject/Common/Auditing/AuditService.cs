using System.Collections.Concurrent;

namespace ServerProject.Common
{
    /// <summary>
    /// 審計服務實現，提供審計記錄的寫入、查詢和簽名功能
    /// </summary>
    public class AuditRepository : IAuditRepository
    {
        /// <summary>
        /// 時鐘提供者，用於獲取當前時間
        /// </summary>
        private readonly ITimeProvider _clock;

        /// <summary>
        /// 審計記錄 ID 序列，從 1 開始遞增
        /// </summary>
        private long _idSeq = 0;

        /// <summary>
        /// 存儲審計記錄的字典，使用長整型 ID 作為鍵
        /// </summary>
        private readonly ConcurrentDictionary<long, Stored> _store = new();

        /// <summary>
        /// 內部存儲類，用於封裝審計條目和記錄
        /// </summary>
        private sealed class Stored
        {
            public AuditEntry Entry { get; init; } = default!;
            public AuditRecord Record { get; init; } = default!;
            public bool Signed { get; set; }
            public string? Signature { get; set; }
        }

        /// <summary>
        /// 審計服務構造函數，接受一個時間提供者以獲取當前 UTC 時間
        /// </summary>
        /// <param name="clock">當前 UTC 時間</param>
        public AuditRepository(ITimeProvider clock)
        {
            _clock = clock;
        }

        /// <summary>
        /// 寫入審計條目到審計系統
        /// </summary>
        /// <param name="entry">審計條目類</param>
        /// <returns>true:寫入成功/false:寫入失敗</returns>
        public bool Write(AuditEntry entry)
        {
            // 正規化：確保 UTC 時戳存在
            var occurredUtc = entry.OccurredAtUtc == default
                ? _clock.UtcNow()
                : entry.OccurredAtUtc.Kind == DateTimeKind.Utc
                    ? entry.OccurredAtUtc
                    : entry.OccurredAtUtc.ToUniversalTime();

            // 生成新的 ID，使用 Interlocked 以確保線程安全
            var id = Interlocked.Increment(ref _idSeq);

            // 建立審計記錄
            var record = new AuditRecord
            {
                Id = (int)id, // 你的模型是 int；這裡示範轉回 int
                UserId = entry.ActorUserId ?? 0,
                Action = entry.Action,
                Timestamp = occurredUtc,
                Details = BuildDetails(entry)
            };

            // 建立存儲對象，包含審計條目和記錄
            var stored = new Stored
            {
                Entry = new AuditEntry
                {
                    OccurredAtUtc = occurredUtc,
                    ActorUserId = entry.ActorUserId,
                    ActorRole = entry.ActorRole,
                    Action = entry.Action,
                    TargetType = entry.TargetType,
                    TargetId = entry.TargetId,
                    CorrelationId = entry.CorrelationId == Guid.Empty ? Guid.NewGuid() : entry.CorrelationId,
                    ClientIp = entry.ClientIp,
                    UserAgent = entry.UserAgent,
                    Status = entry.Status,
                    Reason = entry.Reason,
                    BeforeSnapshot = entry.BeforeSnapshot,
                    AfterSnapshot = entry.AfterSnapshot
                },
                Record = record,
                Signed = false
            };

            // 嘗試將新審計記錄添加到存儲中
            return _store.TryAdd(id, stored);
        }

        /// <summary>
        /// 查詢審計記錄
        /// </summary>
        /// <param name="query">審計條目</param>
        /// <returns>所有審計記錄的基本信息</returns>
        public List<AuditRecord> Query(AuditQuery query)
        {
            // 使用 LINQ 過濾存儲的審計記錄
            IEnumerable<Stored> q = _store.Values;

            // 根據查詢條件過濾
            if (query.From.HasValue)
                q = q.Where(x => x.Record.Timestamp >= query.From.Value);

            // 如果有結束時間，則過濾掉超過該時間的記錄
            if (query.To.HasValue)
                q = q.Where(x => x.Record.Timestamp <= query.To.Value);

            // 根據用戶 ID、操作、目標類型和狀態進行進一步過濾
            if (query.ActorUserId.HasValue)
                q = q.Where(x => x.Entry.ActorUserId == query.ActorUserId.Value);

            // 如果 Action、TargetType 或 TargetId 有值，則進行相應的過濾
            if (!string.IsNullOrWhiteSpace(query.Action))
                q = q.Where(x => string.Equals(x.Entry.Action, query.Action, StringComparison.OrdinalIgnoreCase));

            // 如果 TargetType 有值，則進行相應的過濾
            if (!string.IsNullOrWhiteSpace(query.TargetType))
                q = q.Where(x => string.Equals(x.Entry.TargetType, query.TargetType, StringComparison.OrdinalIgnoreCase));

            // 如果 TargetId 有值，則進行相應的過濾
            if (query.TargetId.HasValue)
                q = q.Where(x => x.Entry.TargetId == query.TargetId.Value);

            // 如果 Status 有值，則進行相應的過濾
            if (query.Status.HasValue)
                q = q.Where(x => x.Entry.Status == query.Status.Value);

            // 回傳精簡的 Record 列表（按照時間倒序）
            return q.OrderByDescending(x => x.Record.Timestamp)
                    .Select(x => x.Record)
                    .ToList();
        }

        /// <summary>
        /// 追加簽名到審計記錄
        /// </summary>
        /// <param name="id">審計記錄ID</param>
        /// <returns>true:追加成功/false:追加失敗</returns>
        public bool AppendSignature(long id)
        {
            // 嘗試從存儲中獲取指定 ID 的審計記錄
            if (!_store.TryGetValue(id, out var stored))
                return false;

            // 簽名的實際內容可替換為你們的簽章/雜湊流程
            stored.Signature = $"SIGNED:{id}:{stored.Record.Timestamp:O}";
            stored.Signed = true;

            // 你若需要把「已簽名」反映到 Details 亦可在此同步修改：
            stored.Record.Details += " | signature=appended";
            return true;
        }

        /// <summary>
        /// 生成審計記錄的詳細信息字符串
        /// </summary>
        /// <param name="e">審計條目</param>
        /// <returns>審計記錄的詳細信息字符串</returns>
        private static string BuildDetails(AuditEntry e)
        {
            // 依需要擴充；避免把大型快照直接序列化塞進 Details
            return $"action={e.Action}; target={e.TargetType}/{e.TargetId}; " +
                   $"status={e.Status}; reason={e.Reason}; actor={e.ActorUserId}({e.ActorRole}); " +
                   $"corr={e.CorrelationId}; ip={e.ClientIp}";
        }
    }
}
