-- 提升令牌版本語句
UPDATE "CustomerAuth"
			SET token_version = token_version + 1,
			updated_at = @Now
			WHERE customer_id = @UserId AND is_deleted = false;