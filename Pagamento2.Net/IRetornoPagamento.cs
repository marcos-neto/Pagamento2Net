using Boleto2Net.Util;
using Pagamento2Net.Enums;
using Pagamento2Net.Entidades;

namespace Pagamento2Net
{
    public interface IRetornoPagamento
    {
        /// <summary>
        /// Le o Header do Arquivo de retorno de pagamento
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="payee"></param>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistro"></param>
        /// <returns></returns>
        void LerHeaderRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento Pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço);

        /// <summary>
        /// Le o Header do lote do Arquivo de retorno de pagamento
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="paymentDocuments"></param>
        /// <param name="registro"></param>
        void LerHeaderLoteRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento Pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço);

        /// <summary>
        /// Le o os detalhes do lote do Arquivo de retorno de pagamento
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="paymentDocument"></param>
        /// <param name="registro"></param>
        void LerDetalheRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento Pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço);

        /// <summary>
        /// Le o Trailer do lote do Arquivo de retorno de pagamento
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="paymentDocument"></param>
        /// <param name="registro"></param>
        void LerTrailerLoteRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento Pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço);

        /// <summary>
        /// Le o Trailer do Arquivo de retorno de pagamento
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="paymentDocument"></param>
        /// <param name="registro"></param>
        void LerTrailerRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento Pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço);
    }
}