namespace ServerProject.Common
{
    /// <summary>
    /// 審計狀態枚舉，用於表示操作的結果
    /// </summary>
    public enum AuditStatus
    {
        Success = 0, // 操作成功
        Failed = 1, // 操作失敗
        Denied = 2, // 操作被拒絕
        Warning = 3, // 操作有警告
    }

    /// <summary>
    /// 審計條目類，用於記錄操作的詳細信息
    /// </summary>
    public class AuditEntry
    {
        public DateTime OccurredAtUtc { get; set; } // 事件發生的UTC時間
        public int? ActorUserId { get; set; } // 執行操作的用戶ID
        public string ActorRole { get; set; } = string.Empty; // 執行操作的用戶角色
        public string Action { get; set; } = string.Empty; // 執行的操作名稱
        public string TargetType { get; set; } = string.Empty; // 目標對象的類型
        public long? TargetId { get; set; } // 目標對象的ID
        public Guid CorrelationId { get; set; } // 相關的請求ID，用於追蹤請求鏈
        public string? ClientIp { get; set; } // 客戶端IP地址
        public string? UserAgent { get; set; } // 客戶端用戶代理字符串
        public AuditStatus Status { get; set; } // 審計狀態
        public string? Reason { get; set; } // 操作失敗或被拒絕的原因
        public object? BeforeSnapshot { get; set; } // 操作前的快照（如果有的話）
        public object? AfterSnapshot { get; set; } // 操作後的快照（如果有的話）
    }

    /// <summary>
    /// 審計查詢類，用於篩選和查詢審計條目
    /// </summary>
    public class AuditQuery
    {
        public DateTime? From { get; set; } // 查詢的起始時間
        public DateTime? To { get; set; } // 查詢的結束時間
        public int? ActorUserId { get; set; } // 查詢的用戶ID
        public string? Action { get; set; } // 查詢的操作名稱
        public string? TargetType { get; set; } // 查詢的目標對象類型
        public long? TargetId { get; set; } // 查詢的目標對象ID
        public AuditStatus? Status { get; set; } // 查詢的審計狀態
    }

    /// <summary>
    /// 審計記錄類，用於存儲審計記錄的基本信息
    /// </summary>
    public class AuditRecord
    {
        public int Id { get; set; } // 審計記錄ID
        public int UserId { get; set; } // 執行操作的用戶ID
        public string Action { get; set; } = string.Empty; // 執行的操作名稱
        public DateTime Timestamp { get; set; } // 審計記錄的時間戳
        public string Details { get; set; } = string.Empty; // 審計記錄的詳細信息
    }
}
