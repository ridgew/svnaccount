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
    public sealed class IdentityFileStore : StreamStoreBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityFileStore"/> class.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        public IdentityFileStore(string filepath)
        {
            FilePath = filepath;
            bool newStore = CreatNewFile(filepath, HeadIndexLength);
            ReInitial();
            if (newStore) BuildEmptyFile();
        }

        private void ReInitial()
        {
            storeStream = null;
            InitialFileStream();

            _internalReader = null;
            InitializeReader();

            _internalWriter = null;
            InitializeWriter();
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
        static readonly long NEXT_WRITEINDEX_OFFSET = 28; //4+8+8+8

        /// <summary>
        /// 每条版本记录索引所占长度
        /// </summary>
        static readonly int SINGLE_VERSION_UNIT = 28;

        private bool CanWriteVersion(uint newVer)
        {
            if (storeStream.Length == 0)
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
        /// <returns>压缩是否成功</returns>
        public bool CompactFile()
        {
            return RefactHeadIndex(0);
        }

        private void BuildEmptyFile()
        {
            //新建文件
            storeStream.SetLength((long)HeadIndexLength);

            _internalWriter.Write(Encoding.ASCII.GetBytes("IFS 1.0 "));                             //文件版本                      +8
            _internalWriter.Write(BitConverter.GetBytes(HeadIndexLength));                          //索引空间长度                  +4
            _internalWriter.Write(BitConverter.GetBytes((int)0));                                   //文件头索引偏移量              +4

            _internalWriter.Write(BitConverter.GetBytes((uint)0));                                  //当前版本                      +4
            _internalWriter.Write(BitConverter.GetBytes(0L));                                       //数据文件长度                  +8
            _internalWriter.Write(BitConverter.GetBytes((int)61));                                  //文件头结束索引(下次写入位置)  +4
            _internalWriter.Write('\n');                                                            //                              +1

            _internalWriter.Write(BitConverter.GetBytes((uint)0));                                  //数据版本                      +4
            _internalWriter.Write(BitConverter.GetBytes(DateTime.Now.ToUniversalTime().Ticks));     //创建时间                      +8
            _internalWriter.Write(BitConverter.GetBytes((long)HeadIndexLength));                    //数据开始开始所在索引          +8
            _internalWriter.Write(BitConverter.GetBytes(0L));                                       //数据文件长度                  +8
        }

        /// <summary>
        /// 写入指定版本的文件数据
        /// </summary>
        /// <param name="version">文件版本</param>
        /// <param name="fDat">文件二进制字节数据</param>
        public void WriteReversion(uint version, byte[] fDat)
        {
            if (!CanWriteVersion(version)) throw new InvalidOperationException("写入的数据版本必须高于版本：" + GetHeadVersion());

            int datWOffset = HeadIndexLength;  //数据写入索引位置
            _internalReader.BaseStream.Position = DATA_INDEX_OFFSET - 4;
            datWOffset = _internalReader.ReadInt32();                                               //读取索引空间长度              +4
            //Console.WriteLine("索引空间长度为：{0}", datWOffset);

            _internalReader.BaseStream.Seek(NEXT_WRITEINDEX_OFFSET, SeekOrigin.Begin);
            int curHWIdx = _internalReader.ReadInt32();

            //Console.WriteLine("文件头索引写入位置：{0}", curHWIdx);
            if (curHWIdx > datWOffset - SINGLE_VERSION_UNIT)
            {
                //Console.WriteLine("索引空间增加为：{0}", datWOffset + HeadIndexLength);
                //Console.WriteLine("Refact: {0}", RefactHeadIndex(datWOffset + HeadIndexLength));
                RefactHeadIndex(datWOffset + HeadIndexLength);
                ReInitial();
            }

            _internalWriter.Seek(LASTED_VERSION_OFFSET, SeekOrigin.Begin);
            _internalWriter.Write(BitConverter.GetBytes(version));                                      //当前版本                      +4
            _internalWriter.Write(BitConverter.GetBytes(fDat.LongLength));                              //数据文件长度                  +8
            _internalWriter.Write(BitConverter.GetBytes(curHWIdx + SINGLE_VERSION_UNIT));               //文件头结束索引(下次写入位置)  +4

            _internalWriter.Seek(curHWIdx, SeekOrigin.Begin);
            _internalWriter.Write(BitConverter.GetBytes(version));                                      //数据版本                      +4
            _internalWriter.Write(BitConverter.GetBytes(DateTime.Now.ToUniversalTime().Ticks));         //创建时间                      +8
            long lwIdx = GetOffSetDat<long>(_internalReader, (long)(curHWIdx - 16));
            long lwLen = GetOffSetDat<long>(_internalReader, (long)(curHWIdx - 8));
            _internalWriter.Write(BitConverter.GetBytes(lwIdx + lwLen));                                //数据开始开始所在索引          +8
            _internalWriter.Write(BitConverter.GetBytes(fDat.LongLength));                              //数据文件长度                  +8

            datWOffset = (int)storeStream.Length;

            _internalWriter.Seek(datWOffset, SeekOrigin.Begin);
            _internalWriter.Write(fDat);
        }

        /// <summary>
        /// 重新调整索引文件头空间（增加或压缩）
        /// </summary>
        /// <param name="idxNewSize">新的索引空间大小，为0则执行压缩。</param>
        /// <param name="outStream">调整大小时的输出字节序列</param>
        /// <returns>是否进行了相关操作</returns>
        private bool RefactHeadIndexSize(int idxNewSize, Stream outStream)
        {
            int oldIdxSize = GetIndexSize();
            //索引空间大小不变
            if (oldIdxSize == idxNewSize) return false;

            int oldOffSet = (int)GetDataReadOffset();
            int iTotalIdx = GetNextIndexWriteOffset();
            long IdxOffset = GetDataIndexOffset();

            //执行压缩
            if (iTotalIdx > idxNewSize) idxNewSize = iTotalIdx;
            long nFileLen = storeStream.Length + idxNewSize - oldIdxSize;
            int nOffSet = oldOffSet + idxNewSize - oldIdxSize;

            //Console.WriteLine("新文件长度：{0}， 数据偏移量：{1}", nFileLen, oldOffSet);
            //Console.WriteLine("新偏移量：{0}", nOffSet);
            //Console.WriteLine("旧存储空间：{0}", oldIdxSize);
            //Console.WriteLine("已占用空间：{0}", iTotalIdx);
            //Console.WriteLine("修改后空间：{0}", idxNewSize);

            //更新索引偏移量条件
            if (nFileLen <= 0 || nOffSet == oldOffSet) return false;

            #region 复制旧索引及开始数据
            byte[] buffer = new byte[oldIdxSize];
            outStream.SetLength(nFileLen);
            outStream.Position = 0;
            storeStream.Position = 0;
            storeStream.Read(buffer, 0, oldIdxSize);
            outStream.Write(buffer, 0, oldIdxSize);
            #endregion

            #region 更新偏移量
            if (IndexSizeChange != null) IndexSizeChange(idxNewSize, outStream);
            if (IdxOffset > 0)
            {
                outStream.Position = IdxOffset;
                buffer = new byte[4];
                buffer = BitConverter.GetBytes(nOffSet);
                //Console.WriteLine("修改新偏移量为：{0}", nOffSet);
                outStream.Write(buffer, 0, buffer.Length);
            }
            #endregion

            #region 分段读取并写入
            outStream.Position = idxNewSize;
            //Console.WriteLine("POS：{0}", idxNewSize); nOffSet
            //Console.WriteLine("Offset New：{0}", nOffSet); 

            int currentRead = 0;

            int lTest = 4096;
            buffer = new byte[lTest];
            while ((currentRead = storeStream.Read(buffer, 0, lTest)) != 0)
            {
                //Console.WriteLine("read:{0}", currentRead);
                outStream.Write(buffer, 0, currentRead);
                outStream.Flush();
                //Console.WriteLine();
                //Console.WriteLine(Utility.FileWrapHelper.GetHexViewString(buffer));
                //Console.WriteLine();
            }
            outStream.Flush();
            #endregion

            return true;
        }

        /// <summary>
        /// 重新调整索引文件头空间（增加或压缩）
        /// </summary>
        /// <param name="idxNewSize">新的索引空间大小，为0则执行压缩。</param>
        /// <returns>是否进行了相关操作</returns>
        internal bool RefactHeadIndex(int idxNewSize)
        {
            long oldPos = base.storeStream.Position;

            string nFileName = FilePath + ".tmp";
            FileStream nFStream = new FileStream(nFileName, FileMode.Create, FileAccess.Write, FileShare.None);

            base.IndexSizeChange = new RefreshIndexSizeChange((size, stm) => {
                    byte[] buffer = new byte[4];
                    buffer = BitConverter.GetBytes(size);
                    stm.Position = DATA_INDEX_OFFSET - 4;
                    stm.Write(buffer, 0, buffer.Length);
                });

            bool result = RefactHeadIndexSize(idxNewSize, nFStream);
            nFStream.Close();
            nFStream.Dispose();
            if (result == false)  File.Delete(nFileName);

            #region 覆盖旧文件，并还原索引位置信息
            if (result == true)
            {
                base.storeStream.Close();
                base.storeStream.Dispose();
                base.storeStream = null;

                File.Delete(FilePath);
                //Console.WriteLine("Delete {0} OK!", FilePath);

                FileInfo nFileInfo = new FileInfo(nFileName);
                nFileInfo.MoveTo(FilePath);
            }
            #endregion

            ReInitial();

            if (oldPos < base.storeStream.Length)
            {
                base.storeStream.Position = oldPos;
            }

            return result;
        }

        /// <summary>
        /// 获取最新的版本(0)
        /// </summary>
        public byte[] GetHeadVersionData()
        {
            return ReadReversion(0);
        }

        /// <summary>
        /// 获取指定版本的二进制字节数据
        /// </summary>
        /// <param name="version">文件版本</param>
        /// <returns>该文件二进制字节数据，如果指定版本不存在则长度为0的字节数组。</returns>
        public byte[] ReadReversion(uint ver)
        {
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
                VersionSnippet[] verDat = GetAllVersionsWithBreak(spt => (spt.Version > ver)); 
                VersionSnippet lastMV = verDat[verDat.Length - 1];

                //目标版本处在大于下一小版本且小于终止版本时，获取下一小版本
                if (lastMV.Version > ver && verDat.Length>=2) lastMV = verDat[verDat.Length - 2];

                fIdx = lastMV.StoreIndex;
                fLen = lastMV.FileLength;
            }
            fIdx += offSet;
            return base.ReadData(fIdx, (int)fLen);
        }

        private void InitialFileStream()
        {
            if (base.storeStream == null)
            {
                storeStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            }
        }

        internal static bool CreatNewFile(string filePath, long? length)
        {
            if (!File.Exists(filePath))
            {
                FileInfo newFile = new FileInfo(filePath);
                using (FileStream nFS = newFile.Create())
                {
                    nFS.SetLength(length.HasValue ? length.Value : 0);
                    nFS.Close();
                }
                return true;
            }
            return false;
        }

        private uint GetMaxVersion(BinaryReader reader)
        {
            if (_maxVersion == 0 && reader != null)
            {
                if (reader.BaseStream.Length > 0)
                {
                    long oldPos = reader.BaseStream.Position;
                    reader.BaseStream.Seek((long)LASTED_VERSION_OFFSET, SeekOrigin.Begin);
                    _maxVersion = reader.ReadUInt32(); //最后修改版本 
                    reader.BaseStream.Position = oldPos;
                }
            }
            return _maxVersion;
        }

        /// <summary>
        /// 获取实现的文件版本
        /// </summary>
        public override string GetStoreVersion()
        {
            long oldPos = _internalReader.BaseStream.Position;
            byte[] fvBytes = _internalReader.ReadBytes(8);
            _internalReader.BaseStream.Position = oldPos;
            return Encoding.ASCII.GetString(fvBytes);
        }

        /// <summary>
        /// 获取索引数据大小
        /// </summary>
        public override int GetIndexSize()
        {
            return GetOffSetDat<int>(_internalReader, DATA_INDEX_OFFSET - 4);
        }

        /// <summary>
        /// 获取非数据区的文件头数据大小
        /// </summary>
        public override int GetFileHeadSize()
        {
            return GetIndexSize();
        }

        /// <summary>
        /// 获取数据索引偏移量值保存的偏移位置
        /// </summary>
        /// <returns></returns>
        public override long GetDataIndexOffset()
        {
            return DATA_INDEX_OFFSET;
        }

        /// <summary>
        /// 获取读取数据位置开始的偏移量(默认为0，即索引后紧跟数据)
        /// </summary>
        /// <returns></returns>
        public override long GetDataReadOffset()
        {
            return GetOffSetDat<int>(_internalReader, DATA_INDEX_OFFSET);
        }

        /// <summary>
        /// 获取下次索引写入位置的偏移量
        /// </summary>
        public override int GetNextIndexWriteOffset()
        {
            return GetOffSetDat<int>(_internalReader, NEXT_WRITEINDEX_OFFSET);
        }

        /// <summary>
        /// 获取数据的最新版本号
        /// </summary>
        public uint GetHeadVersion()
        {
            if (_headVer == 0)
            {
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
                _footVer = GetOffSetDat<uint>(_internalReader, NEXT_WRITEINDEX_OFFSET + 5);
            }
            return _footVer;
        }

        /// <summary>
        /// 获取该文件保存的所有版本信息
        /// </summary>
        /// <returns></returns>
        public VersionSnippet[] GetAllVersions()
        {
            return GetAllVersionsWithBreak(null);
        }

        /// <summary>
        /// 获取该文件保存的所有版本信息
        /// </summary>
        /// <param name="breakMatch">终止读取版本信息的判断</param>
        /// <returns></returns>
        public VersionSnippet[] GetAllVersionsWithBreak(Predicate<VersionSnippet> breakMatch)
        {
            List<VersionSnippet> vers = new List<VersionSnippet>();

            uint tVer = GetMaxVersion(_internalReader); //最后修改版本
            _internalReader.BaseStream.Seek(NEXT_WRITEINDEX_OFFSET + 5, SeekOrigin.Begin); //第一个版本索引位置
            uint cVer = _internalReader.ReadUInt32();
            VersionSnippet snippet;
            bool midBreaked = false;

            while (cVer != tVer)
            {
                snippet = new VersionSnippet();
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
                snippet = new VersionSnippet();
                snippet.Version = cVer;
                snippet.CreateTimeUTC = DateTime.FromBinary(_internalReader.ReadInt64());
                snippet.StoreIndex = _internalReader.ReadInt64();
                snippet.FileLength = _internalReader.ReadInt64();
                vers.Add(snippet);
            }
            return vers.ToArray();
        }
        
    }

    /// <summary>
    /// 标识文件数据存储片段
    /// </summary>
    public struct VersionSnippet
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
