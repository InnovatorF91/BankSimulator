-- 检查账户ID和卡类型的可用卡数量
SELECT COUNT(*)
FROM "Cards"
WHERE account_id = @AccountId
  AND card_type = @CardType
  AND card_status IN (@ActiveStatus, @BlockedStatus)
  AND (
      expiry_year > @CurrentYear
      OR (
          expiry_year = @CurrentYear
          AND expiry_month >= @CurrentMonth
      )
  );