namespace ShareProject.Response
{
    /// <summary>
    /// 基礎回應類別，所有回應類別都應繼承自此類別
    /// </summary>
    public class BaseResponse
    {
        public bool Success { get; set; } // 是否成功
        public string Message { get; set; } = string.Empty; // 回應訊息
        public string[] Errors { get; set; } = Array.Empty<string>(); // 錯誤訊息列表
    }
}
