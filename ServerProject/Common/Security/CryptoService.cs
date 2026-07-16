using System.Security.Cryptography;

namespace ServerProject.Common
{
    public class CryptoRepository : ICryptoRepository
    {
        /// <summary>
        /// 哈希選項，用於配置哈希算法的參數
        /// </summary>
        private readonly HashOption _option;

        /// <summary>
        /// CryptoRepository 構造函數，初始化哈希選項
        /// </summary>
        /// <param name="option">哈希選項</param>
        public CryptoRepository(HashOption option)
        {
            _option = option;
        }

        /// <summary>
        /// 生成哈希值
        /// </summary>
        /// <param name="input">輸入</param>
        /// <param name="profile">配置文件</param>
        /// <returns>哈希值</returns>
        public string Hash(string input, HashProfile? profile)
        {
            // 根據 profile 決定不同的參數
            var iterations = _option.Iterations;
            var saltSize = _option.SaltByte;
            var algorithm = _option.Algorithm;

            switch (profile)
            {
                case HashProfile.UserPassword:
                    iterations = _option.Iterations;      // 密碼：安全優先
                    saltSize = _option.SaltByte;          // 16 bytes salt
					algorithm = "PBKDF2-SHA256";          // 明確標註使用 PBKDF2-SHA256 算法
					break;

                case HashProfile.CardPIN:
                    iterations = _option.Iterations / 10; // PIN：稍微降低迭代數，加快驗證
                    saltSize = 8;                         // PIN 使用短一些的 salt
					algorithm = "PBKDF2-SHA256";          // 明確標註使用 PBKDF2-SHA256 算法
					break;
                case HashProfile.RefreshToken:
					iterations = _option.Iterations / 5;  // 刷新令牌：平衡安全與性能
					saltSize = 0;                         // 不使用 salt，因為令牌本身已經是隨機的
					algorithm = "SHA256";                 // 明確標註使用 SHA256 算法
					break;
                case HashProfile.PasswordResetToken:
                    iterations = _option.Iterations / 5;  // 密碼重置令牌：平衡安全與性能
                    saltSize = 0;                         // 不使用 salt，因為令牌本身已經是隨機的
					algorithm = "SHA256";                 // 明確標註使用 SHA256 算法
					break;
			}

			// 如果不使用 salt，直接使用 SHA256 哈希
			if (saltSize == 0)
            {
				using var sha256 = SHA256.Create();
				var bytes = System.Text.Encoding.UTF8.GetBytes(input);
				var hash = sha256.ComputeHash(bytes);
				return $"{algorithm}.{iterations}..{Convert.ToBase64String(hash)}";
			}

			// 生成 Salt
			var salt = new byte[saltSize];
			using (var rng = RandomNumberGenerator.Create())
			{
				rng.GetBytes(salt);
			}

			// 生成 Key
			using (var pbkdf2 = new Rfc2898DeriveBytes(input, salt, iterations, HashAlgorithmName.SHA256))
            {
                var key = pbkdf2.GetBytes(32); // 256-bit

                // 儲存格式： algorithm.iterations.salt.key
                return $"{algorithm}.{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
            }
        }

        /// <summary>
        /// 驗證輸入是否與哈希值匹配
        /// </summary>
        /// <param name="hash">哈希值</param>
        /// <param name="input">輸入</param>
        /// <returns>true:匹配成功/false:匹配失敗</returns>
        public bool Verify(string hash, string input)
        {
			var parts = hash.Split('.');
			if (parts.Length != 4)
				return false;

			string algorithm = parts[0];
			int iterations = int.Parse(parts[1]);
			string saltPart = parts[2];
			var key = Convert.FromBase64String(parts[3]);

			// 无 salt：走确定性 SHA256
			if (string.IsNullOrEmpty(saltPart))
			{
				using var sha256 = SHA256.Create();
				var bytes = System.Text.Encoding.UTF8.GetBytes(input);
				var hashToCheck = sha256.ComputeHash(bytes);
				return CryptographicOperations.FixedTimeEquals(key, hashToCheck);
			}

			// 有 salt：走 PBKDF2
			var salt = Convert.FromBase64String(saltPart);

			using var pbkdf2 = new Rfc2898DeriveBytes(input, salt, iterations, HashAlgorithmName.SHA256);
			var keyToCheck = pbkdf2.GetBytes(32);
			return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
		}
    }

    public interface ICryptoRepository
    {
        /// <summary>
        /// 生成哈希值
        /// </summary>
        /// <param name="input">輸入</param>
        /// <param name="profile">配置文件</param>
        /// <returns>哈希值</returns>
        string Hash(string input, HashProfile? profile);

        /// <summary>
        /// 驗證輸入是否與哈希值匹配
        /// </summary>
        /// <param name="hash">哈希值</param>
        /// <param name="input">輸入</param>
        /// <returns>true:匹配成功/false:匹配失敗</returns>
        bool Verify(string hash, string input);
    }
}
