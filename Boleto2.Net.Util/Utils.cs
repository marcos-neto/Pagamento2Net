using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Boleto2Net.Util
{
    public static class Utils
    {
        public static string FormatCode(string text, int length)
        {
            return text.PadLeft(length, '0');
        }

        public static bool ToBool(object value)
        {
            try
            {
                return Convert.ToBoolean(value);
            }
            catch
            {
                return false;
            }
        }

        public static int ToInt32(string value)
        {
            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        public static long ToInt64(string value)
        {
            try
            {
                return Convert.ToInt64(value);
            }
            catch
            {
                return 0;
            }
        }

        public static string ToString(object value)
        {
            try
            {
                return Convert.ToString(value).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static DateTime ToDateTime(object value)
        {
            try
            {
                return Convert.ToDateTime(value, CultureInfo.GetCultureInfo("pt-BR"));
            }
            catch
            {
                return new DateTime(1, 1, 1);
            }
        }

        public static T ToEnum<T>(string value, bool ignoreCase, T defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(value))
            {
                return defaultValue;
            }

            if (Enum.TryParse(value, ignoreCase, out T result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Formata o CPF ou CNPJ do Cedente ou do Sacado no formato: 000.000.000-00, 00.000.000/0001-00 respectivamente.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string FormataCPFCPPJ(string value)
        {
            if (value.Trim().Length == 11)
            {
                return FormataCPF(value);
            }

            if (value.Trim().Length == 14)
            {
                return FormataCNPJ(value);
            }

            throw new Exception($"O CPF ou CNPJ: {value} � inv�lido.");
        }

        /// <summary>
        /// Formata o n�mero do CPF 92074286520 para 920.742.865-20
        /// </summary>
        /// <param name="cpf">Sequencia num�rica de 11 d�gitos. Exemplo: 00000000000</param>
        /// <returns>CPF formatado</returns>
        public static string FormataCPF(string cpf)
        {
            try
            {
                return $"{cpf.Substring(0, 3)}.{cpf.Substring(3, 3)}.{cpf.Substring(6, 3)}-{cpf.Substring(9, 2)}";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formata o CNPJ. Exemplo 00.316.449/0001-63
        /// </summary>
        /// <param name="cnpj">Sequencia num�rica de 14 d�gitos. Exemplo: 00000000000000</param>
        /// <returns>CNPJ formatado</returns>
        public static string FormataCNPJ(string cnpj)
        {
            try
            {
                return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.{cnpj.Substring(5, 3)}/{cnpj.Substring(8, 4)}-{cnpj.Substring(12, 2)}";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formato o CEP em 00000-000
        /// </summary>
        /// <param name="cep">Sequencia num�rica de 8 d�gitos. Exemplo: 00000000</param>
        /// <returns>CEP formatado</returns>
        public static string FormataCEP(string cep)
        {
            try
            {
                return $"{cep.Substring(0, 2)}{cep.Substring(2, 3)}-{cep.Substring(5, 3)}";
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Formata o campo de acordo com o tipo e o tamanho
        /// </summary>
        public static string FitStringLength(this string sringToBeFit, int maxLength, char fitChar)
        {
            return sringToBeFit.Length > maxLength ? sringToBeFit.Substring(0, maxLength) : sringToBeFit.PadLeft(maxLength, fitChar);
        }

        public static string SubstituiCaracteresEspeciais(string strline)
        {
            try
            {
                strline = strline.Replace("�", "a");
                strline = strline.Replace('�', 'A');
                strline = strline.Replace('�', 'a');
                strline = strline.Replace('�', 'A');
                strline = strline.Replace('�', 'a');
                strline = strline.Replace('�', 'A');
                strline = strline.Replace('�', 'a');
                strline = strline.Replace('�', 'A');
                strline = strline.Replace('�', 'c');
                strline = strline.Replace('�', 'C');
                strline = strline.Replace('�', 'e');
                strline = strline.Replace('�', 'E');
                strline = strline.Replace('�', 'E');
                strline = strline.Replace('�', 'e');
                strline = strline.Replace('�', 'o');
                strline = strline.Replace('�', 'O');
                strline = strline.Replace('�', 'o');
                strline = strline.Replace('�', 'O');
                strline = strline.Replace('�', 'o');
                strline = strline.Replace('�', 'O');
                strline = strline.Replace('�', 'u');
                strline = strline.Replace('�', 'U');
                strline = strline.Replace('�', 'u');
                strline = strline.Replace('�', 'U');
                strline = strline.Replace('�', 'i');
                strline = strline.Replace('�', 'I');
                strline = strline.Replace('�', 'a');
                strline = strline.Replace('�', 'o');
                strline = strline.Replace('�', 'o');
                strline = strline.Replace('&', 'e');
                return strline;
            }
            catch (Exception ex)
            {
                Exception tmpEx = new Exception("Erro ao formatar string.", ex);
                throw tmpEx;
            }
        }

        /// <summary>
        /// Converte uma imagem em array de bytes.
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static byte[] ConvertImageToByte(Image image)
        {
            if (image == null)
            {
                return null;
            }

            byte[] bytes;
            if (image.GetType().ToString() == "System.Drawing.Image")
            {
                ImageConverter converter = new ImageConverter();
                bytes = (byte[])converter.ConvertTo(image, typeof(byte[]));
                return bytes;
            }
            else if (image.GetType().ToString() == "System.Drawing.Bitmap")
            {
                bytes = (byte[])TypeDescriptor.GetConverter(image).ConvertTo(image, typeof(byte[]));
                return bytes;
            }
            else
            {
                throw new NotImplementedException("ConvertImageToByte invalid type " + image.GetType().ToString());
            }
        }

        public static Image DrawText(string text, Font font, Color textColor, Color backColor)
        {
            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap((int)textSize.Width - Convert.ToInt32(font.Size * 1.5), (int)textSize.Height, PixelFormat.Format24bppRgb);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            //create a brush for the text
            Brush textBrush = new SolidBrush(textColor);

            drawing.DrawString(text, font, textBrush, 0, 0);

            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }

        public static string FormataLinhaArquivoCNAB(string strLinha, TipoArquivo tipoArquivo)
        {
            int tamanhoRegistro = 0;

            switch (tipoArquivo)
            {
                case TipoArquivo.CNAB240:
                    tamanhoRegistro = 240;
                    break;

                case TipoArquivo.CNAB400:
                    tamanhoRegistro = 400;
                    break;

                case TipoArquivo.POS500:
                    tamanhoRegistro = 500;
                    break;

                default:
                    break;
            }

            strLinha = strLinha.ToUpper();
            //if (Banco.RemoveAcentosArquivoRemessa)
            //{
            //    strLinha = Utils.SubstituiCaracteresEspeciais(strLinha);
            //}
            if (tamanhoRegistro != 0)
            {
                string[] strLinhas = strLinha.Split('\n');
                foreach (string s in strLinhas)
                {
                    if (s.Replace("\r", "").Length != tamanhoRegistro)
                    {
                        throw new Exception("Registro com tamanho incorreto:" + s.Replace("\r", "").Length.ToString() + " - " + s);
                    }
                }
            }
            return strLinha.ToUpper();
        }

        /// <summary>
        /// M�todo que transforma a linha digit�vel no c�digo de barras
        /// </summary>
        /// <param name="barcode">A cadeia de caracteres que ser� convertida. Indifere se � c�digo de barras ou linha digit�vel.</param>
        /// <returns>C�digo de barras.</returns>
        public static string BarcodeFormated(string barcode) //TODO: verificar os coment�rios
        {
            // se for tamanho 44, j� � um c�digo de barras
            if (barcode.Length == 44)
            {
                return barcode;
            }

            string cleaned = barcode;
            // se n�o � do tamanho esperado, tenta limpar
            if (cleaned.Length != 47)
            {
                cleaned = new string(cleaned.Where(char.IsDigit).ToArray());
            }

            // se mesmo assim n�o � do tamanho esperado, � porque tem algo errado.
            if (cleaned.Length != 47)
            {
                throw new Exception($"C�digo de barras em formato incorreto: {barcode}");
            }

            string A = cleaned.Substring(0, 3);
            string B = cleaned.Substring(3, 1);
            string C = cleaned.Substring(4, 5) + cleaned.Substring(10, 10) + cleaned.Substring(21, 10);
            string D = cleaned.Substring(32, 1);
            string E = cleaned.Substring(33, 14);

            return $"{A}{B}{D}{E}{C}";
        }

        public static string FormataDataParaArquivo(DateTime data)
        {
            return data.ToString("yyyy/MM/dd");
        }

        public static string RemoveCharactersEspeciais(string text)
        {
            string comAcentos = "���������������������������������������������窺";
            string semAcentos = "AAAAAAaaaaaEEEEeeeeIIIIiiiiOOOOOoooooUUUuuuuCcao";

            for (int i = 0; i < comAcentos.Length; i++)
            {
                text = text.Replace(comAcentos[i].ToString(), semAcentos[i].ToString());
            }

            string pattern = @"(?i)[^0-9a-z-,.\s]";
            string replacement = "";
            Regex rgx = new Regex(pattern);
            return rgx.Replace(text, replacement);
        }

        /// <summary>
        /// C�lculo do m�dulo 11
        /// </summary>
        /// <param name="chaveAcesso"></param>
        /// <returns></returns>
        public static string CalculoDigitoVerificadorM�dulo11(string chaveAcesso, int valorBase)
        {

            // O peso � o multiplicador da express�o, deve ser somente de 2 � 9, ent�o j� iniciamos com 2.
            int peso = 2;
            // Somat�ria do resultado.
            int soma = 0;

            //Dividendo da opera��o
            int dividendo = 11;

            try
            {
                // Passa n�mero a n�mero da chave pegando da direita pra esquerda (pra isso o Reverse()).
                var arrayReverse = chaveAcesso.ToCharArray()
                    .Reverse()
                    .ToList();

                foreach (var item in arrayReverse)
                {
                    // Acumula valores da soma gerada das multiplica��es (peso).
                    soma += (Convert.ToInt32(item.ToString()) * peso);
                    // Como o peso pode ir somente at� 9 � feito essa valida��o.
                    peso = (peso == valorBase) ? 2 : peso + 1;

                };

                var restoDivisao = soma % dividendo;
                var result = string.Empty;

                //Quando o resto da divis�o for igual a 0(zero), 1 (um) ou maior que 9 (nove), 
                //o d�gito do c�digo de barras obrigatoriamente dever� ser igual a 1 (um).
                if (restoDivisao <= 1 || restoDivisao > 9)
                {
                    result = "1";
                }
                else
                {
                    //Quando o resto da divis�o for diferente de 0(zero), 1(um) ou maior que 9(nove), 
                    //efetuar a subtra��o entre dividendo e o resto, cujo resultado ser� o d�gito verificador do c�digo de barras.
                    result = (dividendo - restoDivisao).ToString();
                }

                return result;
            }
            catch
            {
                return "ERRO: A chave de acesso deve conter apenas n�meros.";
            }
        }

    }
}