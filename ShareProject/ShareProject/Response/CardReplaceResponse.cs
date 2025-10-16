namespace ShareProject.Response
{
    /// <summary>
    /// 卡片替換回應類別，包含被替換的舊卡ID和新卡ID
    /// </summary>
    public class CardReplaceResponse : BaseResponse
    {
        public Guid OldCardId { get; set; } // 被替換的舊卡ID
        public Guid NewCardId { get; set; } // 新卡ID
    }
}
