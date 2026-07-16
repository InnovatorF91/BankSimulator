-- 更新客戶基本資料
UPDATE "Customers"
            SET name = @Name ,gender = @Gender, birth_date = @BirthDate ,updated_at = @UpdateAt
            WHERE customer_id = @CustomerId AND is_deleted = false;