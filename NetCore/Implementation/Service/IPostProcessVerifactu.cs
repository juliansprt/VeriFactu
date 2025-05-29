using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IPostProcessVerifactu
    {
        void ProcessVerifactu(int companyId, int invoiceId, ElectronicInvoiceStates status);
    }

    public class PostProcessVerifactu : IPostProcessVerifactu
    {
        public void ProcessVerifactu(int companyId, int invoiceId, ElectronicInvoiceStates status)
        {
            throw new NotImplementedException();
        }
    }
}
