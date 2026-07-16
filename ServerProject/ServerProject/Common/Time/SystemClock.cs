namespace ServerProject.Common
{
    /// <summary>
    /// 系統時鐘實現，提供當前時間的獲取方法
    /// </summary>
    public class SystemClock : ITimeProvider
    {
        /// <summary>
        /// 獲取當前UTC時間
        /// </summary>
        /// <returns>UTC時間</returns>
        public DateTime UtcNow()
        {
            return DateTime.UtcNow;
        }

        /// <summary>
        /// 獲取當前本地時間（不包含時間部分）
        /// </summary>
        /// <returns>本地時間（不包含時間部分）</returns>
        public DateTime Today()
        {
            return DateTime.Today;
        }

        /// <summary>
        /// 獲取當前本地時間
        /// </summary>
        /// <returns>本地時間</returns>
        public DateTime Now()
        {
            return DateTime.Now;
        }
    }
}
