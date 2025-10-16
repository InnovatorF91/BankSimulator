using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShareProject.Common;

namespace ServerProject.Models
{
    /// <summary>
    /// 交易模型
    /// </summary>
    public class TransactionModel
    {
        [Key]
        [Column("transaction_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long TransactionId { get; set; } // 交易ID
        [Column("account_id")]
        public long AccountId { get; set; } // 帳戶ID
        [Column("transaction_type")]
        public TransactionType TransactionType { get; set; } // 交易類型
        [Column("amount_delta")]
        public decimal AmountDelta { get; set; } // 金額變化（正數表示存款，負數表示取款）
        [Column("related_account")]
        public long? RelatedAccount { get; set; } // 相關帳戶ID（如果有的話，例如轉帳交易）
        [Column("create_at")]
        public DateTime CreateAt { get; set; } // 交易創建時間
        [Column("status")]
        public TransactionStatus Status { get; set; } // 交易狀態
        [Column("group_id")]
        public long? GrouppId { get; set; } // 群組ID（如果有的話，例如批量交易）
        [Column("note")]
        public string? Note { get; set; } // 交易備註

        public AccountModel Account { get; set; } = new AccountModel(); // 關聯的帳戶模型
    }
}
