namespace ServerProject.Common
{
    /// <summary>
    /// 審計服務接口，定義了審計記錄的寫入、查詢和簽名功能
    /// </summary>
    public interface IAuditRepository
    {
        /// <summary>
        /// 寫入審計條目到審計系統
        /// </summary>
        /// <param name="entry">審計條目類</param>
        /// <returns>true:寫入成功/false:寫入失敗</returns>
        bool Write(AuditEntry entry);

        /// <summary>
        /// 查詢審計記錄
        /// </summary>
        /// <param name="query">審計條目</param>
        /// <returns>所有審計記錄的基本信息</returns>
        List<AuditRecord> Query(AuditQuery query);

        /// <summary>
        /// 追加簽名到審計記錄
        /// </summary>
        /// <param name="id">審計記錄ID</param>
        /// <returns>true:追加成功/false:追加失敗</returns>
        bool AppendSignature(long id);
    }
}
