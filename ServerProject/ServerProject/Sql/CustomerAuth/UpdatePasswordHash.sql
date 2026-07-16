-- 更新密碼哈希值語句
UPDATE "CustomerAuth"
			SET password_hash = @Hash,
			updated_at = @UpdateDate
			WHERE customer_id = @UserId AND is_deleted = false;