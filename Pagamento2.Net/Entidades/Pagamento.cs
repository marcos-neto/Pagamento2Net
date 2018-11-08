using System.Collections.Generic;

namespace Pagamento2Net.Entidades
{
    public class Pagamento
    {
        /// <summary>
        ///
        /// </summary>
        public Pagador Pagador { get; set; }

        /// <summary>
        ///
        /// </summary>
        public IList<Documento> Documentos { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int NúmeroRemessa { get; set; }
    }
}