using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace SynUtil
{
    /// <summary>
    /// 同步规则基类
    /// </summary>
    [Serializable, XmlInclude(typeof(FileRuleBase)), XmlInclude(typeof(FolderRuleBase))]
    public class SynRule
    {
        /// <summary>
        /// 该规则的值配置
        /// </summary>
        [XmlAttribute]
        public string Setting { get; set; }

        /// <summary>
        /// 如前一规则是OR关系，默认为false。
        /// </summary>
        [XmlAttribute]
        public bool OrBeforeRule { get; set; }

        /// <summary>
        /// Activeds the specified rt val.
        /// </summary>
        /// <param name="rtVal">The rt val.</param>
        /// <returns></returns>
        public virtual bool Actived(object rtVal)
        {
            return false;
        }

        internal protected bool GetRegxPatternActived(Func<string> cmpString)
        {
            string cmpSet = Setting.Trim();
            int idxLast = cmpSet.Length - 1;
            string myPattern = cmpSet.Replace("?", "\\w");

            //以"/"包含的为正则表达式模式定义
            if (cmpSet.Length > 2 && cmpSet[0] == '/'
                && cmpSet[idxLast] == '/')
            {
                myPattern = cmpSet.Substring(1, idxLast - 1);
            }
            else
            {
                if (myPattern[0] == '*')
                {
                    myPattern = Regex.Escape(myPattern.Substring(1)) + "$";
                }

                idxLast = myPattern.Length - 1;
                if (idxLast > 0 && myPattern[idxLast] == '*')
                {
                    myPattern = "^" + Regex.Escape(myPattern.Substring(0, idxLast));
                }
            }
            return Regex.IsMatch(cmpString(), myPattern, RegexOptions.IgnoreCase);
        }

        internal protected bool GetTimeActived(Func<DateTime> cmpTime)
        {
            bool blnRet = false;
            DateTime tTime = cmpTime();
            string cmpSet = Setting.Trim();

            int idxLast = cmpSet.Length - 1;
            if (cmpSet.IndexOf('-') != -1 && cmpSet.Length > 3
                && (cmpSet[0] == '[' || cmpSet[0] == '(')
                && (cmpSet[idxLast] == ']' || cmpSet[idxLast] == ')'))
            {
                bool leftVal = false, rightVal = false;
                string[] tVals = cmpSet.Split('-');

                if (tVals.Length == 2)
                {
                    leftVal = (cmpSet[0] == '[') ? IsTimeMatch(">=" + tVals[0].TrimStart('(', '['), tTime) : IsTimeMatch(">" + tVals[0].TrimStart('(', '['), tTime);
                    rightVal = (cmpSet[0] == ']') ? IsTimeMatch("<=" + tVals[1], tTime) : IsTimeMatch("<" + tVals[1], tTime);
                }

                blnRet = leftVal && rightVal;
            }
            else
            {
                blnRet = IsTimeMatch(cmpSet, tTime);
            }
            return blnRet;
        }

        internal static bool IsTimeMatch(string timeExp, DateTime tTime)
        {
            bool blnRet = false;
            string cmpSet = timeExp.Trim(new char[] { ' ', '[', ']', '(', ')' });

            // > >= == < <=
            int nIndex = GlobalUtil.FindFirstNumber(cmpSet);
            if (nIndex == -1)
            {
                throw new InvalidOperationException("时间值规则定义错误，必须包含数字(以及比较符号> >= == < <=) !");
            }

            string cmpStr = cmpSet.Substring(0, nIndex).Trim();
            string cmpValue = cmpSet.Substring(nIndex).Trim();

            if (cmpStr == ">")
            {
                blnRet = Convert.ToDateTime(cmpValue) > tTime;
            }
            else if (cmpStr == ">=")
            {
                blnRet = Convert.ToDateTime(cmpValue) >= tTime;
            }
            else if (cmpStr == "==")
            {
                blnRet = Convert.ToDateTime(cmpValue) == tTime;
            }
            else if (cmpStr == "<")
            {
                blnRet = Convert.ToDateTime(cmpValue) < tTime;
            }
            else if (cmpStr == "<=")
            {
                blnRet = Convert.ToDateTime(cmpValue) <= tTime;
            }
            return blnRet;
        }

        internal protected bool GetAttributeActived(Func<FileAttributes> testAttr)
        {
            FileAttributes tAttr = testAttr();
            bool blnRet = false;
            string cmpSet = Setting.Trim();
            Type AttrType = typeof(FileAttributes);
            if (cmpSet.IndexOf('|') == -1)
            {
                if (Enum.IsDefined(AttrType, cmpSet))
                {
                    FileAttributes cmpAttr = (FileAttributes)Enum.Parse(AttrType, cmpSet);
                    blnRet = (tAttr & cmpAttr) == cmpAttr;
                }
            }
            else
            {
                //多个属性条件合并
                string[] diffAtts = cmpSet.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                bool attrMatch = false;
                for (int i = 0, j = diffAtts.Length; i < j; i++)
                {
                    cmpSet = diffAtts[i].Trim();
                    if (Enum.IsDefined(AttrType, cmpSet))
                    {
                        FileAttributes cmpAttr = (FileAttributes)Enum.Parse(AttrType, cmpSet);
                        attrMatch = (tAttr & cmpAttr) == cmpAttr;

                        if (!attrMatch) break;
                    }
                }
                blnRet = attrMatch;
            }
            return blnRet;
        }

        internal static T[] GetSubRuleArrayIn<T>(SynRule[] allRules)
            where T : SynRule
        {
            return Array.ConvertAll<SynRule, T>((Array.FindAll<SynRule>(allRules,
              r => { return r is T; })),
              (a) => { return a as T; });
        }

    }

}
