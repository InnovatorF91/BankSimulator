-- 更新會話的最後活動時間語句
UPDATE "CustomerSession"
SET
    last_seen_at = @Now,
    expired_at   = @NewExpiredAt
WHERE
    session_id = @SessionId
    AND is_valid = true
    AND expired_at > @Now;