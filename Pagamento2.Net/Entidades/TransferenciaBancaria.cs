using Pagamento2Net.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagamento2Net.Entidades
{
    public class TransferenciaBancaria
    {
        /// <summary>
        ///
        /// </summary>
        public Pagador Pagador { get; set; }

        /// <summary>
        ///
        /// </summary>
        public IList<Transferencia> Transferencias { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int NúmeroRemessa { get; set; }
    }
}
