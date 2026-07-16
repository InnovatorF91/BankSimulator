namespace ShareProject.Common
{
	/// <summary>
	/// 帳戶類型
	/// </summary>
	public enum AccountType : short
	{
		/// <summary>
		/// 儲蓄帳戶：主要用於存款和積累利息，通常提供較高的利率，但可能有提款限制。
		/// </summary>
		Savings = 1,

		/// <summary>
		/// 支票帳戶：主要用於日常交易和支付，通常提供較低的利率，但允許無限制的提款和支票使用。
		/// </summary>
		Checking = 2,

		/// <summary>
		/// 外幣帳戶：用於持有和交易外幣，允許客戶以不同的貨幣進行存款和提款，通常提供多種貨幣選擇，但可能會有較高的手續費和較低的利率。
		/// </summary>
		ForeignCurrency = 3
	}
}
