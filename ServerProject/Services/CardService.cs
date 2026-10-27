using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;

namespace ServerProject.Services
{
	public class CardService : ServiceBase, ICardService
	{
		/// <summary>
		/// 帳戶服務實例，用於處理帳戶相關的數據操作
		/// </summary>
		private readonly IAccountRepository _accountRepository;

		/// <summary>
		/// 客戶服務實例，用於處理客戶相關的操作
		/// </summary>
		private readonly ICustomerRepository _customerRepository;

		/// <summary>
		/// 交易服務實例，用於處理交易相關的操作
		/// </summary>
		private readonly ITransactionRepository _transactionRepository;

		/// <summary>
		/// 當前客戶實例，用於獲取當前登入的客戶信息
		/// </summary>
		private readonly ICurrentCustomer _currentCustomer;

		/// <summary>
		/// 時間提供者實例，用於獲取當前時間
		/// </summary>
		private readonly ITimeProvider _timeProvider;

		/// <summary>
		/// 卡片服務實例，用於處理卡片相關的操作
		/// </summary>
		private readonly ICardRepository _cardRepository;

		public CardService(IConnectionFactory connectionFactory,
			IAccountRepository accountRepository,
			ICustomerRepository customerRepository,
			ITransactionRepository transactionRepository,
			ITimeProvider timeProvider,
			ICurrentCustomer currentCustomer,
			ICardRepository cardRepository) : base(connectionFactory)
		{
			_accountRepository = accountRepository;
			_customerRepository = customerRepository;
			_transactionRepository = transactionRepository;
			_timeProvider = timeProvider;
			_currentCustomer = currentCustomer;
			_cardRepository = cardRepository;
		}

		/// <summary>
		/// 獲取當前登入客戶的未关闭账户下的卡片
		/// </summary>
		/// <returns>獲取我的卡片結果資料傳輸物件</returns>
		public async Task<GetMyCardsResultDto> GetMyCards()
		{
			// 從當前客戶獲取客戶ID
			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return GetMyCardsResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			try
			{
				// 验证当前客户是否存在
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);

				// 验证客户是否存在
				if (customer == null)
				{
					return GetMyCardsResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return GetMyCardsResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 获取当前客户的所有非关闭账户
				var accounts = await ExecuteDbAsync<List<AccountEntity>>(
							async dataAccess =>
							{
								_accountRepository.DataAccess = dataAccess;
								return await _accountRepository.GetNonClosedAccountsByCustomerId(customerId);
							}
						);

				// 获取所有账户的卡片信息
				var allCards = await ExecuteDbAsync<List<CardEntity>>(
					async dataAccess =>
					{
						_cardRepository.DataAccess = dataAccess;

						var result = new List<CardEntity>();

						foreach (var account in accounts)
						{
							var accountCards =
								await _cardRepository.GetCardsByAccountId(account.AccountId);

							result.AddRange(accountCards);
						}

						return result;
					}
				);

				// 将卡片实体转换为DTO，并按创建时间和卡片ID降序排序
				var cardDtos = allCards
					.OrderByDescending(card => card.CreatedAt)
					.ThenByDescending(card => card.CardId)
					.Select(card => new CardDto
					{
						CardId = card.CardId,
						AccountId = card.AccountId,
						MaskedCardNumber = MaskCardNumber(card.CardNumber),
						ExpiryYear = card.ExpiryYear,
						ExpiryMonth = card.ExpiryMonth,
						CardType = card.CardType,
						Status = card.Status
					})
					.ToList();

				return GetMyCardsResultDto.SuccessDto(cardDtos);
			}
			catch (UnableToOperateDBException)
			{
				// 捕获数据库异常，返回数据库操作错误
				return GetMyCardsResultDto.Failure((int)CardErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 遮蔽卡片號碼，只顯示最後四位數字
		/// </summary>
		/// <param name="cardNumber">卡片號碼</param>
		/// <returns>卡片最後四位數字</returns>
		private string MaskCardNumber(string? cardNumber)
		{
			if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length < 4)
			{
				return "****";
			}

			return $"****{cardNumber[^4..]}";
		}

	}

	public interface ICardService
	{
		/// <summary>
		/// 獲取當前登入客戶的未关闭账户下的卡片
		/// </summary>
		/// <returns>獲取我的卡片結果資料傳輸物件</returns>

		Task<GetMyCardsResultDto> GetMyCards();
	}
}
