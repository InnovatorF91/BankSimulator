-- 根据TokenHash获取密码重置令牌信息
SELECT
    token_id,
    customer_id,
    token_hash,
    created_at,
    expires_at,
    used_at,
    created_by_ip,
    created_by_device
FROM "PasswordResetToken"
WHERE token_hash = @TokenHash;