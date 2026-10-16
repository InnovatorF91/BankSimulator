-- 根据账户ID取得账户，并锁定该账户记录以便更新
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
FOR UPDATE;