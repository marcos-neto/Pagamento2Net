using Boleto2Net.Util;
using Pagamento2.Net.Entidades;
using Pagamento2.Net.Enums;
using Pagamento2Net.Entidades;
using Pagamento2Net.Enums;
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
        public string GerarHeaderRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, int numeroArquivoRemessa, ref int numeroRegistro)
        {
            try
            {
                string header = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.POS500:

                        header += GerarHeaderRemessaPagamentoPOS500(pagador, numeroArquivoRemessa, ref numeroRegistro);
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
        public string GerarHeaderLoteRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, TipoPagamentoEnum tipoPagamento, ref int loteServico, string tipoServico, int numeroArquivoRemessa, ref int numeroRegistroGeral, ref int numeroRegistrosLote)
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
                        strline = GerarDetalheRemessaPagamentoPOS500(documento, ref loteServico, ref numeroRegistroLote, ref numeroRegistroLote);
                        break;

                    default:
                        throw new Exception("Santander - Header Lote - Tipo de arquivo inexistente.");
                }
                if (String.IsNullOrWhiteSpace(strline))
                {
                    throw new Exception("Registro Segmento obrigatório.");
                }

                return Utils.FormataLinhaArquivoCNAB(strline, tipoArquivo);
            }
            catch (Exception ex)
            {
                throw Pagamento2NetException.ErroAoGerarRegistroHeaderLoteDoArquivoRemessa(ex);
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
        public string GerarTrailerLoteRemessaPagamento(TipoArquivo tipoArquivo, ref int numeroRegistroGeral, int loteServico, int numeroRegistros, decimal valorTotalRegistros)
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
        public string GerarTrailerRemessaPagamento(TipoArquivo tipoArquivo, int numeroRegistros, int numeroLotes, decimal valorTotalRegistros)
        {
            try
            {
                string trailer = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.POS500:
                        // Trailler do Arquivo
                        trailer = GerarTrailerRemessaPagamentoPOS500(numeroRegistros, numeroLotes, valorTotalRegistros);
                        break;

                    default:
                        throw new Exception("Santander - Trailler - Tipo de arquivo inexistente.");
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
                numeroRegistroGeral++;

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
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0107, 074, 0, Empty, ' ');                            // Reservado - empresa
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
        private string GerarDetalheRemessaPagamentoPOS500(Documento documento, ref int loteServico, ref int numeroRegistroLote, ref int numeroRegistroGeral)
        {
            try
            {
                numeroRegistroGeral++;

                Dictionary<int, object> parameters = new Dictionary<int, object>
                {
                    [0002] = documento.Favorecido.TipoNúmeroCadastro("0") // Tipo de Inscrição do Fornecedor
                };

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

                parameters[0018] = documento.Favorecido.Nome;                           // Nome do Fornecedor
                parameters[0048] = documento.Favorecido.Endereço.FormataLogradouro(40); // Endereço do Fornecedor
                parameters[0088] = documento.Favorecido.Endereço.PrefixoCEP();          // Número do CEP
                parameters[0093] = documento.Favorecido.Endereço.SufixoCEP();           // Sufixo do CEP
                parameters[0120] = documento.NúmeroDocumentoCliente;                    // Número do Pagamento
                parameters[0166] = documento.DataDoVencimento;                          // Data de vencimento
                parameters[0195] = documento.ValorDoDocumento;                          // Valor do Documento
                parameters[0205] = documento.ValorDoPagamento;                          // Valor do Pagamento
                parameters[0220] = documento.ValorDoDesconto;                           // Valor do Desconto
                parameters[0235] = documento.ValorDaMora + documento.ValorDaMulta;      // Valor do Acréscimo
                parameters[0250] = 5;                                                   // Tipo de documento
                parameters[0266] = documento.DataDoPagamento;                           // Data para efetivação do pagamento
                parameters[0289] = (int)documento.TipoDeMovimento;                      // Tipo de movimento

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
                parameters[0480] = documento.Favorecido.ContaFinanceira.ContaComplementar + documento.Favorecido.ContaFinanceira.TipoContaComplementar; // Conta complementar

                DecomporCodigodeBarrasOuLinhaDigitavel(
                     documento.CódigoDeBarras,
                     out string carteira,
                     out string banco,
                     out string fatorDeVencimento,
                     out string campoLivre,
                     out string digitoVerificador,
                     out string codigoMoeda);

                parameters[0136] = carteira; // Carteira
                parameters[0191] = fatorDeVencimento; // Fator de Vencimento


                switch (documento.TipoDePagamento)
                {
                    #region 01 CRÉDITO EM CONTA-CORRENTE OU POUPANÇA

                    case TipoPagamentoEnum.CreditoContaCorrente:
                    case TipoPagamentoEnum.CreditoContaPoupanca:
                        parameters[0096] = 237;                                                     // Código do Banco do Fornecedor
                        parameters[0099] = documento.Favorecido.ContaFinanceira.Agência;            // Código da Agência
                        parameters[0104] = documento.Favorecido.ContaFinanceira.DígitoAgência;      // Dígito da Agência
                        parameters[0105] = documento.Favorecido.ContaFinanceira.Conta;              // Conta Corrente
                        parameters[0118] = documento.Favorecido.ContaFinanceira.DígitoConta;        // Dígito da Conta Corrente
                        parameters[0136] = Empty;                                                   // Informações Complementares
                        parameters[0139] = 0;                                                       // Nosso número
                        parameters[0151] = Empty;                                                   // Seu Número
                        parameters[0264] = 01;                                                      // Modalidade de Pagamento

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

                        break;

                    #endregion 01 CRÉDITO EM CONTA-CORRENTE OU POUPANÇA

                    #region 02 CHEQUE OP (ORDEM DE PAGAMENTO)

                    case TipoPagamentoEnum.OrdemPagamento:
                        parameters[0096] = 237;                                                 // Código do Banco do Fornecedor
                        parameters[0099] = documento.Favorecido.ContaFinanceira.Agência;        // Código da Agência
                        parameters[0104] = documento.Favorecido.ContaFinanceira.DígitoAgência;  // Dígito da Agência
                        parameters[0105] = documento.Favorecido.ContaFinanceira.Conta;          // Conta Corrente
                        parameters[0118] = documento.Favorecido.ContaFinanceira.DígitoConta;    // Dígito da Conta Corrente
                        parameters[0136] = Empty;                                               // Informações Complementares
                        parameters[0139] = 0;                                                   // Nosso número
                        parameters[0151] = Empty;                                               // Seu Número
                        parameters[0264] = 02;                                                  // Modalidade de Pagamento
                        break;

                    #endregion 02 CHEQUE OP (ORDEM DE PAGAMENTO)

                    #region 03 DOC COMPE && 08 TED

                    case TipoPagamentoEnum.Doc:
                    case TipoPagamentoEnum.Ted:

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

                        if (documento is Transferência)
                        {
                            Transferência transferência = documento as Transferência;

                            parameters[0381] = ((int)transferência.FinalidadeDocTed).ToString("00"); // Finalidade do DOC/TED

                        }
                        parameters[0383] = ((int)documento.Favorecido.ContaFinanceira.TipoConta).ToString("00"); // Tipo de Conta

                        // ISSO AQUI ESTÁ MEGA CONFUSO NO LAYOUT, ENTÃO IREMOS PASSAR TUDO VAZIO
                        parameters[0385] = new string('0', 29); // Código Identificador de Depósito Judicial E Código Identificador de Transferência

                        parameters[0374] = String.Concat(parameters[0374], parameters[0375], parameters[0383], parameters[0385]);

                        #endregion Informações Complementares

                        break;

                    #endregion 03 DOC COMPE && 08 TED

                    #region 30 RASTREAMENTO DE TÍTULOS

                    case TipoPagamentoEnum.LiquidacaoTitulosMesmoBanco:
                        parameters[0096] = 237;                                                 // Código do Banco do Fornecedor
                        parameters[0099] = documento.Favorecido.ContaFinanceira.Agência;        // Código da Agência
                        parameters[0104] = documento.Favorecido.ContaFinanceira.DígitoAgência;  // Dígito da Agência
                        parameters[0105] = documento.Favorecido.ContaFinanceira.Conta;          // Conta Corrente
                        parameters[0118] = documento.Favorecido.ContaFinanceira.DígitoConta;    // Dígito da Conta Corrente
                        parameters[0139] = 0;                                                   // Nosso número
                        parameters[0151] = documento.NúmeroDocumentoCliente;                    // Seu Número
                        parameters[0264] = 30;                                                  // Modalidade de Pagamento

                        if (documento.TipoDeMovimento == TipoMovimentoEnum.Inclusao)
                        {
                            parameters[0289] = (int)TipoMovimentoEnum.Alteracao;                // Tipo de Movimento
                        }

                        parameters[0374] = String.Concat(new string(' ', 25), new string('0', 15)); // Informações Complementares

                        break;

                    #endregion 30 RASTREAMENTO DE TÍTULOS

                    #region 31 TÍTULOS DE TERCEIROS

                    case TipoPagamentoEnum.LiquidacaoTitulosOutrosBancos:
                        if (documento.Favorecido.ContaFinanceira.Banco.Equals("237"))
                        {
                            parameters[0096] = 237;                                                 // Código do Banco do Fornecedor
                            parameters[0099] = documento.Favorecido.ContaFinanceira.Agência;        // Código da Agência
                            parameters[0104] = documento.Favorecido.ContaFinanceira.DígitoAgência;  // Dígito da Agência
                            parameters[0105] = documento.Favorecido.ContaFinanceira.Conta;          // Conta Corrente
                            parameters[0118] = documento.Favorecido.ContaFinanceira.DígitoConta;    // Dígito da Conta Corrente
                            parameters[0139] = documento.NúmeroDocumentoBanco;                      // Nosso número
                        }
                        else
                        {
                            parameters[0096] = documento.Favorecido.ContaFinanceira.Banco;          // Código do Banco do Fornecedor
                            parameters[0099] = 0;                                                   // Código da Agência
                            parameters[0104] = 0;                                                   // Dígito da Agência
                            parameters[0105] = 0;                                                   // Conta Corrente
                            parameters[0118] = 0;                                                   // Dígito da Conta Corrente
                            parameters[0139] = 0;                                                   // Nosso número
                        }

                        parameters[0151] = Empty;                                                   // Seu Número
                        parameters[0264] = 31;                                                      // Modalidade de Pagamento

                        if (documento is Título)
                        {

                            parameters[0374] = String.Concat(campoLivre, digitoVerificador, codigoMoeda, new string('0', 13)); // Informações Complementares
                        }

                        break;

                    #endregion 31 TÍTULOS DE TERCEIROS

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

                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 001, 0, 1, '0');                // Identificação do registro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0002, 001, 0, parameters[0002], '0'); // Tipo de Inscrição do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0003, 009, 0, parameters[0003], '0'); // CNPJ/CPF - Número da Inscrição
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0012, 004, 0, parameters[0012], '0'); // CNPJ/CPF - Filial
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0016, 002, 0, parameters[0016], '0'); // CNPJ/CPF - Controle
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0018, 047, 0, parameters[0018], '0'); // Nome do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0048, 040, 0, parameters[0048], '0'); // Endereço do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0088, 005, 0, parameters[0088], '0'); // Número do CEP
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0093, 002, 0, parameters[0093], '0'); // Sufixo do CEP
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0096, 003, 0, parameters[0096], '0'); // Código do Banco do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0099, 005, 0, parameters[0099], '0'); // Código da Agência
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0104, 001, 0, parameters[0104], '0'); // Dígito da Agência
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0105, 013, 0, parameters[0105], '0'); // Conta Corrente
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0118, 002, 0, parameters[0118], '0'); // Dígito da Conta Corrente
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0120, 016, 0, parameters[0120], '0'); // Número do Pagamento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0136, 003, 0, parameters[0136], '0'); // Carteira
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0139, 012, 0, parameters[0139], '0'); // Nosso número
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0151, 015, 0, parameters[0151], '0'); // Seu Número
                reg.Adicionar(TTiposDadoEDI.ediDataAAAAMMDD_________, 0166, 008, 0, parameters[0166], '0'); // Data de vencimento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0174, 008, 0, 0, '0');                // Data de Emissão do Documento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0182, 008, 0, 0, '0');                // Data Limite para Desconto
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0190, 001, 0, 0, '0');                // Fixos zeros
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0191, 004, 0, parameters[0191], '0'); // Fator de Vencimento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0195, 010, 0, parameters[0195], '0'); // Valor do Documento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0205, 015, 0, parameters[0205], '0'); // Valor do Pagamento
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0220, 015, 0, parameters[0220], '0'); // Valor do Desconto
                reg.Adicionar(TTiposDadoEDI.ediNumericoSemSeparador_, 0235, 015, 0, parameters[0235], '0'); // Valor do Acréscimo
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0250, 002, 0, parameters[0250], '0'); // Tipo de Documento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0252, 010, 0, 0, '0');                // Número Nota Fiscal/Fatura/Duplicata
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0262, 002, 0, Empty, '0');            // Série Documento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0264, 002, 0, parameters[0264], '0'); // Modalidade de Pagamento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0266, 008, 0, parameters[0266], '0'); // Data para efetivação do pagamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0274, 003, 0, Empty, ' ');            // Moeda
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0277, 002, 0, "01", '0');             // Situação do Agendamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0279, 002, 0, Empty, ' ');            // Informação de retorno 1
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0279, 002, 0, Empty, ' ');            // Informação de retorno 2
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0279, 002, 0, Empty, ' ');            // Informação de retorno 3
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0279, 002, 0, Empty, ' ');            // Informação de retorno 4
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0279, 002, 0, Empty, ' ');            // Informação de retorno 5
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0289, 001, 0, parameters[0289], '0'); // Tipo de Movimento
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0290, 002, 0, parameters[0290], '0'); // Código do Movimento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0292, 004, 0, Empty, '0');            // Horário para consulta de saldo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0296, 015, 0, Empty, ' ');            // Saldo disponível no momento da consulta
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0311, 015, 0, Empty, ' ');            // Valor da taxa pré funding
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0326, 006, 0, Empty, ' ');            // Reserva
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0332, 040, 0, Empty, '0');            // Sacador/Avalista
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0372, 001, 0, Empty, ' ');            // Reserva
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0373, 001, 0, Empty, ' ');            // Nível da informação de Retorno
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0374, 040, 0, parameters[0374], ' '); // Informações Complementares
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0414, 002, 0, 0, '0');                // Código de área na empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0416, 035, 0, Empty, '0');            // Campo para uso da empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0451, 022, 0, Empty, ' ');            // Reserva
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0473, 005, 0, 0, '0');                // Código de Lançamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0478, 001, 0, Empty, ' ');            // Reserva
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0479, 001, 0, parameters[0479], '0'); // Tipo de Conta do Fornecedor
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0480, 007, 0, parameters[0480], '0'); // Conta Complementar
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0487, 008, 0, Empty, ' ');            // Reserva
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0495, 006, 0, parameters[0495], '0'); // Número Sequencial do Registro

                reg.CodificarLinha();

                return reg.LinhaRegistro;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao gerar Segmento A do lote no arquivo de remessa do CNAB240 SIGCB.", ex);
            }
        } //TODO: verificar

        /// <summary>
        /// Geração do trailer do arquivo
        /// </summary>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        public string GerarTrailerRemessaPagamentoPOS500(int numeroRegistroGeral, int numeroLotes, decimal valorTotalRegistros)
        {
            try
            {
                numeroRegistroGeral++;

                TRegistroEDI reg = new TRegistroEDI();
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 001, 0, 9, '0');                    // Identificação do registro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0002, 006, 0, numeroRegistroGeral, '0');  // Quantidade de registros
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0008, 017, 0, valorTotalRegistros, '0');  // Total dos valores de pagamento
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
            out string codigoMoeda)
        {
            string barcode = new string(cadeia.Where(char.IsDigit).ToArray());

            if (barcode.Length == 44) // Código de barras
            {
                banco = barcode.Substring(0, 3);
                codigoMoeda = barcode.Substring(3, 1);
                digitoVerificador = barcode.Substring(4, 1);
                fatorDeVencimento = barcode.Substring(5, 4);
                //string valor = barcode.Substring(9, 10);
                campoLivre = barcode.Substring(19, 25);
                carteira = banco == "237" ? barcode.Substring(23, 2) : Empty;
            }
            else if (barcode.Length == 47) // Linha digitável
            {
                banco = barcode.Substring(0, 3);
                codigoMoeda = barcode.Substring(3, 1);
                campoLivre = barcode.Substring(4, 28);
                digitoVerificador = barcode.Substring(32, 1);
                fatorDeVencimento = barcode.Substring(33, 4);
                //string valor = barcode.Substring(37,10);
                carteira = banco == "237" ? barcode.Substring(8, 1) + barcode.Substring(10, 1) : Empty;
            }
            else
            {
                throw new Exception($"Código de barras ou linha digitável em formato incorreto: {barcode}");
            }
        }

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

                        LerDetalheRetornoPagamentoPOS500(ref pagamento, registro, ref tipoDeServiço);
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
                pagamento.Pagador = new Pagador
                {
                    // Código do Convenio no Banco 002 A 009(8)
                    CódigoConvênio = registro.Substring(1, 8),

                    // Tipo de Inscrição da Empresa 010 A 010(01) / 1 = CPF 2 = CNPJ 3 = OUTROS
                    // Número Inscrição da Empresa 011 A 025 (15) CNPJ/CPF
                    NúmeroCadastro = registro.Substring(9, 1) == "1" ? registro.Substring(14, 11) : registro.Substring(11, 14),

                    // Nome da Empresa 026 A 065 (040) Obrigatório
                    Nome = registro.Substring(25, 40),

                    ContaFinanceira = new ContaFinanceira()
                    {
                        Banco = "237"
                    }
                };

                //// Agência Mantenedora da Conta 053 057 9(005)
                //paymentDocuments.Payee.FinancialAccount.Agency = registro.Substring(52, 5);
                //// Dígito Verificador da Agência 058 058 X(001) Branco
                //paymentDocuments.Payee.FinancialAccount.AgencyDigit = registro.Substring(57, 1);
                //// Número da Conta Corrente 059 070 9(012)
                //paymentDocuments.Payee.FinancialAccount.Account = registro.Substring(58, 12);
                //// Dígito Verificador da Conta 071 071 X(001)
                //paymentDocuments.Payee.FinancialAccount.AccountDigit = registro.Substring(70, 1);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler Header do arquivo de RETORNO / POS 500", ex);
            }
        }

        private void LerDetalheRetornoPagamentoPOS500(ref Pagamento pagamento, string registro, ref TipoServiçoEnum? currentServiceType)
        {
            try
            {
                //Cria o documento
                Transferência transferência = new Transferência();

                if (transferência == null)
                    throw new Exception("Objeto documento não identificado");

                #region Favored
                transferência.Favorecido = new Favorecido();

                // Tipo de Inscrição do Favorecido 002 A 002(001)
                var tipoIncricao = registro.Substring(1, 1);

                // CNPJ/CPF do Favorecido 003 017(015) CPF/CNPJ
                if (tipoIncricao.Equals("1")) //CPF
                    transferência.Favorecido.NúmeroCadastro = registro.Substring(2, 9) + registro.Substring(15, 2);
                else //CNPJ
                    transferência.Favorecido.NúmeroCadastro = registro.Substring(2, 9) + registro.Substring(11, 4) + registro.Substring(15, 2);

                // Nome do Favorecido 018 A 047(030)
                transferência.Favorecido.Nome = registro.Substring(17, 30);

                #region Address
                // Logradouro do Favorecido 48 87(040) Opcional
                transferência.Favorecido.Endereço.Rua = registro.Substring(47, 40);

                // CEP do Favorecido 87 A 92(005) Opcional
                // CEP Complemento 92 A 95(003) Opcional
                transferência.Favorecido.Endereço.CEP = registro.Substring(87, 8);
                #endregion

                #region FinancialAccount
                //Tipo de conta do favorecido 479 A 479(001)
                var accountType = registro.Substring(478, 1);

                transferência.Favorecido.ContaFinanceira = new ContaFinanceira
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
                };

                #endregion FinancialAccount

                #endregion Favored

                // Número do Pagamento 120 A 135 (016)
                transferência.NúmeroDocumentoCliente = registro.Substring(119, 16);

                // Nro. do Documento Banco 139 A 150(012)
                transferência.NúmeroDocumentoBanco = registro.Substring(138, 12);

                // Data de Vencimento 166 A 173(008) DDMMAAAA
                transferência.DataDoVencimento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(165, 8)).ToString("##-##-####"));

                // Valor do Documento 195 A 204 (010) Opcional
                transferência.ValorDoDocumento = Convert.ToDecimal(registro.Substring(194, 10)) / 100;

                // Valor Real do Pagamento 205 A 219(015)Opcional
                transferência.ValorDoPagamento = Convert.ToDecimal(registro.Substring(204, 15)) / 100;

                // Valor do Desconto 220 A 234(015) Opcional
                transferência.ValorDoDesconto = Convert.ToDecimal(registro.Substring(219, 15));

                // Valor da Mora 235 A 249(015) Opcional
                transferência.ValorDaMora = Convert.ToDecimal(registro.Substring(234, 15));

                // Modalidade de pagamento 264 A 265(002)
                transferência.TipoDePagamento = (TipoPagamentoEnum)Enum.Parse(typeof(TipoPagamentoEnum), registro.Substring(263, 2));

                // Data para efetivação do pagamento (Retorno) 266 A 273(008) DDMMAAAA
                transferência.DataDoPagamento = Utils.ToDateTime(Utils.ToInt32(registro.Substring(265, 8)).ToString("##-##-####"));

                // Tipo de Movimento 289 A 289(001)
                transferência.TipoDeMovimento = (TipoMovimentoEnum)Enum.Parse(typeof(TipoMovimentoEnum), registro.Substring(288, 1));

                // Código da Instrução para Movimento 290 A 291(002)
                transferência.CódigoDaInstruçãoParaMovimento = (InstruçãoMovimentoEnum)Enum.Parse(typeof(InstruçãoMovimentoEnum), registro.Substring(289, 2));



                // Tipo do DOC/TED 374 A 374(001)
                if (!IsNullOrWhiteSpace(registro.Substring(373, 1)))
                {
                    transferência.Favorecido.ContaFinanceira.TipoTitularidade = registro.Substring(373, 1).Equals("D") ? TitularidadeEnum.MesmaTitularidade : TitularidadeEnum.TitularidadeDiferente;
                }

                // Finalidade do DOC/TED 381 A 383(003)
                if (!IsNullOrWhiteSpace(registro.Substring(380, 3)))
                {
                    transferência.FinalidadeDocTed = (FinalidadeEnum)Enum.Parse(typeof(FinalidadeEnum), registro.Substring(380, 3));
                }

                // Ocorrências para o Retorno 279 288(010)
                var occurrences = registro.Substring(278, 10);

                transferência.OcorrênciasParaRetorno = new List<Ocorrência>();

                if (!IsNullOrWhiteSpace(occurrences.Trim()))
                {
                    for (int i = 0; i < 10; i += 2)
                    {
                        if (!IsNullOrWhiteSpace(occurrences.Substring(i, 2)))
                        {
                            transferência.OcorrênciasParaRetorno.Add(new Ocorrência(occurrences.Substring(i, 2), new OcorrênciasBradesco()));
                        }
                    }
                }

                transferência.TipoDeServiço = currentServiceType.Value;

                pagamento.Documentos.Add(transferência);
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao ler detalhe do arquivo de RETORNO / POS 500", ex);
            }
        }

        #endregion IRetornoPagamento

    }
}