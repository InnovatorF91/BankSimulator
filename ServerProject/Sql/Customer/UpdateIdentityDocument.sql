-- 更新客戶身份證明文件信息
UPDATE "Customers"
            SET id_type = @IdType ,id_number = @IdNumber ,updated_at = @UpdateAt
            WHERE customer_id = @CustomerId AND is_deleted = false;