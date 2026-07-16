-- 更新認證類型語句
UPDATE "CustomerAuth"
			SET auth_type = @AuthType,
			updated_at = @UpdateAt
			WHERE customer_id = @UserId AND is_deleted = false;