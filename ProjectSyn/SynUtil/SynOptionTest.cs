using System;
using System.IO;


#if UnitTest
using NUnit.Framework;
#endif

namespace SynUtil
{
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


            FileRuleBase[] filematchRules = SynRule.GetSubRuleArrayIn<FileRuleBase>(option.MatchRules);

            string synDir = @"D:\DevRoot\EaseV2\EaseV1_3\www";
            SynFileItemWorker synWorker = new SynFileItemWorker(synDir, option);
            using (FileSystemWorker fSysWorker = new FileSystemWorker(synDir,
                   synWorker))
            {
                string siteRoot = AppDomain.CurrentDomain.BaseDirectory;

                //通过匹配模式查找
                fSysWorker.FileMatch = f =>
                {
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
