using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Xml.Factu;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface ISettingsServices
    {
        Settings GetSettings();
    }

    public class SettingsServices : ISettingsServices
    {

        public Settings GetSettings()
        {
            return new Settings()
            {
                IDVersion = "1.0",
                VeriFactuEndPointPrefix = "https://prewww1.aeat.es/wlpl/TIKE-CONT/ws/SistemaFacturacion/VerifactuSOAP",
                SkipNifAeatValidation = true,
                VeriFactuEndPointValidatePrefix = "https://prewww2.aeat.es/wlpl/TIKE-CONT/ValidarQR",
                SistemaInformatico = new SettingsSistemaInformatico()
                {
                    NombreRazon = "FacturasApp",
                    NIF = "B72877814",
                    NombreSistemaInformatico = "FacturasApp",
                    IdSistemaInformatico = "01",
                    Version = "5.51.0.2",
                    NumeroInstalacion = "1",
                    TipoUsoPosibleSoloVerifactu = "N",
                    TipoUsoPosibleMultiOT = "S",
                    IndicadorMultiplesOT = "S"
                }
            };
        }
    }
    public class Settings
    {
        public string IDVersion { get; set; }

        public string VeriFactuEndPointPrefix { get; set; }

        public bool SkipNifAeatValidation { get; set; }

        public string VeriFactuEndPointValidatePrefix { get; set; }
        public SettingsSistemaInformatico SistemaInformatico { get; set; }
    }

    public class SettingsSistemaInformatico
    {
        public string NIF { get; set; }
        public string NombreRazon { get; set; }
        public string NombreSistemaInformatico { get; set; }
        public string IdSistemaInformatico { get; set; }
        public string Version { get; set; }
        public string NumeroInstalacion { get; set; }

        public string TipoUsoPosibleSoloVerifactu { get; set; }

        public string TipoUsoPosibleMultiOT { get; set; }

        public string IndicadorMultiplesOT { get; set; }

    }

    public static class SettingsExtenstions
    {
        public static SistemaInformatico ToSistemaInformatico(this SettingsSistemaInformatico sistemaInformatico)
        {
            return new SistemaInformatico()
            {
                NIF = sistemaInformatico.NIF,
                NombreRazon = sistemaInformatico.NombreRazon,
                NombreSistemaInformatico = sistemaInformatico.NombreSistemaInformatico,
                IdSistemaInformatico = sistemaInformatico.IdSistemaInformatico,
                Version = sistemaInformatico.Version,
                NumeroInstalacion = sistemaInformatico.NumeroInstalacion,
                TipoUsoPosibleSoloVerifactu = sistemaInformatico.TipoUsoPosibleSoloVerifactu,
                TipoUsoPosibleMultiOT = sistemaInformatico.TipoUsoPosibleMultiOT,
                IndicadorMultiplesOT = sistemaInformatico.IndicadorMultiplesOT
            };
        }
    }
}
