-- 檢查帳戶是否有任何活躍的卡片
SELECT COUNT(1) FROM "Cards"
            WHERE account_id = @AccountId AND card_status = 0;