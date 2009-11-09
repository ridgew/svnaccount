using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace SynUtil
{
    public class SynLicense : License
    {
        string licenseKey;
        bool isEvaluation;

        public override void Dispose() { }

        /// <summary>
        /// 当在派生类中被重写时，获取授予该组件的许可证密钥。
        /// </summary>
        /// <value></value>
        /// <returns>授予该组件的许可证密钥。</returns>
        public override string LicenseKey
        { get { return licenseKey; } }

        /// <summary>
        /// Gets a value indicating whether this instance is evaluation.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is evaluation; otherwise, <c>false</c>.
        /// </value>
        public bool IsEvaluation
        { get { return isEvaluation; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="SynLicense"/> class.
        /// </summary>
        /// <param name="licenseKey">The license key.</param>
        /// <param name="isEvaluation">if set to <c>true</c> [is evaluation].</param>
        public SynLicense(string licenseKey, bool isEvaluation)
        {
            this.licenseKey = licenseKey;
            this.isEvaluation = isEvaluation;
        }

    }
}
