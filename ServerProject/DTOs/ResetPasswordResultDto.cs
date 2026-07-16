namespace ServerProject.DTOs
{
	public class ResetPasswordResultDto : DtoBase
	{
		public static ResetPasswordResultDto SuccessDto(string message)
		{
			var dto = new ResetPasswordResultDto();
			dto.MarkSuccess(0,message);
			return dto;
		}

		public static ResetPasswordResultDto Fail(int errorCode, string message)
		{
			var dto = new ResetPasswordResultDto();
			dto.MarkFail(errorCode, message);
			return dto;
		}
	}
}
