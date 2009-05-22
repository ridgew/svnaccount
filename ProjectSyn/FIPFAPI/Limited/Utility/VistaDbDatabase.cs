using System;
using System.Collections;
using System.Data;
using FIPFAPI.SqlDb;
using System.Data.Common;

namespace FIPFAPI.Limited.Utility
{
    /// <summary>
    /// 
    /// </summary>
    public class VistaDbDatabase : Database
    {
        public VistaDbDatabase(string connStr)
            : base(connStr)
        { }

        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        /// <returns></returns>
        public override ApiResult CreateDatabase(string dbName)
        {
            
            throw new NotImplementedException();
        }

        public override ApiResult RemoveDatabase(string dbName)
        {
            throw new NotImplementedException();
        }

        public override ApiResult AlterDatabase(string sql)
        {
            throw new NotImplementedException();
        }

        public override ApiResult ExecuteSql(string sql)
        {
            throw new NotImplementedException();
        }

        public override System.Data.DataSet RetrieveDataSet(string sql)
        {
            throw new NotImplementedException();
        }

        public override ApiResult CompressDatabase()
        {
            throw new NotImplementedException();
        }

        public override ApiResult BackupDatabase()
        {
            throw new NotImplementedException();
        }

        public override ApiResult CreateTable(string sql)
        {
            throw new NotImplementedException();
        }

        public override ApiResult DropTable(string tabName)
        {
            throw new NotImplementedException();
        }

        public override DataTable RetrieveDataTable(string sql)
        {
            throw new NotImplementedException();
        }

        public override ApiResult AlterTable(string sql)
        {
            throw new NotImplementedException();
        }

        public override ApiResult AddTableRow(string tabName, Hashtable nv)
        {
            throw new NotImplementedException();
        }

        public override System.Data.DataRow RetrieveDataRow(string sql)
        {
            throw new NotImplementedException();
        }

        public override ApiResult UpdateTableRow(string tabName, RowIdentity ID, Hashtable nv)
        {
            throw new NotImplementedException();
        }

        public override ApiResult RemoveRowByCondition(string tabName, RowCondition cond)
        {
            throw new NotImplementedException();
        }

        public override object RetrieveObject(string sql)
        {
            throw new NotImplementedException();
        }

        public override ApiResult ExecuteCommand(DbCommand cmd)
        {
            throw new NotImplementedException();
        }
    }
}
