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
        public TipoPagamentoEnum TipoDePagamento { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public TipoServiçoEnum TipoDeServiço { get; set; }

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
        public DateTime DataDoVencimento { get; set; }

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
        ///
        /// </summary>
        [Required]
        public decimal ValorDoDocumento { get; set; }

        /// <summary>
        /// Código das Ocorrências para Retorno.
        /// Código adotado para identificar as ocorrências detectadas no processamento.
        /// Pode-se informar até 5 ocorrências simultaneamente, cada uma delas codificada com dois dígitos
        /// </summary>
        public IList<Ocorrencia> OcorrênciasParaRetorno { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Favorecido Favorecido { get; set; }

        /// <summary>
        /// Código para identificar emissão de aviso de pagamento (comprovante) ao Favorecido (em endereço especificado no segmento B) ou Remetente.
        /// 0 = Não Emite Aviso
        /// 2 = Emite Aviso Somente para o Remetente
        /// 5 = Emite Aviso Somente para o Favorecido
        /// 6 = Emite Aviso para o Remetente e Favorecido
        /// </summary>
        public string AvisoAoFavorecido { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal ValorDoDesconto { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal ValorDoAbatimento { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal ValorDaMulta { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal ValorDaMora { get; set; }

        /// <summary>
        /// Código do histórico para crédito na conta do favorecido (somente para Crédito em Conta Corrente ou Crédito em Conta Poupança).
        /// Necessário que o convênio esteja com a opção de “Histórico do Arquivo” ativada.
        /// Caso campo seja enviado com zeros, será adotada a opção default cadastrada no convênio.
        /// </summary>
        public int CódigoHistórico { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string CódigoDeBarras { get; set; }
    }
}