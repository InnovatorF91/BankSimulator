using System.Data;
using Npgsql;

namespace ServerProject.Common
{
    /// <summary>
    /// 數據庫連接工廠實現類
    /// </summary>
    public class ConnectionFactory : IConnectionFactory
    {
        /// <summary>
        /// 數據庫連接名稱
        /// </summary>
        private string _connectionName = string.Empty;

        /// <summary>
        /// 配置對象，用於讀取連接字符串
        /// </summary>
        private IConfiguration _configuration;

        /// <summary>
        /// 數據庫連接工廠構造函數
        /// </summary>
        /// <param name="configuration">配置對象</param>
        /// <exception cref="ArgumentNullException">参数空异常</exception>
        public ConnectionFactory(IConfiguration configuration)
        {
            // 確保配置對象不為空
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// 是否使用事務
        /// </summary>
        public bool Transaction { get; private set; }

        /// <summary>
        /// 釋放資源方法
        /// </summary>
        public void Dispose()
        {
            _connectionName = "EMPTY";
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 取得數據庫連接對象
        /// </summary>
        /// <param name="transaction">是否使用事務</param>
        /// <returns>數據庫連接對象</returns>
        /// <exception cref="ArgumentException">参数空异常</exception>
        public IDbConnection GetConnection(bool transaction = false)
        {
            _connectionName = "Bankingdb";

            // 從配置中讀取連接字符串
            var connectionString = _configuration.GetConnectionString(_connectionName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                // 如果連接字符串為空，則拋出異常
                throw new ArgumentException("Connection string is required.", nameof(_connectionName));
            }

            Transaction = transaction;

            // 返回新的 Npgsql 連接對象
            return new NpgsqlConnection(connectionString);
        }
    }
}
