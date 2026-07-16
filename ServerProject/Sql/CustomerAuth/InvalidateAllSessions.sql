-- 使所有會話無效語句
UPDATE "CustomerSession"
			SET is_valid = false, expired_at = @Now
			WHERE user_id = @UserId AND is_valid = true;