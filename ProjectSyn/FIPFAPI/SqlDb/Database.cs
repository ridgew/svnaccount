using System.Collections;
using System.Data;
using System.Data.Common;

namespace FIPFAPI.SqlDb
{
    /// <summary>
    /// 抽象数据库
    /// </summary>
    public abstract class Database : IDatabaseAPI
    {
        private Database() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="connStr">The connection string.</param>
        public Database(string connStr)
        {
            ConnectionString = connStr;
        }

        private string _connStr;

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <value>The connection string.</value>
        public string ConnectionString
        {
            get { return _connStr; }
            set { _connStr = value; }
        }

        #region IDatabaseAPI 成员

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        /// <returns></returns>
        public abstract ApiResult CreateDatabase(string dbName);

        /// <summary>
        /// 删除数据库
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        /// <returns></returns>
        public abstract ApiResult RemoveDatabase(string dbName); 

        /// <summary>
        /// 修改数据库
        /// </summary>
        /// <param name="sql">sql修改</param>
        /// <returns></returns>
        public abstract ApiResult AlterDatabase(string sql);

        /// <summary>
        /// 运行sql语句
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public abstract ApiResult ExecuteSql(string sql);


        /// <summary>
        /// 执行相关数据操作命令
        /// </summary>
        /// <param name="cmd">The CMD.</param>
        /// <returns></returns>
        public abstract ApiResult ExecuteCommand(DbCommand cmd);

        /// <summary>
        /// 获取多个数据表组成的内存数据库
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public abstract DataSet RetrieveDataSet(string sql);

        /// <summary>
        /// 压缩数据库
        /// </summary>
        /// <returns></returns>
        public abstract ApiResult CompressDatabase();

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <returns></returns>
        public abstract ApiResult BackupDatabase();

        /// <summary>
        /// 创建表
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public abstract ApiResult CreateTable(string sql);

        /// <summary>
        /// 删除表
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        /// <returns></returns>
        public abstract ApiResult DropTable(string tabName);

        /// <summary>
        /// 获取表格数据
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public abstract DataTable RetrieveDataTable(string sql);

        /// <summary>
        /// 修改表结构模式
        /// </summary>
        /// <param name="sql">The SQL.</param>
        /// <returns></returns>
        public abstract ApiResult AlterTable(string sql);

        /// <summary>
        /// 添加数据行
        /// </summary>
        /// <param name="tabName"></param>
        /// <param name="nv"></param>
        /// <returns></returns>
        public abstract ApiResult AddTableRow(string tabName, Hashtable nv);

        /// <summary>
        /// 获取数据行
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract DataRow RetrieveDataRow(string sql);

        /// <summary>
        /// 更新数据行
        /// </summary>
        /// <param name="tabName"></param>
        /// <param name="ID"></param>
        /// <param name="nv"></param>
        /// <returns></returns>
        public abstract ApiResult UpdateTableRow(string tabName, RowIdentity ID, Hashtable nv);

        /// <summary>
        /// 删除数据行
        /// </summary>
        /// <param name="tabName"></param>
        /// <param name="cond"></param>
        /// <returns></returns>
        public abstract ApiResult RemoveRowByCondition(string tabName, RowCondition cond);

        /// <summary>
        /// 获取相关字段数据
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public abstract object RetrieveObject(string sql);

        #endregion
    }

}
