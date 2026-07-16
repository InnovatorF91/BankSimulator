using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class AccountDto
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 账户類型
		/// </summary>
		public AccountType AccountType { get; set; }

		/// <summary>
		/// 帳戶餘額
		/// </summary>
		public decimal Balance { get; set; }

		/// <summary>
		/// 帳戶貨幣
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 帳戶狀態
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 開戶日期
		/// </summary>
		public DateTime OpenDate { get; set; }

		/// <summary>
		/// 關戶日期
		/// </summary>
		public DateTime? CloseDate { get; set; }

		/// <summary>
		/// 最後更新日期
		/// </summary>
		public DateTime UpdateDate { get; set; }
	}
}
