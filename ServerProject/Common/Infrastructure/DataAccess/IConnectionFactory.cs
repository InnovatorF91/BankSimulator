using System.Data;

namespace ServerProject.Common
{
    /// <summary>
    /// 數據庫連接工廠接口
    /// </summary>
    public interface IConnectionFactory
    {
        /// <summary>
        /// 取得數據庫連接對象
        /// </summary>
        /// <returns>數據庫連接對象</returns>
        IDbConnection GetConnection();
    }
}
