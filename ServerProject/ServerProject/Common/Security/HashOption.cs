namespace ServerProject.Common
{
    /// <summary>
    /// 哈希選項類，用於配置哈希算法的參數
    /// </summary>
    public class HashOption
    {
        public string Algorithm { get; set; } = "SHA256"; // 哈希算法，默認為SHA256
        public int Iterations { get; set; } = 10000; // 哈希迭代次數，默認為10000
        public int MemorySize { get; set; } = 65536; // 哈希內存大小，默認為65536字節
        public int Parallelism { get; set; } = 1; // 哈希並行度，默認為1
        public int SaltByte { get; set; } = 16; // 隨機鹽的字節數，默認為16字節
    }

    /// <summary>
    /// 哈希配置文件枚舉，用於指定不同的哈希配置文件
    /// </summary>
    public enum HashProfile
    {
        UserPassword, // 用戶密碼哈希配置
        CardPIN, // 卡片PIN碼哈希配置
    }
}
