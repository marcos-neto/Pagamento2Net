using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pagamento2.Net.Enums
{
    public enum TipoServiçoEnum
    {
        BloquetoEletronico = 03,
        PagamentoDividendos = 10,
        ConsultaDeTributosPagarDETRANComRENAVAM = 14,
        PagamentoFornecedor = 20,
        PagamentoDeContasTributosEImpostos = 22,
        AlegacaoDoSacado = 29,
        PagamentoSinistrosSegurados = 50,
        PagamentoDespesasViajanteEmTransito = 60,
        PagamentoAutorizado = 70,
        PagamentoCredenciados = 75,
        PagamentoRepresentantes = 80,
        PagamentoBeneficios = 90,
        PagamentosDiversos = 98
    }
}
