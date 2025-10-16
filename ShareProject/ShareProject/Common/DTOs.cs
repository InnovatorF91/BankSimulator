using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ShareProject.Common
{
    /// <summary>
    /// 顧客資料傳輸物件 (DTO)
    /// </summary>
    public class CustomerDto
    {
        public int CustomerId { get; set; } // 客戶ID
        public string Name { get; set; } = string.Empty; // 客戶名稱
        public Gender? Gender { get; set; } // 性別
        public DateTime BirthDate { get; set; } // 出生日期
        public IDType? IDType { get; set; } // 身份證明類型
        public string? IDNumber { get; set; } = string.Empty; // 身份證號碼
        public string? Address { get; set; } = string.Empty; // 地址
        public string? Phone { get; set; } = string.Empty; // 電話號碼
        public string? Email { get; set; } = string.Empty; // 電子郵件
        public KYCStatus? KYCStatus { get; set; } // KYC狀態
        public DateTime? CreatedAt { get; set; } // 創建時間
        public DateTime? UpdateAt { get; set; } // 更新日期
        public DateTime? DeletedAt { get; set; } // 刪除日期
        public bool IsDeleted { get; set; } // 是否已刪除
        public string? DeletedReason { get; set; } = string.Empty; // 刪除原因 
    }

    /// <summary>
    /// 銀行帳戶資料傳輸物件 (DTO)
    /// </summary>
    public class AccountDto
    {
        public long AccountId { get; set; } // 帳戶ID
        public int CustomerId { get; set; } // 客戶ID
        public AccountType AccountType { get; set; } // 帳戶類型
        public decimal Balance { get; set; } // 帳戶餘額
        public CurrencyCode Currency { get; set; } // 帳戶貨幣
        public AccountStatus Status { get; set; } // 帳戶狀態
        public DateTime? OpenDate { get; set; } // 開戶日期
        public DateTime? CloseDate { get; set; } // 關戶日期（如果有的話）
        public bool IsClosed { get; set; } // 是否已關閉帳戶
        public DateTime? UpdateDate { get; set; } // 更新日期
    }

    /// <summary>
    /// 交易資料傳輸物件 (DTO)
    /// </summary>
    public class TransactionDto
    {
        public Guid TransactionId { get; set; } // 交易ID  
        public Guid AccountId { get; set; } // 帳戶ID
        public TransactionType Type { get; set; } // 交易類型
        public decimal Amount { get; set; } // 金額
        public CurrencyCode Currency { get; set; } // 幣種
        public DateTimeOffset Timestemp { get; set; } // 交易時間戳記
        public string Status { get; set; } = string.Empty; // 交易狀態
        public string Memo { get; set; } = string.Empty; // 備註
    }

    /// <summary>
    /// 卡片資料傳輸物件 (DTO)
    /// </summary>
    public class CardDto
    {
        public Guid CardId { get; set; } // 卡片ID  
        public Guid AccountId { get; set; } // 帳戶ID  
        public CardType CardType { get; set; } // 卡片類型
        public string Last4 { get; set; } = string.Empty; // 卡片末四位數
        public DateTime Expiry { get; set; } // 到期日
        public bool Active { get; set; } // 是否啟用
    }

    /// <summary>
    /// 分頁資訊物件 (DTO)
    /// </summary>
    public class PageInfo
    {
        public int Page { get; set; } // 當前頁碼
        public int PageSize { get; set; } // 每頁顯示數量
        public int TotalCount { get; set; } // 總數量
    }
}
