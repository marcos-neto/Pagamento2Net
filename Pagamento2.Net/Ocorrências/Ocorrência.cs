using System.Collections.Generic;

namespace Pagamento2Net
{
    public class Ocorrência
    {
        public string Código { get; set; }

        public string Descrição { get; set; }

        public Ocorrência(string código, OcorrênciaParser parser)
        {
            KeyValuePair<string, string> a = parser.Parse(código);
            Código = a.Key;
            Descrição = a.Value;
        }
    }
}