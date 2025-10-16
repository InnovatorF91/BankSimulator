namespace ShareProject.Common
{
    /// <summary>
    /// 幣種
    /// </summary>
    public enum CurrencyCode
    {
        JPY, // 日圓
        USD, // 美元
        EUR, // 歐元
        CNY, // 人民幣
        HKD, // 港幣
        TWD, // 新台幣
    }

    /// <summary>
    /// 銀行帳戶狀態
    /// </summary>
    public enum AccountStatus
    {
        Active, // 啟用
        Frozen, // 凍結
        Closed, // 關閉
    }

    /// <summary>
    /// 交易類型
    /// </summary>
    public enum TransactionType
    {
        Deposit, // 存款
        Withdrawal, // 提款
        Transfer, // 轉帳
    }

    /// <summary>
    /// 卡片類型
    /// </summary>
    public enum CardType
    {
        Debit, // 借記卡
        ATM, // ATM卡
    }

    /// <summary>
    /// 卡片替換原因
    /// </summary>
    public enum CardReplaceReason
    {
        Damaged, // 損壞
        Lost, // 遺失
        Stolen, // 盜竊
        Expired, // 過期
    }

    /// <summary>
    /// 帳戶類型
    /// </summary>
    public enum AccountType
    {
        Checking, // 活期存款
        Savings, // 定期存款
    }

    /// <summary>
    /// 交易狀態
    /// </summary>
    public enum TransactionStatus
    {
        Pending, // 待處理
        Completed, // 已完成
        Failed, // 失敗
        Reversed, // 已撤銷
    }

    /// <summary>
    /// 性別
    /// </summary>
    public enum Gender
    {
        male, // 男性
        female, // 女性
    }

    /// <summary>
    /// 身份證明類型
    /// </summary>
    public enum IDType
    {
        IDCard, // 身份證
        Passport, // 護照
        ResudenceCard, // 居留證
        DriverLicense, // 駕駛執照
    }

    /// <summary>
    /// KYC (Know Your Customer) 狀態
    /// </summary>
    public enum KYCStatus
    {
        Unreviewed, // 未審核
        Passed, // 已通過
        Refused, // 已拒絕
    }

    /// <summary>
    /// 卡片狀態
    /// </summary>
    public enum CardStatus
    {
        Active, // 啟用
        Inactive, // 停用
        Blocked, // 阻止
        Expired, // 過期
    }
}
