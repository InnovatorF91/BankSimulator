using ShareProject.Common;

namespace ShareProject.Response
{
	/// <summary>
	/// 开户并完成首存的回应。
	/// </summary>
	public class OpenAccountWithInitialDepositResponse : OpenAccountResponse
	{
		public long InitialDepositTransactionId { get; set; }
		public decimal InitialDepositAmount { get; set; }
		public TransactionStatus TransactionStatus { get; set; }
		public DateTime TransactionTime { get; set; }
	}
}