-- 更新客户的两步验证状态为禁用
UPDATE "CustomerAuth"
SET
    two_factor_status = 0,
    two_factor_secret = NULL,
    two_factor_pending_expires_at = NULL,
    two_factor_enabled_at = NULL,
    updated_at = @UpdatedAt
WHERE customer_id = @CustomerId
  AND is_deleted = FALSE;