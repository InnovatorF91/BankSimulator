using ShareProject.Common;

namespace ShareProject.Request
{
	/// <summary>
	/// 開戶初始交易請求類別
	/// </summary>
	public class OpenAccountWithInitialDepositRequest
	{
		/// <summary>
		/// 帳戶類型，1: 儲蓄帳戶, 2: 支票帳戶, 3: 外幣帳戶
		/// </summary>
		public AccountType AccountType { get; set; }

		/// <summary>
		/// 貨幣類型，1: JPY, 2: USD, 3: EUR, 4: CNY, 5: HKD, 6: TWD
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 初始存款金額
		/// </summary>
		public decimal InitialDepositAmount { get; set; }
	}
}
