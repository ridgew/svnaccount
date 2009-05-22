using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FSVN.Util
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProjectPackage : IDisposable
    {
        /// <summary>
        /// Packages the file.
        /// </summary>
        /// <param name="pkgFileInfo">The PKG file info.</param>
        void PackageFile(FileInfo pkgFileInfo);

        /// <summary>
        /// Gets the package file info.
        /// </summary>
        /// <returns></returns>
        List<FileInfo> GetPackageFileInfo();

        /// <summary>
        /// Gets or sets the project item.
        /// </summary>
        /// <value>The project item.</value>
        ProjectInfo ProjectItem { get; set; }

        /// <summary>
        /// Gets or sets the base dir.
        /// </summary>
        /// <value>The base dir.</value>
        string BaseDir { get; set; }
    }
}
