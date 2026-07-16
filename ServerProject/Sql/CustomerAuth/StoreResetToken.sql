-- 存储密码重置令牌的SQL脚本
INSERT INTO "PasswordResetToken"
(
    customer_id,
    token_hash,
    created_at,
    expires_at,
    used_at,
    created_by_ip,
    created_by_device
)
VALUES
(
    @CustomerId,
    @TokenHash,
    @CreatedAt,
    @ExpiresAt,
    NULL,
    @CreatedByIp,
    @CreatedByDevice
)
RETURNING token_id;