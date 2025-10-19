namespace ServerProject.Common
{
    /// <summary>
    /// 邏輯基類，提供數據庫連接工廠的支持
    /// </summary>
    public class ServiceBase
    {
        /// <summary>
        /// 數據庫連接工廠，用於創建數據庫連接
        /// </summary>
        protected readonly IConnectionFactory _connectionFactory;

        /// <summary>
        /// 數據服務基類構造函數
        /// </summary>
        /// <param name="connectionFactory">數據庫連接工廠</param>
        public ServiceBase(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// 創建數據庫連接對象
        /// </summary>
        /// <param name="transaction">是否開啓事務</param>
        /// <returns>數據訪問接口</returns>
        protected virtual Task<IDataAccess> CreateConnectionAsync(bool transaction = false)
        {
            return Task.Run((Func<IDataAccess>)(() => new DataAccess(_connectionFactory.GetConnection(transaction), _connectionFactory.Transaction)));
        }
    }
}
