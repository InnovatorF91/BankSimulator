-- 更新卡片狀態
UPDATE "Cards"
            SET card_status = @Status, deactivated_at = @DeactivatedAt, replaced_by = @ReplacedBy
            WHERE card_id = @CardId;