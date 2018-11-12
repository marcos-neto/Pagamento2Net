using System.Collections.Generic;

namespace Pagamento2Net
{
    public abstract class OcorrênciaParser
    {
        public abstract Dictionary<string, string> Ocorrências { get; set; }

        public virtual KeyValuePair<string, string> Parse(string código)
        {
            if (Ocorrências.TryGetValue(código, out string descrição))
            {
                return new KeyValuePair<string, string>(código, descrição);
            }
            else
            {
                return new OcorrênciasFebraban().Parse(código);
            }
        }
    }
}