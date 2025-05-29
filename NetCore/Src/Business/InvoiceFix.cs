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
using VeriFactu.Net.Core.Implementation.Service;
using VeriFactu.Xml.Factu.Alta;

namespace VeriFactu.Business
{

    /// <summary>
    /// Representa una acción de subsanación de un registro
    /// anteriormente presentado.
    /// </summary>
    public class InvoiceFix : InvoiceEntry
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="invoice">Instancia de factura de entrada en el sistema.</param>
        public InvoiceFix(Invoice invoice, IBlockchainService blockchainService, ICertificateService certificateService, IFileStorage fileStorage, IElectronicInvoiceStateService stateProcess, Settings settings, ILogger logger) : base(invoice, blockchainService, certificateService, fileStorage, stateProcess, settings, logger)
        {
        }


        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Establece el registro relativo a la entrada
        /// a contabilizar y enviar.
        /// </summary>
        internal override void SetRegistro()
        {

            base.SetRegistro();

            // Establecemos que se trata de una subsanación
            var registroAlta = Registro as RegistroAlta;

            if (registroAlta == null)
                throw new Exception($"No se ha encontrado el RegistroAlta correspondiente a la entrada {this}.");

            registroAlta.Subsanacion = "S";


        }

        #endregion

        #region Métodos Públicos de Instancia

        /// <summary>
        /// Devuelve una lista con los errores de la
        /// factura por el incumplimiento de reglas de negocio.
        /// </summary>
        /// <returns>Lista con los errores encontrados.</returns>
        public override List<string> GetBusErrors()
        {

            var errors = new List<string>();

            if (string.IsNullOrEmpty(Invoice.SellerName))
                errors.Add($"Es necesario que la propiedad Invoice.SellerName tenga un valor.");

            // Limite listas
            if (Invoice.RectificationItems?.Count > 1000)
                errors.Add($"Invoice.RectificationItems.Count no puede ser mayor de 1.000.");

            if (Invoice.TaxItems?.Count > 12)
                errors.Add($"Invoice.TaxItems.Count no puede ser mayor de 12.");

            errors.AddRange(GetTaxItemsValidationErrors());
            errors.AddRange(GetInvoiceValidationErrors());

            return errors;

        }

        #endregion

    }

}