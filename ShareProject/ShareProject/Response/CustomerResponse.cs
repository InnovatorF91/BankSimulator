using ShareProject.Common;

namespace ShareProject.Response
{
    /// <summary>
    /// 客戶回應類別，包含客戶資料
    /// </summary>
    public class CustomerResponse : BaseResponse
    {
        public CustomerDto Customer { get; set; } = new CustomerDto(); // 客戶資料
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>(); // 客戶資料列表
    }
}
