using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Exceptions
{
    public class VerifactuRequestXMLException : VerifactuExceptions
    {
        public VerifactuRequestXMLException(string message) : base(message, EnumVerifactuProcess.GenerandoRequestXML)
        {
        }
        public VerifactuRequestXMLException(string message, Exception innerException) : base(message, EnumVerifactuProcess.GenerandoRequestXML, innerException)
        {
        }
    }
}
