using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ServerProject.Common
{
	public class SmtpEmailSender : IEmailSender
	{
		private readonly SmtpOption _smtpOption;

		public SmtpEmailSender(IOptions<SmtpOption> smtpOption)
		{
			_smtpOption = smtpOption.Value;
		}

		public async Task SendResetPasswordEmailAsync(string toEmail, string resetToken, DateTime expiresAt)
		{
			var message = new MimeMessage();

			// 发件人
			message.From.Add(new MailboxAddress(_smtpOption.FromName, _smtpOption.FromEmail));

			// 收件人
			message.To.Add(MailboxAddress.Parse(toEmail));

			// 标题
			message.Subject = "BankSimulator - Password Reset";

			// 正文
			var bodyBuilder = new BodyBuilder
			{
				TextBody =
				$@"You requested to reset your password.

                Reset Token:
                {resetToken}
                
                This token will expire at:
                {expiresAt:yyyy-MM-dd HH:mm:ss}
                
                If you did not request this, please ignore this email."
			};

			message.Body = bodyBuilder.ToMessageBody();

			using var client = new SmtpClient();

			// 开发阶段常用 StartTls
			var secureSocketOption = _smtpOption.EnableSsl
				? SecureSocketOptions.StartTls
				: SecureSocketOptions.None;

			await client.ConnectAsync(_smtpOption.Host, _smtpOption.Port, secureSocketOption);

			if (!string.IsNullOrWhiteSpace(_smtpOption.UserName))
			{
				await client.AuthenticateAsync(_smtpOption.UserName, _smtpOption.Password);
			}

			await client.SendAsync(message);
			await client.DisconnectAsync(true);
		}
	}
}
