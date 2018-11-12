using Boleto2Net.Util;
using Pagamento2.Net.Enums;
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

        private void Initialize(StreamReader file)
        {
            try
            {
                string códigoDoBanco = "";

                var registro = file.ReadLine();

                switch (registro.Length)
                {
                    case 240:
                        TipoArquivo = TipoArquivo.CNAB240;
                        códigoDoBanco = registro.Substring(0, 3);
                        break;
                    case 400:
                        TipoArquivo = TipoArquivo.CNAB400;
                        códigoDoBanco = registro.Substring(76, 3);
                        break;
                    case 500:
                        TipoArquivo = TipoArquivo.POS500;
                        códigoDoBanco = "237";
                        break;
                    default:
                        throw new Exception("Layout nao implementado.");
                }

                switch (códigoDoBanco)
                {
                    case "033":
                        Banco = new BancoSantander();
                        break;

                    case "237":
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
        }

        private Pagamento LerArquivoRetorno(Stream fileStream)
        {
            Pagamento pagamento = new Pagamento();

            // Ao percorrer o arquivo, armazena qual o tipo de serviço do lote atual, para setar nos documentos subsequentes.
            TipoServiçoEnum? currentServiceType = null;
            try
            {
                using (StreamReader arquivo = new StreamReader((Stream)fileStream, System.Text.Encoding.UTF8))
                {
                    Initialize(arquivo);

                    //busca o primeiro registro do arquivo
                    var registro = arquivo.ReadLine();

                    //define a posicao do reader para o início
                    arquivo.DiscardBufferedData();
                    arquivo.BaseStream.Seek(0, SeekOrigin.Begin);

                    while (!arquivo.EndOfStream)
                    {
                        registro = arquivo.ReadLine();
                        string caracterTipoRegistro;
                        TipoRegistroEnum tipoRegistro = default(TipoRegistroEnum);

                        switch (TipoArquivo)
                        {
                            case TipoArquivo.CNAB240:
                                caracterTipoRegistro = registro.Substring(7, 1);

                                if (caracterTipoRegistro.Equals("0"))
                                    tipoRegistro = TipoRegistroEnum.HeaderArquivo;
                                else if (caracterTipoRegistro.Equals("1"))
                                    tipoRegistro = TipoRegistroEnum.HeaderLote;
                                else if (caracterTipoRegistro.Equals("3"))
                                    tipoRegistro = TipoRegistroEnum.Detalhe;
                                else if (caracterTipoRegistro.Equals("5"))
                                    tipoRegistro = TipoRegistroEnum.TrailerLote;
                                else if (caracterTipoRegistro.Equals("9"))
                                    tipoRegistro = TipoRegistroEnum.TrailerArquivo;
                                else
                                    throw new Exception("Tipo de registro nao implementado - CNAB240");
                                break;

                            case TipoArquivo.CNAB400:
                                caracterTipoRegistro = registro.Substring(7, 1);
                                break;

                            case TipoArquivo.POS500:
                                caracterTipoRegistro = registro.Substring(0, 1);
                                if (caracterTipoRegistro.Equals("0"))
                                    tipoRegistro = TipoRegistroEnum.HeaderArquivo;
                                else if (caracterTipoRegistro.Equals("1"))
                                    tipoRegistro = TipoRegistroEnum.Detalhe;
                                else if (caracterTipoRegistro.Equals("9"))
                                    tipoRegistro = TipoRegistroEnum.TrailerArquivo;
                                else
                                    throw new Exception("Tipo de registro nao implementado - POS500");
                                break;

                            default:
                                throw new Exception("Tipo de Arquivo não implementado");
                        }

                        switch (tipoRegistro)
                        {
                            case TipoRegistroEnum.HeaderArquivo:
                                Banco.LerHeaderRetornoPagamento(TipoArquivo, ref pagamento, registro, ref currentServiceType);
                                break;
                            case TipoRegistroEnum.HeaderLote:
                                Banco.LerHeaderLoteRetornoPagamento(TipoArquivo, ref pagamento, registro, ref currentServiceType);
                                break;
                            case TipoRegistroEnum.Detalhe:
                                Banco.LerDetalheRetornoPagamento(TipoArquivo, ref pagamento, registro, ref currentServiceType);
                                break;
                            case TipoRegistroEnum.TrailerLote:
                                Banco.LerTrailerLoteRetornoPagamento(TipoArquivo, ref pagamento, registro, ref currentServiceType);
                                break;
                            case TipoRegistroEnum.TrailerArquivo:
                                Banco.LerTrailerRetornoPagamento(TipoArquivo, ref pagamento, registro, ref currentServiceType);
                                break;
                            default:
                                break;
                        }
                    }
                }

                return pagamento;
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

        public Pagamento ParseFile(byte[] contents)
        {
            MemoryStream stream = new MemoryStream(contents);
            LerArquivoRetorno(stream);

            return Pagamento;
        }
    }
}