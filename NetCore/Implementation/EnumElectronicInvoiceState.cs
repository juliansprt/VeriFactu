using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation
{
    public enum ElectronicInvoiceStates
    {
        Failed = 0,
        Created = 1,
        PendingSendAEAT = 2,
        SendedAEAT = 3,
        Valid = 4,
        PartlyCorrect=5,
        Incorrect = 6
    }
}
