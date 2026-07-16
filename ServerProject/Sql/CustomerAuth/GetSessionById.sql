-- 根據會話ID獲取會話信息
Select * from "CustomerSession" where session_id = @SessionId;