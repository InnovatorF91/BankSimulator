using System.Security.Cryptography;

namespace ServerProject.Common
{
    public class CryptoService : ICryptoService
    {
        /// <summary>
        /// 哈希選項，用於配置哈希算法的參數
        /// </summary>
        private readonly HashOption _option;

        /// <summary>
        /// CryptoService 構造函數，初始化哈希選項
        /// </summary>
        /// <param name="option">哈希選項</param>
        public CryptoService(HashOption option)
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
            // 根據 profile 決定不同的參數 (例: PIN 用更少迭代以提升速度)
            int iterations = _option.Iterations;
            int saltSize = _option.SaltByte;
            string algorithm = _option.Algorithm;

            switch (profile)
            {
                case HashProfile.UserPassword:
                    iterations = _option.Iterations;   // 密碼：安全優先
                    saltSize = _option.SaltByte;       // 16 bytes salt
                    break;

                case HashProfile.CardPIN:
                    iterations = _option.Iterations / 10; // PIN：稍微降低迭代數，加快驗證
                    saltSize = 8;                         // PIN 使用短一些的 salt
                    break;
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
            // 解析哈希值
            var parts = hash.Split('.');
            if (parts.Length != 4)
                return false;

            // 確保格式正確
            string algorithm = parts[0];
            int iterations = int.Parse(parts[1]);
            var salt = Convert.FromBase64String(parts[2]);
            var key = Convert.FromBase64String(parts[3]);

            // 確認算法是否為 SHA256
            using (var pbkdf2 = new Rfc2898DeriveBytes(input, salt, iterations, HashAlgorithmName.SHA256))
            {
                var keyToCheck = pbkdf2.GetBytes(32);
                return CryptographicOperations.FixedTimeEquals(key, keyToCheck); // 使用固定時間比較以防止時間攻擊
            }
        }
    }

    public interface ICryptoService
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
