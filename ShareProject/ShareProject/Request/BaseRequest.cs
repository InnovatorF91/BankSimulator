namespace ShareProject.Request
{
    /// <summary>
    /// 基礎請求類別，包含所有請求的共通屬性
    /// </summary>
    public class BaseRequest
    {
        public Guid RequestId { get; set; } // 請求ID
        public DateTimeOffset Timestamp { get; set; } // 請求時間戳記
        public string CorrelationId { get; set; } = string.Empty; // 關聯ID，用於追蹤請求
        public string PerfrmedBy { get; set; } = string.Empty; // 執行者，通常是使用者ID或系統名稱
        public string ClientVersion { get; set; } = string.Empty; // 客戶端版本
        public string Locale { get; set; } = string.Empty; // 語言區域設定
    }
}
