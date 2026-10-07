namespace Conde.Isomorfismo;

/// <summary>
/// O REFINAMENTO DE CORES, que e o metodo que quase todo programa de isomorfismo
/// usa por baixo.
///
/// Comece com todo mundo da mesma cor. A cada rodada, troque a cor de cada
/// vertice pelo par (cor antiga, lista das cores dos vizinhos em ordem), e
/// renumere. Pare quando o numero de cores parar de crescer.
///
/// O resultado e uma particao dos vertices que nao depende dos NOMES deles. Dois
/// grafos isomorfos terminam com o mesmo numero de vertices de cada cor, sempre.
/// Entao, se as contagens finais sao diferentes, os grafos sao diferentes, e isso
/// custa quase nada.
///
/// O caminho de volta nao vale, e e ESSE o assunto. Contagens iguais nao fazem
/// grafos iguais, e o menor contraexemplo tem SEIS vertices: o ciclo de seis e
/// dois triangulos separados.
///
/// Este metodo tem nome na literatura, Weisfeiler e Leman de dimensao um, e o que
/// ele nao consegue ver e exatamente a fronteira entre o que e barato e o que nao
/// e neste problema.
/// </summary>
public static class Refinamento
{
    /// <param name="Cor">a cor final de cada vertice</param>
    /// <param name="Quantas">quantas cores diferentes sobraram</param>
    /// <param name="Rodadas">quantas rodadas ate estabilizar</param>
    public record Resultado(int[] Cor, int Quantas, int Rodadas);

    public static Resultado Refinar(Grafo grafo)
    {
        var cor = new int[grafo.Vertices];
        var quantas = grafo.Vertices == 0 ? 0 : 1;
        var rodadas = 0;

        while (true)
        {
            var chave = new string[grafo.Vertices];

            for (var v = 0; v < grafo.Vertices; v++)
            {
                var vizinhas = grafo.De(v).Select(u => cor[u]).OrderBy(c => c);

                chave[v] = cor[v] + ":" + string.Join(",", vizinhas);
            }

            // renumerar por ordem da chave deixa a cor independente dos NOMES dos
            // vertices, que e a unica coisa que o metodo nao pode olhar
            var ordem = chave.Distinct().OrderBy(k => k, StringComparer.Ordinal).ToList();
            var nova = new int[grafo.Vertices];

            for (var v = 0; v < grafo.Vertices; v++) nova[v] = ordem.IndexOf(chave[v]);

            rodadas++;

            if (ordem.Count == quantas) return new Resultado(nova, quantas, rodadas);

            cor = nova;
            quantas = ordem.Count;
        }
    }

    /// <summary>
    /// A CONTAGEM por cor, que e o que de fato se compara entre dois grafos.
    ///
    /// Comparar as cores vertice a vertice nao faria sentido: os vertices tem nomes
    /// diferentes nos dois grafos, e o metodo inteiro existe para nao depender
    /// disso. O que se compara e quantos vertices de cada cor existem.
    /// </summary>
    public static string Contagem(Grafo grafo)
    {
        var r = Refinar(grafo);
        var quantos = new int[r.Quantas];

        foreach (var c in r.Cor) quantos[c]++;

        return string.Join(",", quantos);
    }

    /// <summary>
    /// Os dois grafos sobrevivem ao refinamento? Se nao, eles sao com certeza
    /// diferentes. Se sim, nao se sabe nada.
    /// </summary>
    public static bool Sobrevivem(Grafo um, Grafo outro) =>
        um.Vertices == outro.Vertices && um.Arestas == outro.Arestas
        && Contagem(um) == Contagem(outro);

    /// <summary>
    /// O grafo e DISCRETO depois do refinamento quando cada vertice ficou com uma
    /// cor so dele.
    ///
    /// Quando os dois grafos ficam discretos, o isomorfismo, se existir, e UNICO e
    /// sai de graca: cada vertice so pode corresponder ao de mesma cor no outro. E
    /// e por isso que o refinamento nao e so um filtro, e tambem um atalho.
    /// </summary>
    public static bool EhDiscreto(Grafo grafo) => Refinar(grafo).Quantas == grafo.Vertices;

    /// <summary>
    /// O grafo e REGULAR quando todo vertice tem o mesmo grau.
    ///
    /// Em grafo regular o refinamento nao tem por onde comecar: a primeira rodada
    /// da a mesma chave para todo mundo, e a segunda tambem, e ele para com UMA cor
    /// so. Todo par de grafos regulares do mesmo tamanho e com o mesmo grau
    /// sobrevive ao refinamento, isomorfos ou nao.
    /// </summary>
    public static bool EhRegular(Grafo grafo) =>
        grafo.Vertices == 0 || Enumerable.Range(0, grafo.Vertices).All(v => grafo.Grau(v) == grafo.Grau(0));
}
