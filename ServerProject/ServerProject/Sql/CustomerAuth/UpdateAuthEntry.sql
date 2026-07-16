-- 更新客戶認證信息
UPDATE "CustomerAuth"
            SET login_id = @LoginId, password_hash = @PasswordHash,
			 updated_at = @UpdateAt , auth_type = @AuthType
            WHERE customer_id = @CustomerId AND is_deleted = false;