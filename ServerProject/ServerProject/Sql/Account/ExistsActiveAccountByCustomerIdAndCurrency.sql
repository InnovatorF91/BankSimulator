-- 检查客户是否已经拥有同币种的有效账户。
SELECT EXISTS (
    SELECT 1
    FROM "Accounts"
    WHERE customer_id = @CustomerId
      AND currency = @Currency
      AND status <> 3
);