using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IElectronicInvoiceStateService
    {
        void SetInvoiceState(int invoiceId, ElectronicInvoiceStates state, string message);

    }

    public class ElectronicInvoiceStateService : IElectronicInvoiceStateService
    {
        
        public void SetInvoiceState(int invoiceId, ElectronicInvoiceStates state, string message)
        {

        }
    }
}
