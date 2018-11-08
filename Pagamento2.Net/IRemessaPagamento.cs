using Boleto2Net.Util;
using Pagamento2Net.Entidades;
using Pagamento2Net.Enums;

namespace Pagamento2Net
{
    /// <summary>
    /// ESSA INTERFACE DEVE SER INTEGRADA À INTERFACE IBANCO
    /// </summary>
    public interface IRemessaPagamento
    {
        string GerarHeaderRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistro);

        string GerarHeaderLoteRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, TipoPagamentoEnum tipoPagamento, ref int loteServico, string tipoServico, int numeroArquivoRemessa, ref int numeroRegistroGeral);

        string GerarDetalheRemessaPagamento(TipoArquivo tipoArquivo, Documento documento, TipoPagamentoEnum tipoPagamento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral);

        string GerarTrailerLoteRemessaPagamento(TipoArquivo tipoArquivo, ref int numeroRegistroGeral, int loteServico, int numeroRegistros, decimal valorTotalRegistros);

        string GerarTrailerRemessaPagamento(TipoArquivo tipoArquivo, int numeroRegistros, int numeroLotes, decimal valorTotalRegistros);
    }
}