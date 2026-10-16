namespace ShareProject.Common
{
	public enum TransactionErrorCode
	{
		TransactionNotFound = 3001,          // 交易記錄未找到
		TransactionAccessDenied = 3002,      // 無權訪問該交易記錄
		InvalidTransactionId = 3003,         // 無效的交易ID
		InvalidTransactionType = 3004,       // 無效的交易類型
		InvalidTransactionStatus = 3005,     // 無效的交易狀態

		InvalidAmount = 3010,                 // 無效的交易金額
		InsufficientBalance = 3011,           // 帳戶餘額不足
		BalanceCalculationFailed = 3012,      // 交易前後餘額計算不一致
		CurrencyMismatch = 3013,              // 轉出帳戶與轉入帳戶的貨幣不一致

		SourceAccountNotFound = 3020,         // 轉出帳戶未找到
		TargetAccountNotFound = 3021,         // 轉入帳戶未找到
		SourceAccountAccessDenied = 3022,     // 無權操作轉出帳戶
		AccountNotAvailable = 3023,           // 帳戶當前狀態不允許交易
		SameAccountTransfer = 3024,           // 轉出帳戶與轉入帳戶不能相同

		InvalidTransactionPeriod = 3030,      // 無效的交易查詢期間
		TransactionGroupNotFound = 3031,      // 交易組未找到
		InvalidGroupId = 3032,                // 無效的交易組ID

		DepositFailed = 3040,                 // 存款處理失敗
		WithdrawFailed = 3041,                // 取款處理失敗
		TransferFailed = 3042,                // 轉帳處理失敗
		TransactionInsertFailed = 3043,       // 交易記錄新增失敗
		AccountBalanceUpdateFailed = 3044,    // 帳戶餘額更新失敗

		UnableToOperateDb = 3099,              // 無法操作資料庫
		ValidationFailed = 4000,               // 通用驗證失敗
	}
}
