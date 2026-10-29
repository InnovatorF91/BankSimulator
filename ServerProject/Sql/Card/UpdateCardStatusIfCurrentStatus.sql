-- 更新卡片狀態
UPDATE "Cards"
SET card_status = @NewStatus,
    update_at = @UpdatedAt
WHERE card_id = @CardId
  AND card_status = @CurrentStatus;