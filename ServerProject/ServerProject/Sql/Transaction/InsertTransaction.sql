-- 插入新的交易記錄
INSERT INTO "Transactions"
            (account_id, transaction_type, amount_delta, related_account, create_at, status, note)
            VALUES
            (@AccountId, @TransactionType, @AmountDelta, @RelatedAccount, @CreateAt, @Status, @Note)
            RETURNING transaction_id;