-- 插入一条新的账户操作日志
INSERT INTO "AccountOperationLogs"
(
    account_id,
    customer_id,
    operation_type,
    old_status,
    new_status,
    reason,
    operated_by,
    operated_at
)
VALUES
(
    @AccountId,
    @CustomerId,
    @OperationType,
    @OldStatus,
    @NewStatus,
    @Reason,
    @OperatedBy,
    @OperatedAt
)
RETURNING operation_log_id;