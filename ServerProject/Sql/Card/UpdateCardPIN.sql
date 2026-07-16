-- 更新卡片PIN碼
UPDATE "Cards"
            SET pin_hash = @PINHash
            WHERE card_id = @CardId;