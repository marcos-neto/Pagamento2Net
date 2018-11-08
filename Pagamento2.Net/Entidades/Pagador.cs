using System.ComponentModel.DataAnnotations;

namespace Pagamento2Net.Entidades
{
    public class Pagador : Pessoa
    {
        /// <summary>
        /// Código do Convênio no Banco
        /// </summary>
        [Required]
        public string CódigoConvênio { get; set; }
    }
}