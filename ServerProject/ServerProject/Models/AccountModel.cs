using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShareProject.Common;

namespace ServerProject.Models
{
    /// <summary>
    /// 帳戶模型
    /// </summary>
    public class AccountModel
    {
        [Key]
        [Column("account_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long AccountId { get; set; } // 帳戶ID
        [Column("customer_id")]
        public int CustomerId { get; set; } // 客戶ID
        [Column("account_type")]
        public AccountType AccountType { get; set; } // 帳戶類型
        [Column("balance")]
        public decimal Balance { get; set; } // 帳戶餘額
        [Column("currency")]
        public string Currency { get; set; } = string.Empty; // 帳戶貨幣
        [Column("status")]
        public AccountStatus Status { get; set; } // 帳戶狀態
        [Column("open_date")]
        public DateTime? OpenDate { get; set; } // 開戶日期
        [Column("close_date")]
        public DateTime? CloseDate { get; set; } // 關戶日期（如果有的話）
        [Column("is_closed")]
        public bool IsClosed { get; set; } // 是否已關閉帳戶
        [Column("update_date")]
        public DateTime? UpdateDate { get; set; } // 更新日期

        public CustomerModel Customer { get; set; } = new CustomerModel(); // 關聯的客戶模型
        public List<CardModel> Cards { get; set; } = new List<CardModel>(); // 關聯的卡片列表
        public List<TransactionModel> Transactions { get; set; } = new List<TransactionModel>(); // 關聯的交易列表
    }
}
