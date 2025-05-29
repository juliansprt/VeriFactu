using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IElectronicInvoiceStateService
    {
        void SetInvoiceState(int invoiceId, ElectronicInvoiceStates state, string message);

        bool FlagInvoiceProcess(ElectronicInvoiceStates invoiceState, EnumVerifactuProcess process);
    }

    public class ElectronicInvoiceStateService : IElectronicInvoiceStateService
    {
        public bool FlagInvoiceProcess(ElectronicInvoiceStates invoiceState, EnumVerifactuProcess process)
        {
            switch (process)
            {
                case EnumVerifactuProcess.ValidacionInicial:
                    switch (invoiceState)
                    {
                        case ElectronicInvoiceStates.Created:
                            return true;
                        case ElectronicInvoiceStates.PendingSendAEAT:
                            return true;
                        case ElectronicInvoiceStates.Failed:
                            return true;
                        default:
                            return false;
                    }
                case EnumVerifactuProcess.GenerandoBlockchain:
                    switch (invoiceState)
                    {
                        case ElectronicInvoiceStates.Created:
                            return true;
                        case ElectronicInvoiceStates.PendingSendAEAT:
                            return true;
                        case ElectronicInvoiceStates.Failed:
                            return true;
                        default:
                            return false;
                    }
                case EnumVerifactuProcess.GenerandoRequestXML:
                    switch (invoiceState)
                    {
                        case ElectronicInvoiceStates.Created:
                            return true;
                        case ElectronicInvoiceStates.PendingSendAEAT:
                            return true;
                        case ElectronicInvoiceStates.Failed:
                            return true;
                        default:
                            return false;
                    }
                case EnumVerifactuProcess.EnviandoRequest:
                    switch (invoiceState)
                    {
                        case ElectronicInvoiceStates.Created:
                            return true;
                        case ElectronicInvoiceStates.PendingSendAEAT:
                            return true;
                        case ElectronicInvoiceStates.Failed:
                            return true;
                        default:
                            return false;
                    }
                case EnumVerifactuProcess.ProcesandoRespuesta:
                    switch (invoiceState)
                    {
                        case ElectronicInvoiceStates.Created:
                            return true;
                        case ElectronicInvoiceStates.PendingSendAEAT:
                            return true;
                        case ElectronicInvoiceStates.Failed:
                            return true;
                        default:
                            return false;
                    }
                default:
                    return false;
            }
        }

        public void SetInvoiceState(int invoiceId, ElectronicInvoiceStates state, string message)
        {

        }
    }
}
