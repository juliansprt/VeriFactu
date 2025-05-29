using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VeriFactu.Blockchain;

namespace VeriFactu.Net.Core.Implementation.Service
{
    public interface IBlockchainService
    {
        BlockchainModel GetBlockchain(int companyId);

        void AddBlockchain(BlockchainModel blockchainModel);

        void RemoveBlockchain(Guid id);


    }


    public class BlockchainModel
    {
        public BlockchainModel(string sellerId)
        {
            SellerID = sellerId;
        }

        public Guid Id { get; set; } = Guid.NewGuid();
        public int CompanyId { get; set; }
        public string SellerID { get; set; }
        public ulong CurrentID { get; set; }

        public ulong PreviousID { get; set; }

        public DateTime? CurrentTimeStamp { get; set; }

        public BlockchainRegistro Current { get; set; }

        public BlockchainRegistro Previous { get; set; }

    }


    public class BlockchainRegistro
    {
        public ulong Id { get; set; }

        public DateTime? Timestamp { get; set; }

        public string FacturaFechaExpedicion { get; set; }

        public string FacturaIDEmisor { get; set; }

        public string FacturaNumSerie { get; set; }

        public string Huella { get; set; }



    }

    public class BlockchainService : IBlockchainService
    {
        List<BlockchainModel> blockchains = new List<BlockchainModel>();
        Dictionary<int, string> companies = new Dictionary<int, string>() {{ 100, "B70960752" }};



        public void AddBlockchain(BlockchainModel blockchainModel)
        {
            blockchains.Add(blockchainModel);
        }

        public BlockchainModel GetBlockchain(int companyId)
        {
            var result = blockchains.OrderBy(p => p.CurrentID).LastOrDefault(p => p.CompanyId == companyId);
            if(result == null)
                return new BlockchainModel(companies[companyId]) { CompanyId = companyId };

            return result;
        }

        public void RemoveBlockchain(Guid id)
        {
            blockchains.RemoveAt(blockchains.FindIndex(p => p.Id == id));
        }

    }

    public static class BlockchainExtensions
    {
        public static Blockchain.Blockchain ToBlockchainVerifactu(this BlockchainModel model, IBlockchainService blockchainService)
        {
            var result = new Blockchain.Blockchain(model.SellerID, model.CompanyId, blockchainService)
            {
                Id = model.Id,
                CurrentID = model.CurrentID,
                PreviousID = model.PreviousID,
                CurrentTimeStamp = model.CurrentTimeStamp,
                PreviousTimeStamp = model.Previous?.Timestamp
            };
            if (model.Current != null)
            {
                result.Current = new Xml.Factu.Registro()
                {
                    IDFactura = new Xml.IDFactura()
                    {
                        FechaExpedicion = model.Current.FacturaFechaExpedicion,
                        IDEmisorFactura = model.Current.FacturaIDEmisor,
                        NumSerieFactura = model.Current.FacturaNumSerie,
                    },
                    Huella = model.Current.Huella
                };
            }

            if (model.Previous != null)
            {
                result.Previous = new Xml.Factu.Registro()
                {
                    IDFactura = new Xml.IDFactura()
                    {
                        FechaExpedicion = model.Previous.FacturaFechaExpedicion,
                        IDEmisorFactura = model.Previous.FacturaIDEmisor,
                        NumSerieFactura = model.Previous.FacturaNumSerie,
                    },
                    
                    Huella = model.Previous.Huella
                };
            }
            return result;
        }

        public static BlockchainModel ToBlockchainFacturasApp(this Blockchain.Blockchain model, int companyId)
        {
            var result = new BlockchainModel(model.SellerID)
            {
                Id = model.Id,
                CurrentID = model.CurrentID,
                PreviousID = model.PreviousID,
                CurrentTimeStamp = model.CurrentTimeStamp,
                CompanyId = companyId
            };

            if (model.Current != null)
            {
                result.Current = new BlockchainRegistro()
                {
                    Id = model.CurrentID,
                    Timestamp = model.CurrentTimeStamp,
                    FacturaFechaExpedicion = model.Current.IDFactura.FechaExpedicion,
                    FacturaIDEmisor = model.Current.IDFactura.IDEmisorFactura,
                    FacturaNumSerie = model.Current.IDFactura.NumSerieFactura,
                    Huella = model.Current.Huella
                };
            }

            if(model.Previous != null)
            {
                result.Previous = new BlockchainRegistro()
                {
                    Id = model.PreviousID,
                    Timestamp = model.PreviousTimeStamp,
                    FacturaFechaExpedicion = model.Previous.IDFactura.FechaExpedicion,
                    FacturaIDEmisor = model.Previous.IDFactura.IDEmisorFactura,
                    FacturaNumSerie = model.Previous.IDFactura.NumSerieFactura,
                    Huella = model.Previous.Huella
                };
            }
            return result;
        }
    }
    
}
