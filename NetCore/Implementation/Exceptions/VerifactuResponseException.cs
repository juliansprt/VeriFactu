using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Xml.Factu.Fault;

namespace VeriFactu.Net.Core.Implementation.Exceptions
{
    public class VerifactuResponseException : VerifactuExceptions
    {
        public Fault Fault { get; set; }

        public List<VerifactuResponseError> Errors { get; set; } = new List<VerifactuResponseError>();

        public string EstadoEnvio { get; set; }

        public override bool RemoveBlockchain 
        {
            get 
            {
                return
                    EstadoEnvio != ConstEstadosEnvio.AceptadaConErrores;
            }
        }

        public VerifactuResponseException(string message, string estadoEnvio,  Fault fault = null, IEnumerable<VerifactuResponseError> errors = null) : base(message, EnumVerifactuProcess.ProcesandoRespuesta)
        {
            Fault = fault;
            EstadoEnvio = estadoEnvio;
            if(fault != null)
            {
                StringBuilder builderDescription = new StringBuilder();
                builderDescription.AppendLine($"{nameof(fault.faultstring)}: {fault.faultstring}");
                if (fault.detail != null)
                {
                    builderDescription.AppendLine($"{nameof(fault.detail.callstack)}: {fault.detail.callstack}");
                }

                Errors.Add(new VerifactuResponseError(fault.faultcode, builderDescription.ToString()));
            }

            if(errors != null)
            {
                Errors.AddRange(errors);
            }
        }

        public VerifactuResponseException(string message, string estadoEnvio, Exception innerException, Fault fault = null, IEnumerable<VerifactuResponseError> errors = null) : base(message, EnumVerifactuProcess.ProcesandoRespuesta, innerException)
        {
            Fault = fault;
            EstadoEnvio = estadoEnvio;
            if (fault != null)
            {
                StringBuilder builderDescription = new StringBuilder();
                builderDescription.AppendLine($"{nameof(fault.faultstring)}: {fault.faultstring}");
                if (fault.detail != null)
                {
                    builderDescription.AppendLine($"{nameof(fault.detail.callstack)}: {fault.detail.callstack}");
                }

                Errors.Add(new VerifactuResponseError(fault.faultcode, builderDescription.ToString()));
            }
            if (errors != null)
            {
                Errors.AddRange(errors);
            }
        }

        public VerifactuResponseException(string message, string estadoEnvio, Fault fault = null, VerifactuResponseError error = null) : base(message, EnumVerifactuProcess.ProcesandoRespuesta)
        {
            Fault = fault;
            EstadoEnvio = estadoEnvio;
            if (fault != null)
            {
                StringBuilder builderDescription = new StringBuilder();
                builderDescription.AppendLine($"{nameof(fault.faultstring)}: {fault.faultstring}");
                if (fault.detail != null)
                {
                    builderDescription.AppendLine($"{nameof(fault.detail.callstack)}: {fault.detail.callstack}");
                }

                Errors.Add(new VerifactuResponseError(fault.faultcode, builderDescription.ToString()));
            }
            if (error != null)
            {
                Errors.Add(error);
            }
        }
    }


    public class VerifactuResponseError
    {
        public VerifactuResponseError(string codigoError, string descripcionError)
        {
            CodigoError = codigoError;
            DescripcionError = descripcionError;
        }
        public string CodigoError { get; set; }

        public string DescripcionError { get; set; }
    }


}
