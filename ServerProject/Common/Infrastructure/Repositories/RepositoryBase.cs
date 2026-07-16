using System.Collections.Concurrent;
using System.Text;

namespace ServerProject.Common
{
	/// <summary>
	/// 存儲庫基類，提供 SQL 文件加載和緩存功能
	/// </summary>
	public class RepositoryBase
	{
		/// <summary>
		/// 缓存所有已读取的 SQL 文件内容
		/// </summary>
		private static readonly ConcurrentDictionary<string, string> _sqlCache = new();

		/// <summary>
		/// SQL 文件根目录
		/// </summary>
		private static readonly string _sqlRootDir =
			Path.Combine(AppContext.BaseDirectory, "Sql");

		/// <summary>
		/// 数据访问接口
		/// </summary>
		protected IDataAccess _dataAccess;

		/// <summary>
		/// 数据访问接口属性
		/// </summary>
		public virtual IDataAccess DataAccess
		{
			set => _dataAccess = value;
		}

		/// <summary>
		/// 存储库基类构造函数
		/// </summary>
		/// <param name="dataAccess">数据访问接口</param>
		public RepositoryBase(IDataAccess dataAccess)
		{
			_dataAccess = dataAccess;
		}

		/// <summary>
		/// 读取 SQL 文件并缓存
		/// 用法：LoadSql("Transaction.GetById")
		/// => Sql/Transaction/GetById.sql
		/// </summary>
		protected string LoadSql(string key)
		{
			return _sqlCache.GetOrAdd(key, k =>
			{
				// 解析 SQL 文件路径
				var path = ResolveSqlPath(k);

				// 读取文件内容，如果文件不存在则抛出异常
				if (!File.Exists(path))
					throw new FileNotFoundException($"SQL file not found: {path}");

				// 返回文件内容
				return File.ReadAllText(path, Encoding.UTF8);
			});
		}

		/// <summary>
		/// 解析 SQL 文件路径
		/// </summary>
		/// <param name="key">SQL 键，格式为 "Folder.FileName" </param>
		/// <returns> SQL 文件的完整路径 </returns>
		/// <exception cref="ArgumentException">无效参数异常 </exception>
		private string ResolveSqlPath(string key)
		{
			// 分割键以获取文件夹和文件名
			var parts = key.Split('.', 2);

			// 验证格式是否正确
			if (parts.Length != 2)
				throw new ArgumentException($"Invalid SQL key format: {key}");

			// 构建文件路径
			var folder = parts[0];

			// 添加 .sql 扩展名
			var file = parts[1] + ".sql";

			// 返回完整路径
			return Path.Combine(_sqlRootDir, folder, file);
		}
	}
}
