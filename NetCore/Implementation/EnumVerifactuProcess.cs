using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation
{
    public enum EnumVerifactuProcess
    {
        ValidacionInicial = 0,
        GenerandoBlockchain = 2,
        GenerandoRequestXML = 3,
        EnviandoRequest = 4,
        ProcesandoRespuesta = 5
    }
}
