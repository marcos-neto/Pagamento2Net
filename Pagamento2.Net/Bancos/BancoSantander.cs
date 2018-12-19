using Boleto2Net.Util;
using Pagamento2Net.Enums;
using Pagamento2Net.Entidades;
using Pagamento2Net.Exceptions;
using System;
using System.Collections.Generic;
using static System.String;

namespace Pagamento2Net.Bancos
{
    internal sealed partial class BancoSantander : IRemessaPagamento, IRetornoPagamento
    {
        #region IRemessaPagamento métodos

        /// <summary>
        /// Header do Arquivo de Remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="pagador"></param>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        public string GerarHeaderRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                string header = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:

                        header += GerarHeaderRemessaPagamentoCNAB240(
                            pagador,
                            numeroArquivoRemessa,
                            ref numeroRegistroGeral
                            );

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

        /// <summary>
        /// Header do Lote do Arquivo de Remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="pagador"></param>
        /// <param name="tipoPagamento"></param>
        /// <param name="loteServico"></param>
        /// <param name="tipoServico"></param>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <param name="numeroRegistrosLote"></param>
        /// <returns></returns>
        public string GerarHeaderLoteRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, TipoPagamentoEnum tipoPagamento, ref int loteServico, string tipoServico, int numeroArquivoRemessa, ref int numeroRegistroGeral, ref int numeroRegistrosLote)
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
                            numeroRegistroGeral: ref numeroRegistroGeral,
                            numeroRegistrosLote: ref numeroRegistrosLote);
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

        /// <summary>
        /// Detalhes do Lote do arquivo de Remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="documento"></param>
        /// <param name="tipoPagamento"></param>
        /// <param name="loteServico"></param>
        /// <param name="numeroRegistroLote"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
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
                            strline = gerador.Invoke(documento, ref loteServico, ref numeroRegistroLote, ref numeroRegistroGeral);

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
                throw Pagamento2NetException.ErroAoGerarRegistroDetalheDoArquivoRemessa(ex);
            }
        }

        /// <summary>
        /// Trailer do Lote do arquivo de Remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <param name="loteServico"></param>
        /// <param name="numeroRegistros"></param>
        /// <param name="valorTotalRegistros"></param>
        /// <returns></returns>
        public string GerarTrailerLoteRemessaPagamento(TipoArquivo tipoArquivo, int loteServico, int numeroRegistrosLote, decimal valorTotalRegistrosLote, ref int numeroRegistroGeral)
        {
            try
            {
                string trailer = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        // Trailer do Lote
                        trailer = GerarTrailerLoteRemessaPagamentoCNAB240(
                                loteServico,
                                numeroRegistrosLote,
                                valorTotalRegistrosLote,
                                ref numeroRegistroGeral
                                );
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
                throw Pagamento2NetException.ErroAoGerarRegistroTrailerLoteDoArquivoRemessa(ex);
            }
        }

        /// <summary>
        /// Trailer do arquivo de Remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="numeroRegistros"></param>
        /// <param name="numeroLotes"></param>
        /// <param name="valorTotalRegistros"></param>
        /// <returns></returns>
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
                throw Pagamento2NetException.ErroAoGerarRegistroTrailerLoteDoArquivoRemessa(ex);
            }
        }


        #region Geração do Arquivo CNAB240

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
                numeroRegistroGeral++; //Incrementa +1 registro ao contador de registros geral

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');                                    //  Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, "0000", '0');                                   //  Lote de Serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "0", '0');                                      //  Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');                                    //  Branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, pagador.TipoNúmeroCadastro("0"), '0');          //  Tipo de Inscrição da Empresa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, pagador.NúmeroCadastroSomenteNúmeros(), '0');   //  Número Inscrição da Empresa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0033, 020, 0, pagador.CódigoConvênio, '0');                   //  Código do Convenio no Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 005, 0, pagador.ContaFinanceira.Agência, '0');          //  Agência Mantenedora da Conta
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0058, 001, 0, pagador.ContaFinanceira.DígitoAgência, ' ');    //  Dígito Verificador da Agência
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 012, 0, pagador.ContaFinanceira.Conta, ' ');            //  Número da Conta Corrente
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0071, 001, 0, pagador.ContaFinanceira.DígitoConta, ' ');      //  Dígito Verificador da Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, Empty, ' ');                                    //  Dígito Verificador da Agência / Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 030, 0, pagador.Nome.Trim().ToUpper(), ' ');            //  Nome da Empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0103, 030, 0, "BANCO SANTANDER", ' ');                        //  Nome do Banco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0133, 010, 0, Empty, ' ');                                    //  Branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0143, 001, 0, "1", '0');                                      //  Código Remessa / Retorno                
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0144, 008, 0, DateTime.Now, ' ');                             //  Data da Geração do Arquivo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0152, 006, 0, Empty, ' ');                                    //  Hora da Geração do Arquivo
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0158, 006, 0, numeroArquivoRemessa, '0');                     //  Número Seqüencial do Arquivo
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0164, 003, 0, "060", '0');                                    //  Número da Versão do Layout
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0167, 005, 0, "0", '0');                                      //  Densidade de Gravação Arquivo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0172, 020, 0, Empty, ' ');                                    //  Uso Reservado do Banco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0192, 020, 0, Empty, ' ');                                    //  Uso reservado da empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0212, 019, 0, Empty, ' ');                                    //  Branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, '0');                                    //  Ocorrências para o Retorno
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
        private string GerarHeaderLoteRemessaPagamentoCNAB240(
            Pagador pagador, string tipoOperacao, string tipoServico, TipoPagamentoEnum tipoPagamento, int numeroArquivoRemessa, ref int loteServico, ref int numeroRegistrosLote, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++; //Incrementa +1 registro ao contador de registros geral
                numeroRegistrosLote++; //Incrementa +1 registro ao contador de registros do lote
                loteServico++; //Incrementa +1 ao numero de lotes do arquivo
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
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');                                    //  Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');                              //  Lote de Serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "1", '0');                                      //  Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 001, 0, tipoOperacao, ' ');                             //  Tipo da Operação
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0010, 002, 0, tipoServico, '0');                              //  Tipo de Serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0012, 002, 0, (int)tipoPagamento, '0');                       //  Forma de Lançamento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0014, 003, 0, versãoLayout, '0');                             //  Número da Versão do Lote
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0017, 001, 0, Empty, ' ');                                    //  Branco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, pagador.TipoNúmeroCadastro("0"), '0');          //  Tipo de Inscrição da Empresa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, pagador.NúmeroCadastroSomenteNúmeros(), '0');   //  Número de Inscrição da Empresa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0033, 020, 0, pagador.CódigoConvênio, '0');                   //  Código do Convenio no Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0053, 005, 0, pagador.ContaFinanceira.Agência, '0');          //  Agência Mantenedora da Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0058, 001, 0, Empty, ' ');                                    //  Dígito Verificador da Agência
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0059, 012, 0, pagador.ContaFinanceira.Conta, '0');            //  Número da Conta Corrente
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0071, 001, 0, pagador.ContaFinanceira.DígitoConta, '0');      //  Dígito Verificador da Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0072, 001, 0, Empty, ' ');                                    //  Dígito Verificador da Agência/Conta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0073, 030, 0, pagador.Nome.Trim().ToUpper(), ' ');            //  Nome da Empresa
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0103, 040, 0, Empty, ' ');                                    //  Informação 1 - Mensagem
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0143, 030, 0, pagador.Endereço.Rua, ' ');                     //  Endereço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0173, 005, 0, pagador.Endereço.Número, '0');                  //  Número
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0178, 015, 0, pagador.Endereço.Complemento, ' ');             //  Complemento do Endereço
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0193, 020, 0, pagador.Endereço.Cidade, ' ');                  //  Cidade
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0213, 005, 0, pagador.Endereço.PrefixoCEP(), ' ');            //  CEP
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0218, 003, 0, pagador.Endereço.SufixoCEP(), ' ');             //  Complemento do CEP
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0221, 002, 0, pagador.Endereço.Estado, ' ');                  //  UF
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0223, 008, 0, Empty, ' ');                                    //  Branco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0231, 010, 0, Empty, '0');                                    //  Ocorrências para o Retorno
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do lote no arquivo de remessa do CNAB240.", ex);
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
            if (!(documento is Transferencia))
            {
                throw new Exception("tem que ser do tipo transferência"); //TODO: melhorar exception
            }

            Transferencia transferência = documento as Transferencia;

            try
            {
                numeroRegistroGeral++;   //Incrementa +1 registro ao contador de registros geral
                numeroRegistroLote++;  //Incrementa +1 registro ao contador de registros do lote
                int instruçãoMovimento;
                string tipoPagamento;

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
                        instruçãoMovimento = (int)InstruçãoMovimentoEnum.AutorizacaoPagamento;
                        break;

                    case InstruçãoMovimentoEnum.InclusãoDoRegistroDetalheBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoLiberadoParaBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoBloqueadoParaLiberado:
                    case InstruçãoMovimentoEnum.EstornoPorDevoluçãoDaCâmaraCentralizadora:
                    case InstruçãoMovimentoEnum.AlegaçãoDoPagador:
                        instruçãoMovimento = (int)transferência.CódigoDaInstruçãoParaMovimento;
                        break;
                }

                switch (transferência.TipoDePagamento)
                {
                    case TipoPagamentoEnum.CreditoContaCorrente:
                    case TipoPagamentoEnum.CreditoContaPoupanca:
                        tipoPagamento = "000";
                        break;

                    case TipoPagamentoEnum.Doc:
                        tipoPagamento = "700";
                        break;

                    case TipoPagamentoEnum.Ted:
                        tipoPagamento = "018"; //TED CIP (018) / TED STR (810)
                        break;

                    default:
                        throw new Exception("Tipo de documento não implementado para o Segmento A");
                }

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');                                                    //  Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');                                              //  Lote de Serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');                                                      //  Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroLote, '0');                                       //  Número Seqüencial do Registro no Lote
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "A", ' ');                                                      //  Código Segmento do Registro Detalhe
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0015, 001, 0, (int)transferência.TipoDeMovimento, '0');                       //  Tipo de Movimento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, instruçãoMovimento, '0');                                       //  Código da Instrução para Movimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 003, 0, tipoPagamento, '0');                                            //  Código Câmara Compensação
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0021, 003, 0, transferência.Favorecido.ContaFinanceira.Banco, '0');           //  Codigo do banco favorecido
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 005, 0, transferência.Favorecido.ContaFinanceira.Agência, '0');         //  Codigo da agencia favorecido  //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0029, 001, 0, transferência.Favorecido.ContaFinanceira.DígitoAgência, ' ');   //  Codigo do digito da agencia do favorecido
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0030, 012, 0, transferência.Favorecido.ContaFinanceira.Conta, '0');           //  Codigo do conta favorecido //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0042, 001, 0, transferência.Favorecido.ContaFinanceira.DígitoConta, '0');     //  Digito verificador da conta favorecido //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0043, 001, 0, "0", '0');                                                      //  Digito verificador da agencia/conta //caso seja ordem de pagamento informar 00000 para poder sacar em qualquer agencia.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0044, 030, 0, transferência.Favorecido.Nome, ' ');                            //  Nome do favorecido.
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0074, 020, 0, transferência.NúmeroDocumentoCliente, ' ');                     //  Numero do Documento
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0094, 008, 0, transferência.DataDoPagamento, ' ');                            //  Data de Pagamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0102, 003, 0, "BRL", ' ');                                                    //  Tipo de Moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0105, 015, 0, '0', '0');                                                      //  Quantidade de Moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0120, 015, 2, transferência.ValorDoPagamento, '0');                           //  Valor do pagamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0135, 020, 0, transferência.NúmeroDocumentoBanco, ' ');                       //  Numero do Documento Banco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0155, 008, 0, "0", '0');                                                      //  Data Real do Pagamento (retorno)
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0163, 015, 0, '0', '0');                                                      //  Valor Real do Pagamento (retorno)
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0178, 040, 0, transferência.InformaçãoComplementar2, ' ');                    //  Informação 2
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0218, 002, 0, transferência.FinalidadeDocTed.ToString(), ' ');                //  Finalidade do doc/ted
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0220, 010, 0, "0", ' ');                                                      //  Brancos
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0230, 001, 0, "0", '0');                                                      //  Emissão de Aviso ao Favorecido
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0231, 010, 0, Empty, ' ');                                                    //  Ocorrencias para retorno
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
            if (!(documento is Transferencia))
            {
                throw new Exception("tem que ser do tipo transferência"); //TODO: melhorar exception
            }

            Transferencia transferência = documento as Transferencia;

            try
            {
                numeroRegistroGeral++;  //Incrementa +1 registro ao contador de registros geral
                numeroRegistroLote++;   //Incrementa +1 registro ao contador de registros do lote

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');                                                //  Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');                                          //  Lote de Serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "3", '0');                                                  //  Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0009, 005, 0, numeroRegistroLote, '0');                                   //  Número Seqüencial do Registro no Lote
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "B", '0');                                                  //  Código Segmento do Registro Detalhe
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0015, 003, 0, Empty, ' ');                                                //  Brancos
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 001, 0, transferência.Favorecido.TipoNúmeroCadastro("0"), '0');     //  Tipo de documento do favorecido
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0019, 014, 0, transferência.Favorecido.NúmeroCadastro, '0');              //  Numero documento do favorecido
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0033, 030, 0, transferência.Favorecido.Endereço.Rua, ' ');                //  Endereco do favorecido para entrega do recibo.  //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0063, 005, 0, transferência.Favorecido.Endereço.Número, ' ');             //  Número do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0068, 015, 0, transferência.Favorecido.Endereço.Complemento, ' ');        //  Complemento do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0083, 015, 0, transferência.Favorecido.Endereço.Bairro, ' ');             //  Bairro do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0098, 020, 0, transferência.Favorecido.Endereço.Cidade, ' ');             //  Cidade do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 008, 0, transferência.Favorecido.Endereço.CEP, ' ');                //  Cep do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0126, 002, 0, transferência.Favorecido.Endereço.Estado, ' ');             //  Estado do endereco do favorecido para entrega do recibo. //opcional
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0128, 008, 0, transferência.DataDoVencimento, ' ');                       //  Data de vencimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0136, 015, 2, transferência.ValorDoPagamento, '0');                       //  Valor do documento. //opcional
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0151, 015, 2, transferência.ValorDoAbatimento, '0');                      //  Valor do abatimento. //opcional
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0166, 015, 2, transferência.ValorDoDesconto, '0');                        //  Valor do desconto. //opcional
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0181, 015, 2, transferência.ValorDaMora, '0');                            //  Valor de mora. //opcional
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0196, 015, 2, transferência.ValorDaMulta, '0');                           //  Valor de multa. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0211, 004, 0, 0, '0');                                                    //  Horario de Envio de TED. //opcional
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0215, 011, 0, Empty, ' ');                                                //  Brancos
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0226, 004, 0, 0, '0');                                                    //  Código Histórico para Crédito
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0230, 001, 0, "0", '0');                                                  //  Emissão de aviso ao favorecido, opção 0 não enviar.
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, '0');                                                //  Ocorrências para o Retorno
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

            if (!(documento is Titulo))
            {
                throw new Exception("Nao é um titulo"); //TODO: melhorar exception
            }

            Titulo titulo = documento as Titulo;

            try
            {
                numeroRegistroGeral++;  //Incrementa +1 registro ao contador de registros geral
                numeroRegistroLote++;   //Incrementa +1 registro ao contador de registros do lote
                int instruçãoMovimento;

                // Código do Movimento
                switch (titulo.CódigoDaInstruçãoParaMovimento)
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
                        instruçãoMovimento = (int)InstruçãoMovimentoEnum.AutorizacaoPagamento;
                        break;

                    case InstruçãoMovimentoEnum.InclusãoDoRegistroDetalheBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoLiberadoParaBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoBloqueadoParaLiberado:
                    case InstruçãoMovimentoEnum.EstornoPorDevoluçãoDaCâmaraCentralizadora:
                    case InstruçãoMovimentoEnum.AlegaçãoDoPagador:
                        instruçãoMovimento = (int)titulo.CódigoDaInstruçãoParaMovimento;
                        break;
                }

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 003, 0, "033", '0');                                            //  Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0004, 004, 0, loteServico, '0');                                      //  Lote de Serviço
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0008, 001, 0, "3", '0');                                              //  Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0009, 005, 0, numeroRegistroLote, '0');                               //  Número Seqüencial do Registro no Lote
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0014, 001, 0, "J", ' ');                                              //  Código Segmento do Registro Detalhe
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0015, 001, 0, (int)titulo.TipoDeMovimento, '0');                      //  Código do movimento, 0 - Inclusão
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, instruçãoMovimento, '0');                               //  Código de Instrução para Movimento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0018, 044, 0, Utils.BarcodeFormated(titulo.CódigoDeBarras), '0');     //  Codigo de barras - verificar rotina para deixar com 44 posições
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0062, 030, 0, titulo.Favorecido.Nome, ' ');                           //  Nome do Cedente
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0092, 008, 0, titulo.DataDoVencimento, '0');                          //  Data do Vencimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0100, 015, 2, titulo.ValorDoDocumento, '0');                          //  Valor nominal do titulo
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0115, 015, 2, titulo.ValorDescontos(), '0');                          //  Valor desconto + abatimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0130, 015, 2, titulo.ValorAcrescimos(), '0');                         //  Valor multa + juros
                reg.Adicionar(TTiposDadoEDI.ediDataDDMMAAAA_________, 0145, 008, 0, titulo.DataDoPagamento, '0');                           //  Data pagamento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0153, 015, 2, titulo.ValorDoPagamento, '0');                          //  Valor do pagamentos
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0168, 015, 0, "0", '0');                                              //  Quantidade de moeda
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0183, 020, 0, titulo.NúmeroDocumentoCliente, ' ');                    //  Numero do documento cliente
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0203, 020, 0, Empty, ' ');                                            //  Numero do documento banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0223, 002, 0, "00", '0');                                             //  Código da moeda
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0225, 006, 0, Empty, ' ');                                            //  Brancos
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, ' ');                                            //  Ocorrencias para o retorno
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar Segmento J do lote no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        } //Revisar

        /// <summary>
        /// Geração do registro trailer do lote
        /// O registro é o totalizador do lote, contém informações como o numero de registros e o valor total informado no lote.
        /// </summary>
        /// <param name="numeroRegistroGeral"></param>
        /// <param name="loteServico"></param>
        /// <param name="numeroRegistros"></param>
        /// <param name="valorTotalRegistros"></param>
        /// <returns></returns>
        private string GerarTrailerLoteRemessaPagamentoCNAB240(int loteServico, int numeroRegistrosLote, decimal valorTotalRegistrosLote, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++; //Incrementa +1 registro ao contador geral
                numeroRegistrosLote++;  // O número de registros no lote é igual ao número de registros gerados + 1 (trailer do lote)

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0001, 003, 0, "033", '0');                    //  Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0004, 004, 0, loteServico, '0');              //  Lote de Serviço
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 001, 0, "5", '0');                      //  Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');                    //  Filler
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0018, 006, 0, numeroRegistrosLote, '0');    //  Quantidade de Registros do Lote
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0024, 018, 2, valorTotalRegistrosLote, '0');      //  Somatória dos Valores
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0042, 018, 0, "0", '0');                      //  Somatorio quantidade moeda
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0060, 006, 0, "0", '0');                      //  Numero aviso de debito
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0066, 165, 0, Empty, ' ');                    //  Brancos
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0231, 010, 0, Empty, ' ');                    //  Ocorrencias retorno
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
                int numeroRegistrosNoArquivo = numeroRegistroGeral + 1; // O número de registros no arquivo é igual ao número de registros gerados + 1

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 003, 0, "033", '0');                        //  Código do Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0004, 004, 0, "9999", '0');                       //  Lote de Serviço
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0008, 001, 0, "9", '0');                          //  Tipo de Registro
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0009, 009, 0, Empty, ' ');                        //  Branco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0018, 006, 0, numeroLotes, '0');                  //  Quantidade de lotes do arquivo
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0024, 006, 0, numeroRegistrosNoArquivo, '0');     //  Quantidade de registros no arquivo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0030, 211, 0, Empty, ' ');                        //  Branco
                reg.CodificarLinha();
                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar TRAILER do arquivo de remessa do CNAB240.", ex);
            }
        }

        #endregion Geração do Arquivo CNAB240


        #endregion IRemessaPagamento methods

        #region IRetornoPagamento métodos

        /// <summary>
        /// Header do arquivo de retorno
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="pagamento"></param>
        /// <param name="registro"></param>
        /// <param name="tipoDeServiço"></param>
        public void LerHeaderRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:

                        LerHeaderRetornoPagamentoCNAB240(ref pagamento, registro, ref tipoDeServiço);
                        break;

                    default:
                        throw new Exception("Santander - Header - Tipo de arquivo inexistente.");
                }
            }
            catch (Exception ex)
            {
                Pagamento2NetException.ErroAoLerRegistroHeaderDoArquivoRetorno(ex);
            }
        }

        /// <summary>
        /// Header do Lote
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="Documento"></param>
        /// <param name="registro"></param>
        /// <param name="tipoDeServiço"></param>
        public void LerHeaderLoteRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {
                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        tipoDeServiço = (TipoServiçoEnum)Enum.Parse(typeof(TipoServiçoEnum), registro.Substring(9, 2));
                        break;
                    default:
                        throw new Exception("Santander - Header do Lote inexistente.");
                }
            }
            catch (Exception ex)
            {
                Pagamento2NetException.ErroAoLerRegistroHeaderLoteDoArquivoRetorno(ex);
            }
        }

        /// <summary>
        /// Detalhe do lote do arquivo de retorno
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="tipoSegmento"></param>
        /// <param name="documento"></param>
        /// <param name="registro"></param>
        /// <param name="tipoDeServiço"></param>
        public void LerDetalheRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {
                var tipoSegmento = registro.Substring(13, 1);

                switch (tipoArquivo)
                {
                    case TipoArquivo.CNAB240:
                        switch (tipoSegmento)
                        {
                            case "A":
                                LerDetalheRetornoPagamentoCNAB240SegmentoA(ref pagamento, registro, ref tipoDeServiço);
                                break;

                            case "J":
                                LerDetalheRetornoPagamentoCNAB240SegmentoJ(ref pagamento, registro, ref tipoDeServiço);
                                break;

                            case "B":
                                LerDetalheRetornoPagamentoCNAB240SegmentoB(ref pagamento, registro, ref tipoDeServiço);
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

        /// <summary>
        /// Trailer do Lote
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="Documento"></param>
        /// <param name="registro"></param>
        /// <param name="tipoDeServiço"></param>
        public void LerTrailerLoteRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {

        }

        /// <summary>
        /// Trailer do Arquivo de Retorno
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="Documento"></param>
        /// <param name="registro"></param>
        /// <param name="tipoDeServiço"></param>
        public void LerTrailerRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {

        }


        private void LerHeaderRetornoPagamentoCNAB240(ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
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

        private void LerDetalheRetornoPagamentoCNAB240SegmentoA(ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {

                Transferencia transferência = new Transferencia
                {
                    // Tipo de Movimento 015 015 9(001) Nota G011
                    TipoDeMovimento = (TipoMovimentoEnum)Enum.Parse(typeof(TipoMovimentoEnum), registro.Substring(14, 1)),

                    // Código da Instrução para Movimento 016 017 9(002) Nota G012
                    CódigoDaInstruçãoParaMovimento = (InstruçãoMovimentoEnum)Enum.Parse(typeof(InstruçãoMovimentoEnum), registro.Substring(15, 2)),

                    Favorecido = new Favorecido
                    {
                        ContaFinanceira = new ContaFinanceira
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
                            Banco = registro.Substring(20, 3),

                            // Código da Agência Favorecido 024 028 9(005) Nota G003
                            Agência = registro.Substring(23, 4),

                            // Dígito Verificador da Agência 029 029 X(001) Branco
                            DígitoAgência = registro.Substring(28, 1),

                            // Conta Corrente do Favorecido 030 041 9(012) Nota G003
                            Conta = registro.Substring(29, 12),

                            // Dígito Verificador da Conta 042 042 X(001) Nota G003
                            DígitoConta = registro.Substring(41, 1)
                        },

                        //  Nome do Favorecido 044 073 X(030) Obrigatório
                        Nome = registro.Substring(43, 30)
                    },

                    // Data Real do Pagamento (Retorno) 155 162 9(008) DDMMAAAA
                    DataDoPagamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(93, 8)).ToString("##-##-####")),

                    // Valor Real do Pagamento 163 177 9(013)V2 Opcional
                    ValorDoPagamento = Convert.ToDecimal(registro.Substring(119, 15)) / 100,

                    // Nro. do Documento Cliente 074 093 X(020) Nota G006
                    NúmeroDocumentoCliente = registro.Substring(73, 20),

                    // Nro. do Documento Banco 135 154 X(020) Nota G017
                    NúmeroDocumentoBanco = registro.Substring(134, 20),

                    // Finalidade do DOC e TED 218 219 X(002) Nota G013
                    FinalidadeDocTed = (FinalidadeEnum)Enum.Parse(typeof(FinalidadeEnum), registro.Substring(217, 2)),

                    // Emissão de Aviso ao Favorecido 230 230 X(001) Nota G018
                    AvisoAoFavorecido = registro.Substring(229, 1)
                };

                // Ocorrências para o Retorno 231 240 X(010) Nota G007
                string ocorrências = registro.Substring(230, 10);
                for (int i = 0; i < 10; i += 2)
                {
                    transferência.OcorrênciasParaRetorno.Add(new Ocorrencia(ocorrências.Substring(i, 2), new OcorrênciasSantander()));
                }

                //Adiciona a transferencia ao Pagamento
                pagamento.Documentos.Add(transferência);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / A.", ex);
            }
        }

        private void LerDetalheRetornoPagamentoCNAB240SegmentoB(ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {
                Transferencia transferência = new Transferencia()
                {
                    Favorecido = new Favorecido()
                    {
                        // Tipo de Inscrição do Favorecido 018 018 9(001) Nota G023
                        // CNPJ/CPF do Favorecido 019 032 9(014) CPF/CNPJ
                        NúmeroCadastro = registro.Substring(17, 1) == "1" ? registro.Substring(21, 11) : registro.Substring(18, 14),

                        Endereço = new Endereco()
                        {
                            // Logradouro do Favorecido 033 062 X(030) Opcional
                            Rua = registro.Substring(32, 30),

                            // Número do Local do Favorecido 063 067 9(005) Opcional
                            Número = registro.Substring(62, 5),

                            // Complemento do Local Favorecido 068 082 X(015) Opcional
                            Complemento = registro.Substring(67, 15),

                            // Bairro do Favorecido 083 097 X(015) Opcional
                            Bairro = registro.Substring(82, 15),

                            // Cidade do Favorecido 098 117 X(020) Opcional
                            Cidade = registro.Substring(97, 20),

                            // CEP do Favorecido 118 125 9(008) Opcional
                            CEP = registro.Substring(117, 8),

                            // Estado do Favorecido 126 127 X(002) Opcional
                            Estado = registro.Substring(125, 2)
                        }
                    },

                    // Data de Vencimento 128 135 9(008) DDMMAAAA
                    DataDoVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(127, 8)).ToString("##-##-####")),

                    // Valor do Documento 136 150 9(013)V2 Opcional
                    ValorDoDocumento = Convert.ToDecimal(registro.Substring(135, 15)) / 100,

                    // Valor do Abatimento 151 165 9(013)V2 Opcional
                    ValorDoAbatimento = Convert.ToDecimal(registro.Substring(150, 15)) / 100,

                    // Valor do Desconto 166 180 9(013)V2 Opcional
                    ValorDoDesconto = Convert.ToDecimal(registro.Substring(165, 15)),

                    // Valor da Mora 181 195 9(013)V2 Opcional
                    ValorDaMora = Convert.ToDecimal(registro.Substring(180, 15)),

                    // Valor da Multa 196 210 9(013)V2 Opcional
                    ValorDaMulta = Convert.ToDecimal(registro.Substring(195, 15)),

                    // Código Histórico para Crédito 226 229 9(004) Nota G019
                    CódigoHistórico = Utils.ToInt32(registro.Substring(225, 4)),

                    // Emissão de Aviso ao Favorecido 230 230 9(001) Nota G018
                    AvisoAoFavorecido = registro.Substring(229, 1)

                };

                // Ocorrências para o Retorno 231 240 X(010) Nota G007
                string ocorrências = registro.Substring(230, 10);
                for (int i = 0; i < 10; i += 2)
                {
                    transferência.OcorrênciasParaRetorno.Add(new Ocorrencia(ocorrências.Substring(i, 2), new OcorrênciasSantander()));
                }

                //Adiciona a transferência ao pagamento
                pagamento.Documentos.Add(transferência);

                // IGNORADO
                // Horário de Envio de TED 211 214 9(004) Opcional
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / B.", ex);
            }
        }

        private void LerDetalheRetornoPagamentoCNAB240SegmentoJ(ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {
                Titulo título = new Titulo
                {

                    // Tipo de Movimento 015 015 9(001) Nota G011
                    TipoDeMovimento = (TipoMovimentoEnum)Enum.Parse(typeof(TipoMovimentoEnum), registro.Substring(14, 1)),

                    // Código da Instrução para Movimento 016 017 9(002) Nota G012
                    CódigoDaInstruçãoParaMovimento = (InstruçãoMovimentoEnum)Enum.Parse(typeof(InstruçãoMovimentoEnum), registro.Substring(15, 2)),

                    // Código de Barras 018 061 X(044) Nota G008
                    CódigoDeBarras = registro.Substring(17, 44),
                    Favorecido = new Favorecido()
                    {
                        // Nome do Cedente 062 091 X(030) Obrigatório
                        Nome = registro.Substring(61, 30)

                    },

                    // Data do Vencimento 092 099 9(008) DDMMAAAA
                    DataDoVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(91, 8)).ToString("##-##-####")),

                    // Valor Nominal do Título 100 114 9(013)V2 Obrigatório
                    ValorDoDocumento = Convert.ToDecimal(registro.Substring(99, 15)) / 100,

                    // Valor Desconto + Abatimento 115 129 9(013)V2 Opcional
                    ValorDoDesconto = Convert.ToDecimal(registro.Substring(114, 15)),

                    // Valor Multa + Juros 130 144 9(013)V2 Opcional
                    ValorDaMulta = Convert.ToDecimal(registro.Substring(99, 15)),

                    // Data do Pagamento 145 152 9(008) DDMMAAAA
                    DataDoPagamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(144, 8)).ToString("##-##-####")),

                    // Valor do Pagamento 153 167 9(013)V2 Obrigatório
                    ValorDoPagamento = Convert.ToDecimal(registro.Substring(152, 15)) / 100,

                    // Nro. do Documento Cliente 183 202 X(020) Nota G006
                    NúmeroDocumentoCliente = registro.Substring(182, 20),

                    // Nro. do Documento Banco 203 222 X(020) Nota G017
                    NúmeroDocumentoBanco = registro.Substring(202, 20)
                };


                // Ocorrências para o Retorno 231 240 X(010) Nota G007
                string ocorrências = registro.Substring(230, 10);
                for (int i = 0; i <= 8; i += 2)
                {
                    título.OcorrênciasParaRetorno.Add(new Ocorrencia(ocorrências.Substring(i, 2), new OcorrênciasSantander()));
                }

                pagamento.Documentos.Add(título);

                // IGNORADOS
                // Quantidade de Moeda 168 182 9(010)V5 Opcional
                // Código da Moeda 223 224 9(002) Opcional
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / CNAB 240 / J.", ex);
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