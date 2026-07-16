-- 通過帳戶ID獲取該帳戶的所有交易記錄
SELECT * FROM "Transactions" WHERE account_id = @AccountId;