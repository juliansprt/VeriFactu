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
using System.IO;
using VeriFactu.Common;
using VeriFactu.Net.Core.Implementation;
using VeriFactu.Net.Core.Implementation.Exceptions;
using VeriFactu.Net.Core.Implementation.Service;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Business.Operations
{

    /// <summary>
    /// Representa una acción de alta o anulación de registro
    /// en todo lo referente los datos de la factura a la que
    /// pertenece la acción.
    /// </summary>
    public class InvoiceActionData
    {
        protected Settings _settings;
        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoiceID">Identificador de la factura.</param>
        /// <param name="invoiceDate">Fecha emisión de documento.</param>
        /// <param name="sellerID">Identificador del vendedor.</param>        
        /// <exception cref="ArgumentNullException">Los argumentos invoiceID y sellerID no pueden ser nulos</exception>
        public InvoiceActionData(string invoiceID, int invoiceId, int companyId, ElectronicInvoiceStates state, Settings settings, DateTime invoiceDate, string sellerID, ILogger logger)
        {
            _settings = settings;
            if (invoiceID == null || sellerID == null)
                throw new ArgumentNullException($"Los argumentos invoiceID y sellerID no pueden ser nulos.");

            var invoice = new Invoice(invoiceID, invoiceId, companyId, state, invoiceDate, sellerID, settings, logger);
            var errors = GetArgErrors(invoice);
            Invoice = invoice;

        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceActionData(Invoice invoice, Settings settings)
        {
            _settings = settings;
            var errors = GetArgErrors(invoice);

            if (errors.Count > 0)
                throw new VerifactuValidationsInitialExceptions("Ocurrieron las siguientes errores de validacion ", errors.ToArray());

            Invoice = invoice;

            // Establecemos el registro alta/anulación
            SetRegistro();

        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Devuelve una lista con los errores de la
        /// factura como argumento de entrada.
        /// </summary>
        /// <param name="invoice">Instancia de la clase Invlice a verificar.</param>
        /// <returns>Lista con los errores encontrados.</returns>
        internal virtual List<string> GetArgErrors(Invoice invoice)
        {

            var errors = new List<string>();

            if (string.IsNullOrEmpty(invoice.InvoiceID))
                errors.Add($"El objeto Invoice no puede tener como valor de" +
                    $" su propiedad InvoiceID un valor nulo o una cadena vacía.");

            if (string.IsNullOrEmpty(invoice.SellerID))
                errors.Add($"El objeto Invoice no puede tener como valor de" +
                    $" su propiedad SellerID un valor nulo o una cadena vacía.");

            if (invoice.InvoiceDate.Year < 2024)
                errors.Add($"El objeto Invoice no puede tener como valor de" +
                    $" su propiedad InvoiceDate una fecha de años anteriores al 2024.");

            return errors;

        }


        /// <summary>
        /// Establece el registro relativo a la entrada
        /// a contabilizar y enviar.
        /// </summary>
        internal virtual void SetRegistro()
        {

            Registro = Invoice.GetRegistroAlta();

        }


        #endregion

        #region Propiedades Públicas de Instancia

        /// <summary>
        /// Identificador de la factura en formato
        /// hexadecimal.
        /// </summary>
        public virtual string EncodedInvoiceID => Utils.GetEncodedToHex(Invoice.InvoiceID);


        /// <summary>
        /// Objeto Invoice de la entrada.
        /// </summary>
        public Invoice Invoice { get; private set; }

        /// <summary>
        /// Registro Verifactu.
        /// </summary>
        public Registro Registro { get; protected set; }

        /// <summary>
        /// Identificador del vendedro.
        /// </summary>
        public string SellerID => Invoice.SellerID;

        /// <summary>
        /// Indica si el registro ha sido contabilizado en la 
        /// cadena de bloques.
        /// </summary>
        public bool Posted { get; protected set; }

        /// <summary>
        /// Indica si se trata de un intento de reenvío.
        /// </summary>
        public bool IsRetrySend { get; protected set; }

        /// <summary>
        /// Indica si la entrada ya ha sido guardada.
        /// </summary>
        internal bool IsSaved { get; set; }

        /// <summary>
        /// Indica si el resgistro ha sido envíado a la AEAT
        /// en un envío sincrono con el método Save. Si el
        /// envío se ha realizado de manera asíncrona mediante
        /// el mecanismo de control de flujo de InvoiceQueue
        /// su valor no será true aunque se haya envíado.
        /// </summary>
        public bool IsSent { get; protected set; }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Representación textual de la instancia.
        /// </summary>
        /// <returns> Representación textual de la instancia.</returns>
        public override string ToString()
        {

            return $"{Invoice.SellerID}-{Invoice.InvoiceID}-{Invoice.InvoiceDate:dd/MM/yyyy}";

        }

        #endregion

    }

}