-- 根据令牌ID标记密码重置令牌为已使用
UPDATE "PasswordResetToken"
SET used_at = @UsedAt
WHERE token_id = @TokenId
  AND used_at IS NULL;