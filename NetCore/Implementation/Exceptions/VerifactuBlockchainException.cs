using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Exceptions
{
    public class VerifactuBlockchainException : VerifactuExceptions
    {
        public VerifactuBlockchainException(string message) : base(message, EnumVerifactuProcess.GenerandoBlockchain)
        {
        }

        public VerifactuBlockchainException(string message, Exception innerException) : base(message, EnumVerifactuProcess.GenerandoBlockchain, innerException)
        {
        }
    }
}
