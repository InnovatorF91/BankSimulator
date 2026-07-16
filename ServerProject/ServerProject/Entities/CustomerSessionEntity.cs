using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ServerProject.Entities
{
	/// <summary>
	/// 客戶會話模型
	/// </summary>
	[Table("CustomerSession")]
	public class CustomerSessionEntity
	{
		[Key]
		[Column("session_id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public Guid SessionId { get; set; } // 會話ID

		[Column("user_id")]
		public int UserId { get; set; } // 使用者ID

		[Column("device")]
		public string Device { get; set; } = string.Empty; // 裝置資訊

		[Column("ip")]
		public string IP { get; set; } = string.Empty; // IP地址

		[Column("created_at")]
		public DateTime CreatedAt { get; set; } // 會話創建時間

		[Column("last_seen_at")]
		public DateTime LastSeenAt { get; set; } // 最後活動時間

		[Column("expired_at")]
		public DateTime ExpiredAt { get; set; } // 會話過期時間

		[Column("is_valid")]
		public bool IsValid { get; set; } // 會話是否有效
	}
}
