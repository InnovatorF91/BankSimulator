-- 插入新的客戶信息
INSERT INTO "Customers"
            (name, gender,birth_date,id_type,id_number,address,phone,email,kyc_status,create_at)
            VALUES
            (@Name, @Gender,@BirthDate,@IdType,@IdNumber,@Address,@Phone,@Email,@KYCStatus,@CreateAt)
            RETURNING customer_id;