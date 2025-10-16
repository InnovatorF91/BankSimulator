using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShareProject.Common;

namespace ServerProject.Models
{
    /// <summary>
    /// 客戶模型
    /// </summary>
    public class CustomerModel
    {
        [Key]
        [Column("customer_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerId { get; set; } // 客戶ID
        [Column("name")]
        public string Name { get; set; } = string.Empty; // 客戶名稱
        [Column("gender")]
        public Gender? Gender { get; set; } // 性別
        [Column("birth_date")]
        public DateTime BirthDate { get; set; } // 出生日期
        [Column("id_type")]
        public IDType? IDType { get; set; } // 身份證明類型
        [Column("id_number")]
        public string? IDNumber { get; set; } = string.Empty; // 身份證號碼
        [Column("address")]
        public string? Address { get; set; } = string.Empty; // 地址
        [Column("phone")]
        public string? Phone { get; set; } = string.Empty; // 電話號碼
        [Column("email")]
        public string? Email { get; set; } = string.Empty; // 電子郵件
        [Column("kyc_status")]
        public KYCStatus? KYCStatus { get; set; } // KYC狀態
        [Column("created_at")]
        public DateTime? CreatedAt { get; set; } // 創建時間
        [Column("updated_at")]
        public DateTime? UpdateAt { get; set; } // 更新日期
        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; } // 刪除日期
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } // 是否已刪除
        [Column("deleted_reason")]
        public string? DeletedReason { get; set; } = string.Empty; // 刪除原因

        public List<AccountModel> Accounts { get; set; } = new List<AccountModel>(); // 關聯的帳戶列表
    }
}
