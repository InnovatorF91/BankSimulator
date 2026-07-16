-- 根据账户ID查找所有相关的账户操作日志记录
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
WHERE account_id = @AccountId
ORDER BY operated_at DESC, operation_log_id DESC;