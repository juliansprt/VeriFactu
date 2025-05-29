using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface ICertificateService
    {
        X509Certificate2 GetCertificate(int companyId);
    }

    public class CertificateService : ICertificateService
    {
        public X509Certificate2 GetCertificate(int companyId)
        {

            return new X509Certificate2(
                "C:\\Cenet\\Verifactu\\facturasappjjpe.pfx",
                "6J2Y17xKl8yX6khLKdeB", 
                X509KeyStorageFlags.Exportable);
        }
    }
}
