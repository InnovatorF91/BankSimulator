namespace ServerProject.DTOs
{
	public class ChangePasswordResultDto : DtoBase
	{
		public static ChangePasswordResultDto SuccessDto()
		{
			var dto = new ChangePasswordResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static ChangePasswordResultDto Fail(int code, string message)
		{
			var dto = new ChangePasswordResultDto();
			dto.MarkFail(code, message);
			return dto;
		}
	}
}
