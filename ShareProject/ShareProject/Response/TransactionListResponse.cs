using ShareProject.Common;

namespace ShareProject.Response
{
    /// <summary>
    /// 交易列表回應類別，包含交易資料和分頁資訊
    /// </summary>
    public class TransactionListResponse : BaseResponse
    {
        public TransactionDto[] Transactions { get; set; } = Array.Empty<TransactionDto>(); // 交易列表
        public PageInfo PageInfo { get; set; } = new PageInfo(); // 分頁資訊
    }
}
