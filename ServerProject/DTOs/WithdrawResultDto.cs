using ShareProject.Common;

namespace ServerProject.DTOs
{
	/// <summary>
	/// 取款结果Dto
	/// </summary>
	public class WithdrawResultDto : DtoBase
	{
		/// <summary>
		/// 交易ID
		/// </summary>
		public long TransactionId { get; set; }

		/// <summary>
		/// 账户ID
		/// </summary>
		public long AccountId { get; set; }

		/// <summary>
		/// 余额变化
		/// 取款时为负数
		/// </summary>
		public decimal AmountDelta { get; set; }

		/// <summary>
		/// 交易前余额
		/// </summary>
		public decimal BalanceBefore { get; set; }

		/// <summary>
		/// 交易后余额
		/// </summary>
		public decimal BalanceAfter { get; set; }

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

		public static WithdrawResultDto SuccessDto(
			long transactionId,
			long accountId,
			decimal amountDelta,
			decimal balanceBefore,
			decimal balanceAfter,
			TransactionStatus status,
			CurrencyCode currency,
			DateTime createAt)
		{
			var dto = new WithdrawResultDto()
			{
				TransactionId = transactionId,
				AccountId = accountId,
				AmountDelta = amountDelta,
				BalanceBefore = balanceBefore,
				BalanceAfter = balanceAfter,
				Status = status,
				Currency = currency,
				CreatedAt = createAt
			};

			dto.MarkSuccess();
			return dto;
		}

		public static WithdrawResultDto Failure(int code, string message)
		{
			var dto = new WithdrawResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
