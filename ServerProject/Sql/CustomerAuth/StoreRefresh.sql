-- 存儲刷新令牌語句
INSERT INTO "RefreshToken"
			(user_id, token_hash, issued_at, expires_at, meta_device, meta_ip,token_version)
			VALUES
			(@UserId, @TokenHash, @IssuedAt, @ExpiresAt, @Device, @Ip,@TokenVersion)
			RETURNING token_id;