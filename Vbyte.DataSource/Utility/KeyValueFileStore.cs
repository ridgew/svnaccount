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

            _internalReader = null;
            InitializeReader();

            _currentDatOffset = GetOffSetDat<int>(_internalReader, 12);

            _internalWriter = null;
            InitializeWriter();
        }

        private void Reset()
        {
            _verStr = null;
        }

        private void InitialFileStream()
        {
            if (storeStream == null)
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

        /// <summary>
        /// 获取索引数据大小
        /// </summary>
        /// <returns></returns>
        public override int GetIndexSize()
        {
            return GetOffSetDat<int>(_internalReader, 8);;
        }

        /// <summary>
        /// 获取非数据区的文件头数据大小
        /// </summary>
        public override int GetFileHeadSize()
        {
            return HEAD_SUMMARY_BYTES + GetIndexSize() + MAX_DIRTYBLOCK_SIZE;
        }

        private long _currentDatOffset = 0;

        /// <summary>
        /// 获取数据索引偏移量值保存的偏移位置
        /// </summary>
        /// <returns></returns>
        public override long GetDataIndexOffset()
        {
            return DATA_INDEX_OFFSET;
        }

        /// <summary>
        /// 获取读取数据位置开始的偏移量(默认值为0，即索引后紧跟数据)
        /// </summary>
        /// <returns></returns>
        public override long GetDataReadOffset()
        {
            return GetOffSetDat<int>(_internalReader, DATA_INDEX_OFFSET);
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

        public long GetNextDataWriteIndex()
        {
            return GetOffSetDat<int>(_internalReader, 16);
        }

        /// <summary>
        /// 数据索引偏移量记录索引位置
        /// </summary>
        static readonly long DATA_INDEX_OFFSET = 12;
        
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
        static readonly int HEAD_SUMMARY_BYTES = 41;

        private void BuildEmptyFile()
        {
            _internalWriter.Write(Encoding.ASCII.GetBytes("KVS 1.0 "));                             //文件版本                      +8
            _internalWriter.Write(BitConverter.GetBytes(HeadIndexLength));                          //索引空间长度                  +4

            _internalWriter.Write(BitConverter.GetBytes((int)0));                                   //数据索引偏移量                +4
            _internalWriter.Write(BitConverter.GetBytes((long)0));                                  //下次写入数据索引              +8

            byte[] idxBytes = FileWrapHelper.GetBytes(new SortedList<string, KeyValueState>(StringComparer.Ordinal));
            KeepPositionWrite(HEAD_SUMMARY_BYTES, idxBytes);
            _internalWriter.Write(BitConverter.GetBytes(idxBytes.Length));                          //索引有效数据长度              +4

            _internalWriter.Write(BitConverter.GetBytes(MAX_DIRTYBLOCK_SIZE));                      //DirtyBlock空间长度            +4

            byte[] dbBytes = FileWrapHelper.GetBytes(new SortedList<long, DirtyBlock>());
            KeepPositionWrite(HEAD_SUMMARY_BYTES + HeadIndexLength, dbBytes);
            _internalWriter.Write(BitConverter.GetBytes(dbBytes.Length));                           //DirtyBlock有效数据长度        +4

            _internalWriter.Write(BitConverter.GetBytes((int)0));                                   //所有键值总数                  +4
            _internalWriter.Write('\n');                                                            //                              +1
        }

        public int GetIndexRealSize()
        {
            return GetOffSetDat<int>(_internalReader, 24);
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
                int dCount = GetDirtyBlockRealSize();
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
            KeepPositionWrite(HEAD_SUMMARY_BYTES - 5, BitConverter.GetBytes(count));
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
            bool isExist = false;
            SortedList<string, KeyValueState> idxObj = GetIndexObject(null);
            isExist = (idxObj != null && idxObj.ContainsKey(key));
            if (isExist)
            {
                KeyValueState kState = idxObj[key];
                fIndex = kState.DataIndex;
                fLen = (int)kState.Length;
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
                Console.WriteLine("Debug: offset {2} , {0} + {1}", iIdx, iLen, _currentDatOffset);
                return ReadData(iIdx + _currentDatOffset, iLen);
            }
        }

        //[TestMore]
        public bool StoreKeyData(string key, byte[] kDat)
        {
            long iIdx = 0;
            int iLen = 0, CurrentIndexSize = GetIndexSize();
            //Read
            long nDataIndex = GetOffSetDat<long>(_internalReader, 16);
            if (nDataIndex == 0) nDataIndex = HEAD_SUMMARY_BYTES + (long)CurrentIndexSize + MAX_DIRTYBLOCK_SIZE;
            //Console.WriteLine("Store Index: {0}", nDataIndex);

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
                    //Console.WriteLine("Note Exists Key" + key);
                }
                else
                {
                    KeyValueState oldState = IndexObject[key];
                    if ((oldState.Length + oldState.ChipSize) >= kDat.Length)
                    {
                        append = false;

                        //Location Update
                        nDataIndex = oldState.DataIndex + _currentDatOffset;
                        kState.DataIndex = oldState.DataIndex;
                        kState.ChipSize = (oldState.Length + oldState.ChipSize) - kDat.Length;
                    }
                    else
                    {
                        kState.ChipSize = 0;
                        Console.WriteLine("old:{0}, new:{1}, other:{2}", oldState.DataIndex, nDataIndex, nDataIndex - _currentDatOffset);
                        if (nDataIndex == (oldState.DataIndex + _currentDatOffset + oldState.Length + oldState.ChipSize))
                        {
                            append = false;

                            nDataIndex = oldState.DataIndex + _currentDatOffset;
                            kState.DataIndex = oldState.DataIndex;

                            //下次写入数据索引
                            KeepPositionWrite(16, BitConverter.GetBytes(nDataIndex + kDat.LongLength));

                            Console.WriteLine("Location Append");
                        }
                        else
                        {
                            append = true;
                            SortedList<long, DirtyBlock> dirtyDat = GetStoreDirtyData();
                            #region Dirty Block
                            DirtyBlock dBlock = new DirtyBlock { DataIndex = oldState.DataIndex, Length = oldState.Length + kState.ChipSize };
                            Console.WriteLine("Dirty: {0}+{1}", dBlock.DataIndex, dBlock.Length);
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
                            if (UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + CurrentIndexSize), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9))
                            {
                                ClearDirtyData();
                                UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + CurrentIndexSize), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9);
                            }
                            #endregion
                        }
                    }
                }
            }

            if (append)
            {
                //在DirtyBlock总查找可用空间(TODO)

                storeStream.SetLength(storeStream.Length + kDat.Length);
                //下次写入数据索引
                KeepPositionWrite(16, BitConverter.GetBytes(nDataIndex + kDat.LongLength));

                Console.WriteLine("test:{0}, {1}, {2}", nDataIndex, _currentDatOffset, nDataIndex - _currentDatOffset);
                kState.DataIndex = nDataIndex - _currentDatOffset;
            }

            if (addNew)
            {
                kState.ChipSize = 0;
                IndexObject.Add(key, kState);
                SetKeyCount(GetKeyCount() + 1);
                //Console.WriteLine("ADD New: Set {0}", GetKeyCount());
            }
            else
            {
                IndexObject[key] = kState;
            }

            byte[] idxBytes = FileWrapHelper.GetBytes(IndexObject);
            bool blnIndxAdd = false;
            //更新索引内容实际大小
            if (UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, CurrentIndexSize, 24))
            {
                IncrementIndexSize();
                nDataIndex += INDEX_INCREMENT_STEP;
                UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, CurrentIndexSize, 24);
                _currentDatOffset = GetOffSetDat<int>(_internalReader, 12);

                blnIndxAdd = true;
            }

            //Console.WriteLine("DataOffSet: {0}", _currentDatOffset);
            //Console.WriteLine("Store Index: {0}", nDataIndex);

            KeepPositionWrite(nDataIndex, kDat);

            //修改下次写入位置
            if (blnIndxAdd == true)
            {
                KeepPositionWrite(16, BitConverter.GetBytes(nDataIndex + kDat.LongLength)); 
            }
            return true;
        }

        //[TestMore]
        public bool RemoveData(string key)
        {
            long iIdx = 0;
            int iLen = 0, CurrentIndexSize = GetIndexSize();
            if (!ExistsDataKey(key, out iIdx, out iLen))
            {
                return false;
            }
            else
            {
                //remove index data, make as dirty
                IndexObject.Remove(key);
                SetKeyCount(GetKeyCount() - 1);

                byte[] idxBytes = FileWrapHelper.GetBytes(IndexObject);
                //更新索引内容实际大小
                if (UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, CurrentIndexSize, 24))
                {
                    IncrementIndexSize();
                    UpdateDynamicBytes((long)GetNextIndexWriteOffset(), idxBytes, CurrentIndexSize, 24);

                    _currentDatOffset = GetOffSetDat<int>(_internalReader, 12);
                    CurrentIndexSize = GetIndexSize();
                }

                #region Update Dirty Block
                SortedList<long, DirtyBlock> dObjs = GetStoreDirtyData();
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
                if (UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + CurrentIndexSize), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9))
                {
                    ClearDirtyData();
                    UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + CurrentIndexSize), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9);
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

        /// <summary>
        /// 重新调整索引文件头空间（增加或压缩）
        /// </summary>
        /// <param name="idxNewSize">新的索引空间大小，为0则执行压缩。</param>
        /// <param name="outStream">调整大小时的输出字节序列</param>
        /// <returns>是否进行了相关操作</returns>
        private bool RefactHeadIndexSize(int idxNewSize, Stream outStream)
        {
            int oldIdxSize = GetIndexSize();
            int fileHeadSize = GetFileHeadSize();

            //索引空间大小不变
            if (oldIdxSize == idxNewSize) return false;

            int oldOffSet = (int)GetDataReadOffset();
            int iTotalIdx = GetIndexRealSize();// GetNextIndexWriteOffset();
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

            outStream.SetLength(nFileLen);
            //Console.WriteLine("File Size: {0} -> {1},  +{2}", storeStream.Length, nFileLen, nFileLen - storeStream.Length);

            #region 复制旧索引及开始数据
            byte[] buffer = new byte[fileHeadSize - MAX_DIRTYBLOCK_SIZE];
            outStream.Position = 0;
            storeStream.Position = 0;

            storeStream.Read(buffer, 0, fileHeadSize - MAX_DIRTYBLOCK_SIZE);
            outStream.Write(buffer, 0, fileHeadSize - MAX_DIRTYBLOCK_SIZE);
            outStream.Flush();

            //Console.WriteLine();
            //Console.WriteLine(Utility.FileWrapHelper.GetHexViewString(buffer));
            //Console.WriteLine("Copy Data Size:{0}", fileHeadSize - MAX_DIRTYBLOCK_SIZE);
            //Console.WriteLine();

            //DirtyBlock
            storeStream.Position = fileHeadSize - MAX_DIRTYBLOCK_SIZE;
            outStream.Position = HEAD_SUMMARY_BYTES + idxNewSize;
            buffer = new byte[MAX_DIRTYBLOCK_SIZE];
            storeStream.Read(buffer, 0, MAX_DIRTYBLOCK_SIZE);
            outStream.Write(buffer, 0, MAX_DIRTYBLOCK_SIZE);
            outStream.Flush();

            //Console.WriteLine();
            //Console.WriteLine(Utility.FileWrapHelper.GetHexViewString(buffer));
            //Console.WriteLine("Copy Data Size:{0}", MAX_DIRTYBLOCK_SIZE);
            //Console.WriteLine();
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
                outStream.Flush();
            }
            #endregion

            #region 分段读取数据并写入
            outStream.Position = (long)(HEAD_SUMMARY_BYTES + idxNewSize + MAX_DIRTYBLOCK_SIZE);
            //Console.WriteLine("Write Data Index:{0}", HEAD_SUMMARY_BYTES + idxNewSize + MAX_DIRTYBLOCK_SIZE);
            storeStream.Position = fileHeadSize;

            int currentRead = 0;
            int lTest = 4096;
            buffer = new byte[lTest];
            //Console.WriteLine("Read Data：{0}", storeStream.Position);
            while ((currentRead = storeStream.Read(buffer, 0, lTest)) != 0)
            {
                //Console.WriteLine("read:{0}, pos:{1}", currentRead, storeStream.Position);
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

        private void IncrementIndexSize()
        {
            //Console.WriteLine("Idx Size: {0}", GetIndexSize());
            //Console.WriteLine("Real Size: {0}", GetIndexRealSize());
            //Console.WriteLine("Dat Offset：{0}", GetDataReadOffset());
            //Console.WriteLine("Dirty Size: {0}", GetDirtyBlockRealSize());
            //Console.WriteLine("Version: {0}", GetStoreVersion());
            //Console.WriteLine("Keys Count: {0}", GetKeyCount());

            //Console.WriteLine("动态增加索引空间");

            long oldPos = base.storeStream.Position;
            int oldIdxSize = GetIndexSize();
            string nFileName = FilePath + ".tmp";
            FileStream nFStream = new FileStream(nFileName, FileMode.Create, FileAccess.Write, FileShare.None);

            base.IndexSizeChange = new RefreshIndexSizeChange((size, stm) => {
                    byte[] buffer = new byte[4];
                    buffer = BitConverter.GetBytes(oldIdxSize + INDEX_INCREMENT_STEP);
                    stm.Position = 8;
                    stm.Write(buffer, 0, buffer.Length);
                });

            bool result = RefactHeadIndexSize(oldIdxSize + INDEX_INCREMENT_STEP,
                nFStream);
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

                ReInitial();
            }
            #endregion

            if (oldPos < base.storeStream.Length)
            {
                base.storeStream.Position = oldPos;
            }
        }

        //[TODO]
        private void ClearDirtyData()
        {
            SortedList<long, DirtyBlock> dObjs = GetStoreDirtyData();

            #region 整合数据
            /*
             1.复制有效数据  OR 2.位移有效数据
             3.更新有效数据的原始索引
             4.更新索引数据块
             */
            #endregion

            DirtyObject.Clear();
            int CurrentIndexSize = GetIndexSize();
            byte[] dBytes = FileWrapHelper.GetBytes(DirtyObject);
            if (UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + CurrentIndexSize), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9))
            {
                ClearDirtyData();
                UpdateDynamicBytes((long)(HEAD_SUMMARY_BYTES + CurrentIndexSize), dBytes, MAX_DIRTYBLOCK_SIZE, HEAD_SUMMARY_BYTES - 9);
            }   
        }

        //更新索引的脏数据区域
        private bool UpdateDynamicBytes(long offset, byte[] bytes, int cmpLength, long lenUpdateOffset)
        {
            bool result = false;
            if (bytes.Length > cmpLength) result = true;
            if (!result)
            {
                KeepPositionWrite(offset, bytes);
                KeepPositionWrite(lenUpdateOffset, BitConverter.GetBytes(bytes.Length));
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
