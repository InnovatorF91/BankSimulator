using ServerProject.Common;
using ServerProject.DTOs;
using ServerProject.Entities;
using ServerProject.Repositories;
using ShareProject.Common;
using ShareProject.Request;

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

		/// <summary>
		/// 卡号生成实例，用于生成卡号
		/// </summary>
		private readonly ICardNumberGenerator _cardNumberGenerator;

		/// <summary>
		/// 加密服務實例，用於處理加密相關的操作
		/// </summary>
		private readonly ICryptoRepository _cryptoRepository;

		/// <summary>
		/// 连续验证失败次数，最多允许3次
		/// </summary>
		private const int MaxPinFailures = 3;

		/// <summary>
		/// Pin锁定时间，3次验证失败后，锁定15分钟
		/// </summary>
		private const int PinLockMinutes = 15;

		public CardService(IConnectionFactory connectionFactory,
			IAccountRepository accountRepository,
			ICustomerRepository customerRepository,
			ITransactionRepository transactionRepository,
			ITimeProvider timeProvider,
			ICurrentCustomer currentCustomer,
			ICardRepository cardRepository,
			ICryptoRepository cryptoRepository,
			ICardNumberGenerator cardNumberGenerator) : base(connectionFactory)
		{
			_accountRepository = accountRepository;
			_customerRepository = customerRepository;
			_transactionRepository = transactionRepository;
			_timeProvider = timeProvider;
			_currentCustomer = currentCustomer;
			_cardRepository = cardRepository;
			_cryptoRepository = cryptoRepository;
			_cardNumberGenerator = cardNumberGenerator;
		}

		/// <summary>
		/// 封鎖卡片
		/// </summary>
		/// <param name="request">封鎖卡片請求資料傳輸物件</param>
		/// <returns>封鎖卡片結果資料傳輸物件</returns>
		public async Task<BlockCardResultDto> BlockCard(BlockCardRequest request)
		{
			//检查 Request 和 CardId。
			if (request == null || request.CardId <= 0)
			{
				return BlockCardResultDto.Fail((int)CardErrorCode.ValidationFailed, "Invalid request or CardId.");
			}

			//获取当前客户 ID。
			int customerId;

			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return BlockCardResultDto.Fail((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
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
					return BlockCardResultDto.Fail((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}

				// 验证客户是否已被删除
				if (customer.IsDeleted)
				{
					return BlockCardResultDto.Fail((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 获取卡片信息
				var card = await ExecuteDbAsync<CardEntity?>(
					async dataAccess =>
					{
						_cardRepository.DataAccess = dataAccess;
						return await _cardRepository.GetCardById(request.CardId);
					}
				);

				// 验证卡片是否存在
				if (card == null)
				{
					return BlockCardResultDto.Fail((int)CardErrorCode.CardNotFoundOrAccessDenied, "The card does not exist or access is denied.");
				}

				var result = await ExecuteInTxAsync<BlockCardResultDto>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						_cardRepository.DataAccess = dataAccess;

						// 取得卡片所屬帳戶
						var account = await _accountRepository.GetAccountByIdAndCustomerIdForUpdate(card.AccountId, customerId);

						// 驗證帳戶是否存在且屬於當前客戶
						if (account == null || account.CustomerId != customerId)
						{
							return BlockCardResultDto.Fail((int)CardErrorCode.CardNotFoundOrAccessDenied,"The card does not exist or access is denied.");
						}

						// 验证账户状态是否为关闭，如果是，则返回账户已关闭错误
						if (account.Status == AccountStatus.Closed)
						{
							return BlockCardResultDto.Fail((int)AccountErrorCode.AccountAlreadyClosed,"The account has been closed.");
						}

						// 重新获取卡片信息以进行更新操作
						var lockedCard = await _cardRepository.GetCardByIdForUpdate(request.CardId);

						// 取得當前時間
						var now = _timeProvider.Now();

						// 验证卡片是否存在
						if (lockedCard == null)
						{
							return BlockCardResultDto.Fail((int)CardErrorCode.CardNotFoundOrAccessDenied, "The card does not exist or access is denied.");
						}

						// 验证卡片是否属于当前客户的账户
						if (lockedCard.AccountId != account.AccountId)
						{
							return BlockCardResultDto.Fail((int)CardErrorCode.CardNotFoundOrAccessDenied, "The card does not belong to the account.");
						}

						// 验证卡片状态是否已经被封锁，如果是，则直接返回成功结果
						if (lockedCard.Status == CardStatus.Blocked)
						{
							return BlockCardResultDto.SuccessDto(lockedCard.CardId,CardStatus.Blocked);
						}

						// 验证卡片是否已过期
						if (lockedCard.Status == CardStatus.Expired)
						{
							return BlockCardResultDto.Fail((int)CardErrorCode.CardExpired, "The card has expired.");
						}

						// 验证卡片状态是否允许操作
						if (lockedCard.Status != CardStatus.Active)
						{
							return BlockCardResultDto.Fail((int)CardErrorCode.CardStatusInvalid, "The card status does not allow this operation.");
						}

						// 验证卡片是否已过期
						var expired = lockedCard.ExpiryYear < now.Year || (lockedCard.ExpiryYear == now.Year && lockedCard.ExpiryMonth < now.Month);
						if (expired)
						{
							return BlockCardResultDto.Fail((int)CardErrorCode.CardExpired, "The card has expired.");
						}

						// 更新卡片状态为封锁
						var affectedRows = await _cardRepository.UpdateCardStatusIfCurrentStatus(lockedCard.CardId, CardStatus.Active, CardStatus.Blocked, now);

						// 如果更新失败，说明卡片状态已发生变化，返回冲突错误
						if (affectedRows == 0)
						{
							throw new CardOperationException (CardErrorCode.CardStateConflict, "The card status has changed, the update failed.");
						}

						// 返回成功结果
						return BlockCardResultDto.SuccessDto(lockedCard.CardId, CardStatus.Blocked);
					}
				);

				// 如果事务执行成功，返回结果
				return result;
			}
			catch (CardOperationException ex)
			{
				//发生异常则 Rollback
				return BlockCardResultDto.Fail((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				//发生异常则 Rollback
				return BlockCardResultDto.Fail((int)CardErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
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
		/// 發行卡片
		/// </summary>
		/// <param name="request">發行卡片請求資料傳輸物件</param>
		/// <returns>發行卡片結果資料傳輸物件</returns>
		public async Task<IssueCardResultDto> IssueCard(IssueCardRequest request)
		{
			//Request 非空。
			if (request == null)
			{
				return IssueCardResultDto.Failure((int)CardErrorCode.ValidationFailed, "Request cannot be null.");
			}

			//AccountId > 0。
			if (request.AccountId <= 0)
			{
				return IssueCardResultDto.Failure((int)CardErrorCode.ValidationFailed, "AccountId must be greater than 0.");
			}

			//CardType 是已定义枚举值。
			if (!Enum.IsDefined(typeof(CardType), request.CardType))
			{
				return IssueCardResultDto.Failure((int)CardErrorCode.InvalidCardType, "Invalid CardType.");
			}

			//PIN 恰好四位，而且每位都在 '0'～`'9'` 之间。
			if (string.IsNullOrWhiteSpace(request.PIN) || request.PIN.Length != 4 || !request.PIN.All(c => c >= '0' && c <= '9'))
			{
				return IssueCardResultDto.Failure((int)CardErrorCode.InvalidPIN, "PIN must be exactly 4 digits.");
			}

			//获取当前客户 ID。
			int customerId;
			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return IssueCardResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			try
			{
				// 确认客户存在、未被删除。
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);
				if (customer == null)
				{
					return IssueCardResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}
				if (customer.IsDeleted)
				{
					return IssueCardResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 计算PIN的哈希值。
				var pinHash = _cryptoRepository.Hash(request.PIN,HashProfile.CardPIN);

				// 进入事务，执行发卡操作。
				var result = await ExecuteInTxAsync<IssueCardResultDto>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						_cardRepository.DataAccess = dataAccess;
						// 确认账户存在、属于当前客户、未关闭。
						var account = await _accountRepository.GetAccountByIdAndCustomerIdForUpdate(request.AccountId, customerId);
						if (account == null || account.CustomerId != customerId)
						{
							return IssueCardResultDto.Failure((int)AccountErrorCode.AccountAccessDenied, "The account does not exist or access is denied.");
						}
						if (account.Status != AccountStatus.Active)
						{
							return IssueCardResultDto.Failure((int)AccountErrorCode.AccountNotAvailable,"The account is not available for card issuance.");
						}

						// 取得当前时间
						var now = _timeProvider.Now();

						// 检查是否存在同类型未过期的 Active／Blocked 卡
						var count = await _cardRepository.CountUsableCardsByAccountIdAndType(account.AccountId,request.CardType,(short)now.Year,(short)now.Month);
						if (count > 0)
						{
							return IssueCardResultDto.Failure((int)CardErrorCode.CardAlreadyExists,"An unexpired card of this type already exists.");
						}

						// 生成卡片过期年月
						var expiry = now.AddYears(3);

						// 生成卡号
						var cardNumber = _cardNumberGenerator.Generate();

						// 对卡片数据进行构造
						var card = new CardEntity
						{
							AccountId = account.AccountId,
							CardNumber = cardNumber,

							ExpiryYear = (short)expiry.Year,
							ExpiryMonth = (short)expiry.Month,

							PINHash = pinHash,
							PINFailCount = 0,
							PINLockedUntil = null,

							CardType = request.CardType,
							Status = CardStatus.Active,

							DeactivatedAt = null,
							ReplacedBy = null,

							CreatedAt = now,
							UpdatedAt = now
						};

						var cardId = await _cardRepository.InsertNewCard(card);

						// 如果返回的卡片ID不大于0，则抛出异常
						if (cardId <= 0)
						{
							throw new CardOperationException(CardErrorCode.CardCreationFailed,"Failed to create the card.");
						}
						card.CardId = cardId;

						// 将卡片实体转换为DTO
						var cardDto = new CardDto()
						{
							CardId = card.CardId,
							AccountId = card.AccountId,
							MaskedCardNumber = MaskCardNumber(card.CardNumber),
							ExpiryYear = card.ExpiryYear,
							ExpiryMonth = card.ExpiryMonth,
							CardType = card.CardType,
							Status = card.Status
						};

						return IssueCardResultDto.SuccessDto(cardDto);
					});

				return result;
			}
			catch (CardNumberConflictException)
			{
				return IssueCardResultDto.Failure((int)CardErrorCode.CardNumberConflict,"The generated card number already exists.");
			}
			catch (CardOperationException ex)
			{
				//发生异常则 Rollback
				return IssueCardResultDto.Failure((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				//发生异常则 Rollback
				return IssueCardResultDto.Failure((int)CardErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
			}
		}

		/// <summary>
		/// 修改卡片PIN值
		/// </summary>
		/// <param name="request">修改卡片的请求资料传输物件</param>
		/// <returns>修改卡片PIN的结果Dto</returns>
		public async Task<ChangeCardPinResultDto> ChangeCardPin(ChangeCardPinRequest request)
		{
			// Request 非空，`CardId > 0`。
			if (request == null)
			{
				return ChangeCardPinResultDto.Failure((int)CardErrorCode.ValidationFailed, "Request cannot be null.");
			}
			if (request.CardId <= 0)
			{
				return ChangeCardPinResultDto.Failure((int)CardErrorCode.ValidationFailed,"CardId must be greater than 0.");
			}
			// CurrentPIN、`NewPIN` 均为四位数字。
			if (string.IsNullOrWhiteSpace(request.CurrentPIN) || request.CurrentPIN.Length != 4 || !request.CurrentPIN.All(c => c >= '0' && c <= '9'))
			{
				return ChangeCardPinResultDto.Failure((int)CardErrorCode.InvalidPIN,"Current PIN must be exactly 4 digits.");
			}
			if (string.IsNullOrWhiteSpace(request.NewPIN) || request.NewPIN.Length != 4 || !request.NewPIN.All(c => c >= '0' && c <= '9'))
			{
				return ChangeCardPinResultDto.Failure((int)CardErrorCode.InvalidPIN,"New PIN must be exactly 4 digits.");
			}

			//获取当前客户 ID。
			int customerId;
			try
			{
				customerId = _currentCustomer.CustomerId;
			}
			catch (InvalidOperationException)
			{
				// 如果獲取客戶ID失敗，返回未授權訪問錯誤
				return ChangeCardPinResultDto.Failure((int)CustomerErrorCode.UnauthorizedAccess, "Failed to retrieve the current customer ID.");
			}

			try
			{
				// 确认客户存在、未被删除。
				var customer = await ExecuteDbAsync<CustomerEntity?>(
					async dataAccess =>
					{
						_customerRepository.DataAccess = dataAccess;
						return await _customerRepository.GetCustomerById(customerId);
					}
				);
				if (customer == null)
				{
					return ChangeCardPinResultDto.Failure((int)CustomerErrorCode.CustomerNotFound, "The customer does not exist.");
				}
				if (customer.IsDeleted)
				{
					return ChangeCardPinResultDto.Failure((int)CustomerErrorCode.InvalidCustomerData, "The customer has been deleted.");
				}

				// 使用 GetCardById 查询卡片
				var card = await ExecuteDbAsync<CardEntity?>(async dataAccess =>
				{
					_cardRepository.DataAccess = dataAccess;
					return await _cardRepository.GetCardById(request.CardId);
				});

				// 如果卡片内容为空，则返回未找到卡片或访问被拒绝。
				if (card == null)
				{
					return ChangeCardPinResultDto.Failure(
						(int)CardErrorCode.CardNotFoundOrAccessDenied,
						"The card does not exist or access is denied.");
				}

				var result = await ExecuteInTxAsync<ChangeCardPinResultDto>(
					async dataAccess =>
					{
						_accountRepository.DataAccess = dataAccess;
						_cardRepository.DataAccess = dataAccess;

						// 确认账户存在、属于当前客户、未关闭。
						var account = await _accountRepository.GetAccountByIdAndCustomerIdForUpdate(card.AccountId, customerId);
						if (account == null || account.CustomerId != customerId)
						{
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.CardNotFoundOrAccessDenied,"The card does not exist or access is denied.");
						}
						if (account.Status != AccountStatus.Active)
						{
							return ChangeCardPinResultDto.Failure((int)AccountErrorCode.AccountNotAvailable, "The account is not available for PIN changes.");
						}

						// 重新读取并锁定卡片。
						var lockedCard = await _cardRepository.GetCardByIdForUpdate(request.CardId);

						// 卡片不存在，或已不属于刚才锁定的账户。
						if (lockedCard == null || lockedCard.AccountId != account.AccountId)
						{
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.CardNotFoundOrAccessDenied,"The card does not exist or access is denied.");
						}

						// 卡片已标记为过期。
						if (lockedCard.Status == CardStatus.Expired)
						{
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.CardExpired,"The card has expired.");
						}

						// 只有 Active 卡片允许修改 PIN。
						if (lockedCard.Status != CardStatus.Active)
						{
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.CardStatusInvalid,"The card status does not allow PIN changes.");
						}

						var now = _timeProvider.Now();

						// 状态仍为 Active，但实际有效期已经过去。
						var expired = lockedCard.ExpiryYear < now.Year || (lockedCard.ExpiryYear == now.Year && lockedCard.ExpiryMonth < now.Month);
						if (expired)
						{
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.CardExpired,"The card has expired.");
						}

						// 检查临时锁定
						if (lockedCard.PINLockedUntil.HasValue && lockedCard.PINLockedUntil.Value > now)
						{
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.PINLocked,"PIN verification is temporarily locked.");
						}

						// 确定本轮已有失败次数。
						// 有锁定时间但走到这里，说明锁定已经到期，从0重新计算。
						// 没有锁定时间，则继续累计已有失败次数。
						var failCount = lockedCard.PINLockedUntil.HasValue ? 0 : (int)lockedCard.PINFailCount;

						// 验证旧PIN：第一个参数为哈希，第二个参数为用户输入。
						var verified = _cryptoRepository.Verify(lockedCard.PINHash,request.CurrentPIN);

						// 旧PIN错误，更新失败次数及锁定时间。
						if (!verified)
						{
							var newFailCount = (short)Math.Min(failCount + 1,MaxPinFailures);

							DateTime? lockedUntil = newFailCount >= MaxPinFailures ? now.AddMinutes(PinLockMinutes) : null;

							var affectedRows = await _cardRepository.UpdatePINFailCount(lockedCard.CardId,newFailCount,now,lockedUntil);

							// 保存失败则抛出异常，由ServiceBase回滚事务。
							if (affectedRows != 1)
							{
								throw new CardOperationException(CardErrorCode.PINUpdateFailed,"Failed to update PIN verification data.");
							}

							// 达到失败次数上限，返回锁定错误。
							if (lockedUntil.HasValue)
							{
								return ChangeCardPinResultDto.Failure((int)CardErrorCode.PINLocked,"PIN verification is temporarily locked.");
							}

							// 未达到上限，返回旧PIN错误。
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.CurrentPINIncorrect,"The current PIN is incorrect.");
						}

						// 旧PIN验证成功，清零失败次数和锁定时间。
						var resetRows = await _cardRepository.UpdatePINFailCount(lockedCard.CardId,0,now,null);

						if (resetRows != 1)
						{
							throw new CardOperationException(CardErrorCode.PINUpdateFailed,"Failed to reset PIN verification data.");
						}

						// 新旧PIN相同，不修改PIN。
						// 正常返回，让事务提交前面的失败次数重置。
						if (request.CurrentPIN == request.NewPIN)
						{
							return ChangeCardPinResultDto.Failure((int)CardErrorCode.NewPINUnchanged,"The new PIN must differ from the current PIN.");
						}

						// 生成新PIN的哈希。
						var newHash = _cryptoRepository.Hash(request.NewPIN,HashProfile.CardPIN);

						// 更新PIN哈希。
						var updatedRows = await _cardRepository.UpdateCardPIN(lockedCard.CardId,newHash,now);

						if (updatedRows != 1)
						{
							// 抛出异常，PIN更新与前面的失败次数重置一起回滚。
							throw new CardOperationException(CardErrorCode.PINUpdateFailed,"Failed to update the card PIN.");
						}

						// 返回成功，由ExecuteInTxAsync提交事务。
						return ChangeCardPinResultDto.SuccessDto(lockedCard.CardId);
					});

				return result;
			}
			catch (CardOperationException ex)
			{
				//发生异常则 Rollback
				return ChangeCardPinResultDto.Failure((int)ex.ErrorCode, ex.Message);
			}
			catch (UnableToOperateDBException)
			{
				//发生异常则 Rollback
				return ChangeCardPinResultDto.Failure((int)CardErrorCode.UnableToOperateDb, "A database error occurred during the operation.");
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

		/// <summary>
		/// 封鎖卡片
		/// </summary>
		/// <param name="request">封鎖卡片請求資料傳輸物件</param>
		/// <returns>封鎖卡片結果資料傳輸物件</returns>
		Task<BlockCardResultDto> BlockCard(BlockCardRequest request);

		/// <summary>
		/// 發行卡片
		/// </summary>
		/// <param name="request">發行卡片請求資料傳輸物件</param>
		/// <returns>發行卡片結果資料傳輸物件</returns>
		Task<IssueCardResultDto> IssueCard(IssueCardRequest request);

		/// <summary>
		/// 修改卡片PIN值
		/// </summary>
		/// <param name="request">修改卡片的请求资料传输物件</param>
		/// <returns>修改卡片PIN的结果Dto</returns>
		Task<ChangeCardPinResultDto> ChangeCardPin(ChangeCardPinRequest request);
	}
}
