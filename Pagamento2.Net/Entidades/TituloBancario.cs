using Pagamento2Net.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagamento2Net.Entidades
{
    public class TituloBancario
    {
        /// <summary>
        ///
        /// </summary>
        public Pagador Pagador { get; set; }

        /// <summary>
        ///
        /// </summary>
        public IList<Titulo> Titulos { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int NúmeroRemessa { get; set; }
    }
}
