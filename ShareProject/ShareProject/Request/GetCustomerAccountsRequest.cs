namespace ShareProject.Request
{
	/// <summary>
	/// 客戶帳戶的請求。
	/// </summary>
	public class GetCustomerAccountsRequest
	{
		/// <summary>
		/// 客戶ID。
		/// </summary>
		public int CustomerId { get; set; }

		/// <summary>
		/// 是否包含已關閉的帳戶。預設為 true，表示包含已關閉的帳戶。
		/// </summary>
		public bool IncludeClosedAccounts { get; set; } = true;
	}
}
