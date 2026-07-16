-- 確認啟用雙重認證語句
UPDATE "CustomerAuth"
			SET
			two_factor_status = 2, two_factor_enabled_at = @Now, two_factor_pending_expires_at = NULL, updated_at = @Now
			WHERE customer_id = @UserId
			AND is_deleted = false
			AND two_factor_status = 1
			AND two_factor_secret IS NOT NULL
			AND two_factor_pending_expires_at >= @Now;