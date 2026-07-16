-- 根据操作日志ID查找该条日志
SELECT
    operation_log_id,
    account_id,
    customer_id,
    operation_type,
    old_status,
    new_status,
    reason,
    operated_by,
    operated_at
FROM "AccountOperationLogs"
WHERE operation_log_id = @OperationLogId;