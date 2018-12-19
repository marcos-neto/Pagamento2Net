using Pagamento2Net.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Pagamento2Net.Entidades
{
    /// <summary>
    /// Formas de Lançamento 01, 03, 05 e 10
    /// Crédito em Conta Corrente e Conta Poupança, DOC, TED, Caixa e OP (Recibo)
    /// </summary>
    public class Transferencia : Documento
    {
        /// <summary>
        /// Código da Câmara Centralizadora
        /// </summary>
        public string CódigoCâmaraCentralizadora { get; set; }

        /// <summary>
        /// Código do Banco Favorecido
        /// </summary>
        public string CódigoBancoFavorecido { get; set; }

        /// <summary>
        /// Código da Agência Favorecido
        /// </summary>
        public string CódigoAgênciaFavorecido { get; set; }

        /// <summary>
        /// Dígito Verificador da Agência
        /// </summary>
        public string DígitoVerificadorAgência { get; set; }

        /// <summary>
        /// Conta Corrente do Favorecido
        /// </summary>
        public string ContaCorrenteFavorecido { get; set; }

        /// <summary>
        /// Dígito Verificador da Conta
        /// </summary>
        public string DígitoVerificadorConta { get; set; }

        /// <summary>
        /// Dígito Verificador da Agência/Conta
        /// </summary>
        public string DígitoVerificadorAgênciaConta { get; set; }

        /// <summary>
        /// Nome do Favorecido
        /// </summary>
        public string NomeFavorecido { get; set; }

        /// <summary>
        /// Tipo da Moeda
        /// </summary>
        public string TipoMoeda { get; set; }

        /// <summary>
        /// Quantidade de Moeda
        /// </summary>
        public string QuantidadeMoeda { get; set; }

        /// <summary>
        /// Data Real do Pagamento (Retorno)
        /// </summary>
        public string DataRealPagamentoRetorno { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DateTime DataRealDoPagamento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal ValorRealDoPagamento { get; set; }

        /// <summary>
        /// Texto referente a mensagens que serão impressas nos documentos e/ou avisos a serem emitidos no campo “Complemento do Tipo de Serviço”.
        /// Informação 1: Genérica. Quando informada constará em todos os avisos e/ou documentos originados dos detalhes desse lote.
        /// </summary>
        public string InformaçãoComplementar1 { get; set; }

        /// <summary>
        /// Texto referente a mensagens que serão impressas nos documentos e/ou avisos a serem emitidos no campo “Complemento do Tipo de Serviço”.
        /// Informação 2: Específica. Quando informada constará apenas naquele aviso ou documento identificado pelo detalhe.
        /// </summary>
        public string InformaçãoComplementar2 { get; set; }

        /// <summary>
        /// Finalidade DOC e TED
        /// </summary>
        [Required]
        public FinalidadeEnum FinalidadeDocTed { get; set; }

        public Transferencia()
        {
            base.TipoDePagamento = TipoPagamentoEnum.Caixa;
        }
    }
}