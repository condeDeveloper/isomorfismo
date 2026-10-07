namespace Conde.Isomorfismo;

/// <summary>
/// Os INVARIANTES: as coisas que nao mudam quando se renomeia os vertices.
///
/// Eles sao a unica ferramenta barata que existe para este problema, e funcionam
/// numa direcao so. Se dois grafos tem invariantes diferentes, eles NAO sao
/// isomorfos, e isso e certeza. Se tem iguais, nao se sabe nada: a resposta pode
/// ser sim ou nao, e descobrir qual custa caro de novo.
///
/// E essa assimetria que faz toda a diferenca na pratica. Um invariante barato
/// resolve a esmagadora maioria dos pares em tempo nenhum, e o caro so e preciso
/// no que sobrou. A tabela do medidor diz quanto sobra.
/// </summary>
public static class Invariantes
{
    /// <summary>A sequencia de graus, em ordem: o invariante mais barato que existe.</summary>
    public static int[] Graus(Grafo grafo) =>
        [.. Enumerable.Range(0, grafo.Vertices).Select(grafo.Grau).OrderBy(g => g)];

    /// <summary>Quantos triangulos o grafo tem, pela definicao.</summary>
    public static int Triangulos(Grafo grafo)
    {
        var quantos = 0;

        for (var a = 0; a < grafo.Vertices; a++)
            for (var b = a + 1; b < grafo.Vertices; b++)
            {
                if (!grafo.Vizinhos(a, b)) continue;

                for (var c = b + 1; c < grafo.Vertices; c++)
                    if (grafo.Vizinhos(a, c) && grafo.Vizinhos(b, c)) quantos++;
            }

        return quantos;
    }

    /// <summary>
    /// Para cada vertice, os graus dos vizinhos dele, em ordem; e depois a lista
    /// disso tudo em ordem.
    ///
    /// E o primeiro passo do refinamento de cores escrito a mao, e ele ja separa
    /// muito mais que a sequencia de graus sozinha.
    /// </summary>
    public static string GrausDosVizinhos(Grafo grafo)
    {
        var linhas = new List<string>();

        for (var v = 0; v < grafo.Vertices; v++)
            linhas.Add(string.Join(",", grafo.De(v).Select(grafo.Grau).OrderBy(g => g)));

        linhas.Sort(StringComparer.Ordinal);

        return string.Join("|", linhas);
    }

    /// <summary>
    /// A lista ordenada das DISTANCIAS entre todos os pares de vertices.
    ///
    /// Distancia nao muda com renomeacao, entao a lista inteira e invariante. E um
    /// invariante bem mais forte que o grau, e ainda assim ele nao basta.
    /// </summary>
    public static int[] Distancias(Grafo grafo)
    {
        var todas = new List<int>();

        for (var raiz = 0; raiz < grafo.Vertices; raiz++)
        {
            var distancia = new int[grafo.Vertices];

            Array.Fill(distancia, -1);
            distancia[raiz] = 0;

            var fila = new Queue<int>();

            fila.Enqueue(raiz);

            while (fila.Count > 0)
            {
                var v = fila.Dequeue();

                foreach (var u in grafo.De(v))
                {
                    if (distancia[u] >= 0) continue;

                    distancia[u] = distancia[v] + 1;
                    fila.Enqueue(u);
                }
            }

            for (var v = raiz + 1; v < grafo.Vertices; v++) todas.Add(distancia[v]);
        }

        todas.Sort();

        return [.. todas];
    }

    /// <summary>
    /// A ASSINATURA barata: vertices, arestas, graus, triangulos e graus dos
    /// vizinhos, tudo junto.
    ///
    /// Dois grafos com assinaturas diferentes sao com certeza diferentes. Com
    /// assinaturas iguais, nao se sabe nada, e e ai que o trabalho de verdade
    /// comeca.
    /// </summary>
    public static string Assinatura(Grafo grafo) =>
        $"{grafo.Vertices}/{grafo.Arestas}/{string.Join(",", Graus(grafo))}" +
        $"/{Triangulos(grafo)}/{GrausDosVizinhos(grafo)}";

    public static bool MesmaAssinatura(Grafo um, Grafo outro) =>
        Assinatura(um) == Assinatura(outro);

    public static bool MesmosGraus(Grafo um, Grafo outro) =>
        Graus(um).SequenceEqual(Graus(outro));

    public static bool MesmasDistancias(Grafo um, Grafo outro) =>
        Distancias(um).SequenceEqual(Distancias(outro));
}
