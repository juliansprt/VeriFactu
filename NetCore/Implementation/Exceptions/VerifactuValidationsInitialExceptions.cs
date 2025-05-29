using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Exceptions
{
    public class VerifactuValidationsInitialExceptions : VerifactuExceptions
    {
        public string[] Validations { get; set; }
        public VerifactuValidationsInitialExceptions(string message, params string[] validations) : base(message, EnumVerifactuProcess.ValidacionInicial)
        {
            Validations = validations;
        }

        public VerifactuValidationsInitialExceptions(string message, Exception innerException, params string[] validations) : base(message, EnumVerifactuProcess.ValidacionInicial, innerException)
        {
            Validations = validations;
        }

    }
}
