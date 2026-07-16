namespace ServerProject.DTOs
{
	public class DisableTwoFactorResultDto : DtoBase
	{
		public static DisableTwoFactorResultDto SuccessDto()
		{
			var dto = new DisableTwoFactorResultDto();
			dto.MarkSuccess();
			return dto;
		}

		public static DisableTwoFactorResultDto Fail(int errorCode, string errorMessage)
		{
			var dto = new DisableTwoFactorResultDto();
			dto.MarkFail(errorCode, errorMessage);
			return dto;
		}
	}
}
