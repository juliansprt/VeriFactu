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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using VeriFactu.Net;
using VeriFactu.Net.Core.Implementation;
using VeriFactu.Net.Core.Implementation.Exceptions;
using VeriFactu.Net.Core.Implementation.Service;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Fault;
using VeriFactu.Xml.Factu.Respuesta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Operations
{

    /// <summary>
    /// Representa una acción de alta o anulación de registro
    /// en todo lo referente a su envío al web service de la AEAT
    /// y su posterior tratamiento.
    /// </summary>
    public class InvoiceActionMessage : InvoiceActionData
    {
        protected readonly IFileStorage _fileStorage;
        protected readonly IElectronicInvoiceStateService _stateProcess;
        protected readonly IProcessErrors _processErrors;
        protected readonly ILogger _logger;
        protected readonly IPostProcessVerifactu _postProcessService;
        protected readonly IAsyncPolicy<string> _resiliencePolicy;
        #region Propiedades Privadas Estáticas

        /// <summary>
        /// Acción para el webservice.
        /// </summary>
        static string _Action = "?op=RegFactuSistemaFacturacion";

        #endregion

        #region Propiedades Privadas de Instacia

        /// <summary>
        /// Acción para el webservice.
        /// </summary>
        internal virtual string Action => _Action;

        /// <summary>
        /// Sobre SOAP de respuesta de la AEAT.
        /// </summary>
        internal Envelope ResponseEnvelope { get; set; }

        /// <summary>
        /// Error Fault.
        /// </summary>
        internal Fault ErrorFault
        {

            get
            {

                if (ResponseEnvelope == null)
                    return null;

                return ResponseEnvelope.Body.Registro as Fault;

            }

        }

        /// <summary>
        /// Respueta AEAT.
        /// </summary>
        internal RespuestaRegFactuSistemaFacturacion RespuestaRegFactuSistemaFacturacion
        {

            get
            {

                if (ResponseEnvelope == null)
                    return null;

                return ResponseEnvelope.Body.Registro as RespuestaRegFactuSistemaFacturacion;

            }

        }

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoiceID">Identificador de la factura.</param>
        /// <param name="invoiceDate">Fecha emisión de documento.</param>
        /// <param name="sellerID">Identificador del vendedor.</param>        
        /// <exception cref="ArgumentNullException">Los argumentos invoiceID y sellerID no pueden ser nulos</exception>
        public InvoiceActionMessage(string invoiceID, int invoiceId, int companyId, ElectronicInvoiceStates state, DateTime invoiceDate, string sellerID, IFileStorage fileStorage, IElectronicInvoiceStateService stateProcess, Settings settings, ILogger logger, IPostProcessVerifactu postProcessVerifactu, IProcessErrors processErrors, IAsyncPolicy<string> resilencePolicy) : base(invoiceID, invoiceId, companyId, state, settings, invoiceDate, sellerID, logger)
        {
            _fileStorage = fileStorage;
            _stateProcess = stateProcess;
            _logger = logger;
            _postProcessService = postProcessVerifactu;
            _processErrors = processErrors;
            _resiliencePolicy = resilencePolicy;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceActionMessage(Invoice invoice, IFileStorage fileStorage, IElectronicInvoiceStateService stateProcess, Settings settings, ILogger logger, IPostProcessVerifactu postProcessVerifactu, IProcessErrors processErrors, IAsyncPolicy<string> resilencePolicy) : base(invoice, settings)
        {
            
            _fileStorage = fileStorage;
            _stateProcess = stateProcess;
            _logger = logger;
            _processErrors = processErrors;
            _logger.Information($"Generando el XML de la factura {invoice.InvoiceID} para su envío a la AEAT", new { invoice.InvoiceID, invoice.CompanyId, invoice } );
            _postProcessService = postProcessVerifactu;
            _resiliencePolicy = resilencePolicy;
            // Generamos el xml
            Xml = GetXml();

        }

        #endregion

        #region Métodos Privados de Instancia

        protected void ChangeStateInvoice(ElectronicInvoiceStates state, string message = null, params VerifactuResponseError[] errors)
        {
            _stateProcess.SetInvoiceState(Invoice.InvoicePrimaryKey, state, message);
            Invoice.State = state;
            if(errors != null)
                _processErrors.SaveErrors(Invoice.CompanyId, Invoice.InvoicePrimaryKey, Invoice.InvoiceID, errors);
        }

        /// <summary>
        /// Genera el sobre SOAP.
        /// </summary>
        /// <returns>Sobre SOAP.</returns>
        internal virtual Envelope GetEnvelope()
        {

            return new Envelope()
            {
                Body = new Body()
                {
                    Registro = new RegFactuSistemaFacturacion()
                    {
                        Cabecera = new Xml.Factu.Cabecera()
                        {
                            ObligadoEmision = new Interlocutor()
                            {
                                NombreRazon = Invoice.SellerName,
                                NIF = Invoice.SellerID
                            }
                        },
                        RegistroFactura = new List<RegistroFactura>()
                        {
                            new RegistroFactura()
                            {
                                Registro = Registro
                            }
                        }
                    }
                }
            };

        }



        /// <summary>
        /// Envía un xml en formato binario a la AEAT.
        /// </summary>
        /// <param name="xml">Archivo xml en formato binario a la AEAT.</param>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        internal async Task<string> SendAsync(
            byte[] xml,
            string endpointVerifactu,
            X509Certificate2 certificate = null)
        {
            _logger.Information($"Enviando el XML de la factura {Invoice.InvoiceID} a la AEAT", new { Invoice.InvoiceID, Invoice.CompanyId, Invoice });

            // Ejecuta la llamada bajo el pipeline de resiliencia
            var result = await _resiliencePolicy.ExecuteAsync(
                context => Task.Run(() =>
                    SendXmlBytes(xml, endpointVerifactu, Action, certificate, simulateTimeOut: _settings.SimulateTimeout)
                ),
                // Context opcional para tus callbacks de retry/fallback
                new Context
                {
                    [nameof(Invoice.SellerID)] = Invoice.SellerID,
                    [nameof(Invoice.CompanyId)] = Invoice.CompanyId,
                    [nameof(Invoice.SellerName)] = Invoice.SellerName,
                    [nameof(Invoice.InvoiceID)] = Invoice.InvoiceID,
                    [nameof(Invoice.InvoiceDate)] = Invoice.InvoiceDate
                });


            _logger.Information($"Recibida la respuesta de la AEAT para la factura {Invoice.InvoiceID}", new { Invoice.InvoiceID, Invoice.CompanyId, Invoice, result });

            return result;
        }

        /// <summary>
        /// Envía un xml en formato binario a la AEAT.
        /// </summary>
        /// <param name="xml">Archivo xml en formato binario a la AEAT.</param>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        internal string Send(byte[] xml, string endpointVerifactu, X509Certificate2 certificate = null)
        {
            return SendAsync(xml, endpointVerifactu, certificate).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Envía el registro a la AEAT.
        /// </summary>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        protected string Send(string endpointVerifactu, X509Certificate2 certificate = null)
        {

            return Send(Xml, endpointVerifactu, certificate);

        }

        /// <summary>
        /// Ejecuta la contabilización del registro.
        /// </summary>
        /// <returns>Si todo funciona correctamente devuelve null.
        /// En caso contrario devuelve una excepción con el error.</returns>
        protected void ExecuteSend(string endpointVerifactu, X509Certificate2 certificate = null)
        {

            if (!Posted)
                throw new InvalidOperationException("No se puede enviar un registro no contabilizado (Posted = false).");

            Response = Send(endpointVerifactu, certificate);
            IsSent = true;

        }

        /// <summary>
        /// Procesa y guarda respuesta de la AEAT al envío.
        /// </summary>
        /// <param name="response">Texto del xml de respuesta.</param>
        internal void ProcessResponse(string response)
        {

            ResponseEnvelope = GetResponseEnvelope(response);
            ProcessResponse(ResponseEnvelope);

            if (ErrorFault != null)
                throw new VerifactuResponseException($"Error en la respuesta de la AEAT: {ErrorFault.faultstring}", RespuestaRegFactuSistemaFacturacion.EstadoEnvio, fault: ErrorFault);
            if(RespuestaRegFactuSistemaFacturacion?.RespuestaLinea?.Count > 0)
            {
                var errores = RespuestaRegFactuSistemaFacturacion?.RespuestaLinea?.Where(p => !string.IsNullOrEmpty(p.CodigoErrorRegistro));
                if (errores != null && errores.Count() > 0)
                    throw new VerifactuResponseException(
                        $"Error en la respuesta de la AEAT: {string.Join(", ", errores.Select(p => p.DescripcionErrorRegistro))}",
                        RespuestaRegFactuSistemaFacturacion.EstadoEnvio,
                        errors: errores.Select(p => new VerifactuResponseError(p.CodigoErrorRegistro, p.DescripcionErrorRegistro)));
            }

        }

        /// <summary>
        /// Procesa y guarda respuesta de la AEAT al envío.
        /// </summary>
        /// <param name="envelope">Sobre con la respuesta de la AEAT.</param>
        internal void ProcessResponse(Envelope envelope)
        {

            // Almaceno xml envíado
            //File.WriteAllBytes(invoiceEntryFilePath, Xml);
            _fileStorage.SaveRequestXML(Xml, Invoice.CompanyId, Invoice.InvoiceID);

            // Almaceno xml de respuesta
            if (!string.IsNullOrEmpty(Response))
                _fileStorage.SaveResponseXML(Response, Invoice.CompanyId, Invoice.InvoiceID);
                //File.WriteAllText(responseFilePath, Response);
            _logger.Information($"Guardando la respuesta de la AEAT para la factura {Invoice.InvoiceID}", new { Invoice.InvoiceID, Invoice.CompanyId, Invoice });
            // Si la respuesta no ha sido correcta renombro archivo de factura
            //if (Status != "Correcto" && File.Exists(InvoiceFilePath))
            //    File.Move(InvoiceFilePath, GeErrorInvoiceFilePath());

        }

        /// <summary>
        /// Guarda respuesta de la AEAT al envío.
        /// </summary>
        internal void ProcessResponse()
        {

            ProcessResponse(Response);
            ResponseProcessed = true;

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador del eslabón de la cadena asociado
        /// a la factura.
        /// </summary>
        public virtual string InvoiceEntryID => Registro?.BlockchainLinkID == null ?
            null : $"{Registro.BlockchainLinkID}".PadLeft(20, '0');



        /// <summary>
        /// Sobre SOAP.
        /// </summary>
        public Envelope Envelope { get; private set; }

#pragma warning disable CA1819

        /// <summary>
        /// Datos binarios del archivo xml de envío.
        /// </summary>
        public byte[] Xml { get; protected set; }

#pragma warning restore CA1819

        /// <summary>
        /// Respuesta del envío a la AEAT.
        /// </summary>
        public string Response { get; private set; }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string ErrorCode
        {

            get
            {

                if (ErrorFault != null)
                    return ErrorFault.faultcode;

                if (RespuestaRegFactuSistemaFacturacion?.RespuestaLinea != null &&
                    RespuestaRegFactuSistemaFacturacion?.RespuestaLinea.Count > 0 &&
                    !string.IsNullOrEmpty(RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].CodigoErrorRegistro))
                    return RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].CodigoErrorRegistro;

                return null;

            }

        }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string ErrorDescription
        {

            get
            {

                if (ErrorFault != null)
                    return ErrorFault.faultstring;

                if (RespuestaRegFactuSistemaFacturacion?.RespuestaLinea != null &&
                    RespuestaRegFactuSistemaFacturacion.RespuestaLinea.Count > 0 &&
                    !string.IsNullOrEmpty(RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].DescripcionErrorRegistro))
                    return RespuestaRegFactuSistemaFacturacion.RespuestaLinea[0].DescripcionErrorRegistro;

                return null;

            }

        }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string Status
        {

            get
            {

                if (RespuestaRegFactuSistemaFacturacion == null)
                    return null;

                return RespuestaRegFactuSistemaFacturacion.EstadoEnvio;

            }

        }

        /// <summary>
        /// Código de error.
        /// </summary>
        public string CSV
        {

            get
            {

                if (RespuestaRegFactuSistemaFacturacion == null)
                    return null;

                return RespuestaRegFactuSistemaFacturacion.CSV;

            }

        }

        /// <summary>
        /// Indica si el registro ha sido envíado y la respuesta ha
        /// sido procesada de la AEAT.
        /// </summary>
        public bool ResponseProcessed { get; private set; }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Envía un xml en formato binario a la AEAT.
        /// </summary>
        /// <param name="xml">Archivo xml en formato binario a la AEAT.</param>
        /// /// <param name="op"> Acción para el webservice.</param>
        /// <returns>Devuelve las respuesta de la AEAT.</returns>
        public static string SendXmlBytes(byte[] xml, string verifactuEndpoint, string op = null, X509Certificate2 certificate = null, bool simulateTimeOut = false)
        {
            try
            {
                if (op == null)
                    op = _Action;

                XmlDocument xmlDocument = new XmlDocument();

                using (var msXml = new MemoryStream(xml))
                    xmlDocument.Load(msXml);

                var url = verifactuEndpoint;
                var action = $"{url}{op}";
                
                if(simulateTimeOut)
                    throw new TimeoutException("Test timeout exception");

                return Wsd.Call(url, action, xmlDocument, certificate);
            }
            catch (Exception ex)
            {
                throw new VerifactuEnviandoRequestException("Error enviando el request a la AEAT", ex);
            }
        }

        /// <summary>
        /// Envía un sobre SOAP.
        /// </summary>
        /// <param name="envelope">Sobre a enviar.</param>
        /// <param name="op">Acción del webservice.</param>
        /// <returns>Respuesta del servidor.</returns>
        public static string SendEnvelope(Envelope envelope, string op) 
        {

            // Generamos el xml
            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);

            return SendXmlBytes(xml, op);

        }

        /// <summary>
        /// Envía un sobre SOAP.
        /// </summary>
        /// <param name="envelope">Sobre a enviar.</param>
        /// <returns>Respuesta del servidor.</returns>
        public static Envelope SendEnvelope(Envelope envelope)
        {

            // Generamos el xml
            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);
            var response =  SendXmlBytes(xml, _Action);

            return Envelope.FromXml(response);

        }


        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Serializa como sobre soap un string de respuesta
        /// de la AEAT.
        /// </summary>
        /// <param name="response">Texto con la respuesta xml de la AEAT.</param>
        /// <returns>Objeto Envelope con la respuesta.</returns>
        public Envelope GetResponseEnvelope(string response)
        {

            if (string.IsNullOrEmpty(response))
                throw new InvalidOperationException("No existe ninguna respuesta que guardar.");

            return Envelope.FromXml(response);

        }

        /// <summary>
        /// Devuelve los bytes del XML serializado con los 
        /// datos actuales.
        /// </summary>
        /// <returns>Bytes del XML serializado con los 
        /// datos actuales.</returns>
        public byte[] GetXml()
        {

            // Creamos el xml de envío SOAP
            Envelope = GetEnvelope();
            // Generamos el xml
            return new XmlParser().GetBytes(Envelope, Namespaces.Items);

        }

        #endregion

    }

}