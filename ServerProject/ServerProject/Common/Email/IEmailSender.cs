namespace ServerProject.Common
{
	/// <summary>
	/// 電子郵件發送器接口，定義了發送重置密碼電子郵件的方法
	/// </summary>
	public interface IEmailSender
	{
		/// <summary>
		/// 發送重置密碼電子郵件
		/// </summary>
		/// <param name="toEmail">收件人電子郵件地址</param>
		/// <param name="resetToken">重置密碼令牌</param>
		/// <param name="expiresAt">令牌過期時間</param>
		/// <returns></returns>
		Task SendResetPasswordEmailAsync(string toEmail, string resetToken, DateTime expiresAt);
	}
}
