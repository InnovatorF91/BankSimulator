-- 通過交易ID獲取該交易的金額變化
SELECT amount_delta FROM "Transactions" WHERE transaction_id = @TransactionId;