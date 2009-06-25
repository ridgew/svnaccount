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
            bool newStore = IdentityFileStore.CreatNewFile(filepath);
            InitialFileStream();
            InitializeReader();
            InitializeWriter();

            if (newStore) BuildEmptyFile(); 
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
        /// <summary>
        /// 获取存储系统实现的版本描述
        /// </summary>
        /// <returns></returns>
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
        /// <returns></returns>
        public override int GetIndexSize()
        {
            return GetOffSetDat<int>(_internalReader, 8);
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
            return 8 + 4 + 4 + 1;
        }
        #endregion

        private void BuildEmptyFile()
        {
            _internalWriter.Write(Encoding.ASCII.GetBytes("KVS 1.0 "));                             //文件版本                      +8
            _internalWriter.Write(BitConverter.GetBytes(HeadIndexLength));                          //索引空间长度                  +4
            _internalWriter.Write(BitConverter.GetBytes((int)0));                                   //索引有效数据长度              +4
            _internalWriter.Write('\n');                                                            //                              +1
        }

        private int GetIndexRealSize()
        {
            return GetOffSetDat<int>(_internalReader, 8 + 4);
        }

        private SortedList<string, KeyValueState> IndexObject;
        private SortedList<string, KeyValueState> GetIndexObject(int? idxCount)
        {
            if (IndexObject == null && idxCount > 0)
            {
                //IndexObject = new SortedList<string, KeyValueState>(StringComparer.Ordinal);
                byte[] idxBytes = ReadData((long)GetNextIndexWriteOffset(), idxCount ?? GetIndexRealSize());
                IndexObject = FileWrapHelper.GetObject<SortedList<string, KeyValueState>>(idxBytes);
            }
            return IndexObject;
        }

        #region CRUD+C
        private bool ExistsDataKey(string key, out long fIndex, out int fLen)
        {
            fIndex = 0;
            fLen = 0;
            int idxCount = GetIndexRealSize();
            if (idxCount == 0)
            {
                return false;
            }
            else
            {
                SortedList<string, KeyValueState> idxObj = GetIndexObject(idxCount);
                return idxObj != null && idxObj.ContainsKey(key);
            }
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
            long nDataIndex = (long)GetIndexSize();
            if (ExistsDataKey(key, out iIdx, out iLen))
            {
                //make old as dirty
                KeyValueState oldState = IndexObject[key];
                if (oldState.Length >= kDat.Length)
                {
                    oldState.Length = kDat.Length;
                }
                else
                { 
                    
                }
            }

            SortedList<string, KeyValueState> idxObj = GetIndexObject(null);
            if (idxObj == null)
            {
                IndexObject = new SortedList<string, KeyValueState>(StringComparer.Ordinal);

                KeyValueState kState = new KeyValueState();
                kState.Key = key;
                kState.Length = kDat.Length;
                kState.IsDirty = false;
                kState.DataIndex = (long)GetIndexSize();
                IndexObject.Add(key, kState);

                WriteData((long)GetNextIndexWriteOffset(), FileWrapHelper.GetBytes(IndexObject));
            }
            else
            {

            }

            //append
            WriteData(nDataIndex, kDat);
            return true;
        }

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
                return true;
            }
        }

        public bool Compact()
        {
            return false;
        }
        #endregion

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

        private bool d;
        public bool IsDirty
        {
            get { return d; }
            set { d = value; }
        }

    }
}
