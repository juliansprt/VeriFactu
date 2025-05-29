using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IFileStorage
    {
        void SaveRequestXML(byte[] bytes, int companyId, string invoiceId);

        void SaveResponseXML(string content, int companyId, string invoiceId);

    }

    public class FileStorage : IFileStorage
    {
        public void SaveRequestXML(byte[] bytes, int companyId, string invoiceId)
        {
            System.IO.File.WriteAllBytes($"D:\\Cenet\\Verifactu\\Request-{invoiceId}.xml", bytes);
        }

        public void SaveResponseXML(string content, int companyId, string invoiceId)
        {
            System.IO.File.WriteAllText($"D:\\Cenet\\Verifactu\\Response-{invoiceId}.xml", content);
        }
    }
}
