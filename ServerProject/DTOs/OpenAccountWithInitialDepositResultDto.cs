using ShareProject.Common;

namespace ServerProject.DTOs
{
	public class OpenAccountWithInitialDepositResultDto : DtoBase
	{
		/// <summary>
		/// 帳戶ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 帳戶類型，1: 儲蓄帳戶, 2: 支票帳戶, 3: 外幣帳戶
		/// </summary>
		public AccountType AccountType { get; set; }

		/// <summary>
		/// 帳戶餘額
		/// </summary>
		public decimal Balance { get; set; }

		/// <summary>
		/// 貨幣類型，1: JPY, 2: USD, 3: EUR, 4: CNY, 5: HKD, 6: TWD
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 帳戶狀態，1: 正常, 2: 冻结, 3: 关闭
		/// </summary>
		public AccountStatus Status { get; set; }

		/// <summary>
		/// 開戶日期
		/// </summary>
		public DateTime OpenDate { get; set; }

		/// <summary>
		/// 初始存款交易ID
		/// </summary>
		public long InitialDepositTransactionId { get; set; }

		/// <summary>
		/// 初始存款金額
		/// </summary>
		public decimal InitialDepositAmount { get; set; }

		/// <summary>
		/// 初始存款交易狀態,1: 待处理, 2: 已完成, 3: 失败，4：已撤销
		/// </summary>
		public TransactionStatus TransactionStatus { get; set; }

		/// <summary>
		/// 初始存款交易時間
		/// </summary>
		public DateTime TransactionTime { get; set; }

		/// <summary>
		/// 建立一個成功的 OpenAccountWithInitialDepositResultDto 實例
		/// </summary>
		/// <param name="accountId">帳戶ID</param>
		/// <param name="accountType">帳戶類型</param>
		/// <param name="balance">帳戶餘額</param>
		/// <param name="currency">貨幣類型</param>
		/// <param name="status">帳戶狀態</param>
		/// <param name="openDate">開戶日期</param>
		/// <param name="initialDepositAmount">初始存款金額</param>
		/// <param name="initialDepositTransactionId">初始存款交易ID</param>
		/// <param name="transactionStatus">初始存款交易狀態</param>
		/// <param name="transactionTime">初始存款交易時間</param>
		/// <returns>成功的 OpenAccountWithInitialDepositResultDto 實例</returns>
		public static OpenAccountWithInitialDepositResultDto SuccessDto(long accountId, AccountType accountType, decimal balance, CurrencyCode currency, AccountStatus status, DateTime openDate, long initialDepositTransactionId, decimal initialDepositAmount,TransactionStatus transactionStatus, DateTime transactionTime)
		{
			var dto = new OpenAccountWithInitialDepositResultDto
			{
				AccountId = accountId,
				AccountType = accountType,
				Balance = balance,
				Currency = currency,
				Status = status,
				OpenDate = openDate,
				InitialDepositTransactionId = initialDepositTransactionId,
				InitialDepositAmount = initialDepositAmount,
				TransactionStatus = transactionStatus,
				TransactionTime = transactionTime
			};
			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 建立一個失敗的 OpenAccountWithInitialDepositResultDto 實例
		/// </summary>
		/// <param name="code">错误码</param>
		/// <param name="message">错误信息</param>
		/// <returns>失敗的 OpenAccountWithInitialDepositResultDto 實例</returns>
		public static OpenAccountWithInitialDepositResultDto Failure(int code, string message)
		{
			var dto = new OpenAccountWithInitialDepositResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
