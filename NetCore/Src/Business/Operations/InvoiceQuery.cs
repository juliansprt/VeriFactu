﻿/*
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

using Serilog;
using System;
using System.Collections.Generic;
using VeriFactu.Common.Exceptions;
using System.Security.Cryptography.X509Certificates;
using VeriFactu.Net.Core.Implementation.Service;
using VeriFactu.Xml;
using VeriFactu.Xml.Factu;
using VeriFactu.Xml.Factu.Consulta;
using VeriFactu.Xml.Factu.Consulta.Respuesta;
using VeriFactu.Xml.Factu.Fault;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Operations
{

    /// <summary>
    /// Consulta de facturas.
    /// </summary>
    public class InvoiceQuery
    {
        protected readonly Settings _settings;
        protected readonly ICertificateService _certificateService;
        protected readonly X509Certificate2 _certificate;
        #region Propiedades Privadas Estáticas

        /// <summary>
        /// Acción para el webservice.
        /// </summary>
        static string _Action = "?op=ConsultaFactuSistemaFacturacion";

        #endregion

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="partyID">NIF sobre el cual ejecutar la consulta.</param>
        /// <param name="partyName">Nombre correpondiente al NIF.</param>
        public InvoiceQuery(string partyID, int companyId, string partyName, Settings settings, ICertificateService certificateService) 
        {
            _settings = settings;
            PartyID = partyID;
            PartyName = partyName;
            _certificateService = certificateService;
            _certificate = _certificateService.GetCertificate(companyId);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="partyID">NIF sobre el cual ejecutar la consulta.</param>
        /// <param name="partyName">Nombre correpondiente al NIF.</param>
        public InvoiceQuery(string partyID, int companyId, string partyName, Settings settings, X509Certificate2 certificate)
        {
            _settings = settings;
            PartyID = partyID;
            PartyName = partyName;
            _certificate = certificate;
        }

        #endregion

        #region Métodos Privados Estáticos

        /// <summary>
        /// Obtiene un objeto invoice a partir de un registro
        /// de consulta de la AEAT.
        /// </summary>
        /// <param name="registro"> Registro de consulta correspondiente a 
        /// una factura.</param>
        /// <param name="sellerName"> Nombre vendedor.</param>        
        /// <returns> Objeto Invoice creado a partir de un registro
        /// de factura de una respuesta de consulta a la AEAT.</returns>
        private static Invoice GetInvoice(RegistroRespuestaConsultaFactuSistemaFacturacion registro, int invoiceId, int companyId, Settings settings, ILogger logger, string sellerName = null)
        {

            var invoice = new Invoice(registro.IDFactura.NumSerieFactura, invoiceId, companyId, Net.Core.Implementation.ElectronicInvoiceStates.Created,
                XmlParser.ToDate(registro.IDFactura.FechaExpedicionFactura), $"{registro.IDFactura.IDEmisorFactura}", settings, logger);

            var registroAlta = registro.DatosRegistroFacturacion;

            invoice.InvoiceType = registroAlta.TipoFactura;
            invoice.SellerName = sellerName;

            if (registroAlta.TipoRectificativaSpecified)
                invoice.RectificationType = registroAlta.TipoRectificativa;

            if (registroAlta.Destinatarios.Count > 1)
                throw new NotImplementedException("El método estático Invoice.FromRegistroAlta" +
                    " no implementa la conversión de RegistrosAlta con más de un destinatario.");

            if (registroAlta.Destinatarios.Count == 1)
            {

                var destinatario = registroAlta.Destinatarios[0];
                invoice.BuyerID = $"{destinatario.NIF}{destinatario?.IDOtro?.ID}";
                invoice.BuyerName = $"{destinatario.NombreRazon}";

                if (!string.IsNullOrEmpty($"{destinatario?.IDOtro?.CodigoPais}"))
                    invoice.BuyerCountryID = $"{destinatario?.IDOtro?.CodigoPais}";

                if (!string.IsNullOrEmpty($"{destinatario?.IDOtro?.IDType}"))
                    invoice.BuyerIDType = destinatario?.IDOtro?.IDType ?? IDType.NIF_IVA;

            }

            invoice.TaxItems = Invoice.FromDesglose(registroAlta.Desglose);

            return invoice;

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve un objeto Envelope con el filtro
        /// para la consulta de facturas emitidas.
        /// </summary>
        /// <param name="year">Año a consultar.</param>
        /// <param name="month">Mes a consultar.</param>
        /// <returns> Objeto Envelope con el filtro
        /// para la consulta de facturas emitidas.</returns>
        private Envelope GetSalesEnvelope(string year, string month)
        {

            return new Envelope()
            {
                Body = new Body()
                {
                    Registro = new ConsultaFactuSistemaFacturacion()
                    {
                        Cabecera = new VeriFactu.Xml.Factu.Consulta.Cabecera()
                        {
                            IDVersion = _settings.IDVersion,
                            ObligadoEmision = new Interlocutor()
                            {
                                NombreRazon = PartyName,
                                NIF = PartyID
                            }
                        },
                        FiltroConsulta = new FiltroConsulta()
                        {
                            PeriodoImputacion = new Xml.Factu.Consulta.PeriodoImputacion()
                            {
                                Ejercicio = year,
                                Periodo = month.PadLeft(2, '0')
                            }
                        }
                    }
                }
            };

        }


        /// <summary>
        /// Devuelve un objeto Envelope con el filtro
        /// para la consulta de facturas emitidas.
        /// </summary>
        /// <param name="year">Año a consultar.</param>
        /// <param name="month">Mes a consultar.</param>
        /// <returns> Objeto Envelope con el filtro
        /// para la consulta de facturas emitidas.</returns>
        private Envelope GetInvoiceEnvelope(string year, string month, string invoiceId)
        {

            return new Envelope()
            {
                Body = new Body()
                {
                    Registro = new ConsultaFactuSistemaFacturacion()
                    {
                        Cabecera = new VeriFactu.Xml.Factu.Consulta.Cabecera()
                        {
                            IDVersion = _settings.IDVersion,
                            ObligadoEmision = new Interlocutor()
                            {
                                NombreRazon = PartyName,
                                NIF = PartyID
                            }
                        },
                        FiltroConsulta = new FiltroConsulta()
                        {
                            PeriodoImputacion = new Xml.Factu.Consulta.PeriodoImputacion()
                            {
                                Ejercicio = year,
                                Periodo = month.PadLeft(2, '0')
                            },
                            NumSerieFactura = invoiceId
                        }
                    }
                }
            };

        }

        /// <summary>
        /// Devuelve un objeto Envelope con el filtro
        /// para la consulta de facturas recibidas.
        /// </summary>
        /// <param name="year">Año a consultar.</param>
        /// <param name="month">Mes a consultar.</param>
        /// <returns> Objeto Envelope con el filtro
        /// para la consulta de facturas recibidas.</returns>
        private Envelope GetPurchasesEnvelope(string year, string month)
        {

            return new Envelope()
            {
                Body = new Body()
                {
                    Registro = new ConsultaFactuSistemaFacturacion()
                    {
                        Cabecera = new VeriFactu.Xml.Factu.Consulta.Cabecera()
                        {
                            IDVersion = _settings.IDVersion,
                            Destinatario = new Interlocutor()
                            {
                                NombreRazon = PartyName,
                                NIF = PartyID
                            }
                        },
                        FiltroConsulta = new FiltroConsulta()
                        {
                            PeriodoImputacion = new Xml.Factu.Consulta.PeriodoImputacion()
                            {
                                Ejercicio = year,
                                Periodo = month.PadLeft(2, '0')
                            }
                        }
                    }
                }
            };

        }

        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// NIF sobre el que opera la consulta.
        /// </summary>
        public string PartyID { get; private set; }

        /// <summary>
        /// NIF sobre el que opera la consulta.
        /// </summary>
        public string PartyName { get; private set; }

        public int CompanyId { get; private set; }

        #endregion

        #region Métodos Públicos Estáticos

        /// <summary>
        /// Obtiene una lista de objetos Invoice
        /// a partir de una respuesta de la AEAT a 
        /// una consulta de documentos.
        /// </summary>
        /// <param name="queryAeatResponse"> Respuesta consulta AEAT.</param>
        /// <returns> Lista de objetos Invoice
        /// a partir de una respuesta de la AEAT a 
        /// una consulta de documentos.</returns>
        public static List<Invoice> GetInvoices(RespuestaConsultaFactuSistemaFacturacion queryAeatResponse, int invoiceId, int companyId, Settings settings, ILogger logger)
        {

            var invoices = new List<Invoice>();

            var sellerName = queryAeatResponse.Cabecera?.ObligadoEmision?.NombreRazon;

            foreach (var registro in queryAeatResponse.RegistroRespuestaConsultaFactuSistemaFacturacion)
                invoices.Add(GetInvoice(registro, invoiceId, companyId, settings, logger, sellerName));

            return invoices;

        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve las facturas emitidas por el NIF
        /// facilitado en la propiedad PartyID.
        /// </summary>
        /// <param name="year">Año a consultar.</param>
        /// <param name="month">Mes a consultar.</param>
        /// <returns>Facturas emitidas registradas en la AEAT.</returns>
        public RespuestaConsultaFactuSistemaFacturacion GetSales(string year, string month) 
        {

            var envelope = GetSalesEnvelope(year, month);
            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);
            var response = InvoiceActionMessage.SendXmlBytes(xml, _Action, simulateTimeOut: _settings.SimulateTimeout);
            var envelopeResponse = Envelope.FromXml(response);

            var fault = envelopeResponse.Body.Registro as Fault;

            if (fault != null)
                throw new Exception($"{fault.faultstring}");

            return envelopeResponse.Body.Registro as RespuestaConsultaFactuSistemaFacturacion;

        }

        /// <summary>
        /// Devuelve las facturas emitidas por el NIF filtrado por invoiceId
        /// facilitado en la propiedad PartyID.
        /// </summary>
        /// <param name="year">Año a consultar.</param>
        /// <param name="month">Mes a consultar.</param>
        /// <returns>Facturas emitidas registradas en la AEAT.</returns>
        public RespuestaConsultaFactuSistemaFacturacion GetInvoice(string year, string month,  string invoiceId)
        {

            var envelope = GetInvoiceEnvelope(year, month, invoiceId);
            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);

            if (_certificate == null)
                throw new Exception("Existe algún problema con el certificado.");

            var response = InvoiceActionMessage.SendXmlBytes(xml, _settings.VeriFactuEndPointPrefix, _Action, _certificate, simulateTimeOut: _settings.SimulateTimeout);
            var envelopeResponse = Envelope.FromXml(response);

            var fault = envelopeResponse.Body.Registro as Fault;

            if (fault != null)
                throw new FaultException(fault);

            return envelopeResponse.Body.Registro as RespuestaConsultaFactuSistemaFacturacion;

        }

        /// <summary>
        /// Devuelve las facturas emitidas por el NIF
        /// facilitado en la propiedad PartyID.
        /// </summary>
        /// <param name="year">Año a consultar.</param>
        /// <param name="month">Mes a consultar.</param>
        /// <returns>Facturas emitidas registradas en la AEAT.</returns>
        public RespuestaConsultaFactuSistemaFacturacion GetPurchases(string year, string month)
        {

            var envelope = GetPurchasesEnvelope(year, month);
            var xml = new XmlParser().GetBytes(envelope, Namespaces.Items);
            var response = InvoiceActionMessage.SendXmlBytes(xml, _Action, simulateTimeOut: _settings.SimulateTimeout);
            var envelopeResponse = Envelope.FromXml(response);

            var fault = envelopeResponse.Body.Registro as Fault;

            if (fault != null)
                throw new FaultException(fault);

            return envelopeResponse.Body.Registro as RespuestaConsultaFactuSistemaFacturacion;

        }

        /// <summary>
        /// Representacioón textual de la instancia.
        /// </summary>
        /// <returns>Representacioón textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{PartyID}";

        }

        #endregion

    }

}