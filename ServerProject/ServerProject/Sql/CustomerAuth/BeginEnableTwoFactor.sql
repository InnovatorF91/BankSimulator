-- 開始啟用雙重認證語句
UPDATE "CustomerAuth"
			SET
			two_factor_status = 1,two_factor_secret = @Secret,two_factor_pending_expires_at = @ExpiresAt,updated_at = @Now
			WHERE customer_id = @UserId
			AND is_deleted = false
			AND two_factor_status = 0;