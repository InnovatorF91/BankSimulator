using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShareProject.Common;

namespace ServerProject.Models
{
    /// <summary>
    /// 卡片模型
    /// </summary>
    public class CardModel
    {
        [Key]
        [Column("card_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long CardId { get; set; } // 卡片ID
        [Column("account_id")]
        public long AccountId { get; set; } // 帳戶ID
        [Column("card_number")]
        public string CardNumber { get; set; } = string.Empty; // 卡號
        [Column("expiry_year")]
        public short ExpiryYear { get; set; } // 到期年份
        [Column("expiry_month")]
        public short ExpiryMonth { get; set; } // 到期月份
        [Column("pin_hash")]
        public string PINHash { get; set; } = string.Empty; // PIN碼哈希
        [Column("pin_fail_count")]
        public short PINFailCount { get; set; } // PIN碼失敗次數
        [Column("pin_locked_until")]
        public DateTime? PINLockedUntil { get; set; } // PIN碼鎖定到期時間
        [Column("card_type")]
        public CardType CardType { get; set; } // 卡片類型
        [Column("card_status")]
        public CardStatus Status { get; set; } // 卡片狀態
        [Column("deactivated_at")]
        public DateTime? DeactivatedAt { get; set; } // 卡片停用時間（如果有的話）
        [Column("replaced_by")]
        public long? ReplacedBy { get; set; } // 被替換的卡片ID（如果有的話）
        [Column("create_at")]
        public DateTime CreateAt { get; set; } // 卡片創建時間

        public AccountModel Account { get; set; } = new AccountModel(); // 關聯的帳戶模型
    }
}
