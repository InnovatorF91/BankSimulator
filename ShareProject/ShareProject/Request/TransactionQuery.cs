using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 交易查詢請求類別，包含查詢交易所需的屬性
    /// </summary>
    public class TransactionQuery : BaseRequest
    {
        public Guid? AccountId { get; set; } // 帳戶ID
        public Guid? CustomerId { get; set; } // 客戶ID
        public DateTime? DateFrom { get; set; } // 查詢開始日期
        public DateTime? DateTo { get; set; } // 查詢結束日期
        public decimal? MinAmount { get; set; } // 最小金額
        public decimal? MaxAmount { get; set; } // 最大金額
        public TransactionType[] Types { get; set; } = Array.Empty<TransactionType>(); // 交易類型
        public string[] Statuses { get; set; } = Array.Empty<string>(); // 交易狀態
        public int Page { get; set; } // 當前頁碼
        public int PageSize { get; set; } // 每頁顯示的項目數量
        public string SortBy { get; set; } = string.Empty; // 排序依據，預設為交易時間戳記
    }
}
