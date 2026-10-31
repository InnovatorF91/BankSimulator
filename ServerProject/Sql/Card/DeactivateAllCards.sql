-- 停用账户下尚未停用的卡片，保留已有停用时间。
UPDATE "Cards"
SET card_status = @Status,
    deactivated_at = COALESCE(deactivated_at, @DeactivatedAt),
    update_at = @UpdatedAt
WHERE account_id = @AccountId
  AND card_status <> @Status;