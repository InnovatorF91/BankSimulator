-- 撤销所有未使用且未过期的重置密码令牌
UPDATE "PasswordResetToken"
SET used_at = @Now
WHERE customer_id = @CustomerId
  AND used_at IS NULL
  AND expires_at > @Now;