using Pagamento2.Net.Entidades;
using Pagamento2.Net.Enums;
using Pagamento2Net.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pagamento2Net.Entidades
{
    public class DocumentoPagamento2 : IValidatableObject
    {
        #region Public Constructors

        public DocumentoPagamento2()
        {
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        ///
        /// </summary>
        public int DívidaAtiva { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal QuantidadeReceitaBruta { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CódigoDeBarras { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int AnoBase { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime PeríodoDeCálculo { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string InformaçãoComplementar1 { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int CódigoDoHistóricoDeCrédito { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string OpçãoDeRetiradaDoCRLV { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string DígitoDoLacreDoConectividadeSocial { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal Desconto { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal ValorDocumento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime DataVencimento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string LacreDoConectividadeSocial { get; set; }

        /// <summary>
        /// Favorecido ou Cedente ou Fornecedor
        /// </summary>
        [Required]
        public Favorecido Favorecido { get; set; } = new Favorecido();

        /// <summary>
        ///
        /// </summary>
        public string IdentificadorDoFGTS { get; set; }

        /// <summary>
        /// Juros
        /// </summary>
        public decimal Juros { get; set; }

        /// <summary>
        /// Multa
        /// </summary>
        public decimal Multa { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string MêsEAnoCompetência { get; set; }

        /// <summary>
        ///
        /// </summary>
        public InstruçãoMovimentoEnum CódigoDaInstruçãoParaMovimento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TipoMovimentoEnum TipoDeMovimento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal ValorDeOutrasEntidades { get; set; }

        /// <summary>
        /// Nosso número
        /// </summary>
        public string NossoNúmero { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int NúmeroParcela { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string AutenticaçãoDoPagamento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string OpçãoDePagamento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ProtocoloDePagamento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TipoPagamentoEnum TipoDePagamento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal PercentualDaReceitaBrutaAcumulada { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal Abatimento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int NúmeroDeReferência { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string PeríodoDeReferência { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CódigoDoRenavam { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal Restatement { get; set; }

        /// <summary>
        ///
        /// </summary>
        public IList<Ocorrência> OcorrenciasRetorno { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal ValorDaReceita { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TipoServiçoEnum TipoServiço { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string InformaçãoComplementarDoTributo { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string CódigoDeIdentificaçãoDoTributo { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string PlacaDoVeículo { get; set; }

        #endregion Public Properties

        #region Public Methods

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            //Validator.TryValidateObject(Favored,
            //   new ValidationContext(Favored, null, null),
            //   results,
            //   false);

            results.AddRange(CustomValidation());

            return results;
        }

        #endregion Public Methods

        #region Private Methods

        private IEnumerable<ValidationResult> CustomValidation()
        {
            string message = "The {0} field is required";

            //validação comum a todos os tipos
            if (PaymentDate == default(DateTime))
            {
                yield return new ValidationResult(string.Format(message, "PaymentDate"), new[] { "PaymentDate" });
            }

            if (PaymentValue == default(decimal))
            {
                yield return new ValidationResult(string.Format(message, "PaymentValue"), new[] { "PaymentValue" });
            }

            // Validação para as modalidades 01, 03, 05 e 08
            if (PaymentType == PaymentTypeEnum.CreditoContaCorrente
                || PaymentType == PaymentTypeEnum.Doc
                || PaymentType == PaymentTypeEnum.Ted
                || PaymentType == PaymentTypeEnum.CreditoContaPoupanca)
            {
                if (Favored == null)
                {
                    yield return new ValidationResult(string.Format(message, "Favored"), new[] { "Favored" });
                    yield break;
                }

                if (Favored.FinancialAccount == null)
                {
                    yield return new ValidationResult(string.Format(message, "FinancialAccount"), new[] { "FinancialAccount" });
                    yield break;
                }

                if (string.IsNullOrEmpty(Favored.FinancialAccount.Bank))
                {
                    yield return new ValidationResult(string.Format(message, "Bank"), new[] { "Bank" });
                }

                if (string.IsNullOrEmpty(Favored.FinancialAccount.Agency))
                {
                    yield return new ValidationResult(string.Format(message, "Agency"), new[] { "Agency" });
                }

                //if (String.IsNullOrEmpty(Favored.FinancialAccount.AgencyDigit))
                //{
                //    yield return new ValidationResult(String.Format(message, "AgencyDigit"), new[] { "AgencyDigit" });
                //}

                if (string.IsNullOrEmpty(Favored.FinancialAccount.Account))
                {
                    yield return new ValidationResult(string.Format(message, "Account"), new[] { "Account" });
                }

                if (string.IsNullOrEmpty(Favored.FinancialAccount.AccountDigit))
                {
                    yield return new ValidationResult(string.Format(message, "AccountDigit"), new[] { "AccountDigit" });
                }

                //if (String.IsNullOrEmpty(Favored.Name))
                //{
                //    yield return new ValidationResult(String.Format(message, "Name"), new[] { "Name" });
                //}

                //if (String.IsNullOrEmpty(Favored.CPFCNPJ))
                //{
                //    yield return new ValidationResult(String.Format(message, "CPFCNPJ"), new[] { "CPFCNPJ" });
                //}
            }

            // Validação para os tipos 30 e 31
            if (PaymentType == PaymentTypeEnum.LiquidacaoTitulosMesmoBanco ||
                PaymentType == PaymentTypeEnum.LiquidacaoTitulosOutrosBancos)
            {
                if (string.IsNullOrEmpty(Barcode))
                {
                    yield return new ValidationResult(string.Format(message, "Barcode"), new[] { "Barcode" });
                }

                if (DueDate == default(DateTime))
                {
                    yield return new ValidationResult(string.Format(message, "DueDate"), new[] { "DueDate" });
                }

                if (DocumentValue == default(decimal)) //valor nominal??
                {
                    yield return new ValidationResult(string.Format(message, "DocumentValue"), new[] { "DocumentValue" });
                }
            }
        }

        #endregion Private Methods
    }
}