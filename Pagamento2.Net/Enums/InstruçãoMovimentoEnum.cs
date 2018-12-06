namespace Pagamento2Net.Enums
{
    public enum InstruçãoMovimentoEnum
    {
        /// <summary>
        /// Inclusão de Registro Detalhe Liberado
        /// </summary>
        InclusãoDeRegistroDetalheLiberado = 00,

        /// <summary>
        /// Inclusão do Registro Detalhe Bloqueado
        /// </summary>
        InclusãoDoRegistroDetalheBloqueado = 09,

        /// <summary>
        /// Alteração do Pagamento Liberado para Bloqueado (Bloqueio)
        /// </summary>
        AlteraçãoDoPagamentoLiberadoParaBloqueado = 10,

        /// <summary>
        /// Alteração do Pagamento Bloqueado para Liberado (Liberação)
        /// </summary>
        AlteraçãoDoPagamentoBloqueadoParaLiberado = 11,

        /// <summary>
        ///
        /// </summary>
        AutorizacaoPagamento = 14,

        /// <summary>
        /// Alteração do Valor do Título
        /// </summary>
        AlteraçãoDoValorDoTítulo = 17,

        /// <summary>
        /// Alteração da Data de Pagamento
        /// </summary>
        AlteraçãoDaDataDePagamento = 19,

        /// <summary>
        /// Pagamento Direto ao Fornecedor - Baixar
        /// </summary>
        PagamentoDiretoAoFornecedor_Baixar = 23,

        /// <summary>
        /// Manutenção em Carteira - Não Pagar
        /// </summary>
        ManutençãoEmCarteira_NãoPagar = 25,

        /// <summary>
        /// Retirada de Carteira - Não Pagar
        /// </summary>
        RetiradaDeCarteira_NãoPagar = 27,

        /// <summary>
        /// Estorno por Devolução da Câmara Centralizadora
        /// </summary>
        EstornoPorDevoluçãoDaCâmaraCentralizadora = 33,

        /// <summary>
        /// Alegação do Pagador
        /// </summary>
        AlegaçãoDoPagador = 40,

        /// <summary>
        /// Exclusão do Registro Detalhe Incluído Anteriormente
        /// </summary>
        ExclusãoDoRegistroDetalheIncluídoAnteriormente = 99
    }
}