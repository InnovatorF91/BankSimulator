-- 更新PIN碼失敗次數及鎖定時間
UPDATE "Cards"
            SET pin_fail_count = @PINFailCount, pin_locked_until = @PINLockedUntil
            WHERE card_id = @CardId;