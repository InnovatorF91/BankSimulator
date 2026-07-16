namespace ShareProject.Common
{
	/// <summary>
	/// 銀行帳戶狀態
	/// </summary>
	public enum AccountStatus : short
	{
		/// <summary>
		/// 活躍：帳戶正常運作，可以進行交易。
		/// </summary>
		Active = 1,

		/// <summary>
		/// 凍結：帳戶暫時無法使用，可能是因為安全問題、法律要求或其他原因。凍結的帳戶通常無法進行交易，直到問題解決。
		/// </summary>
		Frozen = 2,

		/// <summary>
		/// 關閉：帳戶已被永久關閉，無法再使用。關閉的帳戶通常是由於客戶要求、長期不活動或其他原因造成的。一旦帳戶被關閉，相關的資金和交易記錄可能會被保留一段時間以供查詢，但帳戶本身將無法再進行任何操作。
		/// </summary>
		Closed = 3 
	}
}
