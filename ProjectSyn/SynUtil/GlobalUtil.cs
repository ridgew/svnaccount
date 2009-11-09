using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Shell32;

namespace SynUtil
{
    public static class GlobalUtil
    {
        /// <summary>
        /// 二进制流的16进制字符串表达形式
        /// </summary>
        /// <param name="bytHash">二进制流</param>
        /// <param name="IsLowerCase">小写16进制字母</param>
        public static string ByteArrayToHexStr(byte[] bytHash, bool IsLowerCase)
        {
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
            }
            return (IsLowerCase) ? sTemp.ToLower() : sTemp.ToUpper();
        }

        /// <summary>
        /// 获取本地文件MD5哈希值
        /// </summary>
        /// <param name="FilePath">本地文件完整路径</param>
        /// <remarks>
        /// 如果文件不存在则返回字符N/A
        /// </remarks>
        public static string GetMD5Hash(string FilePath)
        {
            if (!File.Exists(FilePath))
            {
                return "N/A";
            }
            else
            {
                System.IO.FileStream fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                System.Security.Cryptography.MD5 md = new MD5CryptoServiceProvider();
                md.Initialize();
                byte[] b = md.ComputeHash(fs);

                fs.Close();
                fs.Dispose();
                return ByteArrayToHexStr(b, true);
            }
        }

        #region 格式化文件大小
        /// <summary>      
        /// 格式化文件大小的C#方法      
        /// </summary>      
        /// <param name="filesize">文件的大小,传入的是一个bytes为单位的参数</param>      
        /// <returns>格式化后的值</returns>      
        public static String FormatSize(long filesize)
        {
            if (filesize < 0)
            {
                throw new ArgumentOutOfRangeException("filesize");
            }
            else if (filesize >= 1024 * 1024 * 1024) //文件大小大于或等于1024MB      
            {
                return string.Format("{0:0.00} GB", (double)filesize / (1024 * 1024 * 1024));
            }
            else if (filesize >= 1024 * 1024) //文件大小大于或等于1024KB      
            {
                return string.Format("{0:0.00} MB", (double)filesize / (1024 * 1024));
            }
            else if (filesize >= 1024) //文件大小大于等于1024bytes      
            {
                return string.Format("{0:0.00} KB", (double)filesize / 1024);
            }
            else
            {
                return string.Format("{0:0.00} bytes", filesize);
            }
        }
        #endregion  


        /// <summary>
        /// 获取SVN工作目录的结构性同步文件（忽略隐藏文件夹）
        /// </summary>
        /// <param name="synDir">同步的结构目录</param>
        /// <returns>所有文件的结构性同步文件</returns>
        public static SynchronizationFile GetSvnDirSynFile(string synDir)
        {
            return GetSynFileChangeState(synDir, null, false, true, null, null);
        }

        /// <summary>
        /// 获取所有文件的结构性同步文件
        /// </summary>
        /// <param name="synDir">同步的结构目录</param>
        /// <param name="ignoreHiddenFile">是否忽略隐藏文件</param>
        /// <param name="ignoreHiddenFolder">是否忽略隐藏文件夹</param>
        /// <param name="msgRpt">指定消息回报处理方式</param>
        /// <param name="progressRpt">指定进度处理方式</param>
        /// <returns>所有文件的结构性同步文件</returns>
        public static SynchronizationFile GetSynFile(string synDir, bool ignoreHiddenFile, bool ignoreHiddenFolder, ReportMessage msgRpt, ReportProgress progressRpt)
        {
            return GetSynFileChangeState(synDir, null, ignoreHiddenFile, ignoreHiddenFolder, msgRpt, progressRpt);
        }

        /// <summary>
        /// 获取已经包含变更状态的同步文件结构文件
        /// </summary>
        /// <param name="synDir">同步的结构目录</param>
        /// <param name="oldSynFile">就的同步结构文件</param>
        /// <param name="ignoreHiddenFile">是否忽略隐藏文件</param>
        /// <param name="ignoreHiddenFolder">是否忽略隐藏文件夹</param>
        /// <param name="msgRpt">指定消息回报处理方式</param>
        /// <param name="progressRpt">指定进度处理方式</param>
        /// <returns>所有文件的结构性同步文件</returns>
        public static SynchronizationFile GetSynFileChangeState(string synDir, SynchronizationFile oldSynFile, bool ignoreHiddenFile, bool ignoreHiddenFolder, ReportMessage msgRpt, ReportProgress progressRpt)
        {
            SynchronizationFile newSynFile = new SynchronizationFile();
            using (FileSystemWorker fSysWorker = new FileSystemWorker(synDir,
               new BuildSynFileWorker(synDir, newSynFile, oldSynFile)))
            {
                string siteRoot = AppDomain.CurrentDomain.BaseDirectory;
               
                //设置忽视隐藏文件夹或文件
                fSysWorker.FileExclude = f =>
                {
                    if (ignoreHiddenFile)
                    {
                        return f.FullName.EndsWith(".syn", StringComparison.InvariantCultureIgnoreCase) || (f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
                    }
                    else
                    {
                        return f.FullName.EndsWith(".syn", StringComparison.InvariantCultureIgnoreCase);
                    }
                    
                };

                fSysWorker.IgnoreHiddenFolder = ignoreHiddenFolder;
                fSysWorker.MsgReport = msgRpt;
                fSysWorker.ProgressReport = progressRpt;
                fSysWorker.Execute();
            }

            if (oldSynFile != null)
            {
                //比较旧结构不同，找出删除文件
                SetNotExistFileIn(newSynFile, oldSynFile, f =>
                {
                    newSynFile.RemoveFiles.Add(f.FileID, f);
                });

                //修复只是重命名的文件，而不是删除了重新添加的一个文件。
                newSynFile.RenameList = FixedRename(newSynFile.RemoveFiles, newSynFile.AddFiles);
            }
            newSynFile.SynTimeUTC = DateTime.Now.ToUniversalTime();
            return newSynFile;
        }

        internal static void SetNotExistFileIn(SynchronizationFile newFile, SynchronizationFile oldBak, Action<SynFile> tNewAction)
        {
            if (oldBak != null && newFile != null)
            {
                foreach (SynFile file in oldBak.Structure.Values)
                {
                    if (!newFile.Structure.ContainsKey(file.FileID))
                    {
                        tNewAction(file);
                    }
                }
            }
        }

        internal static SynFileRename[] FixedRename(Dictionary<string, SynFile> tDeleteDict, Dictionary<string, SynFile> tAddDict)
        {
            List<SynFileRename> rList = new List<SynFileRename>();
            //如果任何一边为0，则没有修改记录
            if (tDeleteDict.Count == 0 || tAddDict.Count == 0) { return rList.ToArray(); }

            //通过比较文件扩展名和哈希码值 区分
            Dictionary<string, string> dHashDict = ConvertHashIDDict(tDeleteDict);
            Dictionary<string, string> aHashDict = ConvertHashIDDict(tAddDict);

            string currentExt = string.Empty, cmpExt = string.Empty;
            string tAddFileID = string.Empty;

            SynFile tOldSynFile = null, tAddsynFile = null;
            List<string> tDeletList = new List<string>();
            List<string> tAddList = new List<string>();

            //hash code
            foreach (string key in dHashDict.Keys)
            {
                if (!aHashDict.ContainsKey(key)) continue;

                currentExt = Path.GetExtension(dHashDict[key]);
                cmpExt = Path.GetExtension(aHashDict[key]);

                //hascode and extension are equal
                if (aHashDict.ContainsKey(key) && currentExt.Equals(cmpExt, StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (SynFile file in tAddDict.Values)
                    {
                        if (file.HashCode.Equals(key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            //fileid
                            tOldSynFile = (SynFile)tDeleteDict[dHashDict[key]].Clone();
                            tDeletList.Add(dHashDict[key]);

                            //Console.WriteLine();
                            //Console.WriteLine("文件{0}\r\n改为:{1}", dHashDict[key], file.FileID);

                            tAddsynFile = (SynFile)tAddDict[file.FileID].Clone();
                            rList.Add(new SynFileRename { OldSynFile = tOldSynFile, NewSynFile = tAddsynFile });
                            tAddList.Add(file.FileID);
                        }
                    }

                }
            }

            if (tDeletList.Count == tAddList.Count)
            {
                foreach (string dKey in tDeletList) { tDeleteDict.Remove(dKey); }
                foreach (string aKey in tAddList) { tAddDict.Remove(aKey); }
            }
            return rList.ToArray();
        }

        internal static Dictionary<string, string> ConvertHashIDDict(Dictionary<string, SynFile> tDict)
        {
            Dictionary<string, string> tResultDict = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (SynFile file in tDict.Values)
            {
                if (tResultDict.ContainsKey(file.HashCode))
                {
                    throw new InvalidOperationException("哈希码不唯一，请重新寻找其他哈希算法！");
                }
                tResultDict.Add(file.HashCode, file.FileID);
            }
            return tResultDict;
        }


        /// <summary>
        /// Reports the syn file.
        /// </summary>
        /// <param name="synFile">The syn file.</param>
        /// <param name="writer">The writer.</param>
        public static void ReportSynFile(SynchronizationFile synFile, TextWriter writer)
        {
            writer.WriteLine("文件信息：共{0}文件，大小{1}！\r\n + {2}, * {3}, - {4}, R {5}",
                synFile.TotalFileCount,
                FormatSize(synFile.TotalFileSize),
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
                    writer.WriteLine("添加了文件:{0}, 文件大小:{1}bytes", aFile.FileID, aFile.FileSize);
                }
            }

            if (synFile.UpdateFiles.Count > 0)
            {
                foreach (SynFile uFile in synFile.UpdateFiles.Values)
                {
                    writer.WriteLine("修改了文件:{0}, 文件大小:{1}bytes", uFile.FileID, uFile.FileSize);
                }
            }

            if (synFile.RemoveFiles.Count > 0)
            {
                foreach (SynFile dFile in synFile.RemoveFiles.Values)
                {
                    writer.WriteLine("删除了文件:{0}, 文件大小:{1}bytes", dFile.FileID, dFile.FileSize);
                }
            }

            if (synFile.RenameList.Length > 0)
            {
                for (int i = 0, j = synFile.RenameList.Length; i < j; i++)
                {
                    writer.WriteLine("重命名{0}为{1}", synFile.RenameList[i].OldSynFile.FileID, synFile.RenameList[i].NewSynFile.FileID);
                }
            }
            #endregion
        }

        //private static string GetRefFullPath(string src, string destRef)
        //{
        //    if (src.EndsWith("\\")) src = src.Remove(src.Length - 1);
        //    if (destRef.EndsWith("\\")) destRef = destRef.Remove(destRef.Length - 1);

        //    string[] sa = src.Split('\\');
        //    string[] sb = destRef.Split('\\');
        //    int p = 0;
        //    for (int i = 0; i < sb.Length; i++)
        //    {
        //        if (sb[i] == "..")
        //            p++;
        //        else
        //            break;
        //    }
        //    return string.Join("\\", sa, 0, sa.Length - p) + "\\" + string.Join("\\", sb, p, sb.Length - p);

        //}

        /// <summary>
        /// Creates the patch cabinet.
        /// </summary>
        /// <param name="synFileWithChanged">The syn file with changed.</param>
        /// <param name="workDir">The work dir.</param>
        /// <param name="targetCabFile">The target CAB file.</param>
        public static void CreatePatchCabinet(SynchronizationFile synFileWithChanged, string workDir, string targetCabFile)
        {
            if (!Directory.Exists(workDir))
            {
                throw new InvalidOperationException(workDir + "不是一个目录或目录不存在！");
            }

            string batFileName = workDir + "\\patch.bat.syn";
            if (synFileWithChanged.RemoveFiles.Count > 0 || synFileWithChanged.RenameList.Length > 0)
            {
                using (StreamWriter sw = new StreamWriter(batFileName, false, Encoding.Default, 500))
                {
                    if (synFileWithChanged.RemoveFiles.Count > 0)
                    {
                        foreach (SynFile dFile in synFileWithChanged.RemoveFiles.Values)
                        {
                            //writer.WriteLine("删除了文件:{0}, 文件大小:{1}bytes", dFile.FileID, dFile.FileSize);
                            sw.WriteLine("del /P /Q \"{0}\"", dFile.FileID.Replace('/', '\\'));
                        }
                    }

                    if (synFileWithChanged.RenameList.Length > 0)
                    {
                        string relativeDir = string.Empty;
                        int cIndex = -1;
                        for (int i = 0, j = synFileWithChanged.RenameList.Length; i < j; i++)
                        {
                            //writer.WriteLine("重命名{0}为{1}", synFileWithChanged.RenameList[i].OldSynFile.FileID, synFileWithChanged.RenameList[i].NewSynFile.FileID);
                            cIndex = synFileWithChanged.RenameList[i].NewSynFile.FileID.IndexOf('/');
                            if (cIndex != -1)
                            {
                                relativeDir = synFileWithChanged.RenameList[i].NewSynFile.FileID.Substring(0, cIndex);
                            }
                            else
                            {
                                relativeDir = "";
                            }

                            if (relativeDir != string.Empty)
                            {
                                sw.WriteLine("if not exist \"{0}\" mkdir \"{0}\"", relativeDir);
                            }
                            sw.WriteLine("move /Y \"{0}\" \"{1}\"",
                                synFileWithChanged.RenameList[i].OldSynFile.FileID.Replace('/', '\\'),
                                synFileWithChanged.RenameList[i].NewSynFile.FileID.Replace('/', '\\'));
                        }
                    }
                }
            }
            else
            {
                batFileName = string.Empty;
            }

            //有新增和修改文件 创建cab更新包
            string localFileName = string.Empty;
            if (synFileWithChanged.AddFiles.Count > 0 || synFileWithChanged.UpdateFiles.Count > 0)
            {
                using (CabMakeCLR imake = new CabMakeCLR(targetCabFile))
                {
                    if (synFileWithChanged.AddFiles.Count > 0)
                    {
                        foreach (SynFile aFile in synFileWithChanged.AddFiles.Values)
                        {
                            //writer.WriteLine("添加了文件:{0}, 文件大小:{1}bytes", aFile.FileID, aFile.FileSize);

                            localFileName = Path.Combine(workDir, aFile.FileID.Replace('/', '\\'));
                            imake.AddFile(localFileName, localFileName.Replace(workDir, "").Replace('/', '\\').TrimStart('\\'));
                        }
                    }

                    if (synFileWithChanged.UpdateFiles.Count > 0)
                    {
                        foreach (SynFile uFile in synFileWithChanged.UpdateFiles.Values)
                        {
                            //writer.WriteLine("修改了文件:{0}, 文件大小:{1}bytes", uFile.FileID, uFile.FileSize);

                            localFileName = Path.Combine(workDir, uFile.FileID.Replace('/', '\\'));
                            imake.AddFile(localFileName, localFileName.Replace(workDir, "").Replace('/', '\\').TrimStart('\\'));
                        }
                    }

                    if (batFileName != string.Empty)
                    {
                        imake.AddFile(batFileName, "syn.patch.bat");
                    }

                    imake.CloseCab();
                }
            }

        }


        /// <summary>
        /// Extracts the specified zip CAB file path.
        /// </summary>
        /// <param name="zipCabFilePath">The zip CAB file path.</param>
        /// <param name="targetDir">The target dir.</param>
        public static void Extract(string zipCabFilePath, string targetDir)
        {
            if (!File.Exists(zipCabFilePath)) return;
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            /*
                ' These are the available CopyHere options, according to MSDN
                ' (http://msdn2.microsoft.com/en-us/library/ms723207.aspx).
                ' On my test systems, however, the options were completely ignored.
                '      4: Do not display a progress dialog box.
                '      8: Give the file a new name in a move, copy, or rename
                '         operation if a file with the target name already exists.
                '     16: Click "Yes to All" in any dialog box that is displayed.
                '     64: Preserve undo information, if possible.
                '    128: Perform the operation on files only if a wildcard file
                '         name (*.*) is specified.
                '    256: Display a progress dialog box but do not show the file
                '         names.
                '    512: Do not confirm the creation of a new directory if the
                '         operation requires one to be created.
                '   1024: Do not display a user interface if an error occurs.
                '   4096: Only operate in the local directory.
                '         Don't operate recursively into subdirectories.
                '   8192: Do not copy connected files as a group.
                '         Only copy the specified files.
             */

            Shell sh = new Shell();
            Folder fldr = sh.NameSpace(targetDir);

            Folder currentFolder = null;
            string currentDir = string.Empty, lastDir = string.Empty;

            string internalFileName = string.Empty;
            string cabFilename = Path.GetFileName(zipCabFilePath);
            int fIndex = -1, vOptions = 4;
            foreach (FolderItem fItem in sh.NameSpace(zipCabFilePath).Items())
            {
                internalFileName = fItem.Path.Substring(fItem.Path.IndexOf(cabFilename, StringComparison.InvariantCultureIgnoreCase) + cabFilename.Length).TrimStart('\\');
                //Console.WriteLine(f.Path);
                //Console.WriteLine(internalFileName);

                fIndex = internalFileName.IndexOf('\\');
                if (fIndex != -1)
                {
                    currentDir = Path.GetDirectoryName(Path.Combine(targetDir, internalFileName));
                    if (lastDir != currentDir)
                    {
                        if (!Directory.Exists(currentDir)) Directory.CreateDirectory(currentDir);
                        currentFolder = sh.NameSpace(currentDir);

                        lastDir = currentDir;
                    }
                    currentFolder.CopyHere(fItem, vOptions);
                }
                else
                {
                    fldr.CopyHere(fItem, vOptions);
                }
            }

            currentFolder = null;
            fldr = null;
            sh = null;

        }


        public static int FindFirstNumber(string strSearch)
        {
            int iRet = -1;
            if (!string.IsNullOrEmpty(strSearch))
            {
                for (int i = 0, j = strSearch.Length; i < j; i++)
                {
                    if (Char.IsDigit(strSearch[i]))
                    {
                        iRet = i;
                        break;
                    }
                }
            }
            return iRet;
        }


    }
}
