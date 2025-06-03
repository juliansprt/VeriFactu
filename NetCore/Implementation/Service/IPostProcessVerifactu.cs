using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IPostProcessVerifactu
    {
        void ProcessVerifactu(int companyId, int invoiceId, ElectronicInvoiceStates status, byte[] qr);
    }

    public class PostProcessVerifactu : IPostProcessVerifactu
    {
        private readonly IQRService _qrService;

        public PostProcessVerifactu(IQRService qrService)
        {
            _qrService = qrService;
        }
        public void ProcessVerifactu(int companyId, int invoiceId, ElectronicInvoiceStates status, byte[] qr)
        {
            switch (status)
            {
                case ElectronicInvoiceStates.Failed:
                    break;
                case ElectronicInvoiceStates.Created:
                    break;
                case ElectronicInvoiceStates.PendingSendAEAT:
                    break;
                case ElectronicInvoiceStates.SendedAEAT:
                    break;
                case ElectronicInvoiceStates.Valid:

                    _qrService.SaveQR(invoiceId, qr);
                    break;
                case ElectronicInvoiceStates.PartlyCorrect:
                    break;
                case ElectronicInvoiceStates.Incorrect:
                    break;
                default:
                    break;
            }
        }
    }
}
