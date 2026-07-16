-- 通過客戶ID獲取客戶信息
SELECT * FROM "Customers" WHERE customer_id = @Id AND is_deleted = false;