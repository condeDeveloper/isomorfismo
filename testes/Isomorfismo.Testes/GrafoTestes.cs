using Conde.Isomorfismo;
using Xunit;

namespace Conde.Isomorfismo.Testes;

public class GrafoTestes
{
    [Fact]
    public void OSorteioEhDeterminista()
    {
        var a = new Sorteio(7);
        var b = new Sorteio(7);

        for (var i = 0; i < 1000; i++) Assert.Equal(a.Proximo(), b.Proximo());
    }

    /// <summary>
    /// A fracao precisa cobrir os DOIS LADOS do meio: um deslocamento de 33 em vez
    /// de 32 deixaria o gerador com 31 bits, e toda decisao de "esta aresta
    /// existe?" com chance acima de meio sairia errada, com os numeros continuando
    /// plausiveis.
    /// </summary>
    [Fact]
    public void AFracaoCobreOsDoisLadosDoMeio()
    {
        var sorteio = new Sorteio(7);
        var acima = 0;
        var maior = 0.0;

        for (var i = 0; i < 10_000; i++)
        {
            var f = sorteio.Fracao();

            Assert.InRange(f, 0.0, 1.0);
            if (f > 0.5) acima++;
            if (f > maior) maior = f;
        }

        Assert.InRange(acima, 4_700, 5_300);
        Assert.True(maior > 0.999, $"o maior sorteio foi {maior}");
    }

    [Fact]
    public void APermutacaoSorteadaEhUmaPermutacao()
    {
        for (ulong semente = 1; semente <= 20; semente++)
        {
            var p = new Sorteio(semente).Permutacao(50);

            Assert.Equal(50, p.Distinct().Count());
            Assert.Equal(Enumerable.Range(0, 50), p.OrderBy(x => x));
        }
    }

    [Fact]
    public void LigarDuasVezesNaoCriaDuasArestas()
    {
        var grafo = new Grafo(3);

        grafo.Ligar(0, 1);
        grafo.Ligar(1, 0);

        Assert.Equal(1, grafo.Arestas);
        Assert.True(grafo.Vizinhos(1, 0));
    }

    [Fact]
    public void LacoNaoEhGrafoSimples() =>
        Assert.Throws<ArgumentException>(() => new Grafo(3).Ligar(1, 1));

    /// <summary>
    /// RENOMEAR preserva tudo o que importa: o numero de vertices, o numero de
    /// arestas e a estrutura.
    ///
    /// E a operacao que define o problema. Se ela estivesse errada, todo par
    /// "isomorfo por construcao" deste repositorio seria uma mentira, e os testes
    /// que dependem deles estariam medindo nada.
    /// </summary>
    [Fact]
    public void RenomearPreservaOGrafo()
    {
        foreach (var (nome, grafo) in new (string, Grafo)[]
        {
            ("ciclo de 8", Pares.Ciclo(8)),
            ("Petersen", Pares.Petersen()),
            ("grade 3x3", Pares.Grade(3, 3)),
            ("sorteado", Pares.Sorteado(9, 0.4, 3)),
        })
            for (ulong semente = 1; semente <= 10; semente++)
            {
                var outro = grafo.Embaralhar(semente);

                Assert.Equal(grafo.Vertices, outro.Vertices);
                Assert.Equal(grafo.Arestas, outro.Arestas);
                Assert.Equal(Invariantes.Graus(grafo), Invariantes.Graus(outro));
            }
    }

    [Fact]
    public void RenomearPelaIdentidadeDaOMesmoGrafo()
    {
        var grafo = Pares.Sorteado(9, 0.4, 3);
        var igual = grafo.Renomear([.. Enumerable.Range(0, 9)]);

        Assert.True(grafo.MesmasArestas(igual));
    }

    [Fact]
    public void UmaRenomeacaoQueNaoCobreOGrafoEhRecusada() =>
        Assert.Throws<ArgumentException>(() => Pares.Ciclo(5).Renomear([0, 1, 2]));

    /// <summary>
    /// O PRISMA e o TRES POR TRES tem o mesmo tamanho, as mesmas arestas em numero
    /// e o mesmo grau em todo vertice, e sao grafos diferentes.
    ///
    /// E o par que esta no centro deste repositorio, e a diferenca entre eles cabe
    /// numa palavra: triangulo. O prisma tem dois, o outro e bipartido e nao tem
    /// nenhum.
    /// </summary>
    [Fact]
    public void OPrismaEOTresPorTresSaoGemeosENaoSaoIguais()
    {
        var prisma = Pares.Prisma();
        var tresPorTres = Pares.TresPorTres();

        Assert.Equal(prisma.Vertices, tresPorTres.Vertices);
        Assert.Equal(prisma.Arestas, tresPorTres.Arestas);
        Assert.Equal(Invariantes.Graus(prisma), Invariantes.Graus(tresPorTres));

        Assert.Equal(2, Invariantes.Triangulos(prisma));
        Assert.Equal(0, Invariantes.Triangulos(tresPorTres));
        Assert.False(ForcaBruta.SaoIsomorfos(prisma, tresPorTres));
    }

    /// <summary>
    /// E o CICLO DE SEIS contra DOIS TRIANGULOS e o menor par que o refinamento de
    /// cores nao separa: seis vertices, seis arestas, todo mundo de grau dois.
    /// </summary>
    [Fact]
    public void OCicloDeSeisEOsDoisTriangulosSaoGemeos()
    {
        var ciclo = Pares.Ciclo(6);
        var dois = Pares.DoisCiclos(3);

        Assert.Equal(6, ciclo.Vertices);
        Assert.Equal(6, ciclo.Arestas);
        Assert.Equal(6, dois.Vertices);
        Assert.Equal(6, dois.Arestas);
        Assert.Equal(Invariantes.Graus(ciclo), Invariantes.Graus(dois));
        Assert.False(ForcaBruta.SaoIsomorfos(ciclo, dois));
    }

    [Fact]
    public void OsGeradoresDaoOQueOsNomesDizem()
    {
        Assert.Equal(15, Pares.Petersen().Arestas);
        Assert.Equal(9, Pares.Prisma().Arestas);
        Assert.Equal(9, Pares.TresPorTres().Arestas);
        Assert.Equal(12, Pares.Grade(3, 3).Arestas);
        Assert.Equal(9, Pares.TresTriangulos().Arestas);
        Assert.Equal(15, Pares.Completo(6).Arestas);
    }

    [Fact]
    public void OsParesConhecidosTemARespostaQueDizemTer()
    {
        foreach (var par in Pares.Conhecidos().Concat(Pares.Regulares()))
            Assert.Equal(par.Isomorfos, ForcaBruta.SaoIsomorfos(par.Um, par.Outro));
    }
}
