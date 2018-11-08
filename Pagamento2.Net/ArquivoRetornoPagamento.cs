using Boleto2Net.Util;
using Pagamento2Net;
using Pagamento2Net.Bancos;
using Pagamento2Net.Entidades;
using System;
using System.IO;
using System.Linq;

namespace Boleto2Net
{
    public class ArquivoRetornoPagamento
    {
        public IRetornoPagamento Banco { get; set; }

        public TipoArquivo TipoArquivo { get; set; }

        public Pagamento Pagamento { get; set; }

        public ArquivoRetornoPagamento()
        {
        }

        public ArquivoRetornoPagamento(IRetornoPagamento banco, TipoArquivo tipoArquivo)
        {
            Banco = banco;
            TipoArquivo = tipoArquivo;
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

                    case "001":
                    case "041":
                    case "104":
                    case "237":
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
        }

        private void LerArquivoRetorno2(Stream fileStream)
        {
            Pagamento pagamento = null;

            // Ao percorrer o arquivo, armazena qual o tipo de serviço do lote atual, para setar nos documentos subsequentes.
            ServiceTypeEnum? currentServiceType = null;
            try
            {
                using (StreamReader arquivo = new StreamReader((Stream)fileStream, System.Text.Encoding.UTF8))
                {
                    if (arquivo.EndOfStream)
                        return;

                    //busca o primeiro registro do arquivo
                    var registro = arquivo.ReadLine();

                    //atribui o tipo de acordo com o conteúdo do arquivo
                    TipoArquivo = registro.Length == 240 ? TipoArquivo.CNAB240 : TipoArquivo.CNAB400;

                    if (TipoArquivo == TipoArquivo.CNAB400 && ((IBanco)Banco).IdsRetornoCnab400RegistroDetalhe.Count == 0)
                        throw new Exception("Banco " + ((IBanco)Banco).Codigo.ToString() + " não implementou os Ids do Registro Retorno do CNAB400.");

                    //instancia o banco de acordo com o código/id do banco presente no arquivo de retorno
                    Banco = (IRetornoPagamento)Boleto2Net.Banco.Instancia(Utils.ToInt32(registro.Substring(TipoArquivo == TipoArquivo.CNAB240 ? 0 : 76, 3)));

                    //define a posicao do reader para o início
                    arquivo.DiscardBufferedData();
                    arquivo.BaseStream.Seek(0, SeekOrigin.Begin);

                    while (!arquivo.EndOfStream)
                    {
                        // VERSÃO YLEC2403_v7_14/02/2012
                        registro = arquivo.ReadLine();
                        var tipoRegistro = registro.Substring(7, 1);
                        var tipoSegmento = registro.Substring(13, 1);

                        // REGISTRO HEADER DE ARQUIVO
                        if (tipoRegistro == "0")
                        {
                            Banco.LerHeaderRetornoPagamento(TipoArquivo, ref pagamento, registro);
                            return;
                        }

                        // REGISTRO HEADER DO LOTE
                        if (tipoRegistro == "1")
                        {
                            currentServiceType = (ServiceTypeEnum)Enum.Parse(typeof(ServiceTypeEnum), registro.Substring(9, 2));
                            return;
                        }

                        // Segmento A - Indica um novo documento
                        if (tipoRegistro == "3" & tipoSegmento == "A")
                        {
                            //var document = arquivoRetorno.PaymentDocuments.Documents.LastOrDefault();
                            var document = new PaymentDocument();

                            // Se não encontrou um boleto válido, ocorreu algum problema, pois deveria ter criado um novo objeto no registro que foi analisado anteriormente.
                            if (document == null)
                                throw new Exception("Objeto documento não identificado");

                            Banco.LerDetalheRetornoPagamento(TipoArquivo, tipoSegmento, ref document, registro);
                            document.ServiceType = currentServiceType.Value;
                            Pagamento.Documents.Add(document);
                            return;
                        }

                        // Segmento J - Indica um novo documento
                        if (tipoRegistro == "3" & tipoSegmento == "J")
                        {
                            //var document = arquivoRetorno.PaymentDocuments.Documents.LastOrDefault();
                            var document = new PaymentDocument();

                            // Se não encontrou um boleto válido, ocorreu algum problema, pois deveria ter criado um novo objeto no registro que foi analisado anteriormente.
                            if (document == null)
                                throw new Exception("Objeto documento não identificado");

                            Banco.LerDetalheRetornoPagamento(TipoArquivo, tipoSegmento, ref document, registro);
                            document.ServiceType = currentServiceType.Value;
                            Pagamento.Documents.Add(document);
                            return;
                        }

                        // Segmento B - Continuação do segmento A ou J anterior
                        if (tipoRegistro == "3" & tipoSegmento == "B")
                        {
                            // localiza o último boleto da lista
                            var document = Pagamento.Documents.LastOrDefault();

                            // Se não encontrou um boleto válido, ocorreu algum problema, pois deveria ter criado um novo objeto no registro que foi analisado anteriormente.
                            if (document == null)
                                throw new Exception("Objeto documento não identificado");

                            Banco.LerDetalheRetornoPagamento(TipoArquivo, tipoSegmento, ref document, registro);
                            document.ServiceType = currentServiceType.Value;
                            return;
                        }

                        // REGISTRO TRAILER DE LOTE
                        if (tipoRegistro == "5")
                        {
                            // não precisa ler
                        }

                        // REGISTRO TRAILER DE ARQUIVO
                        if (tipoRegistro == "9")
                        {
                            // não precisa ler
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler arquivo.", ex);
            }
        }

        //private void LerLinhaDoArquivoRetornoCNAB400(string registro)
        //{
        //    // Identifica o tipo do registro (primeira posição da linha)
        //    var tipoRegistro = registro.Substring(0, 1);

        //    // Registro HEADER
        //    if (tipoRegistro == "0")
        //    {
        //        Banco.LerHeaderRetornoCNAB400(registro);
        //        return;
        //    }

        //    // Registro TRAILER
        //    if (tipoRegistro == "9")
        //    {
        //        Banco.LerTrailerRetornoCNAB400(registro);
        //        return;
        //    }

        //    // Se o registro não estiver na lista a ser processada pelo banco selecionado, ignora o registro
        //    if (!Banco.IdsRetornoCnab400RegistroDetalhe.Contains(tipoRegistro))
        //        return;

        //    // O primeiro ID da lista, identifica um novo boleto.
        //    bool novoBoleto = false;
        //    if (tipoRegistro == Banco.IdsRetornoCnab400RegistroDetalhe.First())
        //        novoBoleto = true;

        //    // Se for um novo boleto, cria um novo objeto, caso contrário, seleciona o último boleto
        //    // Estamos considerando que, quando houver mais de um registro para o mesmo boleto, no arquivo retorno, os registros serão apresentados na sequencia.
        //    Boleto boleto;
        //    if (novoBoleto)
        //    {
        //        boleto = new Boleto(this.Banco, _ignorarCarteiraBoleto);
        //    }
        //    else
        //    {
        //        boleto = Documentos.Last();
        //        // Se não encontrou um boleto válido, ocorreu algum problema, pois deveria ter criado um novo objeto no registro que foi analisado anteriormente.
        //        if (boleto == null)
        //            throw new Exception("Objeto boleto não identificado");
        //    }

        //    // Identifica o tipo de registro que deve ser analisado pelo Banco.
        //    switch (tipoRegistro)
        //    {
        //        case "1":
        //            Banco.LerDetalheRetornoCNAB400Segmento1(ref boleto, registro);
        //            break;

        //        case "7":
        //            Banco.LerDetalheRetornoCNAB400Segmento7(ref boleto, registro);
        //            break;

        //        default:
        //            break;
        //    }

        //    // Se for um novo boleto, adiciona na lista de boletos.
        //    if (novoBoleto)
        //    {
        //        Documentos.Add(boleto);
        //    }
        //}

        public PaymentDocuments ParseFile(byte[] contents)
        {
            MemoryStream stream = new MemoryStream(contents);
            LerArquivoRetorno2(stream);

            return Pagamento;
        }
    }
}