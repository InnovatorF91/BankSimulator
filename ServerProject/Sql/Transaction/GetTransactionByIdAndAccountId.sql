-- 通过交易ID和账户ID查找交易详情
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
WHERE transaction_id = @TransactionId
  AND account_id = @AccountId;