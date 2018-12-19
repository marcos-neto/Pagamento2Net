namespace Pagamento2Net.Entidades
{
    public class Endereco
    {
        public string Rua { get; set; } = string.Empty;
        public string Número { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;

        /// <summary>
        /// UF - Ex: SP
        /// </summary>
        public string Estado { get; set; } = string.Empty;
        public string CEP { get; set; } = string.Empty;

        public string FormataLogradouro(int? tamanhoFinal = null) //TODO: refatorar
        {
            string endereçoCompleto = string.Empty;

            if (Número.Length != 0)
            {
                endereçoCompleto += " " + Número;
            }

            if (Complemento.Length != 0)
            {
                endereçoCompleto += " " + Complemento;
            }

            if (tamanhoFinal == 0)
            {
                return Rua + endereçoCompleto;
            }

            if (Rua.Length + endereçoCompleto.Length <= tamanhoFinal)
            {
                return Rua + endereçoCompleto;
            }

            return Rua.Substring(0, tamanhoFinal.Value - endereçoCompleto.Length);
        }

        /// <summary>
        /// Retorna os 5 primeiro digitos do CEP
        /// </summary>
        /// <returns>5 primeiros digitos</returns>
        public string PrefixoCEP()
        {
            if (CEP.Length >= 8)
            {
                return CEP.Substring(0, 5);
            }
            else
            {
                return CEP;
            }
        }

        /// <summary>
        /// Retorna os 3 últimos digitos do CEP
        /// </summary>
        /// <returns>3 últimos digitos</returns>
        public string SufixoCEP()
        {
            if (CEP.Length >= 8)
            {
                return CEP.Substring(CEP.Length - 3, 3);
            }
            else
            {
                return CEP;
            }
        }
    }
}