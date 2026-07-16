-- 更新客戶KYC狀態
UPDATE "Customers"
            SET kyc_status = @KYCStatus , updated_at = @UpdateAt
            WHERE customer_id = @CustomerId AND is_deleted = false;