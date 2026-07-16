namespace ServerProject.Common
{
	/// <summary>
	/// 表示密碼驗證結果的類別。該類別包含兩個屬性：IsValid和Message。IsValid是一個布爾值，表示密碼是否有效；Message是一個字符串，當密碼無效時包含具體的錯誤訊息，說明為什麼密碼不符合安全規範。
	/// </summary>
	public class PasswordValidationResult
	{
		/// <summary>
		/// 表示密碼是否有效。如果為true，則密碼符合安全規範；如果為false，則密碼不符合安全規範，並且Message屬性將包含具體的錯誤訊息。
		/// </summary>
		public bool IsValid { get; }

		/// <summary>
		/// 當IsValid為false時，Message屬性將包含具體的錯誤訊息，說明為什麼密碼不符合安全規範；當IsValid為true時，Message屬性將為空字符串。
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// 私有構造函數，僅供內部使用。通過Success和Fail靜態方法創建實例。
		/// </summary>
		/// <param name="isValid">表示密碼是否有效的布爾值</param>
		/// <param name="message">當密碼無效時的錯誤訊息，或者當密碼有效時的空字符串</param>
		private PasswordValidationResult(bool isValid, string message)
		{
			IsValid = isValid;
			Message = message;
		}

		/// <summary>
		/// 創建一個表示密碼驗證成功的PasswordValidationResult實例。當密碼符合安全規範時，IsValid屬性將為true，Message屬性將為空字符串。
		/// </summary>
		/// <returns>一個表示密碼驗證成功的PasswordValidationResult實例</returns>
		public static PasswordValidationResult Success()
		{
			return new PasswordValidationResult(true, string.Empty);
		}

		/// <summary>
		/// 創建一個表示密碼驗證失敗的PasswordValidationResult實例。當密碼不符合安全規範時，IsValid屬性將為false，Message屬性將包含具體的錯誤訊息，說明為什麼密碼不符合安全規範。
		/// </summary>
		/// <param name="message">當密碼無效時的錯誤訊息，說明為什麼密碼不符合安全規範</param>
		/// <returns>一個表示密碼驗證失敗的PasswordValidationResult實例</returns>
		public static PasswordValidationResult Fail(string message)
		{
			return new PasswordValidationResult(false, message);
		}
	}
}
