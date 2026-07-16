-- 解除鎖定語句
UPDATE "CustomerAuth"
			SET locked_until = NULL,
			failed_count = 0,
			updated_at = @UpdateDate
			WHERE customer_id = @UserId AND is_deleted = false;