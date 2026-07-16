-- 撤銷所有刷新令牌語句
UPDATE "RefreshToken"
			SET revoked_at = @Now
			WHERE user_id = @UserId AND revoked_at IS NULL;