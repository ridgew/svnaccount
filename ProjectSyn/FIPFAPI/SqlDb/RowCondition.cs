using System.Collections;
using System.Text;

namespace FIPFAPI.SqlDb
{
    /// <summary>
    /// 数据行限定
    /// </summary>
    public class RowCondition
    {

        /// <summary>
        /// 所有的条件集合
        /// </summary>
        protected ArrayList ConditionList = new ArrayList();
        
        /// <summary>
        /// 添加对某个字段的限定
        /// </summary>
        /// <param name="FieldName">字段名称</param>
        /// <param name="rel">与限定值直接的关系</param>
        /// <param name="val">The val.</param>
        public RowCondition Add(string FieldName, ConditionRelation rel, object val)
        {
            return AddWithRelation(ConditionRelation.AND, FieldName, rel, val);
        }

        /// <summary>
        /// 添加对某个字段的限定,并指定与已存在之间的关系
        /// </summary>
        /// <param name="relPrefix">已存在之间的关系</param>
        /// <param name="FieldName">字段名称</param>
        /// <param name="rel">与限定值直接的关系</param>
        /// <param name="val">The val.</param>
        /// <returns></returns>
        public RowCondition AddWithRelation(ConditionRelation relPrefix, string FieldName, ConditionRelation rel, object val)
        {
            if (ConditionList.Count > 0) ConditionList.Add(relPrefix);
            ConditionList.Add(FieldName);
            ConditionList.Add(rel);
            ConditionList.Add(val);

            return this;
        }

        /// <summary>
        /// 返回表示当前 <see cref="T:System.Object"/> 的 <see cref="T:System.String"/>的关系表示法。
        /// </summary>
        /// <returns>
        /// 	<see cref="T:System.String"/>，表示当前的 <see cref="T:System.Object"/>。
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ConditionList.Count; i += 4)
            {
                if (i > 1) sb.AppendFormat(" {0} ", ConditionList[i-1]);

                if (i + 2 > ConditionList.Count) break;

                sb.AppendFormat("{0} {1} {2}",
                    ConditionList[i],
                    (int)ConditionList[i + 1],
                    ConditionList[i + 2]);
            }
            return sb.ToString();
        }

    }
}
