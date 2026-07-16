-- 鎖定帳號語句
UPDATE "CustomerAuth"
			SET locked_until = @LockedUntil,
			updated_at = @UpdateDate
			WHERE customer_id = @UserId AND is_deleted = false;