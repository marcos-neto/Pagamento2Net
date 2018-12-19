using System.Collections.Generic;
using System.Text;

namespace Boleto2Net.Util
{
    /// <summary>
    /// Classe representativa de um registro (linha) de um arquivo EDI
    /// </summary>
    public class TRegistroEDI
    {
        #region Variáveis Privadas e Protegidas

        protected TTipoRegistroEDI _TipoRegistro;
        protected int _TamanhoMaximo = 0;
        protected char _CaracterPreenchimento = ' ';
        private string _LinhaRegistro;
        protected List<TCampoRegistroEDI> _CamposEDI = new List<TCampoRegistroEDI>();

        #endregion Variáveis Privadas e Protegidas

        #region Propriedades

        /// <summary>
        /// Tipo de Registro da linha do arquivo EDI
        /// </summary>
        public TTipoRegistroEDI TipoRegistro
        {
            get { return _TipoRegistro; }
        }

        /// <summary>
        /// Seta a linha do registro para a decodificação nos campos;
        /// Obtém a linha decodificada a partir dos campos.
        /// </summary>
        public string LinhaRegistro
        {
            get { return _LinhaRegistro; }
            set { _LinhaRegistro = value; }
        }

        /// <summary>
        /// Coleção dos campos do registro EDI
        /// </summary>
        public List<TCampoRegistroEDI> CamposEDI
        {
            get { return _CamposEDI; }
            set { _CamposEDI = value; }
        }

        #endregion Propriedades

        #region Métodos Públicos

        public void Adicionar(TTiposDadoEDI tipo, int posicao, int tamanho, int decimais, object valor, char prenchimento)
        {
            this.CamposEDI.Add(new TCampoRegistroEDI(tipo, posicao, tamanho, decimais, valor, prenchimento));
        }

        /// <summary>
        /// Codifica uma linha a partir dos campos; o resultado irá na propriedade LinhaRegistro
        /// </summary>
        public virtual void CodificarLinha()
        {
            int posição = 0;
            try
            {
                var builder = new StringBuilder();
                foreach (TCampoRegistroEDI campos in this._CamposEDI)
                {
                    posição = campos.PosicaoInicial;
                    campos.CodificarNaturalParaEDI();
                    builder.Append(campos.ValorFormatado);
                }
                this._LinhaRegistro = builder.ToString();
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Erro na posição: " + posição.ToString());
            }
        }

        /// <summary>
        /// Decodifica uma linha a partir da propriedade LinhaRegistro nos campos do registro
        /// </summary>
        public virtual void DecodificarLinha()
        {
            foreach (TCampoRegistroEDI campos in this._CamposEDI)
            {
                if (this._TamanhoMaximo > 0)
                {
                    this._LinhaRegistro = this._LinhaRegistro.PadRight(this._TamanhoMaximo, this._CaracterPreenchimento);
                }
                campos.ValorFormatado = this._LinhaRegistro.Substring(campos.PosicaoInicial, campos.TamanhoCampo);
                campos.DecodificarEDIParaNatural();
            }
        }

        #endregion Métodos Públicos
    }
}