using System;
using System.Collections.Generic;
using System.Text;

namespace CreditCardApplications
{
    public enum CreditCardApplicationDecision
    {
        Uknown,
        AutoAccepted,
        AutoDeclined,
        ReferredToHuman,
        ReferredToHumanFraudRisk
    }
}
