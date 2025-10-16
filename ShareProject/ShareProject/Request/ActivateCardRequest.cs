namespace ShareProject.Request
{
    public class ActivateCardRequest : BaseRequest
    {
        public Guid CardId { get; set; } // 卡片ID
        public string Last4 { get; set; } = string.Empty; // 卡片末四位數
        public string ActivationCode { get; set; } = string.Empty; // 啟用碼
    }
}
