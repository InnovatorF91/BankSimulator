using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerProject.Entities
{
	[Table("PasswordResetToken")]
	public class PasswordResetTokenEntity
	{
		[Key]
		[Column("token_id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Guid TokenId { get; set; } // 令牌ID

		[Key]
		[Column("customer_id")]
		public int CustomerId { get; set; } // 客戶ID

		[Key]
		[Column("token_hash")]
		public string TokenHash { get; set; } = string.Empty; // 令牌哈希值

		[Key]
		[Column("created_at")]
		public DateTime CreatedAt { get; set; } // 創建時間

		[Key]
		[Column("expires_at")]
		public DateTime ExpiresAt { get; set; } // 過期時間

		[Key]
		[Column("used_at")]
		public DateTime? UsedAt { get; set; } // 使用時間

		[Key]
		[Column("created_by_ip")]
		public string? CreatedByIp { get; set; } // 創建時的IP地址

		[Key]
		[Column("created_by_device")]
		public string? CreatedByDevice { get; set; } // 創建時的裝置資訊
	}
}
