using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;

namespace Vbyte.DataSource.Utility
{
    /// <summary>
    /// kernel32平台调用代码
    /// </summary>
    public static class Kernel32
    {
        /*
         DWORD WINAPI GetCompressedFileSize(
              __in       LPCTSTR lpFileName,
              __out_opt  LPDWORD lpFileSizeHigh
            )
         */
        [DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetCompressedFileSize")]
        static extern uint GetCompressedFileSizeAPI(string lpFileName, out uint lpFileSizeHigh);

        /// <summary>
        /// 获取NTFS压缩文件的文件大小
        /// <para>http://www.pinvoke.net/default.aspx/kernel32/GetCompressedFileSize.html</para>
        /// </summary>
        /// <param name="filename">压缩文件的本地路径</param>
        /// <returns></returns>
        public static ulong GetCompressedFileSize(string filename)
        {
            uint high;
            uint low;
            low = GetCompressedFileSizeAPI(filename, out high);
            int error = Marshal.GetLastWin32Error();
            if (high == 0 && low == 0xFFFFFFFF && error != 0)
                throw new Win32Exception(error);
            else
                return ((ulong)high << 32) + low;
        }

        /*
        BOOL WINAPI GetDiskFreeSpace(
          __in   LPCTSTR lpRootPathName,
          __out  LPDWORD lpSectorsPerCluster,
          __out  LPDWORD lpBytesPerSector,
          __out  LPDWORD lpNumberOfFreeClusters,
          __out  LPDWORD lpTotalNumberOfClusters
        );
        */
        /// <summary>
        /// Gets the disk free space.
        /// <para>http://www.pinvoke.net/default.aspx/kernel32/GetDiskFreeSpace.html</para>
        /// </summary>
        /// <param name="drive">The drive.</param>
        /// <param name="sectorsPerCluster">The sectors per cluster.</param>
        /// <param name="bytesPerSector">The bytes per sector.</param>
        /// <param name="numberOfFreeClusters">The number of free clusters.</param>
        /// <param name="totalNumberOfClusters">The total number of clusters.</param>
        /// <returns></returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool GetDiskFreeSpace(string drive, out uint sectorsPerCluster, out uint bytesPerSector,
            out uint numberOfFreeClusters, out uint totalNumberOfClusters);


        private static string LAST_DRIVER_ROOT = string.Empty;
        private static ulong LAST_CLUSTER_SIZE = 0;

        private static ulong GetClusterSize(string driveFilePath)
        {
            string curRoot = Path.GetPathRoot(driveFilePath);
            if (LAST_CLUSTER_SIZE == 0 || curRoot != LAST_DRIVER_ROOT)
            {
                uint sectorsPerCluster, bytesPerSector, numberOfFreeClusters, totalNumberOfClusters;
                if (GetDiskFreeSpace(curRoot, out sectorsPerCluster, out bytesPerSector,
                    out numberOfFreeClusters, out totalNumberOfClusters))
                {
                    LAST_CLUSTER_SIZE = sectorsPerCluster * bytesPerSector;
                    LAST_DRIVER_ROOT = curRoot;
                }
            }
            return LAST_CLUSTER_SIZE;
        }

        /// <summary>
        /// 获取磁盘上文件占用空间的大小
        /// </summary>
        /// <param name="filePath">本地文件路径</param>
        /// <param name="realLen">文件实际大小</param>
        /// <returns>磁盘文件所占用空间大小</returns>
        public static ulong GetFileSizeOnDisk(string filePath, out long realLen)
        {
            FileInfo dFile = new FileInfo(filePath);
            if (!dFile.Exists)
            {
                realLen = 0;
                return 0;
            }
            ulong fSize = 0;
            realLen = dFile.Length;
            if ((dFile.Attributes & FileAttributes.Compressed) != 0)
            {
                fSize = GetCompressedFileSize(filePath);
            }
            else
            {
                //long sizeondisk = clustersize * ((filelength + clustersize - 1) / clustersize);
                ulong clustersize = GetClusterSize(filePath);
                fSize = clustersize * (((ulong)dFile.Length + clustersize - 1) / clustersize);
            }
            return fSize;
        }

    }
}
