-- 通過客戶電話號碼獲取客戶信息
SELECT * FROM "Customers" WHERE phone = @Phone AND is_deleted = false;