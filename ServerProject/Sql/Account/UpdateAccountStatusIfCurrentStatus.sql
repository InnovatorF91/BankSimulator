-- 仅当前状态为 @CurrentStatus 时才更新账户状态为 @NewStatus
UPDATE "Accounts"
SET
    status = @NewStatus,
    close_date = CASE
        WHEN @NewStatus = 3 THEN @UpdatedAt
        ELSE close_date
    END,
    update_date = @UpdatedAt
WHERE account_id = @AccountId
  AND customer_id = @CustomerId
  AND status = @CurrentStatus
  AND (@RequireZeroBalance = false OR balance = 0);