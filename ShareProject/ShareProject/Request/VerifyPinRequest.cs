namespace ShareProject.Request
{
    /// <summary>
    /// 驗證 PIN 請求類別，包含驗證 PIN 所需的屬性
    /// </summary>
    public class VerifyPinRequest : BaseRequest
    {
        public Guid CardId { get; set; } // 卡片ID
        public string PinVerifier { get; set; } = string.Empty; // PIN 驗證碼
        public string Channel { get; set; } = string.Empty; // 請求來源渠道，例如 "mobile" 或 "web"
    }
}
