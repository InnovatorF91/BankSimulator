-- 通過卡片ID獲取卡片信息(带锁)
SELECT
    card_id,
    account_id,
    card_number,
    expiry_year,
    expiry_month,
    pin_hash,
    pin_fail_count,
    pin_locked_until,
    card_type,
    card_status AS "Status",
    deactivated_at,
    replaced_by,
    create_at AS "CreatedAt",
    update_at AS "UpdatedAt"
FROM "Cards"
WHERE card_id = @CardId
FOR UPDATE;