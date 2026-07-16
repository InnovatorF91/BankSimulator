-- 根據客戶ID獲取客戶認證信息
SELECT * FROM "CustomerAuth" WHERE customer_id = @CustomerId AND is_deleted = false;