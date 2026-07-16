-- 插入新的客戶認證信息
INSERT INTO "CustomerAuth"
            (customer_id, login_id, password_hash, created_at)
            VALUES
            (@CustomerId, @LoginId, @PasswordHash,@CreateAt);