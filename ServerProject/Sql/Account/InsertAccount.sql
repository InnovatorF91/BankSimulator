-- 插入新账户记录并返回生成的 account_id
INSERT INTO "Accounts" (
    customer_id,
    account_type,
    balance,
    currency,
    status,
    open_date,
    close_date,
    update_date
)
VALUES (
    @CustomerId,
    @AccountType,
    @Balance,
    @Currency,
    @Status,
    @OpenDate,
    @CloseDate,
    @UpdateDate
)
RETURNING account_id;