using ShareProject.Common;

namespace ShareProject.Response
{
    /// <summary>
    /// 交易回應類別，包含交易資料
    /// </summary>
    public class TransactionResponse : BaseResponse
    {
        public TransactionDto Transaction { get; set; } = new TransactionDto(); // 交易資料
    }
}
