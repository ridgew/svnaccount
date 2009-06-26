using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Vbyte.DataSource.Configuration;

namespace Vbyte.DataSource.Utility
{
    /// <summary>
    /// 键值为基础的数据文件存储实现
    /// </summary>
    [ImplementVersion("KVS 1.0", Description = "键值为基础的单一文件存储实现")]
    public sealed class KeyValueFileStore : StreamStoreBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueFileStore"/> class.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        public KeyValueFileStore(string filepath)
        {
            FilePath = filepath;

            HeadIndexLength *= 100;
            //225,309 字节
            bool newStore = IdentityFileStore.CreatNewFile(filepath,
                HEAD_SUMMARY_BYTES + HeadIndexLength + MAX_DIRTYBLOCK_SIZE);

            ReInitial();

            if (newStore) BuildEmptyFile();
        }

        private void ReInitial()
        {
            storeStream = null;

            InitialFileStream();
            InitializeReader();
            InitializeWriter();
        }

        private void Reset()
        {
            _currentIndexSize = 0;
            _verStr = null;
        }

        private void InitialFileStream()
        {
            if (base.storeStream == null)
            {
                storeStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            }
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

        #region imp
        private string _verStr;
        /// <summary>
        /// 获取存储系统实现的版本描述
        /// </summary>
        /// <returns></returns>
        public override string GetStoreVersion()
        {
            if (string.IsNullOrEmpty(_verStr))
            {
                long oldPos = _internalReader.BaseStream.Position;
                byte[] fvBytes = _internalReader.ReadBytes(8);
                _internalReader.BaseStream.Position = oldPos;
                _verStr = Encoding.ASCII.GetString(fvBytes);
            }
            return _verStr;
        }

        private int _currentIndexSize = 0;
        /// <summary>
        /// 获取索引数据大小
        /// </summary>
        /// <returns></returns>
        public override int GetIndexSize()
        {
            if (_currentIndexSize < HeadIndexLength)
            {
               _currentIndexSize = GetOffSetDat<int>(_internalReader, 8);
            }
            return _currentIndexSize;
        }

        /// <summary>
        /// 获取数据索引偏移量值保存的偏移位置
        /// </summary>
        /// <returns></returns>
        public override long GetDataIndexOffset()
        {
            return -1;
        }

        /// <summary>
        /// 获取读取数据位置开始的偏移量(默认值为0，即索引后紧跟数据)
        /// </summary>
        /// <returns></returns>
        public override long GetDataReadOffset()
        {
            return 0;
        }

        /// <summary>
        /// 获取下次索引写入位置的偏移量
        /// </summary>
        /// <returns></returns>
        public override int GetNextIndexWriteOffset()
        {
            return HEAD_SUMMARY_BYTES;
        }
        #endregion

        /// <summary>
        /// 索引空间增加步长
        /// </summary>
        static readonly int INDEX_INCREMENT_STEP = 204800;  //200K
        /// <summary>
        /// 最大的脏数据保持空间
        /// </summary>
        static readonly int MAX_DIRTYBLOCK_SIZE = 20480;   //20K
        /// <summary>
        /// 文件头描述所占字节数
        /// </summary>
        static readonly int HEAD_SUMMARY_BYTES = 29;

        private void BuildEmptyFile()
        {
            _internalWriter.Write(Encoding.ASCII.GetBytes("KVS 1.0 "));                             //文件版本                      +8
            _internalWriter.Write(BitConverter.GetBytes(HeadIndexLength));                          //索引空间长度                  +4

            byte[] idxBytes = FileWrapHelper.GetBytes(new SortedList<string, KeyValueState>(StringComparer.Ordinal));
            //Console.WriteLine(idxBytes.Length);
            WriteData(HEAD_SUMMARY_BYTES, idxBytes);
            _internalWriter.Write(BitConverter.GetBytes(idxBytes.Length));                          //索引有效数据长度              +4

            _internalWriter.Write(BitConverter.GetBytes(MAX_DIRTYBLOCK_SIZE));                      //DirtyBlock空间长度            +4

            byte[] dbBytes = FileWrapHelper.GetBytes(new SortedList<long, DirtyBlock>());
            //Console.WriteLine(dbBytes.Length);
            WriteData(HEAD_SUMMARY_BYTES + HeadIndexLength, dbBytes);
            _internalWriter.Write(BitConverter.GetBytes(dbBytes.Length));                           //DirtyBlock有效数据长度        +4
            _internalWriter.Write(BitConverter.GetBytes((int)0));                                   //所有键值总数                  +4
            _internalWriter.Write('\n');                                                            //                              +1
        }

        public int GetIndexRealSize()
        {
            return GetOffSetDat<int>(_internalReader, 8 + 4);
        }

        public int GetDirtyBlockRealSize()
        {
            return GetOffSetDat<int>(_internalReader, HEAD_SUMMARY_BYTES - 9);
        }

        private SortedList<string, KeyValueState> IndexObject;
        internal SortedList<string, KeyValueState> GetIndexObject(int? idxCount)
        {
            if (IndexObject == null)
            {
                byte[] idxBytes = ReadData((long)GetNextIndexWriteOffset(),
                    (idxCount.HasValue && idxCount.Value != 0) ? idxCount.Value : GetIndexRealSize());
                //Console.WriteLine("Idx Len: {0}", idxBytes.Length);
                if (idxBytes.Length > 0)
                {
                    //Console.WriteLine("Restore");
                    IndexObject = FileWrapHelper.GetObject<SortedList<string, KeyValueState>>(idxBytes);
                }
                else
                {
                    IndexObject = new SortedList<string, KeyValueState>(StringComparer.Ordinal);
                }
            }
            return IndexObject;
        }

        private SortedList<long, DirtyBlock> DirtyObject = null;
        internal SortedList<long, DirtyBlock> GetStoreDirtyData()
        {
            if (DirtyObject == null)
            {
                int dCount = GetOffSetDat<int>(_internalReader, HEAD_SUMMARY_BYTES - 9);
                if (dCount > 0)
                {
                    byte[] idxBytes = ReadData((long)(HEAD_SUMMARY_BYTES + GetIndexSize()), dCount);
                    if (idxBytes.Length > 0)
                    {
                        DirtyObject = FileWrapHelper.GetObject<SortedList<long, DirtyBlock>>(idxBytes);
                    }
                }
                else
                {
                    DirtyObject = new SortedList<long, DirtyBlock>();
                }
            }
            return DirtyObject;
        }

        public int GetKeyCount()
        {
            //SortedList<string, KeyValueState> idxObject = GetIndexObject(null);
            //if (idxObject != null)
            //{
            //    return idxObject.Count;
            //}
            //else
            //{
            //    return 0;
            //}
            return GetOffSetDat<int>(_internalReader, HEAD_SUMMARY_BYTES - 5);
        }

        private void SetKeyCount(int count)
        {
            WriteData(HEAD_SUMMARY_BYTES - 5, BitConverter.GetBytes(count));
        }

        public string[] GetPredicateKeys(Predicate<string> m, bool isMatch)
        {
            List<string> kList = new List<string>();
            SortedList<string, KeyValueState> idxObject = GetIndexObject(null);
            if (idxObject != null)
            {
                foreach (string k in idxObject.Keys)
                {
                    if (m != null)
                    {
                        if ((isMatch && m(k)) || !isMatch && !m(k))
                        {
                            kList.Add(k);
                        }
                    }
                    else
                    {
                        kList.Add(k);
                    }
                }
            }
            return kList.ToArray();
        }

        public string[] GetAllKeys()
        {
            return GetPredicateKeys(null, false);
        }

        #region CRUD+C
        private bool ExistsDataKey(string key, out long fIndex, out int fLen)
        {
            fIndex = 0;
            fLen = 0;
            int idxCount = GetIndexRealSize();
            bool isExist = false;
            if (idxCount > 0)
            {
                //Console.WriteLine("ExistsDataKey => IDX Len: {0}", idxCount);
                SortedList<string, KeyValueState> idxObj = GetIndexObject(idxCount);
                isExist = (idxObj != null && idxObj.ContainsKey(key));
                if (isExist)
                {
                    KeyValueState kState = idxObj[key];
                    fIndex = kState.DataIndex;
                    fLen = (int)kState.Length;
                }
            }
            return isExist;
        }

        public bool ExistsKey(string key)
        {
            long iIdx = 0;
            int iLen = 0;
            return ExistsDataKey(key, out iIdx, out iLen);
        }

        public byte[] GetKeyData(string key)
        {
            //find Key Index
            long iIdx = 0;
            int iLen = 0;
            if (!ExistsDataKey(key, out iIdx, out iLen))
            {
                return new byte[0];
            }
            else
            {
                return ReadData(iIdx, iLen);
            }
        }

        //[TODO]
        public bool StoreKeyData(string key, byte[] kDat)
        {
            long iIdx = 0;
            int iLen = 0;
            long nDataIndex = HEAD_SUMMARY_BYTES + (long)GetIndexSize() + MAX_DIRTYBLOCK_SIZE;

            bool addNew = false, append = false;
            SortedList<string, KeyValueState> idxObj = GetIndexObject(null);

            KeyValueState kState = new KeyValueState();
            kState.Key = key;
            kState.Length = kDat.Length;

            if (idxObj == null)
            {
                IndexObject = new SortedList<string, KeyValueState>(StringComparer.Ordinal);
                addNew = true;
            }
            else
            {
                if (!ExistsDataKey(key, out iIdx, out iLen))
                {
                    addNew = true;
                    append = true;
                }
                else
                {
                    KeyValueState oldState = IndexObject[key];
                    if ((oldState.Length + oldState.ChipSize) >= kDat.Length)
                    {
                        append = false;
                        nDataIndex = oldState.DataIndex;
                        kState.ChipSize = (oldState.Length + oldState.ChipSize) - kDat.Length;
                    }
                    else
                    {
                        kState.ChipSize = 0;
                        append = true;

                        #region ADD Dirty Block
                        DirtyBlock dBlock = new DirtyBlock { DataIndex = oldState.DataIndex, Length = oldState.Length + kState.ChipSize };
                        Console.WriteLine("Dirty: {0}+{1}", dBlock.DataIndex, dBlock.Length);
                        #endregion
                    }
                }
            }

            if (append)
            {
                storeStream.SetLength(storeStream.Length + kDat.Length);
                nDataIndex = storeStream.Length;
            }
            kState.DataIndex = nDataIndex;

            if (addNew)
            {
                kState.ChipSize = 0;
                IndexObject.Add(key, kState);
                SetKeyCount(IndexObject.Count);
            }
            else
            {
                IndexObject[key] = kState;
            }

            byte[] idxBytes = FileWrapHelper.GetBytes(IndexObject);
            //更新索引内容实际大小
            if (UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, GetIndexSize(), 12))
            {
                IncrementIndexSize();
                UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, GetIndexSize(), 12);
            }
            WriteData(nDataIndex, kDat);
            return true;
        }

        //[TODO]
        public bool RemoveData(string key)
        {
            long iIdx = 0;
            int iLen = 0;
            if (!ExistsDataKey(key, out iIdx, out iLen))
            {
                return false;
            }
            else
            {
                //remove index data, make as dirty
                IndexObject.Remove(key);
                SetKeyCount(IndexObject.Count);

                byte[] idxBytes = FileWrapHelper.GetBytes(IndexObject);
                //更新索引内容实际大小
                if (UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, GetIndexSize(), 12))
                {
                    IncrementIndexSize();
                    UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, GetIndexSize(), 12);
                }

                #region Update Dirty Block
                DirtyBlock dBlock = new DirtyBlock { DataIndex = iIdx, Length = iLen };

                if (DirtyObject.ContainsKey(dBlock.DataIndex))
                {
                    //[impossible]
                    DirtyObject[dBlock.DataIndex] = dBlock;
                }
                else
                {
                    DirtyObject.Add(dBlock.DataIndex, dBlock);
                }

                byte[] dBytes = FileWrapHelper.GetBytes(DirtyObject);
                //更新索引内容实际大小
                if (UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + GetIndexSize()), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9))
                {
                    ClearDirtyData();
                    UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + GetIndexSize()), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9);
                }
                #endregion

                return true;
            }
        }

        //[TODO]
        public bool Compact()
        {
            //删除DirtyBlock数据，及KeyValue数据区的碎片
            return false;
        }
        #endregion

        private void IncrementIndexSize()
        { 
            long oldPos = base.storeStream.Position;

            string nFileName = FilePath + ".tmp";
            FileStream nFStream = new FileStream(nFileName, FileMode.Create, FileAccess.Write, FileShare.None);

            base.IndexSizeChange = new RefreshIndexSizeChange((size, stm) => {
                    byte[] buffer = new byte[4];
                    buffer = BitConverter.GetBytes(size);
                    stm.Position = 8;
                    stm.Write(buffer, 0, buffer.Length);
                });

            bool result = RefactHeadIndexSize(GetIndexSize() + INDEX_INCREMENT_STEP, nFStream);
            nFStream.Close();
            nFStream.Dispose();
            if (result == false) File.Delete(nFileName);

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
            IndexObject = null;

            if (oldPos < base.storeStream.Length)
            {
                base.storeStream.Position = oldPos;
            }
        }

        private void ClearDirtyData()
        { 
            
        }

        //更新索引的脏数据区域
        private bool UpdateDynamicBytes(long offset, byte[] bytes, int cmpLength, long lenUpdateOffset)
        {
            bool result = false;
            if (bytes.Length > cmpLength) result = true;
            if (!result)
            {
                WriteData(offset, bytes);
                WriteData(lenUpdateOffset, BitConverter.GetBytes(bytes.Length));
            }
            //是否需要增加索引空间或清理未使用的空间
            return result;
        }

    }

    [Serializable]
    public struct DirtyBlock
    {
        private long i;
        public long DataIndex
        {
            get { return i; }
            set { i = value; }
        }

        private long l;
        public long Length
        {
            get { return l; }
            set { l = value; }
        }
    }

    [Serializable]
    public struct KeyValueState
    {
        private string k;
        public string Key
        {
            get { return k; }
            set { k = value; }
        }

        private long i;
        public long DataIndex
        {
            get { return i; }
            set { i = value; }
        }

        private long l;
        public long Length
        {
            get { return l; }
            set { l = value; }
        }

        private long c;
        public long ChipSize
        {
            get { return c; }
            set { c = value; }
        }

    }
}
