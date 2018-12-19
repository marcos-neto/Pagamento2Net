using Pagamento2Net.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagamento2Net.Contratos
{
    public interface ICriarArquivoRetorno
    {
        Pagamento LerArquivo(byte[] contents);
    }
}
