using Pagamento2Net.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagamento2Net.Entidades
{
    /// <summary>
    /// Títulos de Cobrança (Próprio e Outros Bancos)
    /// </summary>
    public class Titulo : Documento
    {
        /// <summary>
        /// 
        /// </summary>
        public string NomeCedente { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal ValorDoTitulo { get; set; }

        public decimal ValorDescontos()
        {
            return this.ValorDoDesconto + this.ValorDoAbatimento;
        }

        public decimal ValorAcrescimos()
        {
            return this.ValorDaMulta + this.ValorDaMora;
        }
    }
}
