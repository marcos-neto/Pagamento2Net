using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Pagamento2Net.Entidades
{
    public class Pessoa
    {
        /// <summary>
        /// CPF ou CNPJ
        /// </summary>
        [Required]
        [RegularExpression(@"([0-9]{2}[\.]?[0-9]{3}[\.]?[0-9]{3}[\/]?[0-9]{4}[-]?[0-9]{2})|([0-9]{3}[\.]?[0-9]{3}[\.]?[0-9]{3}[-]?[0-9]{2})")]
        public string NúmeroCadastro { get; set; }

        public ContaFinanceira ContaFinanceira { get; set; }

        public Endereco Endereço { get; set; }

        [Required]
        public string Nome { get; set; } = string.Empty;

        //public string Observations { get; set; } = string.Empty;
        //public Address Address { get; set; } = new Address();

        public string NúmeroCadastroSomenteNúmeros()
        {
            return new string(NúmeroCadastro.Where(char.IsDigit).ToArray());
        }

        public string TipoNúmeroCadastro(string formatoRetorno)
        {
            if (NúmeroCadastro == string.Empty)
            {
                return "0";
            }

            switch (formatoRetorno)
            {
                case "A":
                    return NúmeroCadastro.Length <= 11 ? "F" : "J";

                case "0":
                    return NúmeroCadastro.Length <= 11 ? "1" : "2";

                case "00":
                    return NúmeroCadastro.Length <= 11 ? "01" : "02";
            }
            throw new Exception("TipoNúmeroCadastro: Formato do retorno inválido.");
        }
    }
}