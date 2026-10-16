namespace ServerProject.DTOs
{
	/// <summary>
	/// 当前用户交易记录查询结果Dto
	/// </summary>
	public class GetMyTransactionResultDto : DtoBase
	{
		/// <summary>
		/// 交易记录集合
		/// </summary>
		public List<TransactionRecordDto> Transactions { get; set; } = new();

		/// <summary>
		/// 查询成功
		/// </summary>
		public static GetMyTransactionResultDto SuccessDto(
			List<TransactionRecordDto> transactions)
		{
			var dto = new GetMyTransactionResultDto()
			{
				Transactions = transactions
			};

			dto.MarkSuccess();
			return dto;
		}

		/// <summary>
		/// 查询失败
		/// </summary>
		public static GetMyTransactionResultDto Failure(
			int code,
			string message)
		{
			var dto = new GetMyTransactionResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
