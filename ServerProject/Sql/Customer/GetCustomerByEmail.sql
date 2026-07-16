-- 通過客戶電子郵件獲取客戶信息
SELECT * FROM "Customers" WHERE email = @Email AND is_deleted = false;