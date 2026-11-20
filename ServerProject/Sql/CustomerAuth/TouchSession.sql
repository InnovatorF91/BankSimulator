-- 更新會話的最後活動時間語句
UPDATE "CustomerSession"
SET
    last_seen_at = GREATEST(last_seen_at, @Now),
    expired_at = LEAST(
        created_at + @AbsoluteLifetime,
        GREATEST(last_seen_at, @Now) + @IdleTimeout
    )
WHERE session_id = @SessionId
  AND is_valid = true
  AND expired_at > @Now
  AND last_seen_at + @IdleTimeout > @Now
  AND created_at + @AbsoluteLifetime > @Now;