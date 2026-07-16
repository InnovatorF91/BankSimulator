using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShareProject.Common;

namespace ServerProject.Entities
{
	/// <summary>
	/// 帳戶模型
	/// </summary>
	[Table("Accounts")]
	public class AccountEntity
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		[Key]
		[Column("account_id")]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long AccountId { get; set; }

		/// <summary>
		/// 客戶ID
		/// </summary>
		[Column("customer_id")]
		public int CustomerId { get; set; }

		/// <summary>
		/// 帳戶類型
		/// </summary>
		[Column("account_type")]
		public AccountType AccountType { get; set; }

		/// <summary>
		/// 帳戶餘額
		/// </summary>
		[Column("balance")]
		public decimal Balance { get; set; }

		/// <summary>
		/// 帳戶貨幣
		/// </summary>
		[Column("currency")]
		[StringLength(3)]
		public string Currency { get; set; } = string.Empty;

		/// <summary>
		/// 帳戶狀態
		/// </summary>
		[Column("status")]
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 開戶日期
		/// </summary>
		[Column("open_date")]
		public DateTime OpenDate { get; set; }

		/// <summary>
		/// 關戶日期
		/// </summary>
		[Column("close_date")]
		public DateTime? CloseDate { get; set; }

		/// <summary>
		/// 更新日期
		/// </summary>
		[Column("update_date")]
		public DateTime UpdateDate { get; set; }
	}
}
