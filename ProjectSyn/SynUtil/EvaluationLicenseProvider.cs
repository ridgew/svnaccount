using System;
using System.ComponentModel;
using Microsoft.Win32;

namespace SynUtil
{
    public class EvaluationLicenseProvider : CustomLicenseProvider
    {
        protected override string GetDesignTimeLicenseKey(Type type)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(
                                    "Software\\WdmLicensing\\" + type.FullName);
            if (regKey == null)
            {
                return null;
            }
            return regKey.GetValue("LicenseKey").ToString();
        }

        protected override bool IsLicenseKeyValid(Type type, string licenseKey)
        {
            return (licenseKey.Equals(GetValidLicenseKey(type)) ||
                     licenseKey.Equals("Evaluation"));
        }

        private string GetValidLicenseKey(Type type)
        {
            int hashCode = (type.FullName + "Secret").GetHashCode();
            return String.Format("{0}:{1,8:X}", type.FullName, hashCode);
        }

        protected override License CreateLicense(Type type, string licenseKey)
        {
            return new SynLicense(licenseKey, licenseKey.Equals("Evaluation"));
        }
    }
}
