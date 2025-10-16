using System.Data;

namespace ServerProject.Common
{
    public class DataAccess : IDataAccess
    {
        /// <summary>
        /// 數據庫連接對象
        /// </summary>
        private readonly IDbConnection _conn;

        /// <summary>
        /// 數據庫事務對象
        /// </summary>
        private IDbTransaction? _tx;

        /// <summary>
        /// 是否使用事務
        /// </summary>
        public bool UseTransaction { get; }

        /// <summary>
        /// 是否已經釋放資源
        /// </summary>
        public bool IsDisposed { get; private set; } = false;

        /// <summary>
        /// 數據訪問類構造函數
        /// </summary>
        /// <param name="connection">數據庫連接對象</param>
        /// <param name="useTransaction">數據庫事務對象</param>
        /// <param name="isolationLevel">交易隔離級別（默認是預設）</param>
        public DataAccess(
            IDbConnection connection,
            bool useTransaction = false,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            // 確保連接字符串符合 Npgsql 的格式要求
            _conn = connection;
            _conn.Open(); // Npgsql 内置连接池会复用底层物理连接

            // 設置事務使用狀態
            UseTransaction = useTransaction;
            if (UseTransaction)
            {
                // 如果使用事務，則開始一個新的事務
                _tx = _conn.BeginTransaction(isolationLevel);
            }
        }

        /// <summary>
        /// 數據庫連接對象
        /// </summary>
        public IDbConnection DbConnection => _conn;

        /// <summary>
        /// 數據庫事務對象
        /// </summary>
        public IDbTransaction DbTransaction =>
            // 如果使用事務，則返回當前事務對象，否則拋出異常
            UseTransaction
                ? _tx ?? throw new InvalidOperationException("Transaction has not been started.")
                : throw new InvalidOperationException("This DataAccess was created without transaction.");

        /// <summary>
        /// 提交當前事務
        /// </summary>
        /// <exception cref="InvalidOperationException">当方法调用对于对象的当前状态无效时引发的异常</exception>
        public void Commit()
        {
            // 確保使用事務，並且當前事務不為空
            if (!UseTransaction) throw new InvalidOperationException("Transactions are disabled.");

            // 如果事務為空，則拋出異常
            if (_tx == null) throw new InvalidOperationException("No active transaction to commit.");

            // 提交事務並釋放資源
            _tx.Commit();
            _tx.Dispose();
            _tx = null;
        }

        /// <summary>
        /// 回滾當前事務
        /// </summary>
        /// <exception cref="InvalidOperationException">当方法调用对于对象的当前状态无效时引发的异常</exception>
        public void Rollback()
        {
            // 確保使用事務，並且當前事務不為空
            if (!UseTransaction) throw new InvalidOperationException("Transactions are disabled.");

            // 如果事務為空，則直接返回
            if (_tx == null) return;

            // 回滾事務並釋放資源
            _tx.Rollback();
            _tx.Dispose();
            _tx = null;
        }

        /// <summary>
        /// 釋放資源，關閉數據庫連接並回滾事務（如果有的話）
        /// </summary>
        public void Dispose()
        {
            // 確保沒有重複釋放資源
            if (!IsDisposed)
            {
                // 如果沒有釋放資源，則關閉數據庫連接，並釋放資源
                DbConnection.Close();
                DbConnection.Dispose();
                IsDisposed = true;
                GC.Collect();
            }
        }
    }
}
