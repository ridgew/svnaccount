using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Vbyte.DataSource.Utility
{
    /// <summary>
    /// 基于字节序列的数据存储
    /// </summary>
    public abstract class StreamStoreBase : IDisposable
    {
        protected Stream storeStream = null;
        protected BinaryReader _internalReader;
        protected BinaryWriter _internalWriter;

        /// <summary>
        /// 刷新索引大小变化
        /// </summary>
        public delegate void RefreshIndexSizeChange(int newIdxSize, Stream outSave);

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamStoreBase"/> class.
        /// </summary>
        /// <param name="stm">The stream</param>
        public StreamStoreBase(Stream stm)
        {
            storeStream = stm;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamStoreBase"/> class.
        /// </summary>
        public StreamStoreBase()
            : base()
        {

        }

        /// <summary>
        /// 获取存储系统实现的版本描述
        /// </summary>
        public abstract string GetStoreVersion();

        /// <summary>
        /// 获取索引数据大小
        /// </summary>
        public abstract int GetIndexSize();

        /// <summary>
        /// 获取非数据区的文件头数据大小
        /// </summary>
        public abstract int GetFileHeadSize();

        /// <summary>
        /// 获取数据索引偏移量值保存的偏移位置
        /// </summary>
        public abstract long GetDataIndexOffset();

        /// <summary>
        /// 获取读取数据位置开始的偏移量(默认值为0，即索引后紧跟数据)
        /// </summary>
        public abstract long GetDataReadOffset();

        /// <summary>
        /// 获取下次索引写入位置的偏移量
        /// </summary>
        public abstract int GetNextIndexWriteOffset();

        /// <summary>
        /// 索引大小改变后的调用函数
        /// </summary>
        public RefreshIndexSizeChange IndexSizeChange { get; set; }

        private int _hIdxSize = 2048; //2k的索引文件存储空间
        /// <summary>
        /// 索引数据文件存储长度，默认为2048字节。
        /// </summary>
        public int HeadIndexLength
        {
            get { return _hIdxSize; }
            set { _hIdxSize = value; }
        }

        /// <summary>
        /// 暂时只支持int,long,uint
        /// </summary>
        protected T GetOffSetDat<T>(BinaryReader reader, long offset)
            where T : IConvertible
        {
            T objRet = default(T);
            long oldPos = reader.BaseStream.Position;
            //Console.WriteLine("GetOffSetDat: @({0})", offset);
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            object objRead = 0;
            switch (objRet.GetTypeCode())
            {
                case TypeCode.Int32:
                    objRead = reader.ReadInt32();
                    break;
                case TypeCode.Int64:
                    objRead = reader.ReadInt64();
                    break;
                case TypeCode.UInt32:
                    objRead = reader.ReadUInt32();
                    break;
                default:
                    break;
            }
            objRet = (T)Convert.ChangeType(objRead, typeof(T)); //偏移量
            reader.BaseStream.Position = oldPos;
            return objRet;
        }

        protected void InitializeReader()
        {
            if (_internalReader == null) _internalReader = new BinaryReader(storeStream);
        }

        protected void InitializeWriter()
        {
            if (_internalWriter == null) _internalWriter = new BinaryWriter(storeStream);
        }


        /// <summary>
        /// 读取存储字节序列中指定字节数量的数据
        /// </summary>
        /// <param name="offset">其实偏移位置</param>
        /// <param name="count">字节数据</param>
        /// <returns></returns>
        public byte[] ReadData(long offset, int count)
        {
            byte[] tDat = new byte[count];
            storeStream.Seek(offset, SeekOrigin.Begin);
            storeStream.Read(tDat, 0, count);
            //Console.WriteLine("文件索引：{0}", fIdx);
            //Console.WriteLine("文件长度：{0}", fLen);
            return tDat;
        }

        public byte[] KeepPositionRead(long offset, int count)
        {
            long oldPos = storeStream.Position;
            byte[] tDat = ReadData(offset, count);
            storeStream.Position = oldPos;
            return tDat;
        }

        public void KeepPositionWrite(long offset, byte[] dat)
        {
            long oldPos = storeStream.Position;
            WriteData(offset, dat);
            storeStream.Position = oldPos;
        }

        public void WriteData(long offset, byte[] dat)
        {
            storeStream.Seek(offset, SeekOrigin.Begin);
            storeStream.Write(dat, 0, dat.Length);
            storeStream.Flush();
        }

        #region IDisposable 成员

        /// <summary>
        /// 执行与释放或重置非托管资源相关的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            if (_internalReader != null) _internalReader.Close();
            if (_internalWriter != null) _internalWriter.Close();
            if (storeStream != null)
            {
                storeStream.Close();
                storeStream.Dispose();
            }
        }

        #endregion

    }
}
