using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Net.Core.Implementation.Exceptions;
using VeriFactu.NoVeriFactu.Signature.Xades.Props.BigInt;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IProcessErrors
    {
        void SaveErrors(int companyId, int invoiceId, string invoiceID, List<VerifactuResponseError> errors);


        void SaveErrors(int companyId, int invoiceId, string invoiceID, VerifactuResponseError errors);
    }

    public class ProcessErrors : IProcessErrors
    {
        public void SaveErrors(int companyId, int invoiceId, string invoiceID, List<VerifactuResponseError> errors)
        {
        }

        public void SaveErrors(int companyId, int invoiceId, string invoiceID, VerifactuResponseError errors)
        {
        }
    }
}
