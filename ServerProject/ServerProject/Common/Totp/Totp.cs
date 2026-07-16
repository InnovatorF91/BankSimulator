using System.Security.Cryptography;
using System.Text;

namespace ServerProject.Common
{
	public static class Totp
	{
		/// <summary>
		/// Base32 アルファベット
		/// </summary>
		private static readonly string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

		/// <summary>
		/// TOTP URI を構築する
		/// </summary>
		/// <param name="issuer">発行者名</param>
		/// <param name="accountName">アカウント名</param>
		/// <param name="base64Secret">Base64 エンコードされたシークレット</param>
		/// <param name="digits">桁数 (デフォルト: 6)</param>
		/// <param name="period">有効期間 (デフォルト: 30 秒)</param>
		/// <param name="algorithm">アルゴリズム (デフォルト: SHA1)</param>
		/// <returns>TOTP URI</returns>
		/// <exception cref="ArgumentException">引数が無効な場合にスローされます </exception>
		public static string BuildTotpUri(
			string issuer,
			string accountName,
			string base64Secret,
			int digits = 6,
			int period = 30,
			string algorithm = "SHA1")
		{
			if (string.IsNullOrWhiteSpace(issuer)) throw new ArgumentException("issuer is required", nameof(issuer));
			if (string.IsNullOrWhiteSpace(accountName)) throw new ArgumentException("accountName is required", nameof(accountName));
			if (string.IsNullOrWhiteSpace(base64Secret)) throw new ArgumentException("base64Secret is required", nameof(base64Secret));

			// Base64(secret string) -> bytes -> Base32(for otpauth secret)
			var secretBytes = Convert.FromBase64String(base64Secret);
			var base32Secret = Base32Encode(secretBytes);

			var label = $"{issuer}:{accountName}";
			var labelEncoded = Uri.EscapeDataString(label);
			var issuerEncoded = Uri.EscapeDataString(issuer);

			var secretValue = base32Secret.Replace("=", "").Trim();

			return $"otpauth://totp/{labelEncoded}" +
				   $"?secret={secretValue}" +
				   $"&issuer={issuerEncoded}" +
				   $"&algorithm={Uri.EscapeDataString(algorithm)}" +
				   $"&digits={digits}" +
				   $"&period={period}";
		}

		/// <summary>
		/// Base32 エンコードを実行する
		/// </summary>
		/// <param name="data">エンコードするバイト配列</param>
		/// <returns>Base32 エンコードされた文字列</returns>
		private static string Base32Encode(byte[] data)
		{
			if (data == null || data.Length == 0) return string.Empty;

			var result = new StringBuilder((data.Length + 4) / 5 * 8);

			int buffer = data[0];
			int next = 1;
			int bitsLeft = 8;

			while (bitsLeft > 0 || next < data.Length)
			{
				if (bitsLeft < 5)
				{
					if (next < data.Length)
					{
						buffer <<= 8;
						buffer |= data[next++] & 0xff;
						bitsLeft += 8;
					}
					else
					{
						int pad = 5 - bitsLeft;
						buffer <<= pad;
						bitsLeft += pad;
					}
				}

				int index = (buffer >> (bitsLeft - 5)) & 0x1f;
				bitsLeft -= 5;
				result.Append(Base32Alphabet[index]);
			}

			return result.ToString();
		}

		/// <summary>
		/// 指定された Base64 シークレットから TOTP コードを検証する
		/// </summary>
		/// <param name="base64Secret">Base64 エンコードされたシークレット</param>
		/// <param name="code">検証する TOTP コード</param>
		/// <param name="now">現在の日時</param>
		/// <param name="periodSeconds">TOTP の有効期間（秒単位、デフォルト: 30 秒）</param>
		/// <param name="digits">TOTP コードの桁数（デフォルト: 6 桁）</param>
		/// <param name="allowedDriftSteps">許容される時間ドリフトのステップ数（デフォルト: 1）</param>
		/// <returns>TOTP コードが有効である場合は true、そうでない場合は false</returns>
		public static bool VerifyCodeFromBase64Secret(
			string base64Secret,
	        string code,
	        DateTime now,
	        int periodSeconds = 30,
	        int digits = 6,
	        int allowedDriftSteps = 1)
		{
			if (string.IsNullOrWhiteSpace(base64Secret)) return false;
			if (string.IsNullOrWhiteSpace(code)) return false;

			code = new string(code.Where(char.IsDigit).ToArray());
			if (code.Length != digits) return false;

			var key = Convert.FromBase64String(base64Secret);

			long unixSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();
			long timestep = unixSeconds / periodSeconds;

			for (long i = -allowedDriftSteps; i <= allowedDriftSteps; i++)
			{
				var expected = ComputeTotp(key, timestep + i, digits);
				if (expected == code) return true;
			}

			return false;
		}

		/// <summary>
		/// TOTP コードを計算する
		/// </summary>
		/// <param name="key"> シークレットキーのバイト配列</param>
		/// <param name="timestep">タイムステップ</param>
		/// <param name="digits">TOTP コードの桁数</param>
		/// <returns>計算された TOTP コード</returns>
		private static string ComputeTotp(byte[] key, long timestep, int digits)
		{
			Span<byte> counter = stackalloc byte[8];
			long v = timestep;
			for (int i = 7; i >= 0; i--)
			{
				counter[i] = (byte)(v & 0xff);
				v >>= 8;
			}

			using var hmac = new HMACSHA1(key);
			var hash = hmac.ComputeHash(counter.ToArray());

			int offset = hash[^1] & 0x0f;
			int binary =
				((hash[offset] & 0x7f) << 24) |
				((hash[offset + 1] & 0xff) << 16) |
				((hash[offset + 2] & 0xff) << 8) |
				(hash[offset + 3] & 0xff);

			int otp = binary % (int)Math.Pow(10, digits);
			return otp.ToString(new string('0', digits));
		}
	}
}
