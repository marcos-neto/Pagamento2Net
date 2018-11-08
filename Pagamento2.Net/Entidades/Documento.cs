using Pagamento2.Net.Enums;
using Pagamento2Net.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Pagamento2Net.Entidades
{
    public abstract class Documento
    {
        /// <summary>
        /// Código da Instrução para Movimento
        /// </summary>
        public InstruçãoMovimentoEnum CódigoDaInstruçãoParaMovimento { get; set; }

        /// <summary>
        /// Tipo de Movimento
        /// </summary>
        public TipoMovimentoEnum TipoDeMovimento { get; set; }

        /// <summary>
        ///
        /// </summary>
        public TipoPagamentoEnum TipoDePagamento { get; protected set; }

        /// <summary>
        /// Número de Documento Cliente (Seu Número)
        /// Número atribuído pela Empresa(Pagador) para identificar o documento de Pagamento(Nota Fiscal, Nota Promissória, etc.)
        /// </summary>
        public string NúmeroDocumentoCliente { get; set; }

        /// <summary>
        /// Número do Documento Banco (Nosso Número)
        /// Número atribuído pelo Banco para identificar o lançamento, que será utilizado nas manutenções do mesmo.
        /// </summary>
        public string NúmeroDocumentoBanco { get; set; }

        /// <summary>
        ///
        /// </summary>
        //public DateTime DataDoVencimento { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Required]
        public DateTime DataDoPagamento { get; set; }

        /// <summary>
        ///
        /// </summary>
        [Required]
        public decimal ValorDoPagamento { get; set; }

        /// <summary>
        /// Código das Ocorrências para Retorno.
        /// Código adotado para identificar as ocorrências detectadas no processamento.
        /// Pode-se informar até 5 ocorrências simultaneamente, cada uma delas codificada com dois dígitos
        /// </summary>
        public IList<Ocorrência> OcorrênciasParaRetorno { get; set; }
    }
}