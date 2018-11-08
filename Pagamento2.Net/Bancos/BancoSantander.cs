using Boleto2Net.Util;
using Pagamento2.Net.Entidades;
using Pagamento2.Net.Enums;
using Pagamento2Net.Entidades;
using Pagamento2Net.Enums;
using System;
using System.Collections.Generic;
using static System.String;

namespace Pagamento2Net.Bancos
{
    internal sealed partial class BancoSantander : IRemessaPagamento, IRetornoPagamento
    {
        #region IRemessaPagamento methods

        public string GerarHeaderRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistro)
        {
            try
            {
                string header = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:

                        header += GerarHeaderRemessaPagamentoCNAB240(pagador, numeroArquivoRemessa, ref numeroRegistro);
                        break;

                    default:
                        throw new Exception("Santander - Header - Tipo de arquivo inexistente.");
                }

                if (String.IsNullOrWhiteSpace(header))
                {
                    throw new Exception("Registro HEADER obrigatório.");
                }

                return Utils.FormataLinhaArquivoCNAB(header, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw Pagamento2NetException.ErroAoGerarRegistroHeaderDoArquivoRemessa(ex);
            }
        }

        public string GerarHeaderLoteRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, TipoPagamentoEnum tipoPagamento, ref int loteServico, string tipoServico, int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                string header = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:

                        header += GerarHeaderLoteRemessaPagamentoCNAB240(
                            pagador: pagador,
                            tipoOperacao: "C",
                            loteServico: ref loteServico,
                            tipoServico: tipoServico,
                            tipoPagamento: tipoPagamento,
                            numeroArquivoRemessa: numeroArquivoRemessa,
                            numeroRegistroGeral: ref numeroRegistroGeral);
                        break;

                    default:
                        throw new Exception("Santander - Header Lote - Tipo de arquivo inexistente.");
                }

                if (String.IsNullOrWhiteSpace(header))
                {
                    throw new Exception("Registro HEADER DO LOTE obrigatório.");
                }

                return Utils.FormataLinhaArquivoCNAB(header, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw Pagamento2NetException.ErroAoGerarRegistroHeaderLoteDoArquivoRemessa(ex);
            }
        }

        public string GerarDetalheRemessaPagamento(TipoArquivo tipoArquivo, Documento documento, TipoPagamentoEnum tipoPagamento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral)
        {
            try
            {
                string strline = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:

                        IEnumerable<GeradorSegmento> segmentos = SegmentDecider(tipoPagamento);

                        foreach (GeradorSegmento gerador in segmentos)
                        {
                            strline = gerador.Invoke(documento, ref loteServico, ref numeroRegistroLote, ref numeroRegistroLote);

                            if (String.IsNullOrWhiteSpace(strline))
                            {
                                throw new Exception("Registro Segmento obrigatório.");
                            }
                        }

                        break;

                    default:
                        throw new Exception("Santander - Header Lote - Tipo de arquivo inexistente.");
                }

                return Utils.FormataLinhaArquivoCNAB(strline, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw Pagamento2NetException.ErroAoGerarRegistroHeaderLoteDoArquivoRemessa(ex);
            }
        }

        public string GerarTrailerLoteRemessaPagamento(TipoArquivo tipoArquivo, ref int numeroRegistroGeral, int loteServico, int numeroRegistros, decimal valorTotalRegistros)
        {
            try
            {
                string trailer = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        // Trailer do Lote
                        trailer = GerarTrailerLoteRemessaPagamentoCNAB240(
                                ref numeroRegistroGeral,
                                loteServico,
                                numeroRegistros,
                                valorTotalRegistros);
                        break;

                    default:
                        throw new Exception("Santander - Trailer Lote - Tipo de arquivo inexistente.");
                }

                if (String.IsNullOrWhiteSpace(trailer))
                {
                    throw new Exception("Registro TRAILER DO LOTE obrigatório.");
                }

                return Utils.FormataLinhaArquivoCNAB(trailer, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do Trailer do lote de PAGAMENTO.", ex);
            }
        }

        public string GerarTrailerRemessaPagamento(TipoArquivo tipoArquivo, int numeroRegistros, int numeroLotes, decimal valorTotalRegistros)
        {
            try
            {
                string trailer = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        // Trailer do Arquivo
                        trailer = GerarTrailerRemessaPagamentoCNAB240(numeroRegistros, numeroLotes);
                        break;

                    default:
                        throw new Exception("Santander - Trailer - Tipo de arquivo inexistente.");
                }

                if (String.IsNullOrWhiteSpace(trailer))
                {
                    throw new Exception("Registro TRAILER DO ARQUIVO obrigatório.");
                }

                return Utils.FormataLinhaArquivoCNAB(trailer, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a geração do Trailer do arquivo de PAGAMENTO.", ex);
            }
        }

        /// <summary>
        /// Geração do header do arquivo
        /// </summary>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        private string GerarHeaderRemessaPagamentoCNAB240(Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;
                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0000", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, pagador.TipoNúmeroCadastro("0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, pagador.NúmeroCadastroSomenteNúmeros(), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0033, 020, 0, pagador.CódigoConvênio, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 005, 0, pagador.ContaFinanceira.Agência, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0058, 001, 0, pagador.ContaFinanceira.DígitoAgência, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 012, 0, pagador.ContaFinanceira.Conta, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0071, 001, 0, pagador.ContaFinanceira.DígitoConta, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 030, 0, pagador.Nome.Trim().ToUpper(), ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 030, 0, "BANCO SANTANDER", ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0133, 010, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0143, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0144, 008, 0, DateTime.Now, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0152, 006, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0158, 006, 0, numeroArquivoRemessa, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0164, 003, 0, "060", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0167, 005, 0, "0", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0172, 020, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0192, 020, 0, Empty, ' ');    //uso reservado da empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0212, 019, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, '0');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa do CNAB240 v060.", ex);
            }
        }

        /// <summary>
        /// Geração do header de lote para os tipos de documento
        /// Crédito em Conta Corrente, Crédito em Conta Poupança, DOC, TED, Caixa e OP(Recibo)
        /// </summary>
        /// <param name="loteServico"></param>
        /// <param name="tipoServico"></param>
        /// <param name="tipoPagamento"></param>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <param name="versaoLayout"></param>
        /// <returns></returns>
        private string GerarHeaderLoteRemessaPagamentoCNAB240(Pagador pagador, string tipoOperacao, ref int loteServico, string tipoServico, TipoPagamentoEnum tipoPagamento, int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                loteServico++;
                numeroRegistroGeral++;
                int versãoLayout;

                switch (tipoPagamento)
                {
                    case TipoPagamentoEnum.PagamentoContasTributosComCodigoBarras:
                    case TipoPagamentoEnum.DARFNormal:
                    case TipoPagamentoEnum.GPS:
                    case TipoPagamentoEnum.DARFSimples:
                    case TipoPagamentoEnum.GareSpIcms:
                    case TipoPagamentoEnum.GareSpDr:
                    case TipoPagamentoEnum.GareSpItcmd:
                    case TipoPagamentoEnum.IpvaSp:
                    case TipoPagamentoEnum.LicenciamentoSp:
                    case TipoPagamentoEnum.DpvatSp:
                        versãoLayout = 10;
                        break;

                    case TipoPagamentoEnum.LiquidacaoTitulosMesmoBanco:
                    case TipoPagamentoEnum.LiquidacaoTitulosOutrosBancos:
                        versãoLayout = 30;
                        break;

                    case TipoPagamentoEnum.CreditoContaCorrente:
                    case TipoPagamentoEnum.Cheque:
                    case TipoPagamentoEnum.Doc:
                    case TipoPagamentoEnum.CreditoContaPoupanca:
                    case TipoPagamentoEnum.Ted:
                    case TipoPagamentoEnum.OrdemPagamento:
                    case TipoPagamentoEnum.Caixa:
                    default:
                        versãoLayout = 31;
                        break;
                }

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "1", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 001, 0, tipoOperacao, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0010, 002, 0, tipoServico, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0012, 002, 0, (int)tipoPagamento, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0014, 003, 0, versãoLayout, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0017, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, pagador.TipoNúmeroCadastro("0"), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, pagador.NúmeroCadastroSomenteNúmeros(), '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0033, 020, 0, pagador.CódigoConvênio, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 005, 0, pagador.ContaFinanceira.Agência, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0058, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 012, 0, pagador.ContaFinanceira.Conta, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0071, 001, 0, pagador.ContaFinanceira.DígitoConta, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 030, 0, pagador.Nome.Trim().ToUpper(), ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0103, 040, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0143, 030, 0, pagador.Endereço.Rua, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0173, 005, 0, pagador.Endereço.Número, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0178, 015, 0, pagador.Endereço.Complemento, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0193, 020, 0, pagador.Endereço.Cidade, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0213, 005, 0, pagador.Endereço.PrefixoCEP(), ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0218, 003, 0, pagador.Endereço.SufixoCEP(), ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0221, 002, 0, pagador.Endereço.Estado, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0223, 008, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0231, 010, 0, Empty, '0');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do lote no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        /// <summary>
        /// Geração do registro do Segmento A
        /// Crédito em Conta Corrente, Crédito em Conta Poupança, DOC, TED, Caixa e OP(Recibo)
        /// </summary>
        /// <param name="documento"></param>
        /// <param name="loteServico"></param>
        /// <param name="numeroRegistroLote"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        private string GerarDetalheSegmentoARemessaPagamentoCNAB240(Documento documento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral)
        {
            if (!(documento is Transferência))
            {
                throw new Exception("tem que ser do tipo transferência"); //TODO: melhorar exception
            }

            Transferência transferência = documento as Transferência;

            try
            {
                numeroRegistroLote++;
                numeroRegistroGeral++;
                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroLote, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "A", ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0015, 001, 0, (int)transferência.TipoDeMovimento, '0');

                // Código do Movimento
                switch (transferência.CódigoDaInstruçãoParaMovimento)
                {
                    case InstruçãoMovimentoEnum.InclusãoDeRegistroDetalheLiberado:
                    case InstruçãoMovimentoEnum.AutorizacaoPagamento:
                    case InstruçãoMovimentoEnum.AlteraçãoDoValorDoTítulo:
                    case InstruçãoMovimentoEnum.AlteraçãoDaDataDePagamento:
                    case InstruçãoMovimentoEnum.PagamentoDiretoAoFornecedor_Baixar:
                    case InstruçãoMovimentoEnum.ManutençãoEmCarteira_NãoPagar:
                    case InstruçãoMovimentoEnum.RetiradaDeCarteira_NãoPagar:
                    case InstruçãoMovimentoEnum.ExclusãoDoRegistroDetalheIncluídoAnteriormente:
                    default:
                        reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, (int)InstruçãoMovimentoEnum.AutorizacaoPagamento, '0');
                        break;

                    case InstruçãoMovimentoEnum.InclusãoDoRegistroDetalheBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoLiberadoParaBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoBloqueadoParaLiberado:
                    case InstruçãoMovimentoEnum.EstornoPorDevoluçãoDaCâmaraCentralizadora:
                    case InstruçãoMovimentoEnum.AlegaçãoDoPagador:
                        reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, (int)transferência.CódigoDaInstruçãoParaMovimento, '0');
                        break;
                }

                switch (transferência.TipoDePagamento)
                {
                    case TipoPagamentoEnum.CreditoContaCorrente:
                    case TipoPagamentoEnum.CreditoContaPoupanca:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 003, 0, "000", '0');
                        break;

                    case TipoPagamentoEnum.Doc:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 003, 0, "700", '0');
                        break;

                    case TipoPagamentoEnum.Ted:
                        reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 003, 0, "018", '0'); //TED CIP (018) / TED STR (810)
                        break;

                    default:
                        throw new Exception("Tipo de documento não implementado para o Segmento A");
                }

                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0021, 003, 0, transferência.Favorecido.ContaFinanceira.Banco, '0');           //codigo do banco favorecido
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 005, 0, transferência.Favorecido.ContaFinanceira.Agência, '0');         //codigo da agencia favorecido  //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0029, 001, 0, transferência.Favorecido.ContaFinanceira.DígitoAgência, ' ');   //codigo do digito da agencia do favorecido
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0030, 012, 0, transferência.Favorecido.ContaFinanceira.Conta, '0');           //codigo do conta favorecido //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0042, 001, 0, transferência.Favorecido.ContaFinanceira.DígitoConta, '0');     //digito verificador da conta favorecido //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0043, 001, 0, "0", '0');                                                      //digito verificador da agencia/conta //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0044, 030, 0, transferência.Favorecido.Nome, ' ');                            //Nome do favorecido.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 020, 0, transferência.NúmeroDocumentoCliente, ' ');                     //Numero do Documento
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0094, 008, 0, transferência.DataDoPagamento, ' ');                            //Data de Pagamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0102, 003, 0, "BRL", ' ');                                                    //Tipo de Moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0105, 015, 0, '0', '0');                                                      //Quantidade de Moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0120, 015, 2, transferência.ValorDoPagamento, '0');                           //Valor do pagamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0135, 020, 0, transferência.NúmeroDocumentoBanco, ' ');                       //Numero do Documento Banco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0155, 008, 0, "0", '0');                                                      //Data Real do Pagamento (retorno)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0163, 015, 0, '0', '0');                                                      //Valor Real do Pagamento (retorno)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0178, 040, 0, transferência.InformaçãoComplementar2, ' ');                    //Informação 2
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0218, 002, 0, transferência.FinalidadeDocTed.ToString(), ' ');                //Finalidade do doc/ted
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0220, 010, 0, "0", ' ');                                                      //Filler
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0230, 001, 0, "0", '0');                                                      //Emissão de Aviso ao Favorecido
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0231, 010, 0, Empty, ' ');                                                    //Ocorrencias para retorno
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar Segmento A do lote no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        /// <summary>
        /// Geração do registro Segmento B
        /// Registro obrigatório para pagamentos através de DOC, TED, Caixa e Ordem de pagamento(recibo)
        /// Registro opcional para Crédito em Conta Corrente e Conta Poupança, Emissão de Aviso.
        /// </summary>
        /// <param name="boleto"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        private string GerarDetalheSegmentoBRemessaPagamentoCNAB240(Documento documento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral)
        {
            if (!(documento is Transferência))
            {
                throw new Exception("tem que ser do tipo transferência"); //TODO: melhorar exception
            }

            Transferência transferência = documento as Transferência;

            try
            {
                numeroRegistroGeral++;
                numeroRegistroLote++;
                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroLote, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "B", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 003, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, transferência.Favorecido.TipoNúmeroCadastro("0"), '0');     //tipo de documento do favorecido
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, transferência.Favorecido.NúmeroCadastro, '0');              //numero documento do favorecido
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0033, 030, 0, transferência.Favorecido.Endereço.Rua, ' ');                //endereco do favorecido para entrega do recibo.  //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0063, 005, 0, transferência.Favorecido.Endereço.Número, ' ');             //número do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0068, 015, 0, transferência.Favorecido.Endereço.Complemento, ' ');        //complemento do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0083, 015, 0, transferência.Favorecido.Endereço.Bairro, ' ');             //bairro do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0098, 020, 0, transferência.Favorecido.Endereço.Cidade, ' ');             //cidade do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 008, 0, transferência.Favorecido.Endereço.CEP, ' ');                //cep do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0126, 002, 0, transferência.Favorecido.Endereço.Estado, ' ');             //estado do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0128, 008, 0, transferência.DataDoVencimento, ' ');                       //data de vencimento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0136, 015, 2, transferência.ValorDoPagamento, '0');                       //valor do documento. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0151, 015, 2, transferência.ValorDoAbatimento, '0');                      //valor do abatimento. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0166, 015, 2, transferência.ValorDoDesconto, '0');                        //valor do desconto. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0181, 015, 0, transferência.ValorDaMora, '0');                            //valor de mora. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0196, 015, 2, transferência.ValorDaMulta, '0');                           //valor de multa. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0211, 004, 0, 0, '0');                                                        //Horario de Envio de TED. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0215, 011, 0, Empty, ' ');                                                //brancos
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0226, 004, 0, 0, '0');                      //Código Histórico para Crédito
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0230, 001, 0, "0", '0');                                                  //emissão de aviso ao favorecido, opção 0 não enviar.
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, '0');                                                //Ocorrências para o Retorno
                reg.CodificarLinha();
                string vLinha = reg.LinhaRegistro;
                return vLinha;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar Segmento B do Segmento P no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        /// <summary>
        /// Geração do registro J
        /// Informar o registro para os pagamentos de titulos com código de barra
        /// </summary>
        /// <param name="documento"></param>
        /// <param name="loteServico"></param>
        /// <param name="numeroRegistroLote"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        private string GerarDetalheSegmentoJRemessaPagamentoCNAB240(Documento documento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral)
        {
            decimal
                valorDescontos = documento.Discount + documento.Rebate,
                valorAcrescimos = documento.Interest + documento.LateFee;

            try
            {
                numeroRegistroLote++;
                numeroRegistroGeral++;
                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 003, 0, "033", '0');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0004, 004, 0, loteServico, '0');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0008, 001, 0, "3", '0');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0009, 005, 0, numeroRegistroLote, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "J", ' ');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0015, 001, 0, (int)documento.MovementType, '0');        //código do movimento, 0 - Inclusão

                // Código do Movimento
                switch (documento.CódigoDaInstruçãoParaMovimento)
                {
                    case InstruçãoMovimentoEnum.InclusãoDeRegistroDetalheLiberado:
                    case InstruçãoMovimentoEnum.AutorizacaoPagamento:
                    case InstruçãoMovimentoEnum.AlteraçãoDoValorDoTítulo:
                    case InstruçãoMovimentoEnum.AlteraçãoDaDataDePagamento:
                    case InstruçãoMovimentoEnum.PagamentoDiretoAoFornecedor_Baixar:
                    case InstruçãoMovimentoEnum.ManutençãoEmCarteira_NãoPagar:
                    case InstruçãoMovimentoEnum.RetiradaDeCarteira_NãoPagar:
                    case InstruçãoMovimentoEnum.ExclusãoDoRegistroDetalheIncluídoAnteriormente:
                    default:
                        reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, (int)InstruçãoMovimentoEnum.AutorizacaoPagamento, '0');
                        break;

                    case InstruçãoMovimentoEnum.InclusãoDoRegistroDetalheBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoLiberadoParaBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoBloqueadoParaLiberado:
                    case InstruçãoMovimentoEnum.EstornoPorDevoluçãoDaCâmaraCentralizadora:
                    case InstruçãoMovimentoEnum.AlegaçãoDoPagador:
                        reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, (int)documento.CódigoDaInstruçãoParaMovimento, '0');
                        break;
                }

                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0018, 044, 0, Utils.BarcodeFormated(documento.Barcode), '0'); //codigo de barras - verificar rotina para deixar com 44 posições
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0062, 030, 0, documento.Favored.Name, ' ');
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0092, 008, 0, documento.DataDoVencimento, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0100, 015, 2, documento.DocumentValue, '0');        //valor nominal do titulo
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0115, 015, 2, valorDescontos, '0');                       //valor desconto + abatimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0130, 015, 2, valorAcrescimos, '0');                      //valor multa + juros
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0145, 008, 0, documento.DataDoPagamento, '0');          //data pagamento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0153, 015, 2, documento.ValorDoPagamento, '0');         //valor do pagamentos
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0168, 015, 0, "0", '0');                                  //quantidade de moeda
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0183, 020, 0, documento.NúmeroDocumentoCliente, ' ');       //numero do documento cliente
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0203, 020, 0, Empty, ' ');                                  //numero do documento banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0223, 002, 0, "00", '0');                                 //código da moeda
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0225, 006, 0, Empty, ' ');                                  //brancos
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, ' ');                                  //ocorrencias para o retorno
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar Segmento J do lote no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        }

        /// <summary>
        /// Geração do registro trailer do lote
        /// O registro é o totalizador do lote, contém informações como o numero de registros e o valor total informado no lote.
        /// </summary>
        /// <param name="numeroRegistroGeral"></param>
        /// <param name="loteServico"></param>
        /// <param name="numeroRegistros"></param>
        /// <param name="valorTotalRegistros"></param>
        /// <returns></returns>
        private string GerarTrailerLoteRemessaPagamentoCNAB240(ref int numeroRegistroGeral, int loteServico, int numeroRegistros, decimal valorTotalRegistros)
        {
            try
            {
                // O número de registros no lote é igual ao número de registros gerados + 2 (header e trailer do lote)
                int numeroRegistrosNoLote = numeroRegistros + 1;  //corrigindo para 1
                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "5", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 006, 0, numeroRegistrosNoLote, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 018, 2, valorTotalRegistros, '0');
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0042, 018, 0, "0", '0');  //somatorio quantidade moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0060, 006, 0, "0", '0');  //numero aviso de debito
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0066, 165, 0, Empty, ' '); //brancos
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, ' '); //ocorrencias retorno
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar TRAILER do lote no arquivo de remessa do CNAB240.", ex);
            }
        }

        /// <summary>
        /// Geração do trailer do arquivo
        /// </summary>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        public string GerarTrailerRemessaPagamentoCNAB240(int numeroRegistroGeral, int numeroLotes)
        {
            try
            {
                // O número de registros no arquivo é igual ao número de registros gerados + 4 (header e trailer do lote / header e trailer do arquivo)
                int numeroRegistrosNoArquivo = numeroRegistroGeral + 4;
                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 003, 0, "033", '0');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0004, 004, 0, "9999", '0');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0008, 001, 0, "9", '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0018, 006, 0, numeroLotes, '0');
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0024, 006, 0, numeroRegistrosNoArquivo, '0');
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0030, 211, 0, Empty, ' ');
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar TRAILER do arquivo de remessa do CNAB240.", ex);
            }
        }

        #endregion IRemessaPagamento methods

        #region IRetornoPagamento methods

        public void LerHeaderRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro)
        {
            try
            {
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:

                        LerHeaderRetornoPagamentoCNAB240(ref pagamento, registro);
                        break;

                    default:
                        throw new Exception("Santander - Header - Tipo de arquivo inexistente.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a leitura do registro HEADER do arquivo de REMESSA PAGAMENTO.", ex);
            }
        }

        public void LerDetalheRetornoPagamento(TipoArquivo tipoArquivo, string tipoSegmento, ref Documento documento, string registro)
        {
            try
            {
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        switch (tipoSegmento)
                        {
                            case "A":
                                LerDetalheRetornoPagamentoCNAB240SegmentoA(ref documento, registro);
                                break;

                            case "J":
                                LerDetalheRetornoPagamentoCNAB240SegmentoJ(ref documento, registro);
                                break;

                            case "B":
                                LerDetalheRetornoPagamentoCNAB240SegmentoB(ref documento, registro);
                                break;

                            default:
                                throw new Exception($"Santander - Detalhe - Tipo de Segmento {tipoSegmento} não cadastrado.");
                        }
                        break;

                    default:
                        throw new Exception("Santander - Detalhe - Tipo de arquivo inexistente.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a leitura do registro DETALHE do arquivo de REMESSA PAGAMENTO.", ex);
            }
        }

        private void LerHeaderRetornoPagamentoCNAB240(ref Pagamento pagamento, string registro)
        {
            pagamento.Pagador = new Pagador()
            {
                // Tipo de Inscrição da Empresa 018 018 9(001) Nota G023
                // Número Inscrição da Empresa 019 032 9(014) CNPJ/CPF
                NúmeroCadastro = registro.Substring(16, 1) == "1" ? registro.Substring(21, 11) : registro.Substring(18, 14),

                // Código do Convenio no Banco 033 052 X(020) Nota G009
                CódigoConvênio = registro.Substring(32, 20),

                ContaFinanceira = new ContaFinanceira()
                {
                    // Agência Mantenedora da Conta 053 057 9(005) Nota G003
                    Agência = registro.Substring(52, 5),
                    // Dígito Verificador da Agência 058 058 X(001) Branco
                    DígitoAgência = registro.Substring(57, 1),
                    // Número da Conta Corrente 059 070 9(012) Nota G003
                    Conta = registro.Substring(58, 12),
                    // Dígito Verificador da Conta 071 071 X(001) Nota G003
                    DígitoConta = registro.Substring(70, 1),
                },
                // Nome da Empresa 073 102 X(030) Obrigatório
                Nome = registro.Substring(72, 30),
            };

            pagamento.NúmeroRemessa = Utils.ToInt32(registro.Substring(157, 6));    // Número Seqüencial do Arquivo 158 163 9(006) Nota G010
        }

        private void LerDetalheRetornoPagamentoCNAB240SegmentoA(ref Documento documento, string registro)
        {
            try
            {
                // Tipo de Movimento 015 015 9(001) Nota G011
                documento.MovementType = (MovementTypeEnum)Enum.Parse(typeof(MovementTypeEnum), registro.Substring(14, 1));

                // Código da Instrução para Movimento 016 017 9(002) Nota G012
                documento.MovementInstruction = (MovementInstructionEnum)Enum.Parse(typeof(MovementInstructionEnum), registro.Substring(15, 2));

                documento.Favored = new Person
                {
                    FinancialAccount = new FinancialAccount
                    {
                        // IGNORADOS
                        // Código Câmara Compensação 018 020 9(003) Nota G014
                        // Dígito Verificador da Agência/Conta 043 043 X(001) Nota G003
                        // Data do Pagamento 094 101 9(008) DDMMAAAA
                        // Tipo da Moeda 102 104 X(003) Nota G005
                        // Quantidade de Moeda 105 119 9(010)V5 Zeros
                        // Valor do Pagamento 120 134 9(013)V2 Obrigatório
                        // Informação 2 - Mensagem 178 217 X(040) Nota G016

                        // Código do Banco Favorecido 021 023 9(003) Obrigatório
                        Bank = registro.Substring(20, 3),

                        // Código da Agência Favorecido 024 028 9(005) Nota G003
                        Agency = registro.Substring(23, 4),

                        // Dígito Verificador da Agência 029 029 X(001) Branco
                        AgencyDigit = registro.Substring(28, 1),

                        // Conta Corrente do Favorecido 030 041 9(012) Nota G003
                        Account = registro.Substring(29, 12),

                        // Dígito Verificador da Conta 042 042 X(001) Nota G003
                        AccountDigit = registro.Substring(41, 1)
                    },

                    //  Nome do Favorecido 044 073 X(030) Obrigatório
                    Name = registro.Substring(43, 30)
                };

                // Data Real do Pagamento (Retorno) 155 162 9(008) DDMMAAAA
                documento.PaymentDate = Utils.ToDateTime(Utils.ToInt32(registro.Substring(93, 8)).ToString("##-##-####"));

                // Valor Real do Pagamento 163 177 9(013)V2 Opcional
                documento.PaymentValue = Convert.ToDecimal(registro.Substring(119, 15)) / 100;

                // Nro. do Documento Cliente 074 093 X(020) Nota G006
                documento.DocumentNumber = registro.Substring(73, 20);

                // Nro. do Documento Banco 135 154 X(020) Nota G017
                documento.OurNumber = registro.Substring(134, 20);

                // Finalidade do DOC e TED 218 219 X(002) Nota G013
                documento.Finality = (FinalityEnum)Enum.Parse(typeof(FinalityEnum), registro.Substring(217, 2));

                // Emissão de Aviso ao Favorecido 230 230 X(001) Nota G018
                documento.NoticeToFavored = registro.Substring(229, 1);

                // Ocorrências para o Retorno 231 240 X(010) Nota G007
                string occurrences = registro.Substring(230, 10);
                for (int i = 0; i < 10; i += 2)
                {
                    documento.ReturnOccurrences.Add(new Occurrence(occurrences.Substring(i, 2), new SantanderOccurrences()));
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / A.", ex);
            }
        }

        private void LerDetalheRetornoPagamentoCNAB240SegmentoJ(ref Documento documento, string registro)
        {
            try
            {
                // Tipo de Movimento 015 015 9(001) Nota G011
                documento.MovementType = (MovementTypeEnum)Enum.Parse(typeof(MovementTypeEnum), registro.Substring(14, 1));

                // Código da Instrução para Movimento 016 017 9(002) Nota G012
                documento.MovementInstruction = (MovementInstructionEnum)Enum.Parse(typeof(MovementInstructionEnum), registro.Substring(15, 2));

                // Código de Barras 018 061 X(044) Nota G008
                documento.Barcode = registro.Substring(17, 44);

                // Nome do Cedente 062 091 X(030) Obrigatório
                documento.Favored.Name = registro.Substring(61, 30);

                // Data do Vencimento 092 099 9(008) DDMMAAAA
                documento.DueDate = Utils.ToDateTime(Utils.ToInt32(registro.Substring(91, 8)).ToString("##-##-####"));

                // Valor Nominal do Título 100 114 9(013)V2 Obrigatório
                documento.DocumentValue = Convert.ToDecimal(registro.Substring(99, 15)) / 100;

                // Valor Desconto + Abatimento 115 129 9(013)V2 Opcional
                documento.Discount = Convert.ToDecimal(registro.Substring(114, 15));
                // Valor Multa + Juros 130 144 9(013)V2 Opcional
                documento.Interest = Convert.ToDecimal(registro.Substring(99, 15));

                // Data do Pagamento 145 152 9(008) DDMMAAAA
                documento.PaymentDate = Utils.ToDateTime(Utils.ToInt32(registro.Substring(144, 8)).ToString("##-##-####"));
                ;

                // Valor do Pagamento 153 167 9(013)V2 Obrigatório
                documento.PaymentValue = Convert.ToDecimal(registro.Substring(152, 15)) / 100;

                // Nro. do Documento Cliente 183 202 X(020) Nota G006
                documento.DocumentNumber = registro.Substring(182, 20);

                // Nro. do Documento Banco 203 222 X(020) Nota G017
                documento.OurNumber = registro.Substring(202, 20);

                // Ocorrências para o Retorno 231 240 X(010) Nota G007
                string occurrences = registro.Substring(230, 10);
                for (int i = 0; i <= 8; i += 2)
                {
                    documento.ReturnOccurrences.Add(new Occurrence(occurrences.Substring(i, 2), new SantanderOccurrences()));
                }

                // IGNORADOS
                // Quantidade de Moeda 168 182 9(010)V5 Opcional
                // Código da Moeda 223 224 9(002) Opcional
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / J.", ex);
            }
        }

        private void LerDetalheRetornoPagamentoCNAB240SegmentoB(ref Documento documento, string registro)
        {
            try
            {
                // Tipo de Inscrição do Favorecido 018 018 9(001) Nota G023
                // CNPJ/CPF do Favorecido 019 032 9(014) CPF/CNPJ
                documento.Favored.CPFCNPJ = registro.Substring(17, 1) == "1" ? registro.Substring(21, 11) : registro.Substring(18, 14);

                // Logradouro do Favorecido 033 062 X(030) Opcional
                documento.Favored.Address.Street = registro.Substring(32, 30);

                // Número do Local do Favorecido 063 067 9(005) Opcional
                documento.Favored.Address.Number = registro.Substring(62, 5);

                // Complemento do Local Favorecido 068 082 X(015) Opcional
                documento.Favored.Address.Complement = registro.Substring(67, 15);

                // Bairro do Favorecido 083 097 X(015) Opcional
                documento.Favored.Address.District = registro.Substring(82, 15);

                // Cidade do Favorecido 098 117 X(020) Opcional
                documento.Favored.Address.City = registro.Substring(97, 20);

                // CEP do Favorecido 118 125 9(008) Opcional
                documento.Favored.Address.ZipCode = registro.Substring(117, 8);

                // Estado do Favorecido 126 127 X(002) Opcional
                documento.Favored.Address.State = registro.Substring(125, 2);

                // Data de Vencimento 128 135 9(008) DDMMAAAA
                documento.DueDate = Utils.ToDateTime(Utils.ToInt32(registro.Substring(127, 8)).ToString("##-##-####"));

                // Valor do Documento 136 150 9(013)V2 Opcional
                documento.DocumentValue = Convert.ToDecimal(registro.Substring(135, 15)) / 100;

                // Valor do Abatimento 151 165 9(013)V2 Opcional
                documento.Rebate = Convert.ToDecimal(registro.Substring(150, 15)) / 100;

                // Valor do Desconto 166 180 9(013)V2 Opcional
                documento.Discount = Convert.ToDecimal(registro.Substring(165, 15));

                // Valor da Mora 181 195 9(013)V2 Opcional
                documento.Interest = Convert.ToDecimal(registro.Substring(180, 15));

                // Valor da Multa 196 210 9(013)V2 Opcional
                documento.LateFee = Convert.ToDecimal(registro.Substring(195, 15));

                // Código Histórico para Crédito 226 229 9(004) Nota G019
                documento.CreditHistoryCode = Utils.ToInt32(registro.Substring(225, 4));

                // Emissão de Aviso ao Favorecido 230 230 9(001) Nota G018
                documento.NoticeToFavored = registro.Substring(229, 1);

                // Ocorrências para o Retorno 231 240 X(010) Nota G007
                //TODO: verificar em um arquivo real como retorna. se é aqui ou no registro anterior.

                // IGNORADO
                // Horário de Envio de TED 211 214 9(004) Opcional
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / B.", ex);
            }
        }

        #endregion IRetornoPagamento methods

        #region Segment Helpers

        private delegate string GeradorSegmento(Documento documento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral);

        private IEnumerable<GeradorSegmento> SegmentDecider(TipoPagamentoEnum paymentType)
        {
            switch (paymentType)
            {
                case TipoPagamentoEnum.Doc:
                case TipoPagamentoEnum.Ted:
                case TipoPagamentoEnum.Caixa:
                    return new GeradorSegmento[] { GerarDetalheSegmentoARemessaPagamentoCNAB240, GerarDetalheSegmentoBRemessaPagamentoCNAB240 };

                case TipoPagamentoEnum.CreditoContaCorrente:
                case TipoPagamentoEnum.CreditoContaPoupanca:
                case TipoPagamentoEnum.OrdemPagamento:
                    return new GeradorSegmento[] { GerarDetalheSegmentoARemessaPagamentoCNAB240 };

                case TipoPagamentoEnum.LiquidacaoTitulosMesmoBanco:
                case TipoPagamentoEnum.LiquidacaoTitulosOutrosBancos:
                    return new GeradorSegmento[] { GerarDetalheSegmentoJRemessaPagamentoCNAB240 };

                case TipoPagamentoEnum.GPS:
                //Segmento N1

                case TipoPagamentoEnum.DARFNormal:
                //Segmento N2

                case TipoPagamentoEnum.DARFSimples:
                //Segmento N3

                case TipoPagamentoEnum.GareSpIcms:
                case TipoPagamentoEnum.GareSpDr:
                case TipoPagamentoEnum.GareSpItcmd:
                //Segmento N4

                case TipoPagamentoEnum.IpvaSp:
                //Segmento N5

                case TipoPagamentoEnum.DpvatSp:
                //Segmento N6

                case TipoPagamentoEnum.LicenciamentoSp:
                //Segmento N7

                case TipoPagamentoEnum.PagamentoContasTributosComCodigoBarras:
                //Segmento O
                //Segmento W

                case TipoPagamentoEnum.Cheque:
                default:
                    throw new Exception($"Tipo de Pagamento não implementado: {paymentType.ToString()}");
            }
        }

        #endregion Segment Helpers
    }
}