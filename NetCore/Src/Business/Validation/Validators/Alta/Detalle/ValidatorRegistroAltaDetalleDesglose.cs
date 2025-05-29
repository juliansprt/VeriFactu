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

using System.Collections.Generic;
using VeriFactu.Business.Validation.Validators.Alta.Detalle.Regimen;
using VeriFactu.Net.Core.Implementation.Service;
using VeriFactu.Xml.Factu.Alta;
using VeriFactu.Xml.Soap;

namespace VeriFactu.Business.Validation.Validators.Alta.Detalle
{

    /// <summary>
    /// Valida los datos de RegistroAlta Tercero.
    /// </summary>
    public class ValidatorRegistroAltaDetalleDesglose : ValidatorRegistroAlta
    {

        #region Construtores de Instancia

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="envelope"> Envelope de envío al
        /// servicio Verifactu de la AEAT.</param>
        /// <param name="registroAlta"> Registro de alta del bloque Body.</param>
        public ValidatorRegistroAltaDetalleDesglose(Envelope envelope, RegistroAlta registroAlta, Settings settings) : base(envelope, registroAlta, settings)
        {
        }

        #endregion

        #region Métodos Privados de Instancia

        /// <summary>
        /// Obtiene los errores de un bloque en concreto.
        /// </summary>
        /// <returns>Lista con los errores de un bloque en concreto.</returns>
        protected override List<string> GetBlockErrors()
        {

            var result = new List<string>();


            // 15. Agrupación Desglose / DetalleDesglose.
            
            var detalles = _RegistroAlta.Desglose;

            if (detalles != null) 
            {

                foreach (var detalle in detalles) 
                {

                    // 15.1 TipoImpositivo
                    result.AddRange(new ValidatorRegistroAltaDetalleDesgloseTipoImpositivo(_Envelope, _RegistroAlta, detalle, _settings).GetErrors());
                    // 15.2 BaseImponibleACoste
                    result.AddRange(new ValidatorRegistroAltaDetalleDesgloseBaseImponibleACoste(_Envelope, _RegistroAlta, detalle, _settings).GetErrors());
                    // 15.3 TipoRecargoEquivalencia
                    result.AddRange(new ValidatorRegistroAltaDetalleDesgloseTipoRecargoEquivalencia(_Envelope, _RegistroAlta, detalle, _settings).GetErrors());
                    // 15.4 CalificacionOperacion
                    result.AddRange(new ValidatorRegistroAltaDetalleDesgloseCalificacionOperacion(_Envelope, _RegistroAlta, detalle, _settings).GetErrors());
                    // 15.5 OperacionExenta
                    result.AddRange(new ValidatorRegistroAltaDetalleDesgloseOperacionExenta(_Envelope, _RegistroAlta, detalle, _settings).GetErrors());
                    // 15.6 ClaveRegimen
                    result.AddRange(new ValidatorRegistroAltaDetalleDesgloseClaveRegimen(_Envelope, _RegistroAlta, detalle, _settings).GetErrors());
                    // 15.7 CuotaRepercutida.
                    result.AddRange(new ValidatorRegistroAltaDetalleDesgloseCuotaRepercutida(_Envelope, _RegistroAlta, detalle, _settings).GetErrors());

                }

            }

            return result;

        }

        #endregion

    }

}