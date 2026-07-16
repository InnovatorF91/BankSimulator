-- 刪除客戶認證信息
UPDATE "CustomerAuth"
            SET deleted_at = @DeletedAt, is_deleted = @IsDeleted
            WHERE customer_id = @CustomerId AND is_deleted = false;