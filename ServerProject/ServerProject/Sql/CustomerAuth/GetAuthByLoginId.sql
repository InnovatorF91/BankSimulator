-- 根據登入ID獲取客戶認證信息
SELECT * FROM "CustomerAuth" WHERE login_id = @LoginId AND is_deleted = false;