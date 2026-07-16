-- 根據刷新令牌哈希值獲取刷新令牌信息
Select * from "RefreshToken" where token_hash = @RefreshTokenHash;