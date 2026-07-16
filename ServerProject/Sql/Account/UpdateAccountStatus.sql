-- 普通版更新账户状态
UPDATE "Accounts"
SET
    status = @NewStatus,
    close_date = CASE
        WHEN @NewStatus = 3 THEN @UpdatedAt
        ELSE close_date
    END,
    update_date = @UpdatedAt
WHERE account_id = @AccountId;