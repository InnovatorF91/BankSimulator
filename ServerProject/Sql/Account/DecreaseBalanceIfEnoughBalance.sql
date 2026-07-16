-- 在账户余额足够的情况下，减少账户余额
UPDATE "Accounts"
SET
    balance = balance - @Amount,
    update_date = @UpdatedAt
WHERE account_id = @AccountId
  AND status = 1
  AND balance >= @Amount
  AND @Amount > 0;