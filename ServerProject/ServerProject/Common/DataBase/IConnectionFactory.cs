using System.Data;

namespace ServerProject.Common
{
    /// <summary>
    /// 數據庫連接工廠接口
    /// </summary>
    public interface IConnectionFactory : IDisposable
    {
        /// <summary>
        /// 取得數據庫連接對象
        /// </summary>
        /// <param name="transaction">是否使用事務</param>
        /// <returns>數據庫連接對象</returns>
        IDbConnection GetConnection(bool transaction = false);

        /// <summary>
        /// 是否使用事務
        /// </summary>
        bool Transaction { get; }
    }
}
