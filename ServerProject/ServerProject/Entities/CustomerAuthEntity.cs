using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerProject.Entities
{
    [Table("CustomerAuth")]
	public class CustomerAuthEntity
    {
        [Key]
        [Column("customer_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerId { get; set; } // 客戶ID
        [Column("login_id")]
        public string LoginId { get; set; } = string.Empty; // 登錄ID
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty; // 密碼哈希
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } // 創建時間
        [Column("updated_at")]
        public DateTime? UpdateAt { get; set; } // 更新時間
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; } // 刪除時間
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // 是否已刪除
        [Column("two_factor_secret")]
        public string? TwoFactorSecret { get; set; } = string.Empty; // 雙因素認證密鑰
        [Column("failed_count")]
        public int FailedCount { get; set; } // 登錄失敗次數
        [Column("locked_until")]
        public DateTime? LockedUntil { get; set; } // 鎖定到期時間
        [Column("password_algo")]
        public int TokenVersion { get; set; } // 令牌版本
        [Column("auth_type")]
        public short AuthType { get; set; } // 認證類型
        [Column("two_factor_status")]
        public short TwoFactorStatus { get; set; } // 雙因素認證狀態
        [Column("two_factor_pending_expires_at")]
        public DateTime? TwoFactorPendingExpiresAt { get; set; } // 雙因素認證待處理過期時間
        [Column("two_factor_enabled_at")]
        public DateTime? TwoFactorEnabledAt { get; set; } // 雙因素認證啟用時間
    }
}
