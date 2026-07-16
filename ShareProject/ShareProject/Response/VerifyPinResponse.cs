namespace ShareProject.Response
{
    /// <summary>
    /// 驗證 PIN 碼回應類別，包含卡片 ID、剩餘嘗試次數、鎖定到期時間和是否需要重設 PIN
    /// </summary>
    public class VerifyPinResponse : BaseResponse
    {
        public Guid CardId { get; set; } // 卡片ID
        public int AttemptsRemaining { get; set; } // 剩餘嘗試次數
        public DateTimeOffset? LockedUntil { get; set; } // 鎖定到期時間，若為 null 則未鎖定
        public bool RequiredReset { get; set; } // 是否需要重設 PIN
    }
}
