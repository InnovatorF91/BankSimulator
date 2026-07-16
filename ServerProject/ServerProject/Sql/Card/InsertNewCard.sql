-- 插入新卡片
INSERT INTO "Cards"
            (account_id, card_number, expiry_year, expiry_month, pin_hash, pin_fail_count, pin_locked_until, card_type, card_status, create_at)
            VALUES
            (@AccountId, @CardNumber, @ExpiryYear, @ExpiryMonth, @PINHash, @PINFailCount, @PINLockedUntil, @CardType, @Status, @CreateAt)
            RETURNING card_id;