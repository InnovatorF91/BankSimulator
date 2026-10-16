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
		/// 數據庫連接字符串
		/// </summary>
		private readonly string _connectionString;

		/// <summary>
		/// 初始化數據庫連接工廠實現類
		/// </summary>
		/// <param name="configuration">配置對象</param>
		/// <exception cref="ArgumentException">當連接字符串為空時拋出異常</exception>
		public ConnectionFactory(IConfiguration configuration)
		{
			_connectionString =
				configuration.GetConnectionString("Bankingdb")
				?? throw new ArgumentException(
					"Connection string 'Bankingdb' is required.");
		}

		/// <summary>
		/// 取得數據庫連接對象
		/// </summary>
		/// <returns>数据库连接对象</returns>
		public IDbConnection GetConnection()
		{
			return new NpgsqlConnection(_connectionString);
		}
	}
}
