using Pagamento2Net.Enums;

namespace Pagamento2Net.Entidades
{
    public class ContaFinanceira
    {
        /// <summary>
        /// Conta
        /// </summary>
        public string Conta { get; set; } = string.Empty;

        /// <summary>
        /// Dígito verificador da Conta
        /// </summary>
        public string DígitoConta { get; set; } = string.Empty;

        /// <summary>
        /// Operação da Conta
        /// </summary>
        public string OperaçãoConta { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da conta (corrente, poupança, conjunta, etc)
        /// </summary>
        public TipoContaEnum TipoConta { get; set; } = TipoContaEnum.ContaCorrenteIndividual;

        /// <summary>
        /// Agência
        /// </summary>
        public string Agência { get; set; } = string.Empty;

        /// <summary>
        /// Dígito verificador da Agência
        /// </summary>
        public string DígitoAgência { get; set; } = string.Empty;

        /// <summary>
        /// Banco
        /// </summary>
        public string Banco { get; set; } = string.Empty;

        /// <summary>
        /// Conta complementar, quando possuir mais de uma conta para débito
        /// </summary>
        public string ContaComplementar { get; set; }

        /// <summary>
        /// Dígito da conta complementar, quando possuir mais de uma conta para débito
        /// </summary>
        public string TipoContaComplementar { get; set; }

        /// <summary>
        /// Tipo de titularidade para DOC/TED
        /// </summary>
        public TitularidadeEnum TipoTitularidade { get; set; }

        ////public TipoFormaCadastramento TipoFormaCadastramento { get; set; } = TipoFormaCadastramento.ComRegistro;
        ////public TipoImpressaoBoleto TipoImpressaoBoleto { get; set; } = TipoImpressaoBoleto.Empresa;
        ////public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.Tradicional;
        ////public TipoDistribuicaoBoleto TipoDistribuicao { get; set; } = TipoDistribuicaoBoleto.ClienteDistribui;
        //public void FormatarDados(string localPagamento, string mensagemFixaTopoBoleto, int digitosConta)
        //{
        //    string agencia = Agência;
        //    Agência = agencia.Length <= 4 ? agencia.PadLeft(4, '0') : throw new Exception(string.Format("O tamanho do campo Agência é inválido, são esperados 4 dígitos e foram informadodos {0} digitos.", agencia.Length));

        //    string conta = Conta;
        //    Conta = conta.Length <= digitosConta ? conta.PadLeft(digitosConta, '0') : throw new Exception(string.Format("O tamanho do campo Conta é invalido, são esperados {0} digitos e foram informadodos {1} digitos.", digitosConta, agencia.Length));
        //}
    }
}