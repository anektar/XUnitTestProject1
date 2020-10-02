using System;
using System.Collections.Generic;
using System.Text;

namespace CreditCardApplications
{
    public interface ILicenseData
    {
        string LicenseKey { get; }
    }

    public interface IServiceInformation
    {
        ILicenseData License { get; }
    }

    public interface IFrequentFlyerNumberValidator
    {
        //bool IsValid(string frequentFlyerNumber);
        bool IsValid<T>(T frequentFlyerNumber);
        void IsValid(string frequentFlyerNumber, out bool isValid);
        //string LicenseKey { get; }
        IServiceInformation ServiceInformation { get; }
        ValidationMode ValidationMode { get; set; }

        event EventHandler ValidatorLookupPerformed;
    }
}
