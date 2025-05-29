using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.NoVeriFactu.Signature.Xades.Props.BigInt;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IQRService
    {
        public void SaveQR(int invoiceId , byte[] qr);
    }

    public class QRService : IQRService
    {
        public void SaveQR(int invoiceId, byte[] qr)
        {
            System.IO.File.WriteAllBytes($"D:\\Cenet\\Verifactu\\qr.png", qr);
        }
    }
}
