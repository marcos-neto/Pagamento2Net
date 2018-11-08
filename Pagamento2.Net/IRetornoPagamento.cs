using Boleto2Net.Util;

namespace Pagamento2Net
{
    public interface IRetornoPagamento
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="payee"></param>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistro"></param>
        /// <returns></returns>
        void LerHeaderRetornoPagamento(TipoArquivo tipoArquivo, ref PaymentDocuments paymentDocuments, string registro);

        /// <summary>
        ///
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="paymentDocument"></param>
        /// <param name="registro"></param>
        void LerDetalheRetornoPagamento(TipoArquivo tipoArquivo, string tipoSegmento, ref PaymentDocument paymentDocument, string registro);
    }
}