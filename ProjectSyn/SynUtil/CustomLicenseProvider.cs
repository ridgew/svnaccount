using System;
using System.ComponentModel;

namespace SynUtil
{
    public abstract class CustomLicenseProvider : LicenseProvider
    {
        public override License GetLicense(LicenseContext context, Type type,
            object instance, bool allowExceptions)
        {
            // Step 1: locate the license key
            string licenseKey = null;
            switch (context.UsageMode)
            {
                case LicenseUsageMode.Designtime:
                    licenseKey = GetDesignTimeLicenseKey(type);
                    context.SetSavedLicenseKey(type, licenseKey);
                    break;
                case LicenseUsageMode.Runtime:
                    licenseKey = context.GetSavedLicenseKey(type, null);
                    break;
            }
            if (licenseKey == null)
            {
                if (allowExceptions) throw new LicenseException(type, instance,
                                      "No appropriate license key was located.");
                else return null;
            }


            // Step 2: validate the license key
            bool isValid = IsLicenseKeyValid(type, licenseKey);
            if (!isValid)
            {
                if (allowExceptions) throw new LicenseException(type, instance,
                                          "License key is not valid.");
                else return null;
            }


            // Step 3: grant a license        
            License license = CreateLicense(type, licenseKey);
            if (license == null)
            {
                if (allowExceptions) throw new LicenseException(type, instance,
                                         "license could not be created.");
                else return null;
            }

            return license;
        }

        protected abstract string GetDesignTimeLicenseKey(Type type);
        protected abstract bool IsLicenseKeyValid(Type type, string licenseKey);
        protected abstract License CreateLicense(Type type, string licenseKey);
    }
}