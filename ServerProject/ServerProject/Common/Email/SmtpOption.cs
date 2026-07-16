namespace ServerProject.Common
{
	/// <summary>
	/// SMTP选项类，用于配置SMTP服务器的连接和认证信息，以便发送电子邮件
	/// </summary>
	public class SmtpOption
	{
		/// <summary>
		/// SMTP服务器地址，例如smtp.example.com
		/// </summary>
		public string Host { get; set; } = string.Empty;

		/// <summary>
		/// SMTP服务器端口，通常为587（TLS）或465（SSL）
		/// </summary>	
		public int Port { get; set; }

		/// <summary>
		/// SMTP服务器认证用户名，通常是邮箱地址
		/// </summary>
		public string UserName { get; set; } = string.Empty;

		/// <summary>
		/// SMTP服务器认证密码，建议使用应用专用密码或环境变量存储敏感信息
		/// </summary>
		public string Password { get; set; } = string.Empty;

		/// <summary>
		/// 发件人邮箱地址，通常与认证用户名相同，但也可以不同
		/// </summary>
		public string FromEmail { get; set; } = string.Empty;

		/// <summary>
		/// 发件人显示名称，例如"Bank Support Team"，在收件人邮箱中显示为发件人名称
		/// </summary>
		public string FromName { get; set; } = string.Empty;

		/// <summary>
		/// 是否启用SSL/TLS加密连接，建议启用以确保邮件传输安全
		/// </summary>
		public bool EnableSsl { get; set; }
	}
}
