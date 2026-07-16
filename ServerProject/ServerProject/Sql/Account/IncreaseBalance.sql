-- 增加账户余额的
UPDATE "Accounts"
SET
    balance = balance + @Amount,
    update_date = @UpdatedAt
WHERE account_id = @AccountId
  AND status = 1
  AND @Amount > 0;