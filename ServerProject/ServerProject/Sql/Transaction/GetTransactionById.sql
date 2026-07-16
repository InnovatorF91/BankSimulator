-- 通過交易ID獲取交易信息
SELECT * FROM "Transactions" WHERE transaction_id = @TransactionId;
