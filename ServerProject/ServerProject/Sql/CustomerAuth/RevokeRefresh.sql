-- 撤銷刷新令牌語句
UPDATE "RefreshToken"
			SET revoked_at = @Now
			WHERE token_id = @TokenId AND revoked_at IS NULL;