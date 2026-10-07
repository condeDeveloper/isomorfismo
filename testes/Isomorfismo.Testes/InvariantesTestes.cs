using Conde.Isomorfismo;
using Xunit;

namespace Conde.Isomorfismo.Testes;

/// <summary>
/// Um invariante que muda com a renomeacao nao e invariante, e e esse o unico
/// teste que todos eles precisam passar.
///
/// Se ele mudasse, o filtro diria "sao diferentes" sobre dois grafos iguais, que
/// e o unico erro que um filtro NAO pode cometer: dizer "nao sei" custa tempo,
/// dizer "sao diferentes" errado custa a resposta.
/// </summary>
public class InvariantesTestes
{
    public static IEnumerable<object[]> OsGrafos()
    {
        yield return [Pares.Ciclo(8)];
        yield return [Pares.Petersen()];
        yield return [Pares.Grade(3, 3)];
        yield return [Pares.Prisma()];
        yield return [Pares.TresPorTres()];
        yield return [Pares.Completo(7)];
        yield return [Pares.Sorteado(9, 0.4, 3)];
        yield return [Pares.Sorteado(10, 0.25, 5)];
    }

    [Theory]
    [MemberData(nameof(OsGrafos))]
    public void TodoInvarianteSobreviveARenomeacao(Grafo grafo)
    {
        for (ulong semente = 1; semente <= 20; semente++)
        {
            var outro = grafo.Embaralhar(semente);

            Assert.Equal(Invariantes.Graus(grafo), Invariantes.Graus(outro));
            Assert.Equal(Invariantes.Triangulos(grafo), Invariantes.Triangulos(outro));
            Assert.Equal(Invariantes.GrausDosVizinhos(grafo), Invariantes.GrausDosVizinhos(outro));
            Assert.Equal(Invariantes.Distancias(grafo), Invariantes.Distancias(outro));
            Assert.Equal(Invariantes.Assinatura(grafo), Invariantes.Assinatura(outro));
            Assert.Equal(Refinamento.Contagem(grafo), Refinamento.Contagem(outro));
        }
    }

    /// <summary>
    /// E todo invariante vale numa direcao SO: assinatura igual nao faz grafo
    /// igual.
    ///
    /// E o caso do ciclo de oito contra dois quadrados: mesmos vertices, mesmas
    /// arestas, mesmos graus, zero triangulos nos dois, mesmos graus de vizinhos. A
    /// assinatura inteira bate e os grafos sao diferentes.
    /// </summary>
    [Fact]
    public void AssinaturaIgualNaoFazGrafoIgual()
    {
        var ciclo = Pares.Ciclo(8);
        var dois = Pares.DoisCiclos(4);

        Assert.True(Invariantes.MesmaAssinatura(ciclo, dois));
        Assert.False(ForcaBruta.SaoIsomorfos(ciclo, dois));
    }

    /// <summary>
    /// A DISTANCIA separa esse par, e e por isso que ela esta aqui: nenhum filtro
    /// deste repositorio domina os outros.
    ///
    /// No ciclo de oito, ha pares de vertices a distancia quatro. Em dois quadrados
    /// separados, a distancia maxima e dois, e metade dos pares nem se alcanca.
    /// </summary>
    [Fact]
    public void ADistanciaSeparaOQueAAssinaturaNaoSepara()
    {
        Assert.False(Invariantes.MesmasDistancias(Pares.Ciclo(8), Pares.DoisCiclos(4)));
        Assert.False(Invariantes.MesmasDistancias(Pares.Ciclo(6), Pares.DoisCiclos(3)));
    }

    /// <summary>
    /// E a ASSINATURA separa o que a distancia nao separa: o prisma contra o tres
    /// por tres tem exatamente as mesmas distancias.
    ///
    /// Os dois sao tres-regulares de seis vertices com diametro dois, e a lista de
    /// distancias deles e identica. O que os separa e o triangulo.
    /// </summary>
    [Fact]
    public void AAssinaturaSeparaOQueADistanciaNaoSepara()
    {
        var prisma = Pares.Prisma();
        var tresPorTres = Pares.TresPorTres();

        Assert.True(Invariantes.MesmasDistancias(prisma, tresPorTres));
        Assert.False(Invariantes.MesmaAssinatura(prisma, tresPorTres));
    }

    [Fact]
    public void OsTriangulosSaoOQueOsNomesDizem()
    {
        Assert.Equal(0, Invariantes.Triangulos(Pares.Ciclo(6)));
        Assert.Equal(2, Invariantes.Triangulos(Pares.DoisCiclos(3)));
        Assert.Equal(1, Invariantes.Triangulos(Pares.Ciclo(3)));
        Assert.Equal(20, Invariantes.Triangulos(Pares.Completo(6)));
        Assert.Equal(0, Invariantes.Triangulos(Pares.Petersen()));
    }

    [Fact]
    public void ASequenciaDeGrausVemOrdenada()
    {
        var graus = Invariantes.Graus(Pares.Estrela(5));

        Assert.Equal([1, 1, 1, 1, 1, 5], graus);
    }

    /// <summary>
    /// A lista de distancias conta UM numero por par de vertices, e so uma vez
    /// cada.
    /// </summary>
    [Fact]
    public void ALIstaDeDistanciasTemUmNumeroPorPar()
    {
        foreach (var n in new[] { 4, 7, 10 })
            Assert.Equal(n * (n - 1) / 2, Invariantes.Distancias(Pares.Ciclo(n)).Length);
    }

    /// <summary>
    /// Em grafo desconexo a distancia e menos um, e isso precisa aparecer na lista:
    /// nao alcancar e informacao, e das mais fortes.
    /// </summary>
    [Fact]
    public void NaoAlcancarContaComoDistanciaNegativa()
    {
        var distancias = Invariantes.Distancias(Pares.DoisCiclos(3));

        Assert.Equal(9, distancias.Count(d => d < 0));
    }
}
