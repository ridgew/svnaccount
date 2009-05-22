using System.Collections;
using System.Data;

namespace FIPFAPI.SqlDb
{
    /// <summary>
    /// 数据库操作API
    /// </summary>
    public interface IDatabaseAPI
    {
        #region 数据库
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        /// <returns></returns>
        ApiResult CreateDatabase(string dbName);

        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        /// <returns></returns>
        ApiResult RemoveDatabase(string dbName);

        /// <summary>
        /// 修改数据库
        /// </summary>
        /// <param name="sql">sql修改</param>
        /// <returns></returns>
        ApiResult AlterDatabase(string sql);

        /// <summary>
        /// 运行sql语句
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        ApiResult ExecuteSql(string sql);

        /// <summary>
        /// 获取多个数据表组成的内存数据库
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        DataSet RetrieveDataSet(string sql);

        /// <summary>
        /// 压缩数据库
        /// </summary>
        /// <returns></returns>
        ApiResult CompressDatabase();

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <returns></returns>
        ApiResult BackupDatabase();
        #endregion

        #region 表
        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="sql">The SQL.</param>
        ApiResult CreateTable(string sql);

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        ApiResult DropTable(string tabName);

        /// <summary>
        ///获取表格数据
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        DataTable RetrieveDataTable(string sql);

        /// <summary>
        /// 修改表结构模式
        /// </summary>
        /// <param name="sql">The SQL.</param>
        ApiResult AlterTable(string sql); 
        #endregion

        #region 数据行
        /// <summary>
        /// 添加数据行
        /// </summary>
        ApiResult AddTableRow(string tabName, Hashtable nv);

        /// <summary>
        /// 获取数据行
        /// </summary>
        /// <returns></returns>
        DataRow RetrieveDataRow(string sql);

        /// <summary>
        /// 更新数据行
        /// </summary>
        ApiResult UpdateTableRow(string tabName, RowIdentity ID, Hashtable nv);

        /// <summary>
        /// 删除数据行
        /// </summary>
        ApiResult RemoveRowByCondition(string tabName, RowCondition cond);
        #endregion

        /// <summary>
        /// 获取相关字段数据
        /// </summary>
        /// <returns></returns>
        object RetrieveObject(string sql); 
    }

}
