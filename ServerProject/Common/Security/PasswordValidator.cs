namespace ServerProject.Common
{
	/// <summary>
	/// 密碼驗證器，負責驗證密碼是否符合安全規範
	/// </summary>
	public class PasswordValidator : IPasswordValidator
	{
		/// <summary>
		/// 驗證密碼是否符合以下規範：
		/// </summary>
		/// <param name="password">要驗證的密碼</param>
		/// <returns>密碼驗證結果，包含是否成功和錯誤訊息</returns>
		public PasswordValidationResult Validate(string password)
		{
			// 密碼不能為空或僅包含空白字符
			if (string.IsNullOrWhiteSpace(password))
			{
				return PasswordValidationResult.Fail("Password cannot be empty or whitespace.");
			}

			// 密碼必須至少8個字符長
			if (password.Length < 8)
			{
				return PasswordValidationResult.Fail("Password must be at least 8 characters long.");
			}

			// 密碼必須包含至少一個字母和一個數字
			if (!password.Any(char.IsLetter))
			{
				return PasswordValidationResult.Fail("Password must contain at least one letter.");
			}

			// 密碼必須包含至少一個數字
			if (!password.Any(char.IsDigit))
			{
				return PasswordValidationResult.Fail("Password must contain at least one digit.");
			}

			// 密碼不能包含空格
			if (password.Contains(' '))
			{
				return PasswordValidationResult.Fail("Password cannot contain spaces.");
			}

			// 如果所有驗證都通過，返回成功結果
			return PasswordValidationResult.Success();
		}
	}

	/// <summary>
	/// 密碼驗證器接口，定義了驗證密碼的方法
	/// </summary>
	public interface IPasswordValidator
	{
		/// <summary>
		/// 驗證密碼是否符合以下規範：
		/// </summary>
		/// <param name="password">要驗證的密碼</param>
		/// <returns>密碼驗證結果，包含是否成功和錯誤訊息</returns>
		PasswordValidationResult Validate(string password);
	}
}
