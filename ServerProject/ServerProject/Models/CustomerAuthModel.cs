using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ServerProject.Models
{
    public class CustomerAuthModel
    {
        [Key]
        [Column("customer_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CustomerId { get; set; } // 客戶ID
        [Column("login_id")]
        public string LoginId { get; set; } = string.Empty; // 登錄ID
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty; // 密碼哈希
        [Column("two_factor_enabled")]
        public bool TwoFactorEnabled { get; set; } = false; // 是否啟用雙因素認證
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } // 創建時間
        [Column("updated_at")]
        public DateTime? UpdateAt { get; set; } // 更新時間
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; } // 刪除時間
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // 是否已刪除

        public CustomerModel Customer { get; set; } = new CustomerModel(); // 關聯的客戶模型
    }
}
