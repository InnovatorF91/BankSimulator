using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 转账结果Dto
	/// </summary>
	public class TransferResultDto : DtoBase
	{
		/// <summary>
		/// 交易组ID
		/// </summary>
		public Guid GroupId { get; set; }

		/// <summary>
		/// 源交易ID
		/// </summary>
		public long SourceTransactionId { get; set; }

		/// <summary>
		/// 目标交易ID
		/// </summary>
		public long DestinationTransactionId { get; set; }

		/// <summary>
		/// 源账户ID
		/// </summary>
		public long SourceAccountId { get; set; }

		/// <summary>
		/// 目标账户ID
		/// </summary>
		public long DestinationAccountId { get; set; }

		/// <summary>
		/// 转账金额，必须为正数。
		/// 源账户执行扣款，目标账户执行入账。
		/// </summary>
		public decimal AmountDelta { get; set; }

		/// <summary>
		/// 交易前源账户余额
		/// </summary>
		public decimal SourceBalanceBefore { get; set; }

		/// <summary>
		/// 交易后源账户余额
		/// </summary>
		public decimal SourceBalanceAfter { get; set; }

		/// <summary>
		/// 交易状态
		/// </summary>
		public TransactionStatus Status { get; set; }

		/// <summary>
		/// 货币种类
		/// </summary>
		public CurrencyCode Currency { get; set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTime CreatedAt { get; set; }

		public static TransferResultDto SuccessDto(
			Guid groupId,
			long sourceTransactionId,
			long destinationTransactionId,
			long sourceAccountId,
			long destinationAccountId,
			decimal amountDelta,
			decimal sourceBalanceBefore,
			decimal sourceBalanceAfter,
			TransactionStatus status,
			CurrencyCode currency,
			DateTime createAt)
		{
			var dto = new TransferResultDto()
			{
				GroupId = groupId,
				SourceTransactionId = sourceTransactionId,
				DestinationTransactionId = destinationTransactionId,
				SourceAccountId = sourceAccountId,
				DestinationAccountId = destinationAccountId,
				AmountDelta = amountDelta,
				SourceBalanceBefore = sourceBalanceBefore,
				SourceBalanceAfter = sourceBalanceAfter,
				Status = status,
				Currency = currency,
				CreatedAt = createAt
			};
			dto.MarkSuccess();
			return dto;
		}

		public static TransferResultDto Failure(int code, string message)
		{
			var dto = new TransferResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
