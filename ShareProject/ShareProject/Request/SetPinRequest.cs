namespace ShareProject.Request
{
    /// <summary>
    /// 設定 PIN 請求類別，包含設定 PIN 所需的屬性
    /// </summary>
    public class SetPinRequest : BaseRequest
    {
        public Guid CardId { get; set; } // 卡片ID
        public string PinVerifier { get; set; } = string.Empty; // PIN 驗證碼
        public string OldPinVerifier { get; set; } = string.Empty; // 舊的 PIN 驗證碼，若不提供則不更改舊 PIN
        public bool ForceReset { get; set; } // 是否強制重設 PIN，若為 true 則忽略舊 PIN 驗證碼
        public string Channel { get; set; } = string.Empty; // 請求來源渠道，例如 "mobile" 或 "web"
    }
}
