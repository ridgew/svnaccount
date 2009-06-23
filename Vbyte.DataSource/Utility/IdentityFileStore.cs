using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Vbyte.DataSource.Configuration;

namespace Vbyte.DataSource.Utility
{
    /// <summary>
    /// 唯一标识的版本数据存储实现
    /// </summary>
    [ImplementVersion("IFS 1.0", Description = "同一标识数据的多版本控制单一文件存储实现")]
    public sealed class IdentityFileStore : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityFileStore"/> class.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        public IdentityFileStore(string filepath)
        {
            FilePath = filepath;
        }

        private string _dbPath;
        /// <summary>
        /// 数据文件存储路径
        /// </summary>
        public string FilePath
        {
            get { return _dbPath; }
            set { _dbPath = value; }
        }


        private int _hIdxSize = 2048; //2k的索引文件存储空间
        /// <summary>
        /// 索引数据文件存储长度，默认为2048字节。
        /// </summary>
        public int HeadIndexLength
        {
            get { return _hIdxSize; }
            set { _hIdxSize = value; }
        }

        private BinaryReader _internalReader;
        private BinaryWriter _internalWriter;
        private FileStream _internalFS;
        private bool _StoreReadMode = false;
        private uint _maxVersion = 0;

        /// <summary>
        /// 数据索引偏移量记录索引位置
        /// </summary>
        static readonly long DATA_INDEX_OFFSET = 12;
        /// <summary>
        /// 最新版本记录偏移量
        /// </summary>
        static readonly int LASTED_VERSION_OFFSET = 16;
        /// <summary>
        /// 下次索引记录写入位置偏移量
        /// </summary>
        static readonly long NEXT_WRITEINDEX_OFFSET = 28;

        private bool CanWriteVersion(uint newVer)
        {
            SwitchMode(false); 
            if (_internalFS.Length == 0)
            {
                return true;
            }
            else
            {
                uint min = GetFootVersion();
                uint max = GetHeadVersion();
                return max < newVer;
            }
        }

        /// <summary>
        /// 压缩多余的索引空间
        /// </summary>
        public void CompactFile()
        {
            RefactHeadIndex(0);
        }

        /// <summary>
        /// 写入指定版本的文件数据
        /// </summary>
        /// <param name="version">文件版本</param>
        /// <param name="fDat">文件二进制字节数据</param>
        public void WriteReversion(uint version, byte[] fDat)
        {
            if (!CanWriteVersion(version)) throw new InvalidOperationException("写入的数据版本必须高于版本：" + GetHeadVersion());
            SwitchMode(false);
            
            _internalWriter = new BinaryWriter(_internalFS);
            int datWOffset = _hIdxSize;  //数据写入索引位置
            if (_internalFS.Length == 0)
            {
                #region 初次创建文件
                //新建文件
                _internalFS.SetLength((long)_hIdxSize + fDat.LongLength);

                _internalWriter.Write(Encoding.ASCII.GetBytes("IFS 1.0 "));                             //文件版本                      +8
                _internalWriter.Write(BitConverter.GetBytes(_hIdxSize));                                //索引空间长度                  +4
                _internalWriter.Write(BitConverter.GetBytes((int)0));                                   //文件头索引偏移量              +4

                _internalWriter.Write(BitConverter.GetBytes(version));                                  //当前版本                      +4
                _internalWriter.Write(BitConverter.GetBytes(fDat.LongLength));                          //数据文件长度                  +8
                _internalWriter.Write(BitConverter.GetBytes((int)61));                                  //文件头结束索引(下次写入位置)  +4
                _internalWriter.Write('\n');                                                            //                              +1

                _internalWriter.Write(BitConverter.GetBytes(version));                                  //数据版本                      +4
                _internalWriter.Write(BitConverter.GetBytes(DateTime.Now.ToUniversalTime().Ticks));     //创建时间                      +8
                _internalWriter.Write(BitConverter.GetBytes((long)_hIdxSize));                          //数据开始开始所在索引          +8
                _internalWriter.Write(BitConverter.GetBytes(fDat.LongLength));                          //数据文件长度                  +8
                #endregion
            }
            else
            {
                _internalReader.BaseStream.Position = 8;
                datWOffset = _internalReader.ReadInt32();                                                   //读取索引空间长度              +4
                //Console.WriteLine("索引空间长度为：{0}", datWOffset);

                _internalReader.BaseStream.Seek(NEXT_WRITEINDEX_OFFSET, SeekOrigin.Begin);
                int curHWIdx = _internalReader.ReadInt32();

                int storeSptLen = 28; //4+8+8+8
                //Console.WriteLine("文件头索引写入位置：{0}", curHWIdx);
                if (curHWIdx > datWOffset - storeSptLen)
                {
                    //Console.WriteLine("索引空间增加为：{0}", datWOffset + HeadIndexLength);
                    RefactHeadIndex(datWOffset + HeadIndexLength);
                }

                _internalWriter.Seek(LASTED_VERSION_OFFSET, SeekOrigin.Begin);
                _internalWriter.Write(BitConverter.GetBytes(version));                                      //当前版本                      +4
                _internalWriter.Write(BitConverter.GetBytes(fDat.LongLength));                              //数据文件长度                  +8
                _internalWriter.Write(BitConverter.GetBytes(curHWIdx + storeSptLen));                       //文件头结束索引(下次写入位置)  +4

                _internalWriter.Seek(curHWIdx, SeekOrigin.Begin);
                _internalWriter.Write(BitConverter.GetBytes(version));                                      //数据版本                      +4
                _internalWriter.Write(BitConverter.GetBytes(DateTime.Now.ToUniversalTime().Ticks));         //创建时间                      +8
                long lwIdx = GetOffSetDat<long>(_internalReader, (long)(curHWIdx - 16));
                long lwLen = GetOffSetDat<long>(_internalReader, (long)(curHWIdx - 8));
                _internalWriter.Write(BitConverter.GetBytes(lwIdx + lwLen));                                //数据开始开始所在索引          +8
                _internalWriter.Write(BitConverter.GetBytes(fDat.LongLength));                              //数据文件长度                  +8

                datWOffset = (int)_internalFS.Length;
            }

            _internalWriter.Seek(datWOffset, SeekOrigin.Begin);
            _internalWriter.Write(fDat);
        }

        /// <summary>
        /// 重新调整索引文件头空间（增加或压缩）
        /// </summary>
        internal void RefactHeadIndex(int idxNewSize)
        {
            InitialFileStream();
            long oldPos = _internalFS.Position;
    
            InitializeReader(); 
            int oldSize = GetOffSetDat<int>(_internalReader, 8L);
            if (oldSize == idxNewSize) return;

            int oldOffSet = GetOffSetDat<int>(_internalReader, DATA_INDEX_OFFSET);
            int iTotalIdx = GetOffSetDat<int>(_internalReader, NEXT_WRITEINDEX_OFFSET);

            //执行压缩
            if (iTotalIdx > idxNewSize) idxNewSize = iTotalIdx;

            //Console.WriteLine("旧存储空间：{0}", oldSize);
            //Console.WriteLine("已占用空间：{0}", iTotalIdx);
            //Console.WriteLine("修改后空间：{0}", idxNewSize);

            string nFileName = FilePath + ".tmp";
            FileStream nFStream = new FileStream(nFileName, FileMode.Create, FileAccess.Write, FileShare.None);

            byte[] buffer = new byte[oldSize];
            long nFileLen = _internalFS.Length + idxNewSize - oldSize;
            //更新索引偏移量
            int nOffSet = oldOffSet + idxNewSize - oldSize;

            if (nFileLen > 0)
            {
                nFStream.SetLength(nFileLen);
            }

            nFStream.Position = 0;
            _internalFS.Position = 0;
            _internalFS.Read(buffer, 0, oldSize);
            nFStream.Write(buffer, 0, oldSize);

            #region 分段读取并写入
            nFStream.Position = idxNewSize;
            //Console.WriteLine("POS：{0}", idxNewSize);

            int currentRead = 0;

            int lTest = 4096;
            buffer = new byte[lTest];
            while ((currentRead = _internalFS.Read(buffer, 0, lTest)) != 0)
            {
                //Console.WriteLine("read:{0}", currentRead);
                nFStream.Write(buffer, 0, currentRead);

                //Console.WriteLine();
                //Console.WriteLine(Utility.FileWrapHelper.GetHexViewString(buffer));
                //Console.WriteLine();
            }
            nFStream.Flush();

            //更新偏移量
            nFStream.Position = DATA_INDEX_OFFSET;
            buffer = new byte[4];
            buffer = BitConverter.GetBytes(nOffSet);
            //Console.WriteLine("修改新偏移量为：{0}", nOffSet);
            nFStream.Write(buffer, 0, buffer.Length);

            nFStream.Close();
            nFStream.Dispose();
            #endregion

            #region 覆盖旧文件，并还原索引位置信息
            _internalFS.Close();
            _internalFS.Dispose();
            File.Delete(FilePath);

            FileInfo nFileInfo = new FileInfo(nFileName);
            nFileInfo.MoveTo(FilePath);

            SwitchFileStream();
            if (oldPos < _internalFS.Length)
            {
                _internalFS.Position = oldPos;
            }
            #endregion
        }

        /// <summary>
        /// 获取最新的版本(0)
        /// </summary>
        public byte[] GetHeadVersionData()
        {
            return ReadReversion(0);
        }

        /// <summary>
        /// 切换数据交换模式
        /// </summary>
        /// <param name="isReadMode">是否是读取模式</param>
        private void SwitchMode(bool isReadMode)
        {
            if (isReadMode)
            {
                if (!_StoreReadMode)
                {
                    _StoreReadMode = true;
                    SwitchFileStream();
                }
                else
                {
                    InitialFileStream();
                }
            }
            else
            {
                if (_StoreReadMode)
                {
                    _StoreReadMode = false;
                    SwitchFileStream();
                }
                else
                {
                    InitialFileStream();
                }
            }
            InitializeReader();
        }

        /// <summary>
        /// 获取指定版本的二进制字节数据
        /// </summary>
        /// <param name="version">文件版本</param>
        /// <returns>该文件二进制字节数据，如果指定版本不存在则长度为0的字节数组。</returns>
        public byte[] ReadReversion(uint ver)
        {
            SwitchMode(true);

            //获取最高版本
            uint maxVer = GetMaxVersion(_internalReader); //最后修改版本
            int offSet = GetOffSetDat<int>(_internalReader, DATA_INDEX_OFFSET);
            //Console.WriteLine("OffSET：{0}", offSet);

            long fIdx=0, fLen=0;
            if (ver == 0 || ver >= maxVer)
            {
                //最高版本数据的数据长度
                _internalReader.BaseStream.Seek((long)(LASTED_VERSION_OFFSET + 4), SeekOrigin.Begin);
                fLen = _internalReader.ReadInt64();

                int fLenIdx = _internalReader.ReadInt32();
                _internalReader.BaseStream.Seek(fLenIdx-16, SeekOrigin.Begin);
                fIdx = _internalReader.ReadInt64();
            }
            else
            {
                //当版本高于检索版本时终止
                StoreSnippet[] verDat = GetAllVersionsWithBreak(spt => (spt.Version > ver)); 
                StoreSnippet lastMV = verDat[verDat.Length - 1];

                //目标版本处在大于下一小版本且小于终止版本时，获取下一小版本
                if (lastMV.Version > ver && verDat.Length>=2) lastMV = verDat[verDat.Length - 2];

                fIdx = lastMV.StoreIndex;
                fLen = lastMV.FileLength;
            }

            byte[] fDat = new byte[fLen];

            //索引修改为和偏移量的新值(OK)
            fIdx += offSet;

            _internalFS.Seek(fIdx, SeekOrigin.Begin);
            _internalFS.Read(fDat, 0, (int)fLen);
            //Console.WriteLine("文件索引：{0}", fIdx);
            //Console.WriteLine("文件长度：{0}", fLen);
            return fDat;
        }

        private void InitialFileStream()
        {
            if (_internalFS == null) SwitchFileStream();
        }

        private void CreatNewFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                FileInfo newFile = new FileInfo(filePath);
                using (FileStream nFS = newFile.Create())
                {
                    nFS.SetLength(0);
                    nFS.Close();
                }
            }
        }

        private void SwitchFileStream()
        {
            if (_internalFS != null)
            {
                if (_StoreReadMode)
                {
                    if (_internalFS.CanRead) return;
                }
                else
                {
                    if (_internalFS.CanWrite) return;
                }

                _internalFS.Close();
                _internalFS.Dispose();
            }

            CreatNewFile(FilePath);
            if (_StoreReadMode == true)
            {
                _internalFS = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            else
            {
                _internalFS = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Write);
            }
            //Console.WriteLine("new switch ....");
        }

        private void InitializeReader()
        {
            if (_internalReader == null)  _internalReader = new BinaryReader(_internalFS);
        }

        private uint GetMaxVersion(BinaryReader reader)
        {
            if (_maxVersion == 0 && reader != null)
            {
                long oldPos = reader.BaseStream.Position;
                reader.BaseStream.Seek((long)LASTED_VERSION_OFFSET, SeekOrigin.Begin);
                _maxVersion = reader.ReadUInt32(); //最后修改版本 
                reader.BaseStream.Position = oldPos;
            }
            return _maxVersion;
        }

        /// <summary>
        /// 暂时只支持int,long,uint
        /// </summary>
        private T GetOffSetDat<T>(BinaryReader reader, long offset)
            where T : IConvertible
        {
            T objRet = default(T);
            long oldPos = reader.BaseStream.Position;
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            object objRead = 0;
            switch (objRet.GetTypeCode())
            { 
                case TypeCode.Int32 :
                    objRead = reader.ReadInt32();
                    break;
                case TypeCode.Int64 :
                    objRead = reader.ReadInt64();
                    break;
                case TypeCode.UInt32 :
                    objRead = reader.ReadUInt32();
                    break;
                default :
                    break;
            }
            objRet = (T)Convert.ChangeType(objRead, typeof(T)); //偏移量
            reader.BaseStream.Position = oldPos;
            return objRet;
        }

        /// <summary>
        /// 获取实现的文件版本
        /// </summary>
        public string GetFileVersion()
        {
            SwitchMode(true);
            long oldPos = _internalReader.BaseStream.Position;
            byte[] fvBytes = _internalReader.ReadBytes(8);
            _internalReader.BaseStream.Position = oldPos;
            return Encoding.ASCII.GetString(fvBytes);
        }

        /// <summary>
        /// 获取索引数据大小
        /// </summary>
        public int GetIndexSize()
        {
            SwitchMode(true);
            return GetOffSetDat<int>(_internalReader, DATA_INDEX_OFFSET - 4);
        }

        /// <summary>
        /// 获取数据保存的偏移量
        /// </summary>
        public int GetDataOffset()
        {
            SwitchMode(true);
            return GetOffSetDat<int>(_internalReader, DATA_INDEX_OFFSET);
        }

        /// <summary>
        /// 获取数据的最新版本号
        /// </summary>
        public uint GetHeadVersion()
        {
            if (_headVer == 0)
            {
                SwitchMode(true);
                _headVer = GetMaxVersion(_internalReader);
            }
            return _headVer;
        }

        private uint _headVer=0 , _footVer = 0;

        /// <summary>
        /// 获取数据的最低版本号
        /// </summary>
        public uint GetFootVersion()
        {
            if (_footVer == 0)
            {
                SwitchMode(true);
                _footVer = GetOffSetDat<uint>(_internalReader, NEXT_WRITEINDEX_OFFSET + 5);
            }
            return _footVer;
        }

        /// <summary>
        /// 获取该文件保存的所有版本信息
        /// </summary>
        /// <returns></returns>
        public StoreSnippet[] GetAllVersions()
        {
            return GetAllVersionsWithBreak(null);
        }

        /// <summary>
        /// 获取该文件保存的所有版本信息
        /// </summary>
        /// <param name="breakMatch">终止读取版本信息的判断</param>
        /// <returns></returns>
        public StoreSnippet[] GetAllVersionsWithBreak(Predicate<StoreSnippet> breakMatch)
        {
            SwitchMode(true);
            List<StoreSnippet> vers = new List<StoreSnippet>();

            uint tVer = GetMaxVersion(_internalReader); //最后修改版本
            _internalReader.BaseStream.Seek(NEXT_WRITEINDEX_OFFSET + 5, SeekOrigin.Begin); //第一个版本索引位置
            uint cVer = _internalReader.ReadUInt32();
            StoreSnippet snippet;
            bool midBreaked = false;

            while (cVer != tVer)
            {
                snippet = new StoreSnippet();
                snippet.Version = cVer;
                snippet.CreateTimeUTC = DateTime.FromBinary(_internalReader.ReadInt64());
                snippet.StoreIndex = _internalReader.ReadInt64();
                snippet.FileLength = _internalReader.ReadInt64();
                vers.Add(snippet);

                if (breakMatch != null && breakMatch(snippet))
                {
                    midBreaked = true;
                    break;
                }

                cVer = _internalReader.ReadUInt32();
            }

            if (cVer == tVer && midBreaked == false)
            {
                snippet = new StoreSnippet();
                snippet.Version = cVer;
                snippet.CreateTimeUTC = DateTime.FromBinary(_internalReader.ReadInt64());
                snippet.StoreIndex = _internalReader.ReadInt64();
                snippet.FileLength = _internalReader.ReadInt64();
                vers.Add(snippet);
            }
            return vers.ToArray();
        }


        #region IDisposable 成员

        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            if (_internalReader != null) _internalReader.Close();
            if (_internalWriter != null) _internalWriter.Close();
            if (_internalFS != null)
            {
                _internalFS.Close();
                _internalFS.Dispose();
            }
        }

        #endregion
    }

    /// <summary>
    /// 标识文件数据存储片段
    /// </summary>
    public struct StoreSnippet
    {
        /// <summary>
        /// 数据版本
        /// </summary>
        public uint Version { get; set; }

        /// <summary>
        /// 存储文件中数据开始索引
        /// </summary>
        public long StoreIndex { get; set; }

        /// <summary>
        /// 该段文件长度
        /// </summary>
        public long FileLength { get; set; }

        /// <summary>
        /// 创建时间的UTC格式
        /// </summary>
        public DateTime CreateTimeUTC { get; set; }
    }

}
