-- 通過客戶ID獲取客戶信息（锁定）
SELECT *
FROM "Customers"
WHERE customer_id = @Id
FOR UPDATE;