namespace ShareProject.Common
{
	public enum CustomerErrorCode
	{
		CustomerNotFound = 1001, // 客戶未找到
		InvalidCustomerData = 1002, // 無效的客戶資料
		DuplicateCustomer = 1003, // 重複的客戶
		KYCVerificationFailed = 1004, // KYC驗證失敗
		UnauthorizedAccess = 1005, // 未經授權的訪問
		CustomerCreationFailed = 1006, // 客戶創建失敗
		CustomerUpdateFailed = 1007, // 客戶更新失敗
		CustomerDeletionFailed = 1008, // 客戶刪除失敗
		UnableToOperateDb = 1009, // 無法操作資料庫
	}
}
