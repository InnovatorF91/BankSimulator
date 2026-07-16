-- 刪除客戶信息
UPDATE "Customers"
            SET  deleted_at = @DeletedAt, is_deleted = @IsDeleted, deleted_reason = @DeletedReason
            WHERE customer_id = @CustomerId AND is_deleted = false;