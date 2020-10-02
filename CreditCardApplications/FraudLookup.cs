using System;
using System.Collections.Generic;
using System.Text;

namespace CreditCardApplications
{
    public class FraudLookup
    {
        //As this is not an interface, if virtual is not used here, then in tests when we mock this we would get
        //Non-overridable members (here: FraudLookup.IsFraudRisk) may not be used in setup / verification expressions.
        //So we add the virutal
        public bool IsFraudRisk(CreditCardApplication application)
        {
            return CheckApplication(application);
        }

        protected virtual bool CheckApplication(CreditCardApplication application)
        {
            if (application.LastName == "Smith")
            {
                return true;
            }

            return false;
        }
    }
}
