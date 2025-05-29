using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Exceptions
{
    public class VerifactuEnviandoRequestException : VerifactuExceptions
    {
        public VerifactuEnviandoRequestException(string message) : base(message, EnumVerifactuProcess.EnviandoRequest)
        {
        }

        public VerifactuEnviandoRequestException(string message, Exception innerException) : base(message, EnumVerifactuProcess.EnviandoRequest, innerException)
        {
        }
    }
}
