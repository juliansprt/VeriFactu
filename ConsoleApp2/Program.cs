using Serilog;
using Serilog.Core;
using System.Diagnostics;
using VeriFactu.Business;

using VeriFactu.Net.Core.Implementation;
using VeriFactu.Net.Core.Implementation.Exceptions;
using VeriFactu.Net.Core.Implementation.Service;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Factu.Anulacion;

namespace ConsoleAppVerifactuTest
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var blockchainService = new BlockchainService();
            var certificateService = new CertificateService();
            var fileStorage = new FileStorage();
            var processErrors = new ProcessErrors();
            var stateProcess = new ElectronicInvoiceStateService();
            ILogger logger = Logger.None;
            var qrService = new QRService();

            var settingsService = new SettingsServices();
            var settings = settingsService.GetSettings();



            //// Creamos una instacia de la clase factura
            //var invoice = new Invoice("JO-ED-0020", 1, 100, ElectronicInvoiceStates.Created, new DateTime(2024, 11, 15), "B70960752", settings, logger)
            //{
            //    InvoiceType = TipoFactura.F1,
            //    SellerName = "GUILLERMO RAMIREZ",
            //    BuyerID = "B44531218",
            //    BuyerName = "WEFINZ SOLUTIONS SL",
            //    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
            //    TaxItems = new List<TaxItem>() {
            //        new TaxItem()
            //        {
            //            TaxRate = 4,
            //            TaxBase = 10,
            //            TaxAmount = 0.4m,

            //        },
            //        new TaxItem()
            //        {
            //            TaxRate = 21,
            //            TaxBase = 100,
            //            TaxAmount = 21
            //        }
            //    },
            //};


            // Creamos una instacia de la clase factura
            var invoice = new Invoice("JO-ED-0100", 1, 100, ElectronicInvoiceStates.Created, new DateTime(2025, 5, 18), "B70960752", settings, logger)
            {
                InvoiceType = TipoFactura.F3,
                
                SellerName = "GUILLERMO RAMIREZ",
                BuyerID = "B44531218",
                BuyerName = "WEFINZ SOLUTIONS SL",
                Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
                TaxItems = new List<TaxItem>() {
                    new TaxItem()
                    {
                        TaxRate = 4,
                        TaxBase = 10,
                        TaxAmount = 0.4m,

                    },
                    new TaxItem()
                    {
                        TaxRate = 21,
                        TaxBase = 100,
                        TaxAmount = 21
                    }
                },
            };


            try
            {
                //var invoiceEntry = new InvoiceFix(invoice, blockchainService, certificateService, fileStorage, stateProcess, settings, logger);

                //invoiceEntry.Save();

                // Creamos la entrada de la factura
                var invoiceEntry = new InvoiceEntry(invoice, blockchainService, certificateService, fileStorage, stateProcess, settings, logger);

                // Guardamos la factura
                invoiceEntry.Save();

                var qr = invoiceEntry.Registro.GetValidateQr(settings.VeriFactuEndPointValidatePrefix);
                qrService.SaveQR(invoice.InvoicePrimaryKey, qr);


                // Consultamos el estado
                Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.Status}");

                if (invoiceEntry.Status == "Correcto")
                {

                    // Consultamos el CSV
                    Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.CSV}");

                }
                else
                {
                    // Consultamos el error
                    Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.ErrorCode}: {invoiceEntry.ErrorDescription}");

                }

                // Consultamos el resultado devuelto por la AEAT
                Debug.Print($"Respuesta de la AEAT:\n{invoiceEntry.Response}");
            }
            catch (VerifactuValidationsInitialExceptions ex)
            {
                stateProcess.SetInvoiceState(invoice.InvoicePrimaryKey, ElectronicInvoiceStates.Incorrect, ex.Message);
                processErrors.SaveErrors(invoice.CompanyId, invoice.InvoicePrimaryKey, invoice.InvoiceID, ex.Validations.Select(p => new VerifactuResponseError(string.Empty, p)).ToList());
                throw;
            }
            catch (VerifactuResponseException ex)
            {
                const string EstadoParcialmentCorrecto = "ParcialmenteCorrecto";

                if (ex.EstadoEnvio == EstadoParcialmentCorrecto)
                    stateProcess.SetInvoiceState(invoice.InvoicePrimaryKey, ElectronicInvoiceStates.PartlyCorrect, ex.Message);
                else
                    stateProcess.SetInvoiceState(invoice.InvoicePrimaryKey, ElectronicInvoiceStates.Incorrect, ex.Message);


                processErrors.SaveErrors(invoice.CompanyId, invoice.InvoicePrimaryKey, invoice.InvoiceID, ex.Errors.Select(p => new VerifactuResponseError(p.CodigoError, p.DescripcionError)).ToList());
                throw;
            }
            catch (VerifactuExceptions ex)
            {

                stateProcess.SetInvoiceState(invoice.InvoicePrimaryKey, ElectronicInvoiceStates.Failed, ex.Message);
                processErrors.SaveErrors(invoice.CompanyId, invoice.InvoicePrimaryKey, invoice.InvoiceID, new VerifactuResponseError("INTERNAL", ex.Message));
                throw;
            }

            catch (Exception ex)
            {
                stateProcess.SetInvoiceState(invoice.InvoicePrimaryKey, ElectronicInvoiceStates.Failed, ex.Message);

                throw;
            }







            // Creamos una instacia de la clase factura
            //invoice = new Invoice("JO-ED-0012", new DateTime(2024, 11, 15), "B70960752")
            //{
            //    InvoiceType = TipoFactura.F1,
            //    SellerName = "GUILLERMO RAMIREZ",
            //    BuyerID = "B44531218",
            //    BuyerName = "WEFINZ SOLUTIONS SL",
            //    Text = "PRESTACION SERVICIOS DESARROLLO SOFTWARE",
            //    TaxItems = new List<TaxItem>() {
            //        new TaxItem()
            //        {
            //            TaxRate = 4,
            //            TaxBase = 10,
            //            TaxAmount = 0.4m,

            //        }
            //    },
            //};

            //// Creamos la entrada de la factura
            //invoiceEntry = new InvoiceEntry(invoice, blockchainService, certificateService, fileStorage);

            //// Guardamos la factura
            //invoiceEntry.Save();
        }
    }
}
