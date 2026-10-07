namespace Conde.Isomorfismo;

/// <summary>
/// O JUIZ, e ele e a definicao escrita do jeito mais burro possivel.
///
/// Dois grafos sao isomorfos quando EXISTE uma renomeacao dos vertices que leva
/// um no outro. O jeito de saber se existe e tentar todas, e todas sao n
/// fatorial: com dez vertices sao tres milhoes e meio, com doze sao quase
/// quinhentos milhoes.
///
/// Custa isso e precisa custar. Nenhum invariante deste repositorio pode servir
/// de juiz, porque invariante responde "sao diferentes" ou "nao sei", e "nao sei"
/// nao e veredicto. O juiz e o unico lugar onde a resposta e sim ou nao.
/// </summary>
public static class ForcaBruta
{
    /// <summary>Existe renomeacao que leva um no outro?</summary>
    public static bool SaoIsomorfos(Grafo um, Grafo outro) => UmIsomorfismo(um, outro) is not null;

    /// <summary>
    /// Uma renomeacao que leva um grafo no outro, ou null quando nao existe
    /// nenhuma.
    ///
    /// Devolver a renomeacao, e nao so o sim, nao e luxo: ela e CONFERIVEL. Um
    /// "sim" sozinho nao da para auditar; com a renomeacao na mao, quem duvida
    /// aplica e olha.
    /// </summary>
    public static int[]? UmIsomorfismo(Grafo um, Grafo outro)
    {
        if (um.Vertices != outro.Vertices || um.Arestas != outro.Arestas) return null;
        if (um.Vertices > 11) throw new ArgumentException("forca bruta so ate 11 vertices", nameof(um));

        foreach (var nome in Permutacoes(um.Vertices))
            if (Leva(um, outro, nome)) return [.. nome];

        return null;
    }

    /// <summary>
    /// QUANTAS renomeacoes levam um grafo no outro.
    ///
    /// Em dois grafos isomorfos, esse numero e o mesmo que o numero de
    /// AUTOMORFISMOS, que sao as renomeacoes que levam um grafo nele mesmo. E uma
    /// medida de quao simetrico o grafo e, e ela explica por que os casos dificeis
    /// deste problema sao justamente os grafos muito simetricos.
    /// </summary>
    public static int Quantos(Grafo um, Grafo outro)
    {
        if (um.Vertices != outro.Vertices || um.Arestas != outro.Arestas) return 0;
        if (um.Vertices > 9) throw new ArgumentException("contar todos so ate 9 vertices", nameof(um));

        return Permutacoes(um.Vertices).Count(nome => Leva(um, outro, nome));
    }

    /// <summary>As renomeacoes que levam o grafo NELE MESMO: as simetrias dele.</summary>
    public static int Automorfismos(Grafo grafo) => Quantos(grafo, grafo);

    /// <summary>
    /// A renomeacao leva um grafo no outro?
    ///
    /// Precisa valer nos DOIS sentidos: toda aresta de um vira aresta do outro E
    /// toda nao-aresta vira nao-aresta. Com o mesmo numero de arestas nos dois, a
    /// primeira metade ja garante a segunda, e e por isso que a conferencia de
    /// arestas iguais vem antes.
    /// </summary>
    public static bool Leva(Grafo um, Grafo outro, int[] nome)
    {
        foreach (var (a, b) in um.TodasAsArestas())
            if (!outro.Vizinhos(nome[a], nome[b])) return false;

        return true;
    }

    /// <summary>
    /// Todas as renomeacoes de n elementos, pelo algoritmo de Heap, SEM RECURSAO.
    ///
    /// Sem recursao porque a profundidade seria o numero de vertices e porque o
    /// algoritmo de Heap iterativo gera cada permutacao com UMA troca a partir da
    /// anterior, o que e o que torna possivel percorrer meio bilhao delas.
    /// </summary>
    public static IEnumerable<int[]> Permutacoes(int n)
    {
        var atual = Enumerable.Range(0, n).ToArray();
        var contador = new int[n];

        yield return atual;

        var i = 0;

        while (i < n)
        {
            if (contador[i] >= i) { contador[i] = 0; i++; continue; }

            var j = i % 2 == 0 ? 0 : contador[i];

            (atual[i], atual[j]) = (atual[j], atual[i]);

            yield return atual;

            contador[i]++;
            i = 0;
        }
    }

    /// <summary>Quantas renomeacoes existem de n elementos: n fatorial.</summary>
    public static long Fatorial(int n)
    {
        var f = 1L;

        for (var i = 2; i <= n; i++) f *= i;

        return f;
    }
}
