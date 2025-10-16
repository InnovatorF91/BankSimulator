using ShareProject.Common;

namespace ShareProject.Request
{
    /// <summary>
    /// 客戶請求類別，包含客戶相關的請求屬性
    /// </summary>
    public class CustomerRequest : BaseRequest
    {
        public CustomerDto Customer { get; set; } = new CustomerDto(); // 客戶資料傳輸物件
        public string PasswordHash { get; set; } = string.Empty; // 密碼哈希
        public bool TwoFactorEnabled { get; set; } // 是否啟用雙因素認證
        public DateTime? CreateAt { get; set; } // 創建時間
        public DateTime? UpdateAt { get; set; } // 更新時間
        public DateTime? DeletedAt { get; set; } // 刪除時間
        public bool IsDeleted { get; set; } // 是否已刪除
    }
}
