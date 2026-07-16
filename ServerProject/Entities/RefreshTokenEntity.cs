using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerProject.Entities
{
	[Table("RefreshToken")]
	public class RefreshTokenEntity
	{
		[Key]
		[Column("token_id")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public Guid TokenId { get; set; } // 令牌ID

		[Key]
		[Column("user_id")]
		public int UserId { get; set; } // 使用者ID

		[Key]
		[Column("token_hash")]
		public string TokenHash { get; set; } = string.Empty; // 令牌哈希值

		[Key]
		[Column("issued_at")]
		public DateTime IssuedAt { get; set; } // 發行時間

		[Key]
		[Column("expires_at")]
		public DateTime ExpiresAt { get; set; } // 過期時間

		[Key]
		[Column("revoked_at")]
		public DateTime? RevokedAt { get; set; } // 撤銷時間

		[Key]
		[Column("meta_device")]
		public string MetaDevice { get; set; } = string.Empty; // 裝置資訊

		[Key]
		[Column("meta_ip")]
		public string MetaIP { get; set; } = string.Empty; // IP地址

		[Key]
		[Column("token_version")]
		public int TokenVersion { get; set; } // 令牌版本
	}
}
