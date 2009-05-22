using System;
using System.Collections.Generic;
using System.Text;

namespace FIPFAPI.SqlDb
{
    /// <summary>
    /// 约束
    /// </summary>
    public static class Constraint
    {
        /// <summary>
        /// 关系表示符号
        /// </summary>
        internal static string[] RelationSymbol = new string[] { "AND", "OR", "NOT",
            "=", "<>", ">", ">=", "<", "<=",
            "Like", "?1", "?2"};
    }

    /// <summary>
    /// 条件直接的关系约束
    /// </summary>
    public enum ConditionRelation : ushort
    {
        AND = 0,
        OR = 1,
        Not = 2,

        Equal = 3,
        NotEqual = 4,

        BigThan = 5,
        BigEqualThan = 6,

        SmallThan = 7,
        SmallEqualThan = 8,

        Like = 9,
        Contains = 10,
        NotContains = 11
    }
}
