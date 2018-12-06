using System;

namespace Pagamento2Net.Exceptions
{
    public sealed partial class Pagamento2NetException : Exception
    {
        private Pagamento2NetException(string message)
    : base(message)
        {
        }

        private Pagamento2NetException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        #region Remessa

        public static Exception ErroAoGerarRegistroHeaderDoArquivoRemessa(Exception ex)
            => new Pagamento2NetException("Erro durante a geração do registro HEADER do arquivo de REMESSA.", ex);

        public static Exception ErroAoGerarRegistroDetalheDoArquivoRemessa(Exception ex)
            => new Pagamento2NetException("Erro durante a geração dos registros de DETALHE do arquivo de REMESSA.", ex);

        public static Exception ErroAoGerrarRegistroTrailerDoArquivoRemessa(Exception ex)
            => new Pagamento2NetException("Erro durante a geração do registro TRAILER do arquivo de REMESSA.", ex);

        public static Exception ErroAoGerarRegistroHeaderLoteDoArquivoRemessa(Exception ex)
            => new Pagamento2NetException("Erro durante a geração do registro HEADER do LOTE do arquivo de REMESSA", ex);

        public static Exception ErroAoGerarRegistroTrailerLoteDoArquivoRemessa(Exception ex)
            => new Pagamento2NetException("Erro durante a geração do registro TRAILER do LOTE do arquivo de REMESSA", ex);
        #endregion Remessa

        #region Retorno

        public static Exception ErroAoLerRegistroHeaderDoArquivoRetorno(Exception ex)
            => new Pagamento2NetException("Erro durante a leitura do registro HEADER do arquivo de RETORNO.", ex);

        public static Exception ErroAoLerRegistroHeaderLoteDoArquivoRetorno(Exception ex)
            => new Pagamento2NetException("Erro durante a leitura do registro HEADER do LOTE do arquivo de RETORNO", ex);

        public static Exception ErroAoLerRegistroDetalheDoArquivoRetorno(Exception ex)
            => new Pagamento2NetException("Erro durante a leitura dos registros de DETALHE do arquivo de RETORNO.", ex);

        public static Exception ErroAoLerRegistroTrailerDoArquivoRetorno(Exception ex)
            => new Pagamento2NetException("Erro durante a leitura do registro TRAILER do arquivo de RETORNO.", ex);

        public static Exception ErroAoLerRegistroTrailerLoteDoArquivoRetorno(Exception ex)
            => new Pagamento2NetException("Erro durante a leitura do registro TRAILER do LOTE do arquivo de RETORNO", ex);
        #endregion Retorno

        public static Pagamento2NetException BancoNaoImplementado(int codigoBanco)
            => new Pagamento2NetException($"Banco não implementando: {codigoBanco}");

        public static Pagamento2NetException ErroAoFormatarCedente(Exception ex)
            => new Pagamento2NetException("Erro durante a formatação do cedente.", ex);

        public static Pagamento2NetException ErroAoFormatarCodigoDeBarra(Exception ex)
            => new Pagamento2NetException("Erro durante a formatação do código de barra.", ex);

        public static Exception ErroAoFormatarNossoNumero(Exception ex)
            => new Pagamento2NetException("Erro durante a formatação do nosso número.", ex);

        public static Exception ErroAoValidarBoleto(Exception ex)
            => new Pagamento2NetException("Erro durante a validação do boleto.", ex);

        public static Exception AgenciaInvalida(string agencia, int digitos)
            => new Pagamento2NetException($"O número da agência ({agencia}) deve conter {digitos} dígitos.");

        public static Exception ContaInvalida(string conta, int digitos)
            => new Pagamento2NetException($"O número da conta ({conta}) deve conter {digitos} dígitos.");

        public static Exception CodigoCedenteInvalido(string codigoCedente, int digitos)
            => new Pagamento2NetException($"O código do cedente ({codigoCedente}) deve conter {digitos} dígitos.");

        public static Exception CarteiraNaoImplementada(string carteiraComVariacao)
            => new Pagamento2NetException($"Carteira não implementada: {carteiraComVariacao}");
    }
}