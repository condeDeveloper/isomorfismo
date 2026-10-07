namespace Conde.Isomorfismo;

/// <summary>
/// As TABELAS que o medidor imprime e que os testes conferem.
///
/// Elas ficam aqui, e nao dentro do programa do medidor, por um motivo so: numero
/// em README que ninguem confere envelhece errado em silencio. Alguem troca uma
/// constante, a tabela muda, e o texto continua afirmando a frase antiga. Aqui o
/// teste afirma sobre o MESMO objeto que vira linha impressa.
/// </summary>
public static class Medidas
{
    public record LinhaConhecido(
        string Par, int Vertices, bool Isomorfos, bool ForcaBruta,
        bool MesmosGraus, bool MesmaAssinatura, bool MesmasDistancias, bool SobreviveAoRefinamento);

    /// <summary>
    /// Os pares de resposta SABIDA DE FORA contra a forca bruta e contra cada
    /// invariante.
    ///
    /// A coluna da forca bruta tem de bater com a coluna da resposta sabida em toda
    /// linha; as outras sao filtros, e filtro pode dizer "nao sei".
    /// </summary>
    public static List<LinhaConhecido> Conhecidos()
    {
        var linhas = new List<LinhaConhecido>();

        foreach (var par in Pares.Conhecidos().Concat(Pares.Regulares()))
            linhas.Add(new LinhaConhecido(
                par.Nome, par.Um.Vertices, par.Isomorfos,
                ForcaBruta.SaoIsomorfos(par.Um, par.Outro),
                Invariantes.MesmosGraus(par.Um, par.Outro),
                Invariantes.MesmaAssinatura(par.Um, par.Outro),
                Invariantes.MesmasDistancias(par.Um, par.Outro),
                Refinamento.Sobrevivem(par.Um, par.Outro)));

        return linhas;
    }

    /// <param name="Diferentes">quantos pares NAO sao isomorfos, pela busca exata</param>
    /// <param name="FalsosGraus">quantos desses a sequencia de graus nao separou</param>
    public record LinhaFiltro(
        string Caso, int Pares, int Isomorfos, int Diferentes,
        int FalsosGraus, int FalsosAssinatura, int FalsosDistancias, int FalsosRefinamento);

    /// <summary>
    /// Quantos pares cada filtro deixa passar, em muitos pares sorteados.
    ///
    /// "Passar" quer dizer "o filtro nao soube dizer que sao diferentes". Um par
    /// isomorfo SEMPRE passa por todo filtro, e isso nao e merito: e a definicao de
    /// invariante. O que interessa e quantos DIFERENTES passam, e essa e a ultima
    /// coluna.
    /// </summary>
    public static List<LinhaFiltro> Filtros(int quantos = 500)
    {
        var linhas = new List<LinhaFiltro>();

        foreach (var (caso, montar) in Casos())
        {
            var isomorfos = 0;
            var diferentes = 0;
            var graus = 0;
            var assinatura = 0;
            var distancias = 0;
            var refinamento = 0;

            for (var k = 1UL; k <= (ulong)quantos; k++)
            {
                var (um, outro) = montar(k);

                // a resposta certa vem da BUSCA EXATA, nao de como o par foi
                // montado: dois grafos tres-regulares sorteados do mesmo tamanho
                // podem muito bem ser o mesmo grafo, e chutar que nao seriam
                // transformaria a tabela inteira em ficcao. Que a busca exata
                // concorda com a forca bruta e um teste separado.
                var sabido = Busca.Procurar(um, outro).Achou;

                if (sabido) isomorfos++; else diferentes++;

                // so o par DIFERENTE conta: um par isomorfo passa por todo filtro
                // sempre, e isso nao e merito nenhum, e a definicao de invariante
                if (sabido) continue;

                if (Invariantes.MesmosGraus(um, outro)) graus++;
                if (Invariantes.MesmaAssinatura(um, outro)) assinatura++;
                if (Invariantes.MesmasDistancias(um, outro)) distancias++;
                if (Refinamento.Sobrevivem(um, outro)) refinamento++;
            }

            linhas.Add(new LinhaFiltro(
                caso, quantos, isomorfos, diferentes, graus, assinatura, distancias, refinamento));
        }

        return linhas;
    }

    /// <summary>
    /// Os casos de teste dos filtros: pares sorteados de varios feitios.
    ///
    /// O primeiro caso e o honesto: dois grafos sorteados independentes, que quase
    /// nunca sao isomorfos e quase sempre tem ate o numero de arestas diferente. Os
    /// outros sao cada vez mais parecidos, ate o ultimo, em que os dois sao
    /// REGULARES e nenhum invariante local tem por onde comecar.
    /// </summary>
    public static IEnumerable<(string Caso, Func<ulong, (Grafo Um, Grafo Outro)>)> Casos()
    {
        yield return ("sorteados soltos", k => (
            Pares.Sorteado(8, 0.4, k), Pares.Sorteado(8, 0.4, k + 7919)));

        yield return ("mesmo numero de arestas", k => (
            Pares.ComTantasArestas(8, 12, k), Pares.ComTantasArestas(8, 12, k + 7919)));

        yield return ("um embaralhado do outro", k => (
            Pares.ComTantasArestas(8, 12, k), Pares.ComTantasArestas(8, 12, k).Embaralhar(k + 101)));

        yield return ("tres-regulares de 8", k => (
            Regular(8, 3, k), Regular(8, 3, k + 7919)));

        yield return ("dois-regulares de 9", k => (
            Regular(9, 2, k), Regular(9, 2, k + 7919)));
    }

    /// <summary>
    /// Um grafo REGULAR sorteado: todo vertice com o mesmo grau.
    ///
    /// Montado por sorteio com recomeco, que e o jeito simples: tenta emparelhar as
    /// pontas soltas e recomeca quando trava. Para os tamanhos daqui ele termina
    /// rapido, e o que importa e que o resultado e regular de verdade.
    /// </summary>
    public static Grafo Regular(int vertices, int grau, ulong semente)
    {
        var sorteio = new Sorteio(semente);

        for (var tentativa = 0; tentativa < 2_000; tentativa++)
        {
            var grafo = new Grafo(vertices);
            var pontas = new List<int>();

            for (var v = 0; v < vertices; v++)
                for (var i = 0; i < grau; i++) pontas.Add(v);

            var travou = false;

            while (pontas.Count > 0 && !travou)
            {
                var i = sorteio.Ate(pontas.Count);
                var j = sorteio.Ate(pontas.Count);

                if (i == j || pontas[i] == pontas[j] || grafo.Vizinhos(pontas[i], pontas[j]))
                {
                    if (TudoImpossivel(grafo, pontas)) travou = true;

                    continue;
                }

                grafo.Ligar(pontas[i], pontas[j]);

                var maior = Math.Max(i, j);
                var menor = Math.Min(i, j);

                pontas.RemoveAt(maior);
                pontas.RemoveAt(menor);
            }

            if (!travou && pontas.Count == 0) return grafo;
        }

        throw new InvalidOperationException($"nao consegui um grafo {grau}-regular de {vertices} vertices");
    }

    private static bool TudoImpossivel(Grafo grafo, List<int> pontas)
    {
        for (var i = 0; i < pontas.Count; i++)
            for (var j = i + 1; j < pontas.Count; j++)
                if (pontas[i] != pontas[j] && !grafo.Vizinhos(pontas[i], pontas[j])) return false;

        return true;
    }

    public record LinhaPoda(string Par, int Vertices, long Fatorial, long SemPoda, long ComPoda, long ComPodaECor);

    /// <summary>
    /// O CUSTO da busca, em escolhas consideradas, contra o numero de renomeacoes
    /// que existem.
    ///
    /// Nao ha segundo em lugar nenhum desta tabela, de proposito: segundo depende
    /// de maquina e de quem mais esta rodando. Escolha considerada e o mesmo numero
    /// nos tres sistemas da integracao continua.
    /// </summary>
    public static List<LinhaPoda> Podas()
    {
        var linhas = new List<LinhaPoda>();

        foreach (var par in Pares.Conhecidos().Concat(Pares.Regulares()))
        {
            if (par.Um.Vertices > 10) continue;

            linhas.Add(new LinhaPoda(
                par.Nome, par.Um.Vertices, ForcaBruta.Fatorial(par.Um.Vertices),
                Busca.SemPoda(par.Um, par.Outro).Nos,
                Busca.Procurar(par.Um, par.Outro, false).Nos,
                Busca.Procurar(par.Um, par.Outro).Nos));
        }

        return linhas;
    }

    public record LinhaSimetria(string Grafo, int Vertices, int Arestas, int Automorfismos, int CoresDoRefinamento, bool Regular);

    /// <summary>
    /// As SIMETRIAS de cada grafo: quantas renomeacoes levam ele nele mesmo.
    ///
    /// E a medida que explica por que este problema e dificil onde e dificil.
    /// Grafo sem simetria nenhuma tem um automorfismo so, o que nao mexe em nada, e
    /// o refinamento o deixa discreto: a resposta sai de graca. Grafo muito
    /// simetrico tem muitos, o refinamento nao separa nada, e sobra a busca.
    /// </summary>
    public static List<LinhaSimetria> Simetrias()
    {
        var linhas = new List<LinhaSimetria>();

        foreach (var (nome, grafo) in new (string, Grafo)[]
        {
            ("caminho de 6", Pares.Caminho(6)),
            ("estrela de 5", Pares.Estrela(5)),
            ("ciclo de 6", Pares.Ciclo(6)),
            ("dois triangulos", Pares.DoisCiclos(3)),
            ("prisma", Pares.Prisma()),
            ("tres por tres", Pares.TresPorTres()),
            ("completo de 6", Pares.Completo(6)),
            ("grade 2x3", Pares.Grade(2, 3)),
            ("sorteado 8 (40%)", Pares.Sorteado(8, 0.4, 3)),
            ("sorteado 9 (30%)", Pares.Sorteado(9, 0.3, 5)),
        })
            linhas.Add(new LinhaSimetria(
                nome, grafo.Vertices, grafo.Arestas,
                ForcaBruta.Automorfismos(grafo),
                Refinamento.Refinar(grafo).Quantas,
                Refinamento.EhRegular(grafo)));

        return linhas;
    }
}
