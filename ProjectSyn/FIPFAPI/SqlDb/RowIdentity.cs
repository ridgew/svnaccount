using System.Collections.Specialized;

namespace FIPFAPI.SqlDb
{
    /// <summary>
    /// 数据行标志限定
    /// </summary>
    public class RowIdentity : RowCondition
    {
        private RowIdentity()
            : base()
        {
        }

        /// <summary>
        /// Creates the specified nv.
        /// </summary>
        /// <param name="nv">The nv.</param>
        /// <returns></returns>
        public static RowIdentity Create(NameValueCollection nv)
        {
            RowIdentity Identity = new RowIdentity();
            foreach (string key in nv.AllKeys)
            {
                Identity.AddWithRelation(ConditionRelation.AND, key, ConditionRelation.Equal, nv[key]);
            }
            return Identity;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RowIdentity"/> class.
        /// </summary>
        /// <param name="fieldName">Name of the field.</param>
        /// <param name="val">The val.</param>
        public RowIdentity(string fieldName, object val)
        {
            Add(fieldName, ConditionRelation.AND, val);
        }

    }
}
