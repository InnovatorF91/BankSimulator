-- 使會話無效語句
UPDATE "CustomerSession"
SET
    is_valid    = false,
    expired_at  = @Now,
    last_seen_at = COALESCE(last_seen_at, @Now)
WHERE
    session_id = @SessionId
    AND is_valid = true;