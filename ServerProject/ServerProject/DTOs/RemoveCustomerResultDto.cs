namespace ServerProject.DTOs
{
	public class RemoveCustomerResultDto : DtoBase
	{
		public static RemoveCustomerResultDto SuccessDto()
		{
			var dto = new RemoveCustomerResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static RemoveCustomerResultDto Fail(int code, string message)
		{
			var dto = new RemoveCustomerResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
