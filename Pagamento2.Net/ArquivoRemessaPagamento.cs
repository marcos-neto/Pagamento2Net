using Boleto2Net.Util;
using Pagamento2Net.Contratos;
using Pagamento2Net.Bancos;
using Pagamento2Net.Entidades;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Pagamento2Net
{
    public class ArquivoRemessaPagamento : ICriarArquivoRemessa
    {
        private IRemessaPagamento Banco { get; set; }
        private TipoArquivo TipoArquivo { get; set; }
        private int NumeroArquivoRemessa { get; set; }

        public ArquivoRemessaPagamento()
        {
        }

        private void Initialize(Pagamento pagamento)
        {
            try
            {
                string bankCode;
                bankCode = pagamento.Pagador.ContaFinanceira.Banco;

                switch (bankCode)
                {
                    case "033":
                        TipoArquivo = TipoArquivo.CNAB240;
                        Banco = new BancoSantander();
                        break;

                    case "237":
                        TipoArquivo = TipoArquivo.POS500;
                        Banco = new BancoBradesco();
                        break;

                    case "001":
                    case "041":
                    case "104":
                    case "341":
                    case "756":
                    default:
                        throw new Exception("Banco não suportado.");
                }
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao identificar o banco.", e);
            }

            try
            {
                NumeroArquivoRemessa = pagamento.NúmeroRemessa;
            }
            catch (Exception e)
            {
                throw new Exception("Erro ao identificar o número do arquivo de remessa.", e);
            }

        }

        /// <summary>
        /// Metodo para geração das informações de pagamento no formato de array de bytes
        /// </summary>
        /// <param name="pagamento">Lista de </param>
        /// <returns></returns>
        private void GerarArquivo(Pagamento pagamento, Stream file)
        {
            //Faz algumas validações iniciais
            Initialize(pagamento);

            ////StreamWriter arquivoRemessa = new StreamWriter(file, Encoding.GetEncoding("ISO-8859-1"));
            StreamWriter arquivoRemessa = new StreamWriter(file, Encoding.UTF8); //TODO: Alterar a codificação default

            string strline = string.Empty;

            decimal
                valorTotalRegistrosLote = 0,
                valorTotalRegistrosArquivo = 0;
            int
                numeroRegistrosLote = 0,
                numeroRegistrosGeral = 0,
                numeroLotes = 0,
                loteDeServico = 0;

            // geração do registro header do arquivo
            arquivoRemessa.WriteLine(
                Utils.RemoveCharactersEspeciais(Banco.GerarHeaderRemessaPagamento(
                    TipoArquivo,
                    pagamento.Pagador,
                    pagamento.NúmeroRemessa,
                    ref numeroRegistrosGeral
                    )));

            // agrupa os registros por forma de lancamento (TipoDePagamento)
            var lotes = from d in pagamento.Documentos
                        group d by d.TipoDePagamento into g
                        select new
                        {
                            TipoPagamento = g.Key,
                            Documentos = g.ToList()
                        };

            // registros organizados por tipo de serviço
            foreach (var lote in lotes)
            {
                numeroLotes++;
                numeroRegistrosLote = 0;
                valorTotalRegistrosLote = 0;

                //geração do header dos lotes
                string headerLote = Utils.RemoveCharactersEspeciais(Banco.GerarHeaderLoteRemessaPagamento(
                        tipoArquivo: TipoArquivo,
                        pagador: pagamento.Pagador,
                        tipoPagamento: lote.TipoPagamento,
                        loteServico: ref loteDeServico,
                        tipoServico: ((int)lote.Documentos.First().TipoDeServiço).ToString(),
                        numeroArquivoRemessa: NumeroArquivoRemessa,
                        numeroRegistroGeral: ref numeroRegistrosGeral,
                        numeroRegistrosLote: ref numeroRegistrosLote
                        ));

                //Se nao vier vazio escreve a linha no arquivo
                if (!string.IsNullOrWhiteSpace(headerLote))
                    arquivoRemessa.WriteLine(headerLote);

                foreach (var documento in lote.Documentos)
                {
                    valorTotalRegistrosLote += documento.ValorDoPagamento;
                    valorTotalRegistrosArquivo += documento.ValorDoPagamento;

                    // geração dos detalhes (segmentos)
                    arquivoRemessa.WriteLine(
                         Utils.RemoveCharactersEspeciais(Banco.GerarDetalheRemessaPagamento(
                            tipoArquivo: TipoArquivo,
                            documento: documento,
                            tipoPagamento: lote.TipoPagamento,
                            loteServico: ref loteDeServico,
                            numeroRegistroLote: ref numeroRegistrosLote,
                            numeroRegistroGeral: ref numeroRegistrosGeral
                        )));
                }

                // geração do trailer do lote
                string trailerLote = Utils.RemoveCharactersEspeciais(Banco.GerarTrailerLoteRemessaPagamento(
                    TipoArquivo,
                    loteDeServico,
                    numeroRegistrosLote,
                    valorTotalRegistrosLote,
                    ref numeroRegistrosGeral
                    ));

                //se nao vier vazio escreve no arquivo
                if (!string.IsNullOrWhiteSpace(trailerLote))
                    arquivoRemessa.WriteLine(trailerLote);
            }

            // geração do trailer do arquivo
            arquivoRemessa.WriteLine(
                 Utils.RemoveCharactersEspeciais(Banco.GerarTrailerRemessaPagamento(
                    TipoArquivo,
                    numeroRegistrosGeral,
                    numeroLotes,
                    valorTotalRegistrosArquivo
                )));

            arquivoRemessa.Close();
            arquivoRemessa.Dispose();
            arquivoRemessa = null;
        }

        public string GerarArquivoRemessa(Pagamento pagamento)
        {
            string fileName = Path.GetTempFileName();

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                GerarArquivo(pagamento, fileStream);
            }

            return File.ReadAllText(fileName);
        }

    }
}