-- 根据客户ID取得该客户名下未关闭账户。
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
  AND status <> 3
ORDER BY open_date DESC, account_id DESC;