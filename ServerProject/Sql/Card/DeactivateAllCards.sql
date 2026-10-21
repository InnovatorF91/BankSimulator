-- 停用帳戶下的所有卡片
UPDATE "Cards"
            SET card_status = @Status, deactivated_at = @DeactivatedAt, update_at = @UpdatedAt
            WHERE account_id = @AccountId AND card_status != @Status;