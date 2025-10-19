using Microsoft.AspNetCore.Mvc;
using ServerProject.Services;
using ShareProject.Common;
using ShareProject.Request;
using ShareProject.Response;

namespace ServerProject.Controllers
{
    /// <summary>
    /// 客戶控制器，處理客戶相關的API請求
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        /// <summary>
        /// 客戶邏輯接口，用於處理客戶相關的業務邏輯
        /// </summary>
        private readonly ICustomerService _customerService;

        /// <summary>
        /// 客戶控制器構造函數，注入客戶邏輯接口
        /// </summary>
        /// <param name="customerService">客戶邏輯接口</param>
        public CustomerController(ICustomerService customerService)
        {
            // 注入客戶邏輯接口
            _customerService = customerService;
        }

        /// <summary>
        /// 獲取客戶信息的API端點
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>客戶回應</returns>
        [HttpPost("GetCustomer")]
        public async Task<CustomerResponse> GetCustomer(CustomerRequest customerRequest)
        {
            // 執行客戶邏輯以獲取客戶信息
            var result = new CustomerResponse
            {
                Customer = await _customerService.GetCustomerInfo(customerRequest.Customer.CustomerId),
            };

            // 檢查是否成功獲取客戶信息
            if (result.Customer == null)
            {
                // 客戶信息為空，設置失敗狀態
                result.Success = false;
                result.Message = "Customer not found.";
            }
            else
            {
                // 客戶信息存在，設置成功狀態
                result.Success = true;
                result.Message = "Customer retrieved successfully.";
            }

            // 返回客戶回應
            return result;
        }

        /// <summary>
        /// 獲取所有客戶信息的API端點
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>客戶回應</returns>
        [HttpPost("GetCustomers")]
        public async Task<CustomerResponse> GetCustomers(CustomerRequest customerRequest)
        {
            // 執行客戶邏輯以列出所有客戶信息
            var result = new CustomerResponse
            {
                Customers = await _customerService.ListAllCustomers(),
            };

            // 檢查是否成功獲取客戶列表
            if (result.Customers == null || !result.Customers.Any())
            {
                result.Success = false;
                result.Message = "No customers found.";
            }
            else
            {
                // 客戶列表存在，設置成功狀態
                result.Success = true;
                result.Message = "Customers retrieved successfully.";
            }

            // 返回客戶回應
            return result;
        }

        /// <summary>
        /// 插入新客戶的API端點
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>客戶回應</returns>
        [HttpPost("InsertNewCustomer")]
        public async Task<CustomerResponse> InsertNewCustomer(CustomerRequest customerRequest)
        {
            // 執行客戶邏輯以註冊新客戶
            var result = await _customerService.RegisterCustomer(customerRequest);
            if (result)
            {
                // 插入成功，返回成功回應
                return new CustomerResponse
                {
                    Success = true,
                    Message = "Customer inserted successfully.",
                };
            }
            else
            {
                // 插入失敗，返回失敗回應
                return new CustomerResponse
                {
                    Success = false,
                    Message = "Failed to insert customer.",
                    Errors = ["An error occurred while inserting the customer."]
                };
            }
        }

        /// <summary>
        /// 刪除客戶的API端點
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>客戶回應</returns>
        [HttpPost("DeleteCustomer")]
        public async Task<CustomerResponse> DeleteCustomer(CustomerRequest customerRequest)
        {
            // 執行客戶邏輯以刪除客戶
            var result = await _customerService.RemoveCustomer(customerRequest);
            if (result)
            {
                // 刪除成功，設置成功狀態
                return new CustomerResponse
                {
                    Success = true,
                    Message = "Customer deleted successfully.",
                };
            }
            else
            {
                // 刪除失敗，設置失敗狀態
               return new CustomerResponse
               {
                   Success = false,
                   Message = "Failed to delete customer.",
                   Errors = ["An error occurred while deleting the customer."]
               };
            }
        }

        /// <summary>
        /// 更新客戶信息的API端點
        /// </summary>
        /// <param name="customerRequest">客戶請求</param>
        /// <returns>客戶回應</returns>
        [HttpPost("UpdateCustomer")]
        public async Task<CustomerResponse> UpdateCustomer(CustomerRequest customerRequest)
        {
            // 執行客戶邏輯以修改客戶信息
            var result = await _customerService.ModifyCustomer(customerRequest);
            if (result)
            {
                // 更新成功，返回成功回應
                return new CustomerResponse
                {
                    Success = true,
                    Message = "Customer updated successfully.",
                };
            }
            else
            {
                // 更新失敗，返回失敗回應
                return new CustomerResponse
                {
                    Success = false,
                    Message = "Failed to update customer.",
                    Errors = ["An error occurred while updating the customer."]
                };
            }
        }

		/// <summary>
		/// 更新客戶KYC狀態的API端點
		/// </summary>
		/// <param name="customerRequest">客戶請求</param>
		/// <returns>客戶回應</returns>
		[HttpPost("UpdateCustomerKycStatus")]
		public async Task<CustomerResponse> UpdateCustomerKycStatus(CustomerRequest customerRequest)
		{
			// KYCStatusがnullの場合のデフォルト値を設定（例: Unreviewed）
			var kycStatus = customerRequest.Customer.KYCStatus ?? KYCStatus.Unreviewed;
			var result = await _customerService.UpdateKycStatus(customerRequest.Customer.CustomerId, kycStatus);
			if (result)
			{
				// 更新成功，返回成功回應
				return new CustomerResponse
				{
					Success = true,
					Message = "Customer KYC status updated successfully.",
				};
			}
			else
			{
				// 更新失敗，返回失敗回應
				return new CustomerResponse
				{
					Success = false,
					Message = "Failed to update customer KYC status.",
					Errors = ["An error occurred while updating the customer KYC status."]
				};
			}
		}
	}
}
