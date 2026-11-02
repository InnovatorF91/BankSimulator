-- 停用卡片以進行替換操作
UPDATE "Cards"
SET card_status = @InactiveStatus,
    deactivated_at = @DeactivatedAt,
    replaced_by = @NewCardId,
    update_at = @UpdatedAt
WHERE card_id = @CardId
  AND card_status = @CurrentStatus
  AND replaced_by IS NULL
  AND deactivated_at IS NULL;