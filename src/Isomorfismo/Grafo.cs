namespace Conde.Isomorfismo;

/// <summary>
/// O SORTEIO, escrito aqui em vez de vir da biblioteca.
///
/// A documentacao do sorteio de biblioteca nao promete a mesma sequencia entre
/// versoes nem entre sistemas, e a integracao continua roda em tres. "O
/// refinamento separou 4.978 dos 5.000 pares" com grafos diferentes em cada
/// sistema nao e uma medida: sao tres medidas que ninguem pode comparar.
/// </summary>
public sealed class Sorteio
{
    private ulong estado;

    public Sorteio(ulong semente) => estado = semente == 0 ? 1 : semente;

    public uint Proximo()
    {
        estado = estado * 6364136223846793005UL + 1442695040888963407UL;
        return (uint)(estado >> 32);
    }

    public int Ate(int limite) => (int)(Proximo() % (uint)limite);

    public double Fracao() => Proximo() / 4294967296.0;

    /// <summary>Uma permutacao sorteada, pelo metodo de Fisher e Yates.</summary>
    public int[] Permutacao(int n)
    {
        var p = Enumerable.Range(0, n).ToArray();

        for (var i = n - 1; i > 0; i--)
        {
            var j = Ate(i + 1);

            (p[i], p[j]) = (p[j], p[i]);
        }

        return p;
    }
}

/// <summary>
/// Um GRAFO simples, sem direcao e sem laco.
///
/// O assunto deste repositorio e saber se dois grafos sao O MESMO GRAFO com os
/// vertices renomeados. A pergunta parece boba e nao e: ninguem conhece algoritmo
/// rapido para ela, e ninguem provou que ela e dificil. Ela vive num lugar
/// estranho da teoria, entre os dois.
///
/// O que existe sao INVARIANTES: coisas que nao mudam quando se renomeia. Se dois
/// grafos tem invariantes diferentes, eles sao diferentes, e isso e barato de
/// conferir. Se tem iguais, nao se sabe nada, e e disso que o repositorio trata.
/// </summary>
public sealed class Grafo
{
    private readonly List<int>[] vizinhos;
    private readonly bool[,] ligado;

    public Grafo(int vertices)
    {
        Vertices = vertices;
        vizinhos = new List<int>[vertices];
        ligado = new bool[vertices, vertices];

        for (var v = 0; v < vertices; v++) vizinhos[v] = [];
    }

    public int Vertices { get; }

    public int Arestas { get; private set; }

    public void Ligar(int a, int b)
    {
        if (a == b) throw new ArgumentException("laco nao e grafo simples", nameof(a));
        if (ligado[a, b]) return;

        ligado[a, b] = true;
        ligado[b, a] = true;
        vizinhos[a].Add(b);
        vizinhos[b].Add(a);
        Arestas++;
    }

    public bool Vizinhos(int a, int b) => ligado[a, b];

    public IReadOnlyList<int> De(int vertice) => vizinhos[vertice];

    public int Grau(int vertice) => vizinhos[vertice].Count;

    public IEnumerable<(int A, int B)> TodasAsArestas()
    {
        for (var a = 0; a < Vertices; a++)
            foreach (var b in vizinhos[a])
                if (a < b) yield return (a, b);
    }

    /// <summary>
    /// O MESMO grafo com os vertices renomeados: o vertice v passa a se chamar
    /// nome[v].
    ///
    /// E a operacao que define o problema. Dois grafos sao isomorfos quando existe
    /// uma renomeacao que leva um no outro, e a dificuldade inteira esta em que ha
    /// n fatorial renomeacoes.
    /// </summary>
    public Grafo Renomear(int[] nome)
    {
        if (nome.Length != Vertices) throw new ArgumentException("a renomeacao nao cobre o grafo", nameof(nome));

        var outro = new Grafo(Vertices);

        foreach (var (a, b) in TodasAsArestas()) outro.Ligar(nome[a], nome[b]);

        return outro;
    }

    /// <summary>O mesmo grafo com os vertices embaralhados por um sorteio.</summary>
    public Grafo Embaralhar(ulong semente) => Renomear(new Sorteio(semente).Permutacao(Vertices));

    /// <summary>Dois grafos sao IGUAIS quando tem exatamente as mesmas arestas, com os mesmos nomes.</summary>
    public bool MesmasArestas(Grafo outro)
    {
        if (Vertices != outro.Vertices || Arestas != outro.Arestas) return false;

        return TodasAsArestas().All(a => outro.Vizinhos(a.A, a.B));
    }
}

/// <summary>
/// Os PARES DE GRAFOS que separam os metodos, e por que eles sao assim.
///
/// Par sorteado nao separa quase nada: dois grafos sorteados de tamanhos iguais
/// quase sempre tem ate a sequencia de graus diferente, e o metodo mais burro
/// resolve. O que separa sao os pares ARMADOS, e o menor deles tem SEIS vertices.
/// </summary>
public static class Pares
{
    public static Grafo Ciclo(int vertices)
    {
        var grafo = new Grafo(vertices);

        for (var v = 0; v < vertices; v++) grafo.Ligar(v, (v + 1) % vertices);

        return grafo;
    }

    /// <summary>Dois ciclos separados, sem nada ligando um ao outro.</summary>
    public static Grafo DoisCiclos(int cada)
    {
        var grafo = new Grafo(2 * cada);

        for (var v = 0; v < cada; v++)
        {
            grafo.Ligar(v, (v + 1) % cada);
            grafo.Ligar(cada + v, cada + (v + 1) % cada);
        }

        return grafo;
    }

    public static Grafo Completo(int vertices)
    {
        var grafo = new Grafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++) grafo.Ligar(a, b);

        return grafo;
    }

    public static Grafo Caminho(int vertices)
    {
        var grafo = new Grafo(vertices);

        for (var v = 0; v + 1 < vertices; v++) grafo.Ligar(v, v + 1);

        return grafo;
    }

    public static Grafo Estrela(int folhas)
    {
        var grafo = new Grafo(folhas + 1);

        for (var i = 0; i < folhas; i++) grafo.Ligar(folhas, i);

        return grafo;
    }

    public static Grafo Grade(int linhas, int colunas)
    {
        var grafo = new Grafo(linhas * colunas);

        int No(int l, int c) => l * colunas + c;

        for (var l = 0; l < linhas; l++)
            for (var c = 0; c < colunas; c++)
            {
                if (c + 1 < colunas) grafo.Ligar(No(l, c), No(l, c + 1));
                if (l + 1 < linhas) grafo.Ligar(No(l, c), No(l + 1, c));
            }

        return grafo;
    }

    /// <summary>O grafo de Petersen: dez vertices, todos de grau tres.</summary>
    public static Grafo Petersen()
    {
        var grafo = new Grafo(10);

        for (var i = 0; i < 5; i++)
        {
            grafo.Ligar(i, (i + 1) % 5);
            grafo.Ligar(i, 5 + i);
            grafo.Ligar(5 + i, 5 + (i + 2) % 5);
        }

        return grafo;
    }

    /// <summary>
    /// O PRISMA: dois triangulos ligados em paralelo. Tres-regular, seis vertices,
    /// nove arestas.
    /// </summary>
    public static Grafo Prisma()
    {
        var grafo = new Grafo(6);

        for (var i = 0; i < 3; i++)
        {
            grafo.Ligar(i, (i + 1) % 3);
            grafo.Ligar(3 + i, 3 + (i + 1) % 3);
            grafo.Ligar(i, 3 + i);
        }

        return grafo;
    }

    /// <summary>
    /// O BIPARTIDO COMPLETO de tres por tres, que tambem e tres-regular com seis
    /// vertices e nove arestas, e NAO e o prisma.
    ///
    /// O prisma tem dois triangulos; este nao tem nenhum, porque ele e bipartido.
    /// Os dois tem a mesma sequencia de graus, o mesmo numero de vertices e o mesmo
    /// numero de arestas, e sao grafos diferentes.
    /// </summary>
    public static Grafo TresPorTres()
    {
        var grafo = new Grafo(6);

        for (var i = 0; i < 3; i++)
            for (var j = 0; j < 3; j++) grafo.Ligar(i, 3 + j);

        return grafo;
    }

    /// <summary>Um grafo sorteado, cada aresta com a mesma chance.</summary>
    public static Grafo Sorteado(int vertices, double chance, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Grafo(vertices);

        for (var a = 0; a < vertices; a++)
            for (var b = a + 1; b < vertices; b++)
                if (sorteio.Fracao() < chance) grafo.Ligar(a, b);

        return grafo;
    }

    /// <summary>Um grafo sorteado com um numero fixo de arestas.</summary>
    public static Grafo ComTantasArestas(int vertices, int arestas, ulong semente)
    {
        var sorteio = new Sorteio(semente);
        var grafo = new Grafo(vertices);

        while (grafo.Arestas < arestas)
        {
            var a = sorteio.Ate(vertices);
            var b = sorteio.Ate(vertices);

            if (a != b) grafo.Ligar(a, b);
        }

        return grafo;
    }

    /// <param name="Nome">como o par se chama</param>
    /// <param name="Um">o primeiro grafo</param>
    /// <param name="Outro">o segundo</param>
    /// <param name="Isomorfos">a resposta certa, sabida de fora</param>
    public record Par(string Nome, Grafo Um, Grafo Outro, bool Isomorfos);

    /// <summary>
    /// Os pares em que a resposta e SABIDA DE FORA, por construcao.
    ///
    /// Eles nao dependem de nenhum algoritmo deste repositorio: ou um grafo e o
    /// outro embaralhado, e ai sao isomorfos por construcao, ou os dois tem uma
    /// diferenca que da para apontar com o dedo.
    /// </summary>
    public static IEnumerable<Par> Conhecidos()
    {
        // um grafo embaralhado e ele mesmo
        yield return new Par("ciclo de 8 embaralhado", Ciclo(8), Ciclo(8).Embaralhar(7), true);
        yield return new Par("Petersen embaralhado", Petersen(), Petersen().Embaralhar(11), true);
        yield return new Par("grade 3x3 embaralhada", Grade(3, 3), Grade(3, 3).Embaralhar(13), true);
        yield return new Par("sorteado embaralhado", Sorteado(9, 0.4, 3), Sorteado(9, 0.4, 3).Embaralhar(17), true);

        // e os pares que sao diferentes, cada um por um motivo
        yield return new Par("ciclo de 6 contra dois triangulos", Ciclo(6), DoisCiclos(3), false);
        yield return new Par("prisma contra tres por tres", Prisma(), TresPorTres(), false);
        yield return new Par("caminho de 7 contra estrela de 6", Caminho(7), Estrela(6), false);
        yield return new Par("ciclo de 9 contra tres triangulos", Ciclo(9), TresTriangulos(), false);
        yield return new Par("completo de 6 contra ciclo de 6", Completo(6), Ciclo(6), false);
    }

    /// <summary>Tres triangulos separados: nove vertices, todos de grau dois.</summary>
    public static Grafo TresTriangulos()
    {
        var grafo = new Grafo(9);

        for (var c = 0; c < 3; c++)
            for (var i = 0; i < 3; i++) grafo.Ligar(3 * c + i, 3 * c + (i + 1) % 3);

        return grafo;
    }

    /// <summary>
    /// Os pares REGULARES, que sao os que o refinamento de cores nao consegue
    /// separar.
    ///
    /// Num grafo regular todo vertice tem o mesmo grau, entao o refinamento comeca
    /// com todo mundo da mesma cor e termina com todo mundo da mesma cor: ele nao
    /// tem por onde comecar a distinguir.
    /// </summary>
    public static IEnumerable<Par> Regulares()
    {
        yield return new Par("ciclo de 6 contra dois triangulos", Ciclo(6), DoisCiclos(3), false);
        yield return new Par("ciclo de 8 contra dois quadrados", Ciclo(8), DoisCiclos(4), false);
        yield return new Par("ciclo de 9 contra tres triangulos", Ciclo(9), TresTriangulos(), false);
        yield return new Par("prisma contra tres por tres", Prisma(), TresPorTres(), false);
        yield return new Par("ciclo de 6 contra ele mesmo", Ciclo(6), Ciclo(6).Embaralhar(5), true);
        yield return new Par("prisma contra ele mesmo", Prisma(), Prisma().Embaralhar(19), true);
    }
}
