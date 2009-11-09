using System;
using System.ComponentModel;

public class SynLicenseProvider : LicFileLicenseProvider
{
    protected override bool IsKeyValid(string key, System.Type type)
    {
        return key.Equals( GetValidLicenseKey(type) );
    }

    protected override string GetKey(System.Type type)
    {
        return GetValidLicenseKey(type);
    }

    private string GetValidLicenseKey(Type type)
    {

        int hashCode = (type.FullName + "Secret").GetHashCode();
        //Console.WriteLine(hashCode);
        //Console.WriteLine("{0}:{1,8:X}", type.FullName, hashCode);
        return string.Format("{0}:{1,8:X}", type.FullName, hashCode);
    }
}
