-- 插入新的交易記錄
INSERT INTO "Transactions"
(
    account_id,
    transaction_type,
    amount_delta,
    related_account,
    create_at,
    status,
    note,
    group_id,
    currency,
    balance_before,
    balance_after
)
VALUES
(
    @AccountId,
    @TransactionType,
    @AmountDelta,
    @RelatedAccount,
    @CreateAt,
    @Status,
    @Note,
    @GroupId,
    @Currency,
    @BalanceBefore,
    @BalanceAfter
)
RETURNING transaction_id;