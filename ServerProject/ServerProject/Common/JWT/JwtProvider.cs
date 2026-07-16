using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ServerProject.Common
{
	/// <summary>
	/// JWT提供者
	/// </summary>
	public class JwtProvider : IJwtProvider
	{
		/// <summary>
		/// 密鑰
		/// </summary>
		private readonly string _secret = string.Empty;

		/// <summary>
		/// 加密服務
		/// </summary>
		private readonly ICryptoRepository _crypto;

		/// <summary>
		/// 時間提供者
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// 初始化JWT提供者
		/// </summary>
		/// <param name="secret">密鑰</param>
		/// <param name="cryptoRepository">加密服務</param>
		public JwtProvider(string secret, ICryptoRepository cryptoRepository, ITimeProvider timeProvider)
		{
			// 初始化密鑰和加密服務
			_secret = secret;
			_crypto = cryptoRepository;
			_timeProvider = timeProvider;
		}

		/// <summary>
		/// 訪問令牌有效期
		/// </summary>
		public TimeSpan AccessTokenLifetime => TimeSpan.FromMinutes(30);

		/// <summary>
		/// 刷新令牌有效期
		/// </summary>
		public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(14);

		/// <summary>
		/// 重置密碼令牌有效期
		/// </summary>
		public TimeSpan PasswordResetTokenLifetime => TimeSpan.FromMinutes(30);

		/// <summary>
		/// 創建訪問令牌
		/// </summary>
		/// <param name="userId">用户ID</param>
		/// <param name="tokenVersion">令牌版本</param>
		/// <returns>訪問令牌</returns>
		public string CreateAccessToken(int userId, int tokenVersion)
		{
			var handler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes(_secret);

			var token = handler.CreateJwtSecurityToken(
				subject: new ClaimsIdentity(new[]
				{
				new Claim("uid", userId.ToString()),
				new Claim("ver", tokenVersion.ToString())
				}),
				expires: _timeProvider.Now().Add(AccessTokenLifetime),
				signingCredentials: new SigningCredentials(
					new SymmetricSecurityKey(key),
					SecurityAlgorithms.HmacSha256)
			);

			return handler.WriteToken(token);
		}

		/// <summary>
		/// 創建刷新令牌
		/// </summary>
		/// <returns>刷新令牌</returns>
		public string CreateRefreshToken()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		}

		/// <summary>
		/// 創建刷新令牌哈希值
		/// </summary>
		/// <param name="refreshToken">刷新令牌</param>
		/// <returns>刷新令牌哈希值</returns>
		public string CreateRefreshTokenHash(string refreshToken)
		{
			return _crypto.Hash("rt:" + refreshToken, HashProfile.RefreshToken);
		}

		/// <summary>
		/// 創建重置密碼令牌
		/// </summary>
		/// <returns>重置密碼令牌</returns>
		public string CreatePasswordResetToken()
		{
			return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
		}

		/// <summary>
		/// 創建重置密碼令牌哈希值
		/// </summary>
		/// <param name="resetToken">重置密碼令牌</param>
		/// <returns>重置密碼令牌哈希值</returns>
		public string CreatePasswordResetTokenHash(string resetToken)
		{
			return _crypto.Hash("prt:" + resetToken, HashProfile.PasswordResetToken);
		}
	}

	/// <summary>
	/// JWT提供者接口
	/// </summary>
	public interface IJwtProvider
	{
		/// <summary>
		/// 創建訪問令牌
		/// </summary>
		/// <param name="userId">用户ID</param>
		/// <param name="tokenVersion">令牌版本</param>
		/// <returns>訪問令牌</returns>
		string CreateAccessToken(int userId, int tokenVersion);

		/// <summary>
		/// 創建刷新令牌
		/// </summary>
		/// <returns>刷新令牌</returns>
		string CreateRefreshToken();

		/// <summary>
		/// 創建重置密碼令牌
		/// </summary>
		/// <returns>重置密碼令牌</returns>
		string CreatePasswordResetToken();

		/// <summary>
		/// 創建刷新令牌哈希值
		/// </summary>
		/// <param name="refreshToken">刷新令牌</param>
		/// <returns>刷新令牌哈希值</returns>
		string CreateRefreshTokenHash(string refreshToken);

		/// <summary>
		/// 創建重置密碼令牌哈希值
		/// </summary>
		/// <param name="resetToken">重置密碼令牌</param>
		/// <returns>重置密碼令牌哈希值</returns>
		string CreatePasswordResetTokenHash(string resetToken);

		/// <summary>
		/// 訪問令牌有效期
		/// </summary>
		TimeSpan AccessTokenLifetime { get; }

		/// <summary>
		/// 刷新令牌有效期
		/// </summary>
		TimeSpan RefreshTokenLifetime { get; }

		/// <summary>
		/// 重置密碼令牌有效期
		/// </summary>
		TimeSpan PasswordResetTokenLifetime { get; }
	}
}
