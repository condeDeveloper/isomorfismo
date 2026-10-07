namespace Conde.Isomorfismo;

/// <summary>
/// A BUSCA COM PODA, que e o que se usa de verdade.
///
/// A forca bruta monta a renomeacao inteira e so depois confere. A busca monta um
/// vertice de cada vez e confere a CADA PASSO: se o vertice v ja foi mandado para
/// w, entao todo vizinho de v que ja tem destino precisa ser vizinho de w. Uma
/// escolha incompativel e abandonada ali, sem que as outras n menos k sejam
/// sequer geradas.
///
/// E a mesma resposta, pela mesma definicao, com a ordem das operacoes trocada. A
/// diferenca no custo e de varias ordens de grandeza, e a tabela do medidor diz
/// de quantas.
///
/// Por cima disso entra a PODA POR COR: dois vertices so podem corresponder se
/// terminaram o refinamento com a mesma cor. Ela nao muda a resposta, porque cor
/// e invariante, e corta o que sobrou.
/// </summary>
public static class Busca
{
    /// <param name="Isomorfismo">a renomeacao achada, ou null</param>
    /// <param name="Nos">quantas escolhas de destino foram CONSIDERADAS</param>
    public record Resultado(int[]? Isomorfismo, long Nos)
    {
        public bool Achou => Isomorfismo is not null;
    }

    /// <summary>
    /// Procura uma renomeacao que leve um grafo no outro, com poda.
    ///
    /// A pilha e explicita, e nao por capricho: a profundidade e o numero de
    /// vertices, e um repositorio vizinho ja levou estouro de pilha no macOS da
    /// integracao continua com uma recursao tao funda quanto esta.
    /// </summary>
    public static Resultado Procurar(Grafo um, Grafo outro, bool porCor = true)
    {
        if (um.Vertices != outro.Vertices || um.Arestas != outro.Arestas) return new Resultado(null, 0);

        var n = um.Vertices;
        var corDeUm = porCor ? Refinamento.Refinar(um).Cor : null;
        var corDoOutro = porCor ? Refinamento.Refinar(outro).Cor : null;

        var mapa = new int[n];
        var usado = new bool[n];
        var candidato = new int[n];
        var nos = 0L;

        Array.Fill(mapa, -1);

        var v = 0;

        while (v >= 0)
        {
            if (v == n) return new Resultado([.. mapa], nos);

            if (mapa[v] >= 0) { usado[mapa[v]] = false; mapa[v] = -1; }

            var achou = false;

            while (candidato[v] < n)
            {
                var w = candidato[v]++;

                nos++;

                if (usado[w]) continue;
                if (um.Grau(v) != outro.Grau(w)) continue;
                if (porCor && corDeUm![v] != corDoOutro![w]) continue;
                if (!Combina(um, outro, mapa, v, w)) continue;

                mapa[v] = w;
                usado[w] = true;
                achou = true;

                break;
            }

            if (achou) v++;
            else { candidato[v] = 0; v--; }
        }

        return new Resultado(null, nos);
    }

    /// <summary>
    /// O vertice v pode ir para w, dado o que ja foi decidido?
    ///
    /// Esta e a poda inteira, e ela cabe em tres linhas: para todo vertice u ja
    /// mandado para algum lugar, "u e vizinho de v" precisa valer exatamente
    /// quando "o destino de u e vizinho de w".
    ///
    /// Repare que a conferencia e nos DOIS sentidos. Exigir so que aresta vire
    /// aresta deixaria passar renomeacoes que criam arestas do nada, e com o mesmo
    /// numero de arestas dos dois lados isso parece impossivel e nao e: parece
    /// impossivel no FIM, e esta conferencia acontece no MEIO.
    /// </summary>
    private static bool Combina(Grafo um, Grafo outro, int[] mapa, int v, int w)
    {
        for (var u = 0; u < v; u++)
        {
            if (mapa[u] < 0) continue;
            if (um.Vizinhos(v, u) != outro.Vizinhos(w, mapa[u])) return false;
        }

        return true;
    }

    /// <summary>
    /// A busca SEM poda nenhuma alem da que a propria ordem de montagem da.
    ///
    /// Ela existe para a tabela ter contra o que comparar: ela e a forca bruta
    /// escrita como busca, ou seja, ela gera as mesmas n fatorial renomeacoes e so
    /// confere no fim.
    /// </summary>
    public static Resultado SemPoda(Grafo um, Grafo outro)
    {
        if (um.Vertices != outro.Vertices || um.Arestas != outro.Arestas) return new Resultado(null, 0);

        var nos = 0L;

        foreach (var nome in ForcaBruta.Permutacoes(um.Vertices))
        {
            nos++;

            if (ForcaBruta.Leva(um, outro, nome)) return new Resultado([.. nome], nos);
        }

        return new Resultado(null, nos);
    }
}
