-- 通過帳戶ID獲取該帳戶的所有交易記錄
SELECT
    transaction_id,
    account_id,
    transaction_type,
    amount_delta,
    related_account,
    create_at,
    status,
    note,
    group_id,
    currency,
    balance_before,
    balance_after
FROM "Transactions"
WHERE account_id = @AccountId
  AND (@From IS NULL OR create_at >= @From)
  AND (@To IS NULL OR create_at < @To)
  AND (@TransactionType IS NULL OR transaction_type = @TransactionType)
  AND (@Status IS NULL OR status = @Status)
ORDER BY create_at DESC, transaction_id DESC;