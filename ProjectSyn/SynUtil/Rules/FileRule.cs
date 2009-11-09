using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace SynUtil
{
    #region FileInfo Rules
    [Serializable, XmlInclude(typeof(FileNameRule)), XmlInclude(typeof(FileSizeRule)), XmlInclude(typeof(FileAttributeRule)),
    XmlInclude(typeof(FileChangeTimeRule)), XmlInclude(typeof(FileCreateTimeRule))]
    public abstract class FileRuleBase : SynRule
    {
        public abstract bool Actived(FileInfo fInfo);

        /// <summary>
        /// Activeds the specified rt val.
        /// </summary>
        /// <param name="rtVal">The rt val.</param>
        /// <returns></returns>
        public override bool Actived(object rtVal)
        {
            bool blnRet = false;
            FileInfo fInfo = rtVal as FileInfo;
            if (fInfo != null)
            {
                blnRet = Actived(fInfo);
            }
            return blnRet;
        }
    }

    [Serializable]
    [ImplementState(CompleteState.OK, "0.1", Description = "针对文件大小的过滤规则", ReleaseDateGTM = "Fri, 06 Nov 2009 09:34:58 GMT")]
    public class FileSizeRule : FileRuleBase
    {
        /// <summary>
        /// Activeds the specified f info.
        /// </summary>
        /// <param name="fInfo">The f info.</param>
        /// <returns></returns>
        public override bool Actived(FileInfo fInfo)
        {
            bool blnRet = false;
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
                    leftVal = (cmpSet[0] == '[') ? IsSizeMatch(">=" + tVals[0].TrimStart('(', '['), fInfo) : IsSizeMatch(">" + tVals[0].TrimStart('(', '['), fInfo);

                    rightVal = (cmpSet[idxLast] == ']') ? IsSizeMatch("<=" + tVals[1], fInfo) : IsSizeMatch("<" + tVals[1], fInfo);

                    //Console.Error.WriteLine("Deug:{0}, Left:{1}, Right:{2}, File @ {3}", cmpSet, leftVal, rightVal, fInfo.FullName);
                }

                blnRet = leftVal && rightVal;
            }
            else
            {
                blnRet = IsSizeMatch(cmpSet, fInfo);
            }

            return blnRet;
        }


        private bool IsSizeMatch(string sizeStr, FileInfo fInfo)
        {
            //Console.Error.WriteLine("sizeStr = {0}", sizeStr);

            bool blnRet = false;
            string cmpSet = sizeStr.Trim(new char[] { ' ', '[', ']', '(', ')' });

            // > >= == < <=
            int nIndex = GlobalUtil.FindFirstNumber(cmpSet);
            if (nIndex == -1)
            {
                throw new InvalidOperationException("文件大小规则定义错误，必须包含数字(以及比较符号> >= == < <=) 和单位(b,k,m,g)等!");
            }

            string cmpStr = cmpSet.Substring(0, nIndex).Trim();
            string cmpValue = cmpSet.Substring(nIndex).Trim();

            Char chrUnit = cmpValue[cmpValue.Length - 1];
            if (!Char.IsDigit(chrUnit))
            {
                switch (chrUnit.ToString().ToLower())
                {
                    case "k":
                        cmpValue = (1024 * Convert.ToInt64(cmpValue.TrimEnd('k', 'K'))).ToString();
                        break;

                    case "m":
                        cmpValue = (1024 * 1024 * Convert.ToInt64(cmpValue.TrimEnd('m', 'M'))).ToString();
                        break;

                    case "g":
                        cmpValue = (1024 * 1024 * 1024 * Convert.ToInt64(cmpValue.TrimEnd('g', 'G'))).ToString();
                        break;

                    default:
                        break;
                }
            }

            //Console.Error.WriteLine("cmpStr = {0}", cmpStr);

            if (cmpStr == ">")
            {
                //Console.Error.WriteLine("{1} > {0}", cmpValue, fInfo.Length);

                blnRet = fInfo.Length > Convert.ToInt64(cmpValue);
            }
            else if (cmpStr == ">=")
            {
                blnRet = fInfo.Length >= Convert.ToInt64(cmpValue);
            }
            else if (cmpStr == "==")
            {
                blnRet = fInfo.Length == Convert.ToInt64(cmpValue);
            }
            else if (cmpStr == "<")
            {
                blnRet = fInfo.Length < Convert.ToInt64(cmpValue);
            }
            else if (cmpStr == "<=")
            {
                blnRet = fInfo.Length <= Convert.ToInt64(cmpValue);
            }
            return blnRet;
        }
    }

    /// <summary>
    /// 文件属性规则
    /// </summary>
    [Serializable]
    public class FileAttributeRule : FileRuleBase
    {

        /*
        ReadOnly Hidden System Normal Encrypted
        */

        /// <summary>
        /// Activeds the specified f info.
        /// </summary>
        /// <param name="fInfo">The f info.</param>
        /// <returns></returns>
        public override bool Actived(FileInfo fInfo)
        {
            return GetAttributeActived(() => fInfo.Attributes);
        }
    }

    /// <summary>
    /// 文件名规则
    /// </summary>
    [Serializable]
    [ImplementState(CompleteState.OK, "0.1", Description = "针对文件名称的过滤规则", ReleaseDateGTM = "Fri, 06 Nov 2009 09:34:58 GMT")]
    public class FileNameRule : FileRuleBase
    {

        /// <summary>
        /// Activeds the specified f info.
        /// </summary>
        /// <param name="fInfo">The f info.</param>
        /// <returns></returns>
        public override bool Actived(FileInfo fInfo)
        {
            return GetRegxPatternActived(() => fInfo.Name);
        }
    }

    /// <summary>
    /// 修改时间规则
    /// </summary>
    [Serializable]
    public class FileChangeTimeRule : FileRuleBase
    {
        /// <summary>
        /// Activeds the specified f info.
        /// </summary>
        /// <param name="fInfo">The f info.</param>
        /// <returns></returns>
        public override bool Actived(FileInfo fInfo)
        {
            return GetTimeActived(() => fInfo.LastWriteTime);
        }

    }

    /// <summary>
    /// 创建时间规则
    /// </summary>
    [Serializable]
    public class FileCreateTimeRule : FileRuleBase
    {

        /// <summary>
        /// Activeds the specified f info.
        /// </summary>
        /// <param name="fInfo">The f info.</param>
        /// <returns></returns>
        public override bool Actived(FileInfo fInfo)
        {
            return GetTimeActived(() => fInfo.CreationTime);
        }
    }
    #endregion
}
