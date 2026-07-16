namespace ServerProject.DTOs
{
	public class ConfirmResetPasswordResultDto : DtoBase
	{
		public static ConfirmResetPasswordResultDto SuccessDto()
		{
			var dto = new ConfirmResetPasswordResultDto();
			dto.MarkSuccess();
			return dto;
		}
		public static ConfirmResetPasswordResultDto Fail(int errorCode, string message)
		{
			var dto = new ConfirmResetPasswordResultDto();
			dto.MarkFail(errorCode, message);
			return dto;
		}
	}
}
