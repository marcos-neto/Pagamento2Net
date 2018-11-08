using Boleto2Net.Util;
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
    internal sealed partial class BancoBradesco : IRemessaPagamento
    {
        #region IRemessaPagamento

        // LAYOUT VERSÃO 15 (16/06/2017)

        public string GerarHeaderRemessaPagamento(TipoArquivo tipoArquivo, Pagador payee, int numeroArquivoRemessa, ref int numeroRegistro)
        {
            try
            {
                string header = Empty;
                switch (tipoArquivo)
                {
                    case TipoArquivo.POS500:

                        header += GerarHeaderRemessaPagamentoPOS500(payee, numeroArquivoRemessa, ref numeroRegistro);
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
                throw Pagamento2Net.Exceptions.ErroAoGerarRegistroHeaderDoArquivoRemessa(ex);
            }
        }

        public string GerarHeaderLoteRemessaPagamento(TipoArquivo tipoArquivo, Pagador pagador, TipoPagamentoEnum tipoPagamento, ref int loteServico, string tipoServico, PaymentTypeEnum formaLancamento, int numeroArquivoRemessa, ref int numeroRegistroGeral)
        {
            return Empty;
        }

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
                throw Boleto2NetException.ErroAoGerarRegistroHeaderLoteDoArquivoRemessa(ex);
            }
        }

        public string GerarTrailerLoteRemessaPagamento(TipoArquivo tipoArquivo, ref int numeroRegistroGeral, int loteServico, int numeroRegistros, decimal valorTotalRegistros)
        {
            return Empty;
        }

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
                throw new Exception("Erro durante a geração do Trailer do arquivo de PAGAMENTO.", ex);
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

                string docNumber = pagador.CPFCNPJOnlyNumbers(),
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
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0001, 001, 0, 0, '0');                            // Identificação do registro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0002, 008, 0, pagador.CódigoConvênio, '0');          // Código de Comunicação - Identificação da Empresa no Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0010, 001, 0, pagador.TipoCPFCNPJ("0"), '0');       // Tipo de Inscrição da Empresa Pagadora
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0011, 009, 0, inscrição, '0');                    // CNPJ/CPF - Número da Inscrição
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0020, 004, 0, filial, '0');                       // CNPJ/CPF - Filial
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0024, 002, 0, controle, '0');                     // CNPJ/CPF - Controle
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0026, 040, 0, pagador.Nome.Trim().ToUpper(), ' ');  // Nome da empresa pagadora
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0066, 002, 0, 20, '0');                           // Tipo de Serviço
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0068, 001, 0, 1, '0');                            // Código de origem do arquivo
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0069, 005, 0, numeroArquivoRemessa, '0');         // Número da remessa
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0074, 005, 0, 0, '0');                            // Número do retorno
                reg.Adicionar(TTiposDadoEDI.ediDataAAAAMMDD_________, 0079, 008, 0, DateTime.Now, '0');                 // Data da gravação do arquivo
                reg.Adicionar(TTiposDadoEDI.ediHoraHHMMSS___________, 0087, 006, 0, DateTime.Now, '0');                 // Hora da gravação do arquivo
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0093, 005, 0, Empty, ' ');                        // Densidade de gravação do arquivo/fita
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0098, 003, 0, Empty, ' ');                        // Unidade de densidade da gravação do arquivo/fita
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0101, 005, 0, Empty, ' ');                        // Identificação Módulo Micro
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0106, 001, 0, 0, '0');                            // Tipo de processamento
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0107, 074, 0, Empty, ' ');                        // Reservado - empresa
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0181, 080, 0, Empty, ' ');                        // Reservado - Banco
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0261, 217, 0, Empty, ' ');                        // Reservado - Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0478, 009, 0, 0, '0');                            // Número da Lista de Débito
                reg.Adicionar(TTiposDadoEDI.ediAlphaAliEsquerda_____, 0487, 008, 0, Empty, ' ');                        // Reservado - Banco
                reg.Adicionar(TTiposDadoEDI.ediInteiro______________, 0495, 006, 0, numeroRegistroGeral, '0');          // Número Sequencial do Registro
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
                    [0002] = documento.Favored.TipoCPFCNPJ("0") // Tipo de Inscrição do Fornecedor
                };

                string docNumber = documento.Favored.CPFCNPJOnlyNumbers();
                if (docNumber.Length == 11) //CPF
                {
                    parameters[0003] = docNumber.Substring(0, 9); // CNPJ/CPF - Número da Inscrição
                    parameters[0012] = "0000"; // CNPJ/CPF - Filial
                    parameters[0016] = docNumber.Substring(9, 2); // CNPJ/CPF - Controle
                }
                else if (docNumber.Length == 14) //CNPJ
                {
                    parameters[0003] = docNumber.Substring(0, 8); // CNPJ/CPF - Número da Inscrição
                    parameters[0012] = docNumber.Substring(8, 4); // CNPJ/CPF - Filial
                    parameters[0016] = docNumber.Substring(12, 2); // CNPJ/CPF - Controle
                }

                parameters[0018] = documento.Favored.Name; // Nome do Fornecedor
                parameters[0048] = documento.Favored.Address.FormataLogradouro(40); // Endereço do Fornecedor
                parameters[0088] = documento.Favored.Address.ZipCodePrefix(); // Número do CEP
                parameters[0093] = documento.Favored.Address.ZipCodeSufix(); // Sufixo do CEP
                parameters[0120] = documento.DocumentNumber; // Número do Pagamento
                parameters[0166] = documento.DueDate; // Data de vencimento
                parameters[0195] = documento.DocumentValue; // Valor do Documento
                parameters[0205] = documento.PaymentValue; // Valor do Pagamento
                parameters[0220] = documento.Discount; // Valor do Desconto
                parameters[0235] = documento.Interest + documento.LateFee; // Valor do Acréscimo
                parameters[0250] = 5; // Tipo de documento
                parameters[0266] = documento.PaymentDate; // Data para efetivação do pagamento
                parameters[0289] = (int)documento.MovementType;

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
                parameters[0480] = documento.Favored.FinancialAccount.ComplementaryAccount + documento.Favored.FinancialAccount.ComplementaryAccountDigit; // Conta complementar

                DecomporCodigodeBarrasOuLinhaDigitavel(
                    documento.Barcode,
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
                        parameters[0096] = 237; // Código do Banco do Fornecedor
                        parameters[0099] = documento.Favored.FinancialAccount.Agency; // Código da Agência
                        parameters[0104] = documento.Favored.FinancialAccount.AgencyDigit; // Dígito da Agência
                        parameters[0105] = documento.Favored.FinancialAccount.Account; // Conta Corrente
                        parameters[0118] = documento.Favored.FinancialAccount.AccountDigit; // Dígito da Conta Corrente
                        parameters[0136] = Empty; // Informações Complementares
                        parameters[0139] = 0; // Nosso número
                        parameters[0151] = Empty;  // Seu Número
                        parameters[0264] = 01;  // Modalidade de Pagamento

                        switch (documento.Favored.FinancialAccount.AccountType)
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
                        parameters[0096] = 237; // Código do Banco do Fornecedor
                        parameters[0099] = documento.Favored.FinancialAccount.Agency; // Código da Agência
                        parameters[0104] = documento.Favored.FinancialAccount.AgencyDigit; // Dígito da Agência
                        parameters[0105] = documento.Favored.FinancialAccount.Account; // Conta Corrente
                        parameters[0118] = documento.Favored.FinancialAccount.AccountDigit; // Dígito da Conta Corrente
                        parameters[0136] = Empty; // Informações Complementares
                        parameters[0139] = 0; // Nosso número
                        parameters[0151] = Empty;  // Seu Número
                        parameters[0264] = 02;  // Modalidade de Pagamento
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

                        parameters[0096] = documento.Favored.FinancialAccount.Bank; // Código do Banco do Fornecedor
                        parameters[0099] = documento.Favored.FinancialAccount.Agency; // Código da Agência
                        parameters[0104] = documento.Favored.FinancialAccount.AgencyDigit; // Dígito da Agência
                        parameters[0105] = documento.Favored.FinancialAccount.Account; // Conta Corrente
                        parameters[0118] = documento.Favored.FinancialAccount.AccountDigit; // Dígito da Conta Corrente
                        parameters[0139] = 0; // Nosso número
                        parameters[0151] = Empty;  // Seu Número

                        #region Informações Complementares

                        switch (documento.Favored.FinancialAccount.OwnershipType) // Tipo do DOC/TED
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
                        parameters[0381] = ((int)documento.Finality).ToString("00"); // Finalidade do DOC/TED
                        parameters[0383] = ((int)documento.Favored.FinancialAccount.AccountType).ToString("00"); // Tipo de Conta

                        // ISSO AQUI ESTÁ MEGA CONFUSO NO LAYOUT, ENTÃO IREMOS PASSAR TUDO VAZIO
                        parameters[0385] = new string('0', 29); // Código Identificador de Depósito Judicial E Código Identificador de Transferência

                        parameters[0374] = String.Concat(parameters[0374], parameters[0375], parameters[0383], parameters[0385]);

                        #endregion Informações Complementares

                        break;

                    #endregion 03 DOC COMPE && 08 TED

                    #region 30 RASTREAMENTO DE TÍTULOS

                    case TipoPagamentoEnum.LiquidacaoTitulosMesmoBanco:
                        parameters[0096] = 237; // Código do Banco do Fornecedor
                        parameters[0099] = documento.Favored.FinancialAccount.Agency; // Código da Agência
                        parameters[0104] = documento.Favored.FinancialAccount.AgencyDigit; // Dígito da Agência
                        parameters[0105] = documento.Favored.FinancialAccount.Account; // Conta Corrente
                        parameters[0118] = documento.Favored.FinancialAccount.AccountDigit; // Dígito da Conta Corrente
                        parameters[0139] = 0; // Nosso número
                        parameters[0151] = documento.DocumentNumber;  // Seu Número
                        parameters[0264] = 30;  // Modalidade de Pagamento
                        if (documento.TipoDeMovimento == TipoMovimentoEnum.Inclusao)
                        {
                            parameters[0289] = (int)TipoMovimentoEnum.Alteracao; // Tipo de Movimento
                        }

                        parameters[0374] = String.Concat(new string(' ', 25), new string('0', 15)); // Informações Complementares

                        break;

                    #endregion 30 RASTREAMENTO DE TÍTULOS

                    #region 31 TÍTULOS DE TERCEIROS

                    case TipoPagamentoEnum.LiquidacaoTitulosOutrosBancos:
                        if (documento.Favored.FinancialAccount.Bank.Equals("237"))
                        {
                            parameters[0096] = 237; // Código do Banco do Fornecedor
                            parameters[0099] = documento.Favored.FinancialAccount.Agency; // Código da Agência
                            parameters[0104] = documento.Favored.FinancialAccount.AgencyDigit; // Dígito da Agência
                            parameters[0105] = documento.Favored.FinancialAccount.Account; // Conta Corrente
                            parameters[0118] = documento.Favored.FinancialAccount.AccountDigit; // Dígito da Conta Corrente
                            parameters[0139] = documento.OurNumber; // Nosso número
                        }
                        else
                        {
                            parameters[0096] = documento.Favored.FinancialAccount.Bank; // Código do Banco do Fornecedor
                            parameters[0099] = 0; // Código da Agência
                            parameters[0104] = 0; // Dígito da Agência
                            parameters[0105] = 0; // Conta Corrente
                            parameters[0118] = 0; // Dígito da Conta Corrente
                            parameters[0139] = 0; // Nosso número
                        }

                        parameters[0151] = Empty; // Seu Número
                        parameters[0264] = 31; // Modalidade de Pagamento
                        parameters[0374] = String.Concat(campoLivre, digitoVerificador, codigoMoeda, new string('0', 13)); // Informações Complementares
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
        }

        /// <summary>
        /// Geração do trailer do arquivo
        /// </summary>
        /// <param name="numeroRegistroGeral"></param>
        /// <returns></returns>
        public string GerarTrailerRemessaPagamentoPOS500(int numeroRegistroGeral, int numeroLotes, decimal valorTotalRegistros)
        {
            try
            {
                // O número de registros no arquivo é igual ao número de registros gerados + 4 (header e trailler do lote / header e trailler do arquivo)
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
    }
}