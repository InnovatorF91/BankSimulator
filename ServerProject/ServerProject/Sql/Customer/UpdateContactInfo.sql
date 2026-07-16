-- 更新客戶聯繫信息
UPDATE "Customers"
            SET address = @Address ,phone = @Phone , email = @Email,updated_at = @UpdateAt
            WHERE customer_id = @CustomerId AND is_deleted = false;