-- 通過帳戶ID獲取該帳戶的所有卡片
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
WHERE account_id = @AccountId
ORDER BY create_at DESC, card_id DESC;