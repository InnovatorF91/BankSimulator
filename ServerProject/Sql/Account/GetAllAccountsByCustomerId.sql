-- 根据客户ID取得该客户名下所有账户。
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
WHERE customer_id = @CustomerId
ORDER BY open_date DESC, account_id DESC;