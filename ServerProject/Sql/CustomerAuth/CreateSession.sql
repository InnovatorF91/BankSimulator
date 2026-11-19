-- 創建新會話語句
INSERT INTO "CustomerSession"
    (user_id, created_at, last_seen_at, expired_at, is_valid, device, ip)
VALUES
    (@UserId, @Now, @Now, @ExpiredAt, true, @Device, @Ip)
RETURNING session_id;