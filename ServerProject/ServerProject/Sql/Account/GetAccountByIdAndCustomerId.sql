-- 根据账户ID和客户ID取得账户。
SELECT
    account_id,
    customer_id,
    account_type,
    balance,
    currency,
    status,
    open_date,
    close_date,
    update_date
FROM "Accounts"
WHERE account_id = @AccountId
  AND customer_id = @CustomerId;