namespace ShareProject.Response
{
    /// <summary>
    /// 卡片狀態回應類別，包含卡片ID和是否啟用的狀態
    /// </summary>
    public class CardStatusResponse : BaseResponse
    {
        public Guid CardId { get; set; } // 卡片ID
        public bool Active { get; set; } // 卡片是否啟用
    }
}
