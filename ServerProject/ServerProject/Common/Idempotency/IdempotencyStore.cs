namespace ServerProject.Common.Idempotency
{
    /// <summary>
    /// IdempotencyStatus 枚舉，用於表示操作的冪等性狀態
    /// </summary>
    public enum IdempotencyStatus
    {
        None, // 未處理
        InProgress, // 處理中
        Completed, // 處理完成
    }

    public class IdempotencyStore : IIdempotencyStore
    {
        /// <summary>
        /// 存儲冪等性請求的字典，鍵為請求ID，值為狀態和過期時間的元組
        /// </summary>
        private readonly Dictionary<string, (IdempotencyStatus Status, DateTime Expiration)> _store
        = new Dictionary<string, (IdempotencyStatus, DateTime)>();

        /// <summary>
        /// 用於確保線程安全的鎖對象
        /// </summary>
        private readonly object _lock = new object();

        /// <summary>
        /// 嘗試獲取冪等性狀態
        /// </summary>
        /// <param name="key">請求ID</param>
        /// <param name="ttl">有效時間</param>
        /// <returns>true:可以執行業務邏輯/false:這是重複請求，應該忽略</returns>
        public bool TryStart(string key, TimeSpan ttl)
        {
            lock (_lock)
            {
                if (_store.TryGetValue(key, out var entry))
                {
                    if (entry.Expiration > DateTime.UtcNow)
                    {
                        // 已存在並且還沒過期
                        return false;
                    }
                }

                // 不存在或過期 → 新增記錄
                _store[key] = (IdempotencyStatus.InProgress, DateTime.UtcNow.Add(ttl));
                return true;
            }
        }

        /// <summary>
        /// 完成冪等性操作，標記請求為已完成
        /// </summary>
        /// <param name="key">請求ID</param>
        public void Complete(string key)
        {
            lock (_lock)
            {
                // 如果請求ID存在，將狀態標記為已完成
                if (_store.ContainsKey(key))
                {
                    // 保留原有的過期時間
                    var expiration = _store[key].Expiration;

                    // 更新狀態為已完成
                    _store[key] = (IdempotencyStatus.Completed, expiration);
                }
            }
        }

        /// <summary>
        /// 獲取冪等性狀態
        /// </summary>
        /// <param name="key">請求ID</param>
        /// <returns>冪等性狀態</returns>
        public IdempotencyStatus GetStatus(string key)
        {
            lock (_lock)
            {
                // 嘗試從存儲中獲取請求ID的狀態
                if (_store.TryGetValue(key, out var entry))
                {
                    // 檢查過期時間
                    if (entry.Expiration > DateTime.UtcNow)
                    {
                        // 返回當前狀態
                        return entry.Status;
                    }
                    else
                    {
                        // 過期，從存儲中移除
                        _store.Remove(key); // 過期清理
                    }
                }

                // 如果請求ID不存在或已過期，返回未處理狀態
                return IdempotencyStatus.None;
            }
        }
    }

    public interface IIdempotencyStore
    {
        /// <summary>
        /// 嘗試獲取冪等性狀態
        /// </summary>
        /// <param name="key">請求ID</param>
        /// <param name="ttl">有效時間</param>
        /// <returns>true:可以執行業務邏輯/false:這是重複請求，應該忽略</returns>
        bool TryStart(string key, TimeSpan ttl);

        /// <summary>
        /// 完成冪等性操作，標記請求為已完成
        /// </summary>
        /// <param name="key">請求ID</param>
        void Complete(string key);

        /// <summary>
        /// 獲取冪等性狀態
        /// </summary>
        /// <param name="key">請求ID</param>
        /// <returns>冪等性狀態</returns>
        IdempotencyStatus GetStatus(string key);
    }
}
