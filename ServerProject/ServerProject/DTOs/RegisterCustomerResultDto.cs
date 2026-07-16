namespace ServerProject.DTOs
{
	public class RegisterCustomerResultDto : DtoBase
	{
		public int CustomerId { get; set; } // 客戶ID

		public static RegisterCustomerResultDto SuccessDto(int customerId)
		{
			var dto = new RegisterCustomerResultDto();
			dto.CustomerId = customerId;
			dto.MarkSuccess();
			return dto;
		}

		public static RegisterCustomerResultDto Fail(int code, string message)
		{
			var dto = new RegisterCustomerResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
