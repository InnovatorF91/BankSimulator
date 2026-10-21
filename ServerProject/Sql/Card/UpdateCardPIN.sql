-- 更新卡片PIN碼
UPDATE "Cards"
            SET pin_hash = @PINHash, update_at = @UpdatedAt
            WHERE card_id = @CardId;