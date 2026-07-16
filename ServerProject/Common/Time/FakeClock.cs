namespace ServerProject.Common
{
    /// <summary>
    /// 模擬時鐘實現，用於測試和模擬時間的流逝
    /// </summary>
    public class FakeClock : ITimeProvider
    {
        /// <summary>
        /// 模擬的當前UTC時間
        /// </summary>
        private DateTime _nowUtc;

        /// <summary>
        /// 模擬時鐘構造函數，初始化當前UTC時間
        /// </summary>
        /// <param name="startUtc">起始的UTC時間</param>
        public FakeClock(DateTime? startUtc = null)
        {
            // 如果提供了起始UTC時間，則使用它；否則使用當前UTC時間
            _nowUtc = startUtc.HasValue
                ? (startUtc.Value.Kind == DateTimeKind.Utc
                    ? startUtc.Value
                    : startUtc.Value.ToUniversalTime())
                : DateTime.UtcNow;
        }

        /// <summary>
        /// 現在的UTC時間
        /// </summary>
        /// <returns>UTC時間</returns>
        public DateTime UtcNow() => _nowUtc;

        /// <summary>
        /// 現在的本地時間
        /// </summary>
        /// <returns>本地時間</returns>
        public DateTime Now() => _nowUtc.ToLocalTime();

        /// <summary>
        /// 今天的本地日期（不包含時間部分）
        /// </summary>
        /// <returns>本地日期（不包含時間部分）</returns>
        public DateTime Today()
        {
            var local = _nowUtc.ToLocalTime();
            return new DateTime(local.Year, local.Month, local.Day);
        }

        /// <summary>
        /// 將模擬時鐘向前推進指定的時間間隔
        /// </summary>
        /// <param name="timeSpan">時間間隔</param>
        public void Advance(TimeSpan timeSpan) => _nowUtc = _nowUtc.Add(timeSpan);

        /// <summary>
        /// 設置模擬時鐘的當前UTC時間
        /// </summary>
        /// <param name="utc">當前UTC時間</param>
        public void Set(DateTime utc) =>
            _nowUtc = utc.Kind == DateTimeKind.Utc ? utc : utc.ToUniversalTime();
    }
}
