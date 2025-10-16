namespace ServerProject.Common
{
    /// <summary>
    /// 無法操作資料庫異常類別
    /// </summary>
    public class UnableToOperateDBException : InvalidOperationException
    {
        /// <summary>
        /// 初始化 UnableToOperateDBException 類別的新實例
        /// </summary>
        /// <param name="message">message</param>
        public UnableToOperateDBException(string message) : base(message) { }

        /// <summary>
        /// 初始化 UnableToOperateDBException 類別的新實例
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="innerException">innerException</param>
        public UnableToOperateDBException(string message, Exception innerException) : base(message, innerException) { }

        /// <summary>
        /// 操作結果
        /// </summary>
        public bool Result { get; set; }

		/// <summary>
		/// 錯誤代碼
		/// </summary>
		public int ErrorCode { get; set; }
	}
}
