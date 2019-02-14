using Boleto2Net.Util;
using Pagamento2Net.Enums;
using Pagamento2Net.Entidades;
using Pagamento2Net.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.String;

namespace Pagamento2Net.Bancos
{
    internal sealed partial class BancoBradesco : IRemessaPagamento, IRetornoPagamento
    {
        #region IRemessaPagamento

        /// <summary>
        /// Header do arquivo de remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="pagador"></param>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistro"></param>
        /// <returns></returns>
        public string GerarHeaderRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistrosGeral)
        {
            try
            {
                string header = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.POS500:

                        header += GerarHeaderRemessaPagamentoPOS500(pagador, numeroArquivoRemessa, ref numeroRegistrosGeral);
                        break;

                    default:
                        throw new Exception("Bradesco - Header - Tipo de arquivo inexistente.");
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
        /// Header do lote do arquivo de remessa
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
        public string GerarHeaderLoteRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, TipoPagamentoEnum tipoPagamento, ref int loteServico, string tipoServico, int numeroArquivoRemessa, ref int numeroRegistrosLote, ref int numeroRegistroGeral)
        {
            return Empty;
        }

        /// <summary>
        /// Detalhes do arquivo de remessa
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
                    case TipoArquivo.POS500:
                        strline = GerarDetalheRemessaPagamentoPOS500(documento, ref numeroRegistroLote, ref numeroRegistroGeral);
                        break;

                    default:
                        throw new Exception("Bradesco - Header Lote - Tipo de arquivo inexistente.");
                }
                if (String.IsNullOrWhiteSpace(strline))
                {
                    throw new Exception("Registro Segmento obrigatório.");
                }

                return Utils.FormataLinhaArquivoCNAB(strline, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw Pagamento2NetException.ErroAoGerarRegistroDetalheDoArquivoRemessa(ex);
            }
        }

        /// <summary>
        /// Trailer do lote do arquivo de remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <param name="loteServico"></param>
        /// <param name="numeroRegistros"></param>
        /// <param name="valorTotalRegistros"></param>
        /// <returns></returns>
        public string GerarTrailerLoteRemessaPagamento(TipoArquivo tipoArquivo, int loteServico, int numeroRegistrosLote, decimal valorTotalRegistrosLote, ref int numeroRegistroGeral)
        {
            return Empty;
        }

        /// <summary>
        /// Trailer do arquivo de remessa
        /// </summary>
        /// <param name="tipoArquivo"></param>
        /// <param name="numeroRegistros"></param>
        /// <param name="numeroLotes"></param>
        /// <param name="valorTotalRegistros"></param>
        /// <returns></returns>
        public string GerarTrailerRemessaPagamento(TipoArquivo tipoArquivo, int numeroRegistroGeral, int numeroLotes, decimal valorTotalRegistros)
        {
            try
            {
                string trailer = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.POS500:
                        // Trailler do Arquivo
                        trailer = GerarTrailerRemessaPagamentoPOS500(numeroRegistroGeral, valorTotalRegistros);
                        break;

                    default:
                        throw new Exception("Bradesco - Trailler - Tipo de arquivo inexistente.");
                }

                if (String.IsNullOrWhiteSpace(trailer))
                {
                    throw new Exception("Registro TRAILER DO ARQUIVO obrigatório.");
                }

                return Utils.FormataLinhaArquivoCNAB(trailer, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw Pagamento2NetException.ErroAoGerrarRegistroTrailerDoArquivoRemessa(ex);
            }
        }

        #region Geração do arquivo

        /// <summary>
        /// Geração do header do arquivo
        /// </summary>
        /// <param name="numeroArquivoRemessa"></param>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        private string GerarHeaderRemessaPagamentoPOS500(Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++; //Adiciona +1 ao numero geral de linhas do arquivo

                string docNumber = pagador.NúmeroCadastroSomenteNúmeros(),
                       inscrição = Empty,
                       filial = Empty,
                       controle = Empty;

                if (docNumber.Length == 11) //CPF
                {
                    inscrição = docNumber.Substring(0, 9);
                    filial = "0000";
                    controle = docNumber.Substring(9, 2);
                }
                else if (docNumber.Length == 14) //CNPJ
                {
                    inscrição = docNumber.Substring(0, 8);
                    filial = docNumber.Substring(8, 4);
                    controle = docNumber.Substring(12, 2);
                }
                else
                {
                    throw new Exception("Formato incorreto do CPF/CNPJ da empresa pagadora");
                }

                //Campo reservado para a empresa - usaremos para passar os dados da conta financeira da instituição pagadora
                string contaFinanceira =
                    pagador.ContaFinanceira.Banco.PadLeft(3, '0') +                                 //Banco
                    pagador.ContaFinanceira.Agência.PadLeft(5, '0') +                               //Agencia
                    pagador.ContaFinanceira.DígitoAgência.PadLeft(1, '0') +                         //Digito Agencia
                    pagador.ContaFinanceira.Conta.PadLeft(13, '0') +                                //Conta
                    pagador.ContaFinanceira.DígitoConta.PadLeft(2, '0');                            //Digito Conta


                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 001, 0, 0, '0');                                // Identificação do registro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0002, 008, 0, pagador.CódigoConvênio, '0');           // Código de Comunicação - Identificação da Empresa no Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0010, 001, 0, pagador.TipoNúmeroCadastro("0"), '0');  // Tipo de Inscrição da Empresa Pagadora
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0011, 009, 0, inscrição, '0');                        // CNPJ/CPF - Número da Inscrição
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0020, 004, 0, filial, '0');                           // CNPJ/CPF - Filial
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0024, 002, 0, controle, '0');                         // CNPJ/CPF - Controle
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0026, 040, 0, pagador.Nome.Trim().ToUpper(), ' ');    // Nome da empresa pagadora
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0066, 002, 0, 20, '0');                               // Tipo de Serviço
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0068, 001, 0, 1, '0');                                // Código de origem do arquivo
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0069, 005, 0, numeroArquivoRemessa, '0');             // Número da remessa
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0074, 005, 0, 0, '0');                                // Número do retorno
                reg.Adicionar(TTiposDadoEDI.ediDataAAAAMMDD_________, 0079, 008, 0, DateTime.Now, '0');                     // Data da gravação do arquivo
                reg.Adicionar(TTiposDadoEDI.ediHoraHHMMSS___________, 0087, 006, 0, DateTime.Now, '0');                     // Hora da gravação do arquivo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0093, 005, 0, Empty, ' ');                            // Densidade de gravação do arquivo/fita
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0098, 003, 0, Empty, ' ');                            // Unidade de densidade da gravação do arquivo/fita
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0101, 005, 0, Empty, ' ');                            // Identificação Módulo Micro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0106, 001, 0, 0, '0');                                // Tipo de processamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0107, 003, 0, Empty, ' ');                            // Reservado - empresa  - Branco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0110, 071, 0, contaFinanceira, ' ');                  // Reservado - empresa / Usaremos para passar os dados da conta financeira da instituição pagadora
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0181, 080, 0, Empty, ' ');                            // Reservado - Banco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0261, 217, 0, Empty, ' ');                            // Reservado - Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0478, 009, 0, 0, '0');                                // Número da Lista de Débito
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0487, 008, 0, Empty, ' ');                            // Reservado - Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0495, 006, 0, numeroRegistroGeral, '0');              // Número Sequencial do Registro
                reg.CodificarLinha();

                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar HEADER do arquivo de remessa de 500 posições.", ex);
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
        private string GerarDetalheRemessaPagamentoPOS500(Documento documento, ref int numeroRegistroLote, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++; //Adiciona +1 ao numero total de linhas do arquivo

                Dictionary<int, object> parameters = new Dictionary<int, object>
                {
                    [0002] = documento.Favorecido.TipoNúmeroCadastro("0"),                              // Tipo de Inscrição do Fornecedor
                    [0018] = documento.Favorecido.Nome,                                                 // Nome do Fornecedor
                    [0120] = documento.NúmeroDocumentoCliente,                                          // Número do Pagamento
                    [0166] = documento.DataDoVencimento,                                                // Data de vencimento
                    [0195] = documento.ValorDoDocumento,                                                // Valor do Documento
                    [0205] = documento.ValorDoPagamento,                                                // Valor do Pagamento
                    [0220] = documento.ValorDoDesconto,                                                 // Valor do Desconto
                    [0235] = documento.ValorDaMora + documento.ValorDaMulta,                            // Valor do Acréscimo
                    [0250] = 5,                                                                         // Tipo de documento
                    [0266] = documento.DataDoPagamento,                                                 // Data para efetivação do pagamento
                    [0289] = (int)documento.TipoDeMovimento                                             // Tipo de movimento
                };

                #region Documento

                string docNumber = documento.Favorecido.NúmeroCadastroSomenteNúmeros();

                if (docNumber.Length == 11) //CPF
                {
                    parameters[0003] = docNumber.Substring(0, 9);   // CNPJ/CPF - Número da Inscrição
                    parameters[0012] = "0000";                      // CNPJ/CPF - Filial
                    parameters[0016] = docNumber.Substring(9, 2);   // CNPJ/CPF - Controle
                }
                else if (docNumber.Length == 14) //CNPJ
                {
                    parameters[0003] = docNumber.Substring(0, 8);   // CNPJ/CPF - Número da Inscrição
                    parameters[0012] = docNumber.Substring(8, 4);   // CNPJ/CPF - Filial
                    parameters[0016] = docNumber.Substring(12, 2);  // CNPJ/CPF - Controle
                }

                #endregion

                #region Endereço

                if (documento.Favorecido.Endereço == null)
                {
                    parameters[0048] = Empty;  // Endereço do Fornecedor
                    parameters[0088] = Empty;  // Número do CEP
                    parameters[0093] = Empty;  // Sufixo do CEP
                }
                else
                {
                    parameters[0048] = documento.Favorecido.Endereço.FormataLogradouro(40);                     // Endereço do Fornecedor
                    parameters[0088] = documento.Favorecido.Endereço.PrefixoCEP();                              // Número do CEP
                    parameters[0093] = documento.Favorecido.Endereço.SufixoCEP();                               // Sufixo do CEP
                }

                #endregion

                // Código do Movimento
                switch (documento.CódigoDaInstruçãoParaMovimento)
                {
                    case InstruçãoMovimentoEnum.InclusãoDeRegistroDetalheLiberado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoBloqueadoParaLiberado:
                    case InstruçãoMovimentoEnum.AutorizacaoPagamento:
                    case InstruçãoMovimentoEnum.AlteraçãoDoValorDoTítulo:
                    case InstruçãoMovimentoEnum.AlteraçãoDaDataDePagamento:
                    case InstruçãoMovimentoEnum.PagamentoDiretoAoFornecedor_Baixar:
                    case InstruçãoMovimentoEnum.ManutençãoEmCarteira_NãoPagar:
                    case InstruçãoMovimentoEnum.RetiradaDeCarteira_NãoPagar:
                    case InstruçãoMovimentoEnum.ExclusãoDoRegistroDetalheIncluídoAnteriormente:
                    default:
                        parameters[0290] = "00";
                        break;

                    case InstruçãoMovimentoEnum.InclusãoDoRegistroDetalheBloqueado:
                    case InstruçãoMovimentoEnum.AlteraçãoDoPagamentoLiberadoParaBloqueado:
                    case InstruçãoMovimentoEnum.EstornoPorDevoluçãoDaCâmaraCentralizadora:
                        parameters[0290] = "25";
                        break;

                    case InstruçãoMovimentoEnum.AlegaçãoDoPagador:
                        parameters[0290] = "50";
                        break;
                }

                parameters[0495] = numeroRegistroGeral; // Número Sequencial do Registro

                switch (documento.TipoDePagamento)
                {
                    case TipoPagamentoEnum.CreditoContaCorrente:
                    case TipoPagamentoEnum.CreditoContaPoupanca:
                        GerarTipoPagamentoCreditoEmConta(ref parameters, documento);
                        break;

                    case TipoPagamentoEnum.OrdemPagamento:
                        GerarTipoPagamentoCheque(ref parameters, documento);
                        break;

                    case TipoPagamentoEnum.Doc:
                    case TipoPagamentoEnum.Ted:
                        GerarTipoPagamentoDocTed(ref parameters, documento);
                        break;

                    case TipoPagamentoEnum.LiquidacaoTitulosMesmoBanco:
                        GerarTipoPagamentoRastreamentoDeTitulos(ref parameters, documento);
                        break;

                    case TipoPagamentoEnum.LiquidacaoTitulosOutrosBancos:
                        GerarTipoPagamentoTituloTerceiro(ref parameters, documento);
                        break;

                    #region Tipos de Pagamentos não implementados

                    // 05 CREDITO EM CONTA REAL TIME
                    //parameters[0264] = 05;  // Modalidade de Pagamento
                    //parameters[0136] = Empty; // Informações Complementares

                    case TipoPagamentoEnum.PagamentoContasTributosComCodigoBarras:
                    case TipoPagamentoEnum.DARFNormal:
                    case TipoPagamentoEnum.GPS:
                    case TipoPagamentoEnum.DARFSimples:
                    case TipoPagamentoEnum.Caixa:
                    case TipoPagamentoEnum.GareSpIcms:
                    case TipoPagamentoEnum.GareSpDr:
                    case TipoPagamentoEnum.GareSpItcmd:
                    case TipoPagamentoEnum.IpvaSp:
                    case TipoPagamentoEnum.LicenciamentoSp:
                    case TipoPagamentoEnum.DpvatSp:
                    case TipoPagamentoEnum.Cheque:
                    default:
                        break;

                        #endregion Tipos de Pagamentos não implementados
                }

                TRegistroEDI reg = new TRegistroEDI();
                // TIPO | Posição | Tamanho | C. Decimais | Valor | Preenchimento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 001, 0, 1, '0');                    // Identificação do registro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0002, 001, 0, parameters[0002], '0');     // Tipo de Inscrição do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0003, 009, 0, parameters[0003], '0');     // CNPJ/CPF - Número da Inscrição
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0012, 004, 0, parameters[0012], '0');     // CNPJ/CPF - Filial
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, parameters[0016], '0');     // CNPJ/CPF - Controle
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0018, 030, 0, parameters[0018], '0');     // Nome do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0048, 040, 0, parameters[0048], ' ');     // Endereço do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0088, 005, 0, parameters[0088], '0');     // Número do CEP
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0093, 003, 0, parameters[0093], '0');     // Sufixo do CEP
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0096, 003, 0, parameters[0096], '0');     // Código do Banco do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0099, 005, 0, parameters[0099], '0');     // Código da Agência
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0104, 001, 0, parameters[0104], ' ');     // Dígito da Agência
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0105, 013, 0, parameters[0105], '0');     // Conta Corrente
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 002, 0, parameters[0118], ' ');     // Dígito da Conta Corrente
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0120, 016, 0, parameters[0120], ' ');     // Número do Pagamento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0136, 003, 0, parameters[0136], '0');     // Carteira
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0139, 012, 0, parameters[0139], '0');     // Nosso número
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0151, 015, 0, parameters[0151], ' ');     // Seu Número
                reg.Adicionar(TTiposDadoEDI.ediDataAAAAMMDD_________, 0166, 008, 0, parameters[0166], '0');     // Data de vencimento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0174, 008, 0, 0, '0');                    // Data de Emissão do Documento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0182, 008, 0, 0, '0');                    // Data Limite para Desconto
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0190, 001, 0, 0, '0');                    // Fixos zeros
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0191, 004, 0, parameters[0191], '0');     // Fator de Vencimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0195, 010, 2, parameters[0195], '0');     // Valor do Documento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0205, 015, 2, parameters[0205], '0');     // Valor do Pagamento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0220, 015, 2, parameters[0220], '0');     // Valor do Desconto
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0235, 015, 2, parameters[0235], '0');     // Valor do Acréscimo
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0250, 002, 0, parameters[0250], '0');     // Tipo de Documento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0252, 010, 0, 0, '0');                    // Número Nota Fiscal/Fatura/Duplicata
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0262, 002, 0, Empty, ' ');                // Série Documento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0264, 002, 0, parameters[0264], '0');     // Modalidade de Pagamento
                reg.Adicionar(TTiposDadoEDI.ediDataAAAAMMDD_________, 0266, 008, 0, parameters[0266], '0');     // Data para efetivação do pagamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0274, 003, 0, Empty, ' ');                // Moeda
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0277, 002, 0, "01", '0');                 // Situação do Agendamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0279, 002, 0, Empty, ' ');                // Informação de retorno 1
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0281, 002, 0, Empty, ' ');                // Informação de retorno 2
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0283, 002, 0, Empty, ' ');                // Informação de retorno 3
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0285, 002, 0, Empty, ' ');                // Informação de retorno 4
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0287, 002, 0, Empty, ' ');                // Informação de retorno 5
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0289, 001, 0, parameters[0289], '0');     // Tipo de Movimento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0290, 002, 0, parameters[0290], '0');     // Código do Movimento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0292, 004, 0, Empty, '0');                // Horário para consulta de saldo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0296, 015, 0, Empty, ' ');                // Saldo disponível no momento da consulta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0311, 015, 0, Empty, ' ');                // Valor da taxa pré funding
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0326, 006, 0, Empty, ' ');                // Reserva
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0332, 040, 0, Empty, ' ');                // Sacador/Avalista
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0372, 001, 0, Empty, ' ');                // Reserva
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0373, 001, 0, Empty, ' ');                // Nível da informação de Retorno
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0374, 040, 0, parameters[0374], ' ');     // Informações Complementares
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0414, 002, 0, 0, '0');                    // Código de área na empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0416, 035, 0, Empty, ' ');                // Campo para uso da empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0451, 022, 0, Empty, ' ');                // Reserva
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0473, 005, 0, 0, '0');                    // Código de Lançamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0478, 001, 0, Empty, ' ');                // Reserva
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0479, 001, 0, parameters[0479], '0');     // Tipo de Conta do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0480, 007, 0, parameters[0480], '0');     // Conta Complementar
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0487, 008, 0, Empty, ' ');                // Reserva
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0495, 006, 0, parameters[0495], '0');     // Número Sequencial do Registro

                reg.CodificarLinha();

                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar Segmento do lote no arquivo de remessa do 500POS.", ex);
            }
        } //TODO: verificar

        /// <summary>
        /// Geração do trailer do arquivo
        /// </summary>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        public string GerarTrailerRemessaPagamentoPOS500(int numeroRegistroGeral, decimal valorTotalRegistros)
        {
            try
            {
                numeroRegistroGeral++; //Numero geral de registros do arquivo incluindo Header, transações e o proprio trailler

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 001, 0, 9, '0');                    // Identificação do registro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0002, 006, 0, numeroRegistroGeral, '0');  // Quantidade de registros incluindo o trailler do arquivo
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0008, 017, 2, valorTotalRegistros, '0');  // Total dos valores de pagamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0025, 470, 0, Empty, ' ');                // Reserva
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0495, 006, 0, numeroRegistroGeral, '0');  // Número Sequencial do Registro
                reg.CodificarLinha();

                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar TRAILER do arquivo de remessa de 500 posições.", ex);
            }
        }

        /// <summary>
        /// 01 - CRÉDITO EM CONTA-CORRENTE OU POUPANÇA
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="documento"></param>
        private void GerarTipoPagamentoCreditoEmConta(ref Dictionary<int, object> parameters, Documento documento)
        {
            if (documento.Favorecido.ContaFinanceira == null)
            {
                throw new Exception("Favorecido " + documento.Favorecido.Nome + " sem conta financeira.");
            }

            parameters[0096] = 237;                                                     // Código do Banco do Fornecedor
            parameters[0099] = documento.Favorecido.ContaFinanceira.Agência;            // Código da Agência
            parameters[0104] = documento.Favorecido.ContaFinanceira.DígitoAgência;      // Dígito da Agência
            parameters[0105] = documento.Favorecido.ContaFinanceira.Conta;              // Conta Corrente
            parameters[0118] = documento.Favorecido.ContaFinanceira.DígitoConta;        // Dígito da Conta Corrente
            parameters[0139] = 0;                                                       // Nosso número
            parameters[0151] = Empty;                                                   // Seu Número
            parameters[0264] = 01;                                                      // Modalidade de Pagamento
            parameters[0136] = Empty;                                                   // Carteira
            parameters[0191] = Empty;                                                   // Fator de Vencimento

            switch (documento.Favorecido.ContaFinanceira.TipoConta)
            {
                case TipoContaEnum.ContaCorrenteIndividual:
                case TipoContaEnum.ContaCorrenteConjunta:
                case TipoContaEnum.ContaDepositoJudicialIndividual:
                case TipoContaEnum.ContaDepositoJudicialConjunta:
                default:
                    parameters[0479] = 1;
                    break;

                case TipoContaEnum.ContaPoupançaIndividual:
                case TipoContaEnum.ContaPoupançaConjunta:
                    parameters[0479] = 2;
                    break;
            }

            parameters[0374] = Empty; //Informações Complementares

            parameters[0480] = documento.Favorecido.ContaFinanceira.ContaComplementar; // Conta complementar
        }

        /// <summary>
        /// 02 - CHEQUE OP (ORDEM DE PAGAMENTO)
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="documento"></param>
        private void GerarTipoPagamentoCheque(ref Dictionary<int, object> parameters, Documento documento)
        {
            if (documento.Favorecido.ContaFinanceira == null)
            {
                throw new Exception("Favorecido " + documento.Favorecido.Nome + " sem conta financeira.");
            }

            parameters[0096] = 237;                                                 // Código do Banco do Fornecedor
            parameters[0099] = documento.Favorecido.ContaFinanceira.Agência;        // Código da Agência
            parameters[0104] = documento.Favorecido.ContaFinanceira.DígitoAgência;  // Dígito da Agência
            parameters[0105] = documento.Favorecido.ContaFinanceira.Conta;          // Conta Corrente
            parameters[0118] = documento.Favorecido.ContaFinanceira.DígitoConta;    // Dígito da Conta Corrente
            parameters[0139] = 0;                                                   // Nosso número
            parameters[0151] = Empty;                                               // Seu Número
            parameters[0264] = 02;                                                  // Modalidade de Pagamento
            parameters[0479] = Empty;                                               // Tipo de Conta do Fornecedor
            parameters[0136] = Empty;                                               // Carteira
            parameters[0191] = Empty;                                               // Fator de Vencimento
            parameters[0374] = Empty;                                               //Informações Complementares

            parameters[0480] = documento.Favorecido.ContaFinanceira.ContaComplementar; // Conta complementar

        }

        /// <summary>
        /// 03 - DOC COMPE / 
        /// 08 - TED
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="documento"></param>
        private void GerarTipoPagamentoDocTed(ref Dictionary<int, object> parameters, Documento documento)
        {
            if (documento.Favorecido.ContaFinanceira == null)
            {
                throw new Exception("Favorecido " + documento.Favorecido.Nome + " sem conta financeira.");
            }

            if (documento.TipoDePagamento == TipoPagamentoEnum.Doc)
            {
                parameters[0264] = 03;  // Modalidade de Pagamento
            }
            else if (documento.TipoDePagamento == TipoPagamentoEnum.Ted)
            {
                parameters[0264] = 08;  // Modalidade de Pagamento
            }

            parameters[0096] = documento.Favorecido.ContaFinanceira.Banco;          // Código do Banco do Fornecedor
            parameters[0099] = documento.Favorecido.ContaFinanceira.Agência;        // Código da Agência
            parameters[0104] = documento.Favorecido.ContaFinanceira.DígitoAgência;  // Dígito da Agência
            parameters[0105] = documento.Favorecido.ContaFinanceira.Conta;          // Conta Corrente
            parameters[0118] = documento.Favorecido.ContaFinanceira.DígitoConta;    // Dígito da Conta Corrente
            parameters[0139] = 0;                                                   // Nosso número
            parameters[0151] = Empty;                                               // Seu Número
            parameters[0136] = Empty;                                               // Carteira
            parameters[0191] = Empty;                                               // Fator de Vencimento


            #region Informações Complementares

            switch (documento.Favorecido.ContaFinanceira.TipoTitularidade) // Tipo do DOC/TED
            {
                case TitularidadeEnum.MesmaTitularidade:
                    parameters[0374] = "D";
                    break;

                case TitularidadeEnum.TitularidadeDiferente:
                default:
                    parameters[0374] = "C";
                    break;
            }

            parameters[0375] = 000000; // Número do DOC/TED

            if (documento is Transferencia)
            {
                Transferencia transferência = documento as Transferencia;

                parameters[0381] = ((int)transferência.FinalidadeDocTed).ToString("00"); // Finalidade do DOC/TED
            }

            parameters[0383] = ((int)documento.Favorecido.ContaFinanceira.TipoConta).ToString("00"); // Tipo de Conta

            // ISSO AQUI ESTÁ MEGA CONFUSO NO LAYOUT, ENTÃO IREMOS PASSAR TUDO VAZIO
            parameters[0385] = new string('0', 29); // Código Identificador de Depósito Judicial E Código Identificador de Transferência

            parameters[0374] = String.Concat(parameters[0374], parameters[0375], parameters[0381], parameters[0383], parameters[0385]);

            #endregion Informações Complementares

            // Tipo de Conta do Fornecedor - Apenas usado para a modalidade 1
            parameters[0479] = Empty;

            parameters[0480] = documento.Favorecido.ContaFinanceira.ContaComplementar; // Conta complementar
        }

        /// <summary>
        /// 30 - RASTREAMENTO DE TÍTULOS
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="documento"></param>
        private void GerarTipoPagamentoRastreamentoDeTitulos(ref Dictionary<int, object> parameters, Documento documento)
        {
            DecomporCodigodeBarrasOuLinhaDigitavel(
                                 documento.CódigoDeBarras ?? Empty,
                                 out string banco,
                                 out string carteira,
                                 out string fatorDeVencimento,
                                 out string campoLivre,
                                 out string digitoVerificador,
                                 out string codigoMoeda,
                                 out string agenciaFavorecido,
                                 out string codigoAgencia,
                                 out string contaFavorecido,
                                 out string codigoConta,
                                 out string nossoNumero);

            parameters[0096] = banco;                                               // Código do Banco do Fornecedor
            parameters[0099] = agenciaFavorecido;                                   // Código da Agência
            parameters[0104] = codigoAgencia;                                       // Dígito da Agência
            parameters[0105] = contaFavorecido;                                     // Conta Corrente
            parameters[0118] = codigoConta;                                         // Dígito da Conta Corrente
            parameters[0139] = nossoNumero;                                         // Nosso número
            parameters[0151] = documento.NúmeroDocumentoCliente;                    // Seu Número
            parameters[0264] = 30;                                                  // Modalidade de Pagamento
            parameters[0136] = carteira;                                            // Carteira
            parameters[0191] = fatorDeVencimento;                                   // Fator de Vencimento


            if (documento.TipoDeMovimento == TipoMovimentoEnum.Inclusao)
            {
                parameters[0289] = (int)TipoMovimentoEnum.Alteracao;                // Tipo de Movimento
            }

            parameters[0374] = String.Concat(new string(' ', 25), new string('0', 15)); // Informações Complementares

            // Tipo de Conta do Fornecedor - Apenas usado para a modalidade 1
            parameters[0479] = Empty;

            parameters[0480] = documento.Favorecido.ContaFinanceira.ContaComplementar; // Conta complementar
        }

        /// <summary>
        /// 31 - TÍTULOS DE TERCEIROS
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="documento"></param>
        private void GerarTipoPagamentoTituloTerceiro(ref Dictionary<int, object> parameters, Documento documento)
        {
            DecomporCodigodeBarrasOuLinhaDigitavel(
                                 documento.CódigoDeBarras ?? Empty,
                                 out string banco,
                                 out string carteira,
                                 out string fatorDeVencimento,
                                 out string campoLivre,
                                 out string digitoVerificador,
                                 out string codigoMoeda,
                                 out string agenciaFavorecido,
                                 out string codigoAgencia,
                                 out string contaFavorecido,
                                 out string codigoConta,
                                 out string nossoNumero);

            if (banco.Equals("237")) //Se for bradesco
            {
                parameters[0096] = banco;                                               // Código do Banco do Fornecedor
                parameters[0099] = agenciaFavorecido;                                   // Código da Agência
                parameters[0104] = codigoAgencia;                                       // Dígito da Agência
                parameters[0105] = contaFavorecido;                                     // Conta Corrente
                parameters[0118] = codigoConta;                                         // Dígito da Conta Corrente
                parameters[0139] = nossoNumero;                                         // Nosso número
            }
            else //Se for qualquer outro banco
            {
                parameters[0096] = banco;                                               // Código do Banco do Fornecedor
                parameters[0099] = 0;                                                   // Código da Agência
                parameters[0104] = 0;                                                   // Dígito da Agência
                parameters[0105] = 0;                                                   // Conta Corrente
                parameters[0118] = 0;                                                   // Dígito da Conta Corrente
                parameters[0139] = 0;                                                   // Nosso número
            }

            parameters[0151] = Empty;                                                   // Seu Número
            parameters[0264] = 31;                                                      // Modalidade de Pagamento
            parameters[0136] = carteira;                                                // Carteira
            parameters[0191] = fatorDeVencimento;                                       // Fator de Vencimento

            if (documento is Titulo)
            {
                parameters[0374] = String.Concat(campoLivre, digitoVerificador, codigoMoeda, new string(' ', 13)); // Informações Complementares
            }

            // Tipo de Conta do Fornecedor - Apenas usado para a modalidade 1
            parameters[0479] = Empty;

            parameters[0480] = documento.Favorecido.ContaFinanceira.ContaComplementar; // Conta complementar
        }

        #endregion Geração do arquivo


        #endregion IRemessaPagamento

        #region IRetornoPagamento

        public void LerHeaderRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {
                switch (tipoArquivo)
                {
                    case TipoArquivo.POS500:

                        LerHeaderRetornoPagamentoPOS500(ref pagamento, registro);
                        break;

                    default:
                        throw new Exception("Bradesco - Header - Tipo de arquivo inexistente.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a leitura do registro HEADER do arquivo de RETORNO PAGAMENTO.", ex);
            }
        }

        public void LerHeaderLoteRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            //Banco Bradesco nao implementa Header de lote
        }

        public void LerDetalheRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            try
            {
                switch (tipoArquivo)
                {
                    case TipoArquivo.POS500:

                        LerDetalheRetornoPagamentoPOS500(ref pagamento, registro);
                        break;

                    default:
                        throw new Exception("Bradesco - Header - Tipo de arquivo inexistente.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro durante a leitura do registro DETALHE do arquivo de RETORNO PAGAMENTO.", ex);
            }
        }

        public void LerTrailerLoteRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            //Banco Bradesco nao implementa Trailer de lote
        }

        public void LerTrailerRetornoPagamento(TipoArquivo tipoArquivo, ref Pagamento pagamento, string registro, ref TipoServiçoEnum? tipoDeServiço)
        {
            //Banco Bradesco nao implementa Trailer de Arquivo
        }


        private void LerHeaderRetornoPagamentoPOS500(ref Pagamento pagamento, string registro)
        {
            try
            {
                string tipoDocumento = registro.Substring(9, 1);

                pagamento.Pagador = new Pagador
                {
                    // Código do Convenio no Banco 002 A 009(8)
                    CódigoConvênio = registro.Substring(1, 8),

                    // Tipo de Inscrição da Empresa 010 A 010(01) / 1 = CPF 2 = CNPJ 3 = OUTROS
                    // Número Inscrição da Empresa 011 A 025 (15) CNPJ/CPF
                    NúmeroCadastro = tipoDocumento.Equals("1") ? registro.Substring(14, 11) : registro.Substring(11, 14),

                    // Nome da Empresa 026 A 065 (040) Obrigatório
                    Nome = registro.Substring(25, 40),

                    ContaFinanceira = new ContaFinanceira()
                    {
                        // Banco do Pagador 110 a 112(3)
                        Banco = registro.Substring(109, 3),

                        // Agência Mantenedora da Conta 113 A 117(5)
                        Agência = registro.Substring(112, 5),

                        // Dígito Verificador da Agência 118 A 118(1)
                        DígitoAgência = registro.Substring(117, 1),

                        // Número da Conta Corrente 119 A 131(13)
                        Conta = registro.Substring(118, 13),

                        // Dígito Verificador da Conta 132 A 134(2)
                        DígitoConta = registro.Substring(131, 2)
                    },
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler Header do arquivo de RETORNO / POS 500", ex);
            }
        }

        private void LerDetalheRetornoPagamentoPOS500(ref Pagamento pagamento, string registro)
        {
            try
            {
                TipoPagamentoEnum modalidadePagamento = (TipoPagamentoEnum)Enum.Parse(typeof(TipoPagamentoEnum), registro.Substring(263, 2));

                //Cria o documento
                switch (modalidadePagamento)
                {
                    case TipoPagamentoEnum.CreditoContaCorrente:
                    case TipoPagamentoEnum.CreditoContaPoupanca:
                    case TipoPagamentoEnum.Doc:
                    case TipoPagamentoEnum.Ted:
                    case TipoPagamentoEnum.OrdemPagamento:
                        GerarDocumentoTransferencia(ref pagamento, registro);
                        break;

                    case TipoPagamentoEnum.LiquidacaoTitulosMesmoBanco:
                    case TipoPagamentoEnum.LiquidacaoTitulosOutrosBancos:
                        GerarDocumentoTitulo(ref pagamento, registro);
                        break;

                    #region Tipos de Pagamentos não implementados

                    case TipoPagamentoEnum.PagamentoContasTributosComCodigoBarras:
                    case TipoPagamentoEnum.DARFNormal:
                    case TipoPagamentoEnum.GPS:
                    case TipoPagamentoEnum.DARFSimples:
                    case TipoPagamentoEnum.Caixa:
                    case TipoPagamentoEnum.GareSpIcms:
                    case TipoPagamentoEnum.GareSpDr:
                    case TipoPagamentoEnum.GareSpItcmd:
                    case TipoPagamentoEnum.IpvaSp:
                    case TipoPagamentoEnum.LicenciamentoSp:
                    case TipoPagamentoEnum.DpvatSp:
                    case TipoPagamentoEnum.Cheque:
                    default:
                        break;

                        #endregion Tipos de Pagamentos não implementados
                }

            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / POS 500", ex);
            }
        }


        private void GerarDocumentoTitulo(ref Pagamento pagamento, string registro)
        {
            Titulo titulo = new Titulo();

            if (titulo == null)
                throw new Exception("Documento não identificado");

            // Tipo de Inscrição do Favorecido 002 A 002(001)
            string tipoDocumento = registro.Substring(1, 1);

            //Tipo de conta do favorecido 479 A 479(001)
            string tipoConta = registro.Substring(478, 1);

            titulo.Favorecido = new Favorecido()
            {
                // Nome do Favorecido 018 A 047(030)
                Nome = registro.Substring(17, 30),

                // CNPJ/CPF do Favorecido 003 017(015) CPF/CNPJ
                NúmeroCadastro = tipoDocumento.Equals("1") ? registro.Substring(2, 9) + registro.Substring(15, 2) : registro.Substring(2, 9) + registro.Substring(11, 4) + registro.Substring(15, 2),

                Endereço = new Endereco()
                {
                    // Logradouro do Favorecido 48 87(040) Opcional
                    Rua = registro.Substring(47, 40),

                    // CEP do Favorecido 87 A 92(005) Opcional
                    // CEP Complemento 92 A 95(003) Opcional
                    CEP = registro.Substring(87, 8),
                },

                ContaFinanceira = new ContaFinanceira()
                {
                    // Código do Banco Favorecido 096 A 098(003) Obrigatório
                    Banco = registro.Substring(95, 3),

                    // Código da Agência Favorecido 099 A 103 (005)
                    Agência = registro.Substring(98, 5),

                    // Dígito Verificador da Agência 104 A 104 (001)
                    DígitoAgência = registro.Substring(103, 1),

                    // Conta Corrente do Favorecido 105 A 117(013)
                    Conta = registro.Substring(104, 13),

                    // Dígito Verificador da Conta 118 A 119(002)
                    DígitoConta = registro.Substring(117, 2),

                    //Tipo de conta
                    TipoConta = tipoConta.Equals("1") ? TipoContaEnum.ContaCorrenteIndividual : TipoContaEnum.ContaPoupançaIndividual,

                    // Conta complementar 480 A 486(007)
                    ContaComplementar = registro.Substring(479, 7)
                }

            };

            // Número do Pagamento 120 A 135 (016)
            titulo.NúmeroDocumentoCliente = registro.Substring(119, 16);

            // Nro. do Documento Banco 139 A 150(012)
            titulo.NúmeroDocumentoBanco = registro.Substring(138, 12);

            // Data de Vencimento 166 A 173(008) DDMMAAAA
            titulo.DataDoVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(165, 8)).ToString("##-##-####"));

            // Valor do Documento 195 A 204 (010) Opcional
            titulo.ValorDoDocumento = Convert.ToDecimal(registro.Substring(194, 10));

            // Valor Real do Pagamento 205 A 219(015)Opcional
            titulo.ValorDoPagamento = Convert.ToDecimal(registro.Substring(204, 15));

            // Valor do Desconto 220 A 234(015) Opcional
            titulo.ValorDoDesconto = Convert.ToDecimal(registro.Substring(219, 15));

            // Valor da Mora 235 A 249(015) Opcional
            titulo.ValorDaMora = Convert.ToDecimal(registro.Substring(234, 15));

            // Modalidade de pagamento 264 A 265(002)
            titulo.TipoDePagamento = (TipoPagamentoEnum)Enum.Parse(typeof(TipoPagamentoEnum), registro.Substring(263, 2));

            // Data para efetivação do pagamento (Retorno) 266 A 273(008) DDMMAAAA
            titulo.DataDoPagamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(265, 8)).ToString("##-##-####"));

            // Tipo de Movimento 289 A 289(001)
            titulo.TipoDeMovimento = (TipoMovimentoEnum)Enum.Parse(typeof(TipoMovimentoEnum), registro.Substring(288, 1));

            // Código da Instrução para Movimento 290 A 291(002)
            titulo.CódigoDaInstruçãoParaMovimento = (InstruçãoMovimentoEnum)Enum.Parse(typeof(InstruçãoMovimentoEnum), registro.Substring(289, 2));

            // Ocorrências para o Retorno 279 288(010)
            var occurrences = registro.Substring(278, 10);

            titulo.OcorrênciasParaRetorno = new List<Ocorrencia>();

            if (!IsNullOrWhiteSpace(occurrences.Trim()))
            {
                for (int i = 0; i < 10; i += 2)
                {
                    if (!IsNullOrWhiteSpace(occurrences.Substring(i, 2)))
                    {
                        titulo.OcorrênciasParaRetorno.Add(new Ocorrencia(occurrences.Substring(i, 2), new OcorrênciasBradesco()));
                    }
                }
            }

            pagamento.Documentos.Add(titulo);
        }

        private void GerarDocumentoTransferencia(ref Pagamento pagamento, string registro)
        {
            Transferencia transferencia = new Transferencia();

            if (transferencia == null)
                throw new Exception("Documento não identificado");

            // Tipo de Inscrição do Favorecido 002 A 002(001)
            string tipoIncricao = registro.Substring(1, 1);

            //Tipo de conta do favorecido 479 A 479(001)
            string accountType = registro.Substring(478, 1);

            transferencia.Favorecido = new Favorecido()
            {
                // Nome do Favorecido 018 A 047(030)
                Nome = registro.Substring(17, 30),

                // CNPJ/CPF do Favorecido 003 017(015) CPF/CNPJ
                NúmeroCadastro = tipoIncricao.Equals("1") ? registro.Substring(2, 9) + registro.Substring(15, 2) : registro.Substring(2, 9) + registro.Substring(11, 4) + registro.Substring(15, 2),

                Endereço = new Endereco()
                {
                    // Logradouro do Favorecido 48 87(040) Opcional
                    Rua = registro.Substring(47, 40),

                    // CEP do Favorecido 87 A 92(005) Opcional
                    // CEP Complemento 92 A 95(003) Opcional
                    CEP = registro.Substring(87, 8)
                },

                ContaFinanceira = new ContaFinanceira()
                {
                    // Código do Banco Favorecido 096 A 098(003) Obrigatório
                    Banco = registro.Substring(95, 3),

                    // Código da Agência Favorecido 099 A 103 (005)
                    Agência = registro.Substring(98, 5),

                    // Dígito Verificador da Agência 104 A 104 (001)
                    DígitoAgência = registro.Substring(103, 1),

                    // Conta Corrente do Favorecido 105 A 117(013)
                    Conta = registro.Substring(104, 13),

                    // Dígito Verificador da Conta 118 A 119(002)
                    DígitoConta = registro.Substring(117, 2),

                    //Tipo de conta
                    TipoConta = accountType.Equals("1") ? TipoContaEnum.ContaCorrenteIndividual : TipoContaEnum.ContaPoupançaIndividual,

                    // Conta complementar 480 A 486(007)
                    ContaComplementar = registro.Substring(479, 7)
                }

            };

            // Número do Pagamento 120 A 135 (016)
            transferencia.NúmeroDocumentoCliente = registro.Substring(119, 16);

            // Nro. do Documento Banco 139 A 150(012)
            transferencia.NúmeroDocumentoBanco = registro.Substring(138, 12);

            // Data de Vencimento 166 A 173(008) DDMMAAAA
            transferencia.DataDoVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(165, 8)).ToString("##-##-####"));

            // Valor do Documento 195 A 204 (010) Opcional
            transferencia.ValorDoDocumento = Convert.ToDecimal(registro.Substring(194, 10)) / 100;

            // Valor Real do Pagamento 205 A 219(015)Opcional
            transferencia.ValorDoPagamento = Convert.ToDecimal(registro.Substring(204, 15)) / 100;

            // Valor do Desconto 220 A 234(015) Opcional
            transferencia.ValorDoDesconto = Convert.ToDecimal(registro.Substring(219, 15));

            // Valor da Mora 235 A 249(015) Opcional
            transferencia.ValorDaMora = Convert.ToDecimal(registro.Substring(234, 15));

            // Modalidade de pagamento 264 A 265(002)
            transferencia.TipoDePagamento = (TipoPagamentoEnum)Enum.Parse(typeof(TipoPagamentoEnum), registro.Substring(263, 2));

            // Data para efetivação do pagamento (Retorno) 266 A 273(008) DDMMAAAA
            transferencia.DataDoPagamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(265, 8)).ToString("##-##-####"));

            // Tipo de Movimento 289 A 289(001)
            transferencia.TipoDeMovimento = (TipoMovimentoEnum)Enum.Parse(typeof(TipoMovimentoEnum), registro.Substring(288, 1));

            // Código da Instrução para Movimento 290 A 291(002)
            transferencia.CódigoDaInstruçãoParaMovimento = (InstruçãoMovimentoEnum)Enum.Parse(typeof(InstruçãoMovimentoEnum), registro.Substring(289, 2));


            // Tipo do DOC/TED 374 A 374(001)
            if (!IsNullOrWhiteSpace(registro.Substring(373, 1)))
            {
                transferencia.Favorecido.ContaFinanceira.TipoTitularidade = registro.Substring(373, 1).Equals("D") ? TitularidadeEnum.MesmaTitularidade : TitularidadeEnum.TitularidadeDiferente;
            }

            // Finalidade do DOC/TED 381 A 383(003)
            if (!IsNullOrWhiteSpace(registro.Substring(380, 3)))
            {
                transferencia.FinalidadeDocTed = (FinalidadeEnum)Enum.Parse(typeof(FinalidadeEnum), registro.Substring(380, 3));
            }

            // Ocorrências para o Retorno 279 288(010)
            var occurrences = registro.Substring(278, 10);

            transferencia.OcorrênciasParaRetorno = new List<Ocorrencia>();

            if (!IsNullOrWhiteSpace(occurrences.Trim()))
            {
                for (int i = 0; i < 10; i += 2)
                {
                    if (!IsNullOrWhiteSpace(occurrences.Substring(i, 2)))
                    {
                        transferencia.OcorrênciasParaRetorno.Add(new Ocorrencia(occurrences.Substring(i, 2), new OcorrênciasBradesco()));
                    }
                }
            }

            pagamento.Documentos.Add(transferencia);
        }

        #endregion IRetornoPagamento

        #region Utils

        /// <summary>
        /// Decompõe o código de barras ou a linha digtável nos campos necessários para o processo.
        /// </summary>
        /// <param name="cadeia">A cadeira de caracteres que compõe o Código de Barras ou a Linha Digitável</param>
        /// <param name="carteira">Carteira</param>
        /// <param name="fatorDeVencimento">Fator de vencimento</param>
        /// <param name="campoLivre">Campo livre do código de barras</param>
        /// <param name="digitoCodigoBarra">Dígito do código de barras</param>
        /// <param name="codigoMoeda">Código da Moeda</param>
        private void DecomporCodigodeBarrasOuLinhaDigitavel(
            string cadeia,
            out string banco,
            out string carteira,
            out string fatorDeVencimento,
            out string campoLivre,
            out string digitoVerificador,
            out string codigoMoeda,
            out string agenciaFavorecido,
            out string codigoAgencia,
            out string contaFavorecido,
            out string codigoConta,
            out string nossoNumero)
        {
            string barcode = new string(cadeia.Where(char.IsDigit).ToArray());
            banco = barcode.Substring(0, 3);
            codigoMoeda = barcode.Substring(3, 1);

            if (banco == "237") //Boleto do Bradesco
            {
                if (barcode.Length == 44) // Código de barras
                {
                    digitoVerificador = CalculoDigitoVerificarMódulo11(barcode);
                    fatorDeVencimento = barcode.Substring(5, 4);
                    campoLivre = barcode.Substring(19, 25);
                    carteira = barcode.Substring(23, 2);
                    agenciaFavorecido = barcode.Substring(19, 3);
                    codigoAgencia = barcode.Substring(22, 1);
                    contaFavorecido = barcode.Substring(36, 6);
                    codigoConta = barcode.Substring(42, 1);
                    nossoNumero = barcode.Substring(25, 11);
                }
                else if (barcode.Length == 47) // Linha digitável
                {
                    campoLivre = barcode.Substring(5, 34);
                    digitoVerificador = CalculoDigitoVerificarMódulo11(barcode);
                    fatorDeVencimento = barcode.Substring(33, 4);
                    carteira = barcode.Substring(8, 1) + barcode.Substring(10, 1);
                    agenciaFavorecido = barcode.Substring(4, 3);
                    codigoAgencia = barcode.Substring(7, 1);
                    contaFavorecido = barcode.Substring(24, 6);
                    codigoConta = barcode.Substring(30, 1);
                    nossoNumero = barcode.Substring(11, 9) + barcode.Substring(21, 2);
                }
                else
                {
                    throw new Exception($"Código de barras ou linha digitável em formato incorreto: {barcode}");
                }
            }
            else //Boleto de banco Diferente de bradesco
            {
                if (barcode.Length == 44) // Código de barras
                {
                    digitoVerificador = CalculoDigitoVerificarMódulo11(barcode);
                    fatorDeVencimento = barcode.Substring(5, 4);
                    campoLivre = barcode.Substring(19, 25);
                    carteira = Empty;
                    agenciaFavorecido = Empty;
                    codigoAgencia = Empty;
                    contaFavorecido = Empty;
                    codigoConta = Empty;
                    nossoNumero = Empty;
                }
                else if (barcode.Length == 47) // Linha digitável
                {
                    digitoVerificador = CalculoDigitoVerificarMódulo11(barcode);
                    fatorDeVencimento = barcode.Substring(33, 4);
                    campoLivre = barcode.Substring(4, 5) + barcode.Substring(10, 10) + barcode.Substring(21, 10);
                    carteira = Empty;
                    agenciaFavorecido = Empty;
                    codigoAgencia = Empty;
                    contaFavorecido = Empty;
                    codigoConta = Empty;
                    nossoNumero = Empty;
                }
                else
                {
                    throw new Exception($"Código de barras ou linha digitável em formato incorreto: {barcode}");
                }
            }
        }

        /// <summary>
        /// Cálculo do módulo 11
        /// </summary>
        /// <param name="chaveAcesso"></param>
        /// <returns></returns>
        public static string CalculoDigitoVerificarMódulo11(string chaveAcesso)
        {

            // O peso é o multiplicador da expressão, deve ser somente de 2 à 9, então já iniciamos com 2.
            int peso = 2;
            // Somatória do resultado.
            int soma = 0;

            //Dividendo da operação
            int dividendo = 11;

            try
            {
                // Passa número a número da chave pegando da direita pra esquerda (pra isso o Reverse()).
                var arrayReverse = chaveAcesso.ToCharArray()
                    .Reverse()
                    .ToList();

                foreach (var item in arrayReverse)
                {
                    // Acumula valores da soma gerada das multiplicações (peso).
                    soma += (Convert.ToInt32(item.ToString()) * peso);
                    // Como o peso pode ir somente até 9 é feito essa validação.
                    peso = (peso == 9) ? 2 : peso + 1;

                };

                var restoDivisao = soma % 11;
                var result = Empty;

                //Quando o resto da divisão for igual a 0(zero), 1 (um) ou maior que 9 (nove), 
                //o dígito do código de barras obrigatoriamente deverá ser igual a 1 (um).
                if (restoDivisao <= 1 || restoDivisao > 9)
                {
                    result = "1";
                }
                else
                {
                    //Quando o resto da divisão for diferente de 0(zero), 1(um) ou maior que 9(nove), 
                    //efetuar a subtração entre dividendo e o resto, cujo resultado será o dígito verificador do código de barras.
                    result = (dividendo - restoDivisao).ToString();
                }

                return result;
            }
            catch
            {
                return "ERRO: A chave de acesso deve conter apenas números.";
            }
        }
        #endregion Utils

    }
}