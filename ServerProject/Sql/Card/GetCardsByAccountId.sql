-- 通過帳戶ID獲取該帳戶的所有卡片
SELECT * FROM "Cards" WHERE account_id = @AccountId AND card_status = 0;