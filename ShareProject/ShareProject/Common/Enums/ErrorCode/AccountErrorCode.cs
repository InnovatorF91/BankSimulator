namespace ShareProject.Common
{
	public enum AccountErrorCode
	{
		AccountNotFound = 1001,       // 帳戶未找到
		AccountAccessDenied = 1002,   // 無權訪問帳戶
		AccountAlreadyExists = 1003,  // 帳戶已存在
		InvalidCurrency = 1004,       // 無效的貨幣
		InvalidAccountType = 1005,    // 無效的帳戶類型
		AccountAlreadyClosed = 1006,  // 帳戶已經關閉
		AccountBalanceNotZero = 1007, // 帳戶餘額不為零，無法關閉
		AccountStatusInvalid = 1008,  // 帳戶狀態無效，無法進行操作
		AccountNotAvailable = 1009,   // 帳戶不可用，無法進行操作
		AccountOperationLogNotFound = 1010,// 无法找到账户操作日志
		UnableToOperateDb = 1099,      // 無法操作資料庫
		InvalidInitialDepositAmount = 1100, // 無效的初始存款金額
		ValidationFailed = 2000, // 通用验证失败
	}
}
