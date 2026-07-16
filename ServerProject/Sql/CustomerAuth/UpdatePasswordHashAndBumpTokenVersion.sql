-- 更新密码哈希并增加令牌版本
UPDATE customer_auth
SET
    password_hash = @PasswordHash,
    token_version = token_version + 1,
    updated_at = @Now
WHERE
    customer_id = @CustomerId;