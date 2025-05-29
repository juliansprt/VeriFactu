using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Exceptions
{
    public class VerifactuExceptions : Exception
    {
        public EnumVerifactuProcess RecoveryInvoiceState { get; set; }
        public VerifactuExceptions(string message, EnumVerifactuProcess recoveryInvoiceState) : base(message)
        {
            RecoveryInvoiceState = recoveryInvoiceState;
        }

        public VerifactuExceptions(string message, EnumVerifactuProcess recoveryInvoiceState, Exception innerException) : base(message, innerException)
        {
            RecoveryInvoiceState = recoveryInvoiceState;
        }
    }
}
