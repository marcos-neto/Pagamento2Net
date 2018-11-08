using System;

namespace Pagamento2Net.Exceptions
{
    public sealed partial class Boleto2NetException : Exception
    {
        public static Exception ErroAoGerarRegistroHeaderLoteDoArquivoRemessa(Exception ex)
            => new Pagamento2NetException("Erro durante a geração do registro HEADER LOTE do arquivo de REMESSA.", ex);
    }
}