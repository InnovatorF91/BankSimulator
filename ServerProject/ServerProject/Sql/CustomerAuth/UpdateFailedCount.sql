-- 更新失敗次數語句
UPDATE "CustomerAuth"
			SET
			failed_count = failed_count + 1,
			updated_at   = @UpdateDate
			WHERE customer_id = @UserId
			AND is_deleted = false;