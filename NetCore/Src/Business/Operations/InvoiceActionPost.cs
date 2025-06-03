/*
    This file is part of the VeriFactu (R) project.
    Copyright (c) 2024-2025 Irene Solutions SL
    Authors: Irene Solutions SL.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    IRENE SOLUTIONS SL. IRENE SOLUTIONS SL DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
        http://www.irenesolutions.com/terms-of-use.pdf
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the VeriFactu software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving VeriFactu XML data on the fly in a web application, shipping VeriFactu
    with a closed source product.
    
    For more information, please contact Irene Solutions SL. at this
    address: info@irenesolutions.com
 */

using Polly;
using Serilog;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using VeriFactu.Net;
using VeriFactu.Net.Core.Implementation;
using VeriFactu.Net.Core.Implementation.Exceptions;
using VeriFactu.Net.Core.Implementation.Service;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Business.Operations
{

    /// <summary>
    /// Representa una acción de alta o anulación de registro
    /// en todo lo referente a su gestión contable en la 
    /// cadena de bloques.
    /// </summary>
    public class InvoiceActionPost : InvoiceActionMessage
    {
        protected readonly ICertificateService _certificateService;
        #region Variables Privadas de Instancia



        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceActionPost(Invoice invoice, IBlockchainService blockchainService, ICertificateService certificateService, IFileStorage fileStorage, IElectronicInvoiceStateService stateProcess, Settings settings, ILogger logger, IPostProcessVerifactu postProcessVerifactu, IProcessErrors processErrors, IAsyncPolicy<string> resilencePolicy) : base(invoice, fileStorage, stateProcess, settings, logger, postProcessVerifactu, processErrors, resilencePolicy)
        {
            _certificateService = certificateService;
            //BlockchainManager =  Blockchain.Blockchain.Get(Invoice.SellerID);
            BlockchainManager = blockchainService.GetBlockchain(invoice.CompanyId).ToBlockchainVerifactu(blockchainService);


        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Contabiliza una entrada.
        /// <para> 1. Incluye el registro en la cadena de bloques.</para>
        /// <para> 2. Recalcula Xml con la info Blockchain actualizada.</para>
        /// <para> 3. Guarda el registro en disco en el el directorio de registros emitidos.</para>
        /// <para> 4. Establece Posted = true.</para>
        /// </summary>
        internal virtual void Post()
        {
            try
            {
                // Añadimos el registro de alta (1)
                _logger.Information("Añadiendo el registro {InvoiceID} a la cadena de bloques", Invoice.InvoiceID);
                BlockchainManager.Add(Registro);
                _logger.Information("Registro {InvoiceID} añadido a la cadena de bloques", Invoice.InvoiceID);
            }
            catch (Exception ex)
            {
                throw new VerifactuBlockchainException("Error al generar el Blockchain", ex);
            }


            // Actualizamos datos (2,3,4)
            _logger.Information("Guardando cambios en la cadena de bloques para el registro {InvoiceID}", Invoice.InvoiceID);   
            SaveBlockchainChanges();
            _logger.Information("Cambios guardados en la cadena de bloques para el registro {InvoiceID}", Invoice.InvoiceID);

        }

        /// <summary>
        /// Actualiza los datos tras la incorporación del registro
        /// a la cadena de bloques.
        /// <para> 1. Recalcula Xml con la info Blockchain actualizada.</para>
        /// <para> 2. Guarda el registro en disco en el el directorio de registros emitidos.</para>
        /// <para> 3. Establece Posted = true.</para>
        /// </summary>
        internal virtual void SaveBlockchainChanges() 
        {

            if (Registro.BlockchainLinkID == 0)
                throw new VerifactuBlockchainException($"El registro {Registro}" +
                    $" no está incluido en la cadena de bloques.");

            if (Posted)
                throw new VerifactuBlockchainException($"La operación {this}" +
                    $" ya está contabilizada.");

            try
            {
                // Regeneramos el Xml
                Xml = GetXml();

                // Guardamos el xml
                _fileStorage.SaveRequestXML(Xml, Invoice.CompanyId, Invoice.InvoiceID);
            }
            catch (Exception ex)
            {
                throw new VerifactuRequestXMLException("Error al generar el XML de Envio", ex);
            }


            //File.WriteAllBytes(InvoiceFilePath, Xml);

            // Marcamos como contabilizado
            Posted = true;

        }

        /// <summary>
        /// Deshace cambios de guardado de documente eliminando
        /// el elemento de la cadena de bloques y marcando los
        /// archivos relacionados como erróneos.
        /// </summary>
        protected void ClearPost()
        {
            BlockchainManager.Delete(Registro);
            Posted = false;
        }

        /// <summary>
        /// Ejecuta la contabilización del registro.
        /// </summary>
        /// <returns>Si todo funciona correctamente devuelve null.
        /// En caso contrario devuelve una excepción con el error.</returns>
        internal void ExecutePost(X509Certificate2 certificate = null)
        {

            // Compruebo el certificado
            //var cert = Wsd.GetCheckedCertificate();

            if (certificate == null)
                throw new Exception("Existe algún problema con el certificado.");
            
            Post();

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Gestor de cadena de bloques para el registro.
        /// </summary>
        public Blockchain.Blockchain BlockchainManager { get; private set; }   

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Contabiliza y envía a la AEAT el registro.
        /// </summary>
        public void Save()
        {
            try
            {
                _logger.Information("Iniciando Obtencion del sertificado de la entrada {InvoiceID}", Invoice.InvoiceID);
                var cert = _certificateService.GetCertificate(Invoice.CompanyId);
                _logger.Information("Certificado obtenido de la entrada {InvoiceID}", Invoice.InvoiceID);
                try
                {

                    ExecutePost(cert);
                    ChangeStateInvoice(ElectronicInvoiceStates.PendingSendAEAT);
                    _logger.Information("Iniciando envio a AEAT de la entrada {InvoiceID} Cambio de estado a PendingSendAEAT", Invoice.InvoiceID);

                    ExecuteSend(_settings.VeriFactuEndPointPrefix, cert);

                    ChangeStateInvoice(ElectronicInvoiceStates.SendedAEAT);
                    _logger.Information("Envio a AEAT de la entrada {InvoiceID} realizado con exito Cambio de estado a SendedAEAT", Invoice.InvoiceID);
                    ProcessResponse();
                }
                catch (VerifactuExceptions ex)
                {
                    _logger.Information(ex, "Error al enviar la entrada {InvoiceID}", Invoice.InvoiceID);
                    ClearPost();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error al enviar la entrada {InvoiceID}", Invoice.InvoiceID);
                    ClearPost();
                    throw;
                }

                _logger.Information("Envio a AEAT de la entrada {InvoiceID} realizado con exito", Invoice.InvoiceID);
                ChangeStateInvoice(ElectronicInvoiceStates.Valid);

                var qr = Registro.GetValidateQr(_settings.VeriFactuEndPointValidatePrefix);

                _postProcessService.ProcessVerifactu(Invoice.CompanyId, Invoice.InvoicePrimaryKey, Invoice.State, qr);

                _logger.Information("Cambio de estado a Valid de la entrada {InvoiceID}", Invoice.InvoiceID);
                //if (string.IsNullOrEmpty(CSV))
                //{
                //    ClearPost();
                //}
            }
            catch (VerifactuValidationsInitialExceptions ex)
            {
                _logger.Information(ex, "Error de validación inicial de la entrada {InvoiceID}", Invoice.InvoiceID);
                ChangeStateInvoice(ElectronicInvoiceStates.Incorrect, ex.Message, ex.Validations.Select(p => new VerifactuResponseError(string.Empty, p)).ToArray());
                throw;
            }
            catch (VerifactuResponseException ex)
            {
                const string EstadoParcialmentCorrecto = "ParcialmenteCorrecto";
                ElectronicInvoiceStates state = ElectronicInvoiceStates.Incorrect;

                if (ex.EstadoEnvio == EstadoParcialmentCorrecto)
                    state = ElectronicInvoiceStates.PartlyCorrect;

                _logger.Information(ex, "Cambio de estado {InvoiceID} a {state}", Invoice.InvoiceID, state);
                ChangeStateInvoice(state, ex.Message, ex.Errors.Select(p => new VerifactuResponseError(p.CodigoError, p.DescripcionError)).ToArray());
                if (state == ElectronicInvoiceStates.Incorrect)
                    throw;
            }
            catch (VerifactuExceptions ex)
            {
                _logger.Error(ex, "Error al enviar la entrada {InvoiceID}", Invoice.InvoiceID);
                ChangeStateInvoice(ElectronicInvoiceStates.Failed, ex.Message, new VerifactuResponseError("INTERNAL", ex.Message));
                throw;
            }

            catch (Exception ex)
            {
                _logger.Error(ex, "Error al enviar la entrada {InvoiceID}", Invoice.InvoiceID);
                ChangeStateInvoice(ElectronicInvoiceStates.Failed, ex.Message, new VerifactuResponseError("INTERNAL", ex.Message));
                throw;
            }




        }

        #endregion

    }
}