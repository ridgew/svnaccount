using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;

namespace SynUtil
{
    [LicenseProvider(typeof(SynLicenseProvider))]
    class Program
    {
        static ConsoleColor RawConsoleColor = Console.ForegroundColor;

        static Program()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }
        
        static void Main(string[] args)
        {
            //如果传递的是文件夹，则读取文件夹下命名为.syn的文件，进行比较

            //try
            //{
            //    LicenseManager.Validate(typeof(Key));
            //}
            //catch (LicenseException exp)
            //{
            //    Console.WriteLine("License Invalid:{0}\r\n{1}", exp.Message, exp.StackTrace);   
            //}

            //Console.WriteLine("ok");
            //Console.Read();

            if (args.Length < 1)
            {
                Error("错误：请指定要同步的文件夹或者是要更新的syn文件！");
            }
            else
            {
                FileInfo fInfo = null;
                DirectoryInfo dInfo = null;


                if (Directory.Exists(args[0]))
                {
                    dInfo = new DirectoryInfo(args[0]);
                    if (args.Length == 1)
                    {
                        BuildSynFile(dInfo.FullName);
                    }
                    else
                    {
                        fInfo = new FileInfo(args[1]);
                        //CreateCab(dInfo.FullName, fInfo.FullName);
                    }
                }
                else
                {
                    if (!args[0].EndsWith(".syn"))
                    {
                        Error("错误：请指定要同步的syn旧文件！");
                    }
                    else
                    {
                        fInfo = new FileInfo(args[0]);
                        if (args.Length == 1)
                        {
                            //ExtractCab(fInfo.FullName);
                        }
                        else
                        {
                            dInfo = new DirectoryInfo(args[1]);
                            //ExtractCab(fInfo.FullName, dInfo.FullName);
                        }
                    }
                }
            }

            Console.WriteLine("按任意键继续...");
            Console.Read();
        }

        /// <summary>
        /// Handles the UnhandledException event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.UnhandledExceptionEventArgs"/> instance containing the event data.</param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            
        }

        static void Error(string format, params object[] errMsgs)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(format, errMsgs);
            Console.ForegroundColor = RawConsoleColor;
        }

        static void Progress(string format, params object[] errMsgs)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(format, errMsgs);
            Console.ForegroundColor = RawConsoleColor;
        }

        static void BuildSynFile(string baseDir)
        {
            DirectoryInfo dInfo = new DirectoryInfo(baseDir);
            //string targetName = Directory.GetParent(baseDir) + "\\.syn";
            string targetName = dInfo + "\\.syn";
            //BuildSynFile(baseDir, targetName);

            if (File.Exists(targetName))
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                byte[] bytesOldSyn = File.ReadAllBytes(targetName);
                SynchronizationFile synOld = bytesOldSyn.GetObject<SynchronizationFile>();

                SynchronizationFile synChanged = GlobalUtil.GetSynFileChangeState(dInfo.FullName, synOld,
                    true, true,
                    null,
                    null);

                GlobalUtil.ReportSynFile(synChanged, Console.Out);

                Progress("准备在同名目录下创建补丁更新包...");
                GlobalUtil.CreatePatchCabinet(synChanged, baseDir, Path.GetFullPath(Path.Combine(baseDir, "patch.cab.syn")));

                Progress("更新目录结构文件");
                File.WriteAllBytes(Path.Combine(baseDir, "changed.syn"), synChanged.GetBytes());

                watch.Stop();
                if (watch.ElapsedMilliseconds > 1000)
                {
                    Progress("已处理完成，本次操作共计耗时{0:0.00}秒！", (double)watch.ElapsedMilliseconds / 1000.00);
                }
                else
                {
                    Progress("已处理完成，本次操作共计耗时{0}毫秒！", watch.ElapsedMilliseconds);
                }
                return;
            }

            string siteRoot = AppDomain.CurrentDomain.BaseDirectory;
            SynchronizationFile synFile = GlobalUtil.GetSynFile(dInfo.FullName, true, true,

                str => Console.WriteLine(string.Format("{0}",
                    str.Replace(siteRoot, "").Replace(baseDir, "").Replace("\\", "/"))),

                p => Progress(string.Format("已完成{0}",
                    Regex.Replace((p * 100).ToString("0.00") + "%...", "\\.0{1,}(%...)$", "%")))
               );

            GlobalUtil.ReportSynFile(synFile, Console.Out);
            File.WriteAllBytes(targetName, synFile.GetBytes());
            
        }

        private static byte[] GetLegalKey(SymmetricAlgorithm mobjCryptoService, string Key)
        {
            string sTemp = Key;
            mobjCryptoService.GenerateKey();
            byte[] bytTemp = mobjCryptoService.Key;
            int KeyLength = bytTemp.Length;
            if (sTemp.Length > KeyLength)
                sTemp = sTemp.Substring(0, KeyLength);
            else if (sTemp.Length < KeyLength)
                sTemp = sTemp.PadRight(KeyLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }

        private static byte[] GetLegalIV(SymmetricAlgorithm mobjCryptoService)
        {
            string sTemp = "E4ghj*Ghg7!rNIfb&95GUY86GfghUb#er57HBh(u%g6HJ($jhWk7&!hg4ui%$hjk";
            mobjCryptoService.GenerateIV();

            byte[] bytTemp = mobjCryptoService.IV;
            int IVLength = bytTemp.Length;
            if (sTemp.Length > IVLength)
                sTemp = sTemp.Substring(0, IVLength);
            else if (sTemp.Length < IVLength)
                sTemp = sTemp.PadRight(IVLength, ' ');
            return ASCIIEncoding.ASCII.GetBytes(sTemp);
        }

        static void BuildSynFile(string baseDir, string targetFileName)
        {
            SynchronizationFile synOld = null;
            SynchronizationFile synFile = new SynchronizationFile();

            string sKey = "Guz(%&hj7x89H$yuBI0456FtmaT5&fvHUFCy76*h%(HilJ$lhj!y6&(*jkP87jH7";
            SymmetricAlgorithm mobjCryptoService = new RijndaelManaged();
            byte[] key = GetLegalKey(mobjCryptoService, sKey);
            byte[] iv = GetLegalIV(mobjCryptoService);

            SymmetricCryptography<RijndaelManaged> CryProvider = new SymmetricCryptography<RijndaelManaged>(key, iv);
            
            if (File.Exists(targetFileName))
            {
                byte[] eBytes = File.ReadAllBytes(targetFileName);
                eBytes = CryProvider.Decrypt(eBytes);
                synOld = eBytes.GetObject<SynchronizationFile>();
            }

            using (FileSystemWorker fSysWorker = new FileSystemWorker(baseDir,
                new BuildSynFileWorker(baseDir, synFile, synOld)))
            {
                string siteRoot = AppDomain.CurrentDomain.BaseDirectory;
                //设置忽视隐藏文件夹或文件
                fSysWorker.FileExclude = f =>
                {
                    //return (f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                    return f.FullName.EndsWith(".syn", StringComparison.InvariantCultureIgnoreCase);
                };
                //fSysWorker.IgnoreHiddenFolder = true;

                fSysWorker.MsgReport = str => Console.WriteLine(string.Format("{0}",
                    str.Replace(siteRoot, "").Replace(baseDir, "").Replace("\\", "/")));

                fSysWorker.ProgressReport = p => Progress(string.Format("已完成{0}",
                    Regex.Replace((p * 100).ToString("0.00") + "%...", "\\.0{1,}(%...)$", "%")));

                fSysWorker.Execute();
            }

            synFile.SynTimeUTC = DateTime.Now.ToUniversalTime();

            if (synOld == null)
            {
                File.WriteAllBytes(targetFileName, CryProvider.Encrypt(synFile.GetBytes()));
            }
            else
            {
                //比较旧结构不同，找出删除文件
                GlobalUtil.SetNotExistFileIn(synFile, synOld, f => {
                    synFile.RemoveFiles.Add(f.FileID, f);
                });

                //修复只是重命名的文件，而不是删除了重新添加的一个文件。
                synFile.RenameList = GlobalUtil.FixedRename(synFile.RemoveFiles, synFile.AddFiles);

                //Create Patch
                //File.WriteAllBytes(targetFileName, CryProvider.Encrypt(synFile.GetBytes()));
            }

            Progress(".目录{0}\r\n已创建同步文件为{1}", baseDir, targetFileName);

            Progress("同步文件信息：共{0}文件，大小{1}字节！\r\n + {2}, * {3}, - {4}, R {5}",
                synFile.TotalFileCount,
                synFile.TotalFileSize,
                synFile.AddFiles.Count,
                synFile.UpdateFiles.Count,
                synFile.RemoveFiles.Count,
                synFile.RenameList.Length
                );

            #region 详细变化情况
            if (synFile.AddFiles.Count > 0)
            {
                foreach (SynFile aFile in synFile.AddFiles.Values)
                {
                    Progress("添加了文件:{0}, 文件大小:{1}bytes", aFile.FileID, aFile.FileSize);
                }
            }

            if (synFile.UpdateFiles.Count > 0)
            {
                foreach (SynFile uFile in synFile.UpdateFiles.Values)
                {
                    Progress("修改了文件:{0}, 文件大小:{1}bytes", uFile.FileID, uFile.FileSize);
                }
            }

            if (synFile.RemoveFiles.Count > 0)
            {
                foreach (SynFile dFile in synFile.RemoveFiles.Values)
                {
                    Progress("删除了文件:{0}, 文件大小:{1}bytes", dFile.FileID, dFile.FileSize);
                }
            }

            if (synFile.RenameList.Length > 0)
            {
                for (int i = 0, j = synFile.RenameList.Length; i < j; i++)
                {
                    Progress("重命名{0}为{1}", synFile.RenameList[i].OldSynFile.FileID, synFile.RenameList[i].NewSynFile.FileID);
                }
            }
            #endregion

        }


    }
}
