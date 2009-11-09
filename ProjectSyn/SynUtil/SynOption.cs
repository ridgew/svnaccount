using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
#if UnitTest
using NUnit.Framework;
#endif

namespace SynUtil
{
    /// <summary>
    /// 同步选项配置
    /// </summary>
    [Serializable]
    public class SynOption
    {
        private SynMode _mode = SynMode.Mixed;

        [XmlAttribute]
        public SynMode Mode 
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public SynRule[] MatchRules { get; set; }

        public SynRule[] ExcludeRules { get; set; }
    }

    [Serializable]
    public enum SynMode : byte
    { 
        Match,
        Exclued,
        Mixed
    }

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
    }

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
    [ImplementState(CompleteState.OK, "0.1", Description="针对文件大小的过滤规则", ReleaseDateGTM = "Fri, 06 Nov 2009 09:34:58 GMT")]
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
            string cmpSet = sizeStr.Trim(new char[]{' ', '[',']', '(', ')'});

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

    #region Folder Rule
    [Serializable, XmlInclude(typeof(FolderNameRule)), XmlInclude(typeof(FolderAttributeRule))
    , XmlInclude(typeof(FolderCreateTimeRule)), XmlInclude(typeof(FolderChangeTimeRule)) ]
    public abstract class FolderRuleBase : SynRule
    {
        public abstract bool Actived(DirectoryInfo dInfo);

        /// <summary>
        /// Activeds the specified rt val.
        /// </summary>
        /// <param name="rtVal">The rt val.</param>
        /// <returns></returns>
        public override bool Actived(object rtVal)
        {
            bool blnRet = false;
            DirectoryInfo dInfo = rtVal as DirectoryInfo;
            if (dInfo != null)
            {
                blnRet = Actived(dInfo);
            }
            return blnRet;
        }

    }

    /// <summary>
    /// 文件夹属性规则
    /// </summary>
    [Serializable]
    [ImplementState(CompleteState.OK, "0.1", Description = "针对文件夹属性的过滤规则", ReleaseDateGTM = "Fri, 06 Nov 2009 09:34:58 GMT")]
    public class FolderAttributeRule : FolderRuleBase
    {
        /*
         ReadOnly Hidden System Normal Encrypted
         */
        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetAttributeActived(() => dInfo.Attributes);
        }
    }

    /// <summary>
    /// 文件夹名称规则
    /// </summary>
    [Serializable]
    public class FolderNameRule : FolderRuleBase
    {

        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetRegxPatternActived(() => dInfo.Name);
        }
    }

    /// <summary>
    /// 文件夹创建时间规则
    /// </summary>
    [Serializable]
    public class FolderCreateTimeRule : FolderRuleBase
    {

        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetTimeActived(() => dInfo.CreationTime);
        }
    }

    /// <summary>
    /// 文件夹修改时间规则
    /// </summary>
    [Serializable]
    public class FolderChangeTimeRule : FolderRuleBase
    {

        /// <summary>
        /// Activeds the specified d info.
        /// </summary>
        /// <param name="dInfo">The d info.</param>
        /// <returns></returns>
        public override bool Actived(DirectoryInfo dInfo)
        {
            return GetTimeActived(() => dInfo.LastWriteTime);
        }
    }
    #endregion

#if UnitTest
    public class SynFileItemWorker : IFileItemWorker
    {
        public SynFileItemWorker(string baseDirectory, SynOption options)
        {
            Options = options;
            BaseDir = baseDirectory;
        }

        public SynOption Options
        {
            get;
            set; 
        }
        #region IFileItemWorker 成员

        /// <summary>
        /// Packages the file.
        /// </summary>
        /// <param name="pkgFileInfo">The PKG file info.</param>
        public void PackageFile(FileInfo pkgFileInfo)
        {
            
        }

        /// <summary>
        /// 基础根路径
        /// </summary>
        /// <value>The base dir.</value>
        public string BaseDir
        {
            get;
            set;
        }

        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            
        }

        #endregion
    }

    [TestFixture]
    public class SynOptionTest
    {
        [Test]
        public void FileRuleTest()
        {
            SynOption option = new SynOption();
            option.MatchRules = new SynRule[] { 
                new FileNameRule { Setting = "*.cs", OrBeforeRule = false },
                new FileSizeRule { Setting = ">100k", OrBeforeRule = false },
                new FolderAttributeRule { Setting = "Hidden", OrBeforeRule = false }
            };

            option.ExcludeRules = new SynRule[] { 
                new FileNameRule { Setting = "*.rar", OrBeforeRule = false },
                new FileSizeRule { Setting = ">1M" },
                new FolderNameRule { Setting = "obj", OrBeforeRule = false }
            };

           

            option.GetXmlDoc(true).WriteIndentedContent(Console.Out);


            //FileRuleBase[] fileRules = Array.ConvertAll<SynRule, FileRuleBase>((Array.FindAll<SynRule>(option.MatchRules,
            //   r =>
            //   {
            //       return r is FileRuleBase;
            //   })), (a) =>
            //   {
            //       return a as FileRuleBase;
            //   });

            //foreach (FileRuleBase fRule in fileRules)
            //{
            //    Console.WriteLine("{0}", fRule.GetXmlDoc().OuterXml);
            //}

        }

        [Test]
        public void RuleRestoreTest()
        {
            SynOption option = Vbyte.Configuration.XmlSerializeSectionHandler.GetObject<SynOption>("SynOption");

            option.GetXmlDoc(true).WriteIndentedContent(Console.Out);

        }

        [Test]
        public void FileSystemWithOptionTest()
        {
            /*
             ReadOnly Hidden System Normal Encrypted
             */
            SynOption option = new SynOption();
            option.MatchRules = new SynRule[] { 
                //new FileNameRule { Setting = "/\\.(cs|js|aspx)$/", OrBeforeRule = false }
                //,new FileNameRule { Setting = "*.html", OrBeforeRule = true }
                //,
                new FileSizeRule { Setting = "(0-3k]", OrBeforeRule = false }
                , new FileAttributeRule { Setting = "ReadOnly|Hidden", OrBeforeRule = false }
            };

            option.ExcludeRules = new SynRule[] { 
                new FileNameRule { Setting = "*.rar", OrBeforeRule = false }
                ,new FileSizeRule { Setting = ">1M", OrBeforeRule = true }
                ,new FolderNameRule { Setting = "obj", OrBeforeRule = true }
                ,new FolderNameRule { Setting = "bin", OrBeforeRule = true }
            };

            FileRuleBase[] filematchRules = Array.ConvertAll<SynRule, FileRuleBase>((Array.FindAll<SynRule>(option.MatchRules,
               r =>
               {
                   return r is FileRuleBase;
               })), (a) =>
               {
                   return a as FileRuleBase;
               });

            string synDir = @"D:\DevRoot\EaseV2\EaseV1_3\www";
            SynFileItemWorker synWorker = new SynFileItemWorker(synDir, option);
            using (FileSystemWorker fSysWorker = new FileSystemWorker(synDir,
                   synWorker))
            {
                string siteRoot = AppDomain.CurrentDomain.BaseDirectory;

                //通过匹配模式查找
                fSysWorker.FileMatch = f => {
                    int iLast = -1;
                    bool blnResult = false;
                    foreach (FileRuleBase fRule in filematchRules)
                    {
                        if (iLast == -1)
                        {
                            iLast += 1;
                            blnResult = fRule.Actived(f);
                        }
                        else
                        {
                            if (fRule.OrBeforeRule)
                            {
                                blnResult = blnResult || fRule.Actived(f);
                            }
                            else
                            {
                                blnResult = blnResult && fRule.Actived(f);
                            }
                        }
                    }
                    return blnResult;
                };


                //忽略文件的判断
                fSysWorker.FileExclude = ifile =>
                {
                    return false;
                };

                //忽略文件夹的判断
                fSysWorker.FolderExclude = fd =>
                {
                    return true;
                };

                //fSysWorker.OnSkipFileActive = s =>
                //{
                //    Console.Error.WriteLine("skip {0}", s.FullName);
                //};

                fSysWorker.MsgReport = s => System.Diagnostics.Trace.WriteLine(s);

                //fSysWorker.ProgressReport = p => Console.WriteLine(string.Format("已完成{0}",
                //    Regex.Replace((p * 100).ToString("0.00") + "%...", "\\.0{1,}(%...)$", "%")));
                
                fSysWorker.Execute();
            }

        }
    }
#endif

}
