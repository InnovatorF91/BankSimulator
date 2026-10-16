-- 通过组队ID查找相对应的交易详情
SELECT
    transaction_id,
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
FROM "Transactions"
WHERE group_id = @GroupId
ORDER BY create_at ASC, transaction_id ASC;