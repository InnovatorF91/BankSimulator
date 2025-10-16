using System.Data;

namespace ServerProject.Common
{
    /// <summary>
    /// 數據訪問接口
    /// </summary>
    public interface IDataAccess : IDisposable
    {
        /// <summary>
        /// 數據庫連接對象
        /// </summary>
        IDbConnection DbConnection { get; }

        /// <summary>
        /// 數據庫事務對象
        /// </summary>
        IDbTransaction DbTransaction { get; }

        /// <summary>
        /// 是否使用事務
        /// </summary>
        bool UseTransaction { get; }

        /// <summary>
        /// 回滾事務
        /// </summary>
        void Rollback();

        /// <summary>
        /// 提交事務
        /// </summary>
        void Commit();
    }
}
