using Boleto2Net.Util;
using Pagamento2Net.Entidades;
using Pagamento2Net.Enums;

namespace Pagamento2Net
{
    public interface IRemessaPagamento
    {
        string GerarHeaderRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistrosGeral);

        string GerarHeaderLoteRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, TipoPagamentoEnum tipoPagamento, ref int loteServico, string tipoServico, int numeroArquivoRemessa, ref int numeroRegistrosLote, ref int numeroRegistroGeral);

        string GerarDetalheRemessaPagamento(TipoArquivo tipoArquivo, Documento documento, TipoPagamentoEnum tipoPagamento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral);

        string GerarTrailerLoteRemessaPagamento(TipoArquivo tipoArquivo, int loteServico, int numeroRegistrosLote, decimal valorTotalRegistrosLote, ref int numeroRegistroGeral);

        string GerarTrailerRemessaPagamento(TipoArquivo tipoArquivo, int numeroRegistros, int numeroLotes, decimal valorTotalRegistros);
    }
}