namespace ServerProject.Common
{

    /// <summary>
    /// 時間提供者接口，用於獲取當前時間
    /// </summary>
    public interface ITimeProvider
    {
        /// <summary>
        /// 獲取當前UTC時間
        /// </summary>
        /// <returns>UTC時間</returns>
        DateTime UtcNow();

        /// <summary>
        /// 獲取當前本地時間
        /// </summary>
        /// <returns>本地時間</returns>
        DateTime Now();

        /// <summary>
        /// 獲取當前本地時間（不包含時間部分）
        /// </summary>
        /// <returns>本地時間（不包含時間部分）</returns>
        DateTime Today();
    }
}
