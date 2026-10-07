using Conde.Isomorfismo;
using Xunit;

namespace Conde.Isomorfismo.Testes;

public class RefinamentoTestes
{
    /// <summary>
    /// A CONTAGEM por cor nao muda com a renomeacao, que e tudo o que o
    /// refinamento promete.
    /// </summary>
    [Fact]
    public void AContagemSobreviveARenomeacao()
    {
        foreach (var grafo in new[]
        {
            Pares.Ciclo(8), Pares.Petersen(), Pares.Grade(3, 3),
            Pares.Sorteado(10, 0.3, 7), Pares.Caminho(9), Pares.Estrela(8),
        })
            for (ulong semente = 1; semente <= 20; semente++)
                Assert.Equal(Refinamento.Contagem(grafo), Refinamento.Contagem(grafo.Embaralhar(semente)));
    }

    /// <summary>
    /// O ACHADO PRINCIPAL: em grafo REGULAR o refinamento para com UMA cor so, e
    /// nao enxerga absolutamente nada.
    ///
    /// Num grafo regular todo vertice tem o mesmo grau, entao a primeira rodada da
    /// a mesma chave para todo mundo, e a segunda tambem. Ele nao tem por onde
    /// comecar.
    ///
    /// E isso nao e um detalhe de implementacao: e uma limitacao do metodo. Dois
    /// grafos regulares do mesmo tamanho e com o mesmo grau SEMPRE sobrevivem ao
    /// refinamento, isomorfos ou nao.
    /// </summary>
    [Fact]
    public void EmGrafoRegularORefinamentoParaComUmaCorSo()
    {
        foreach (var grafo in new[]
        {
            Pares.Ciclo(6), Pares.DoisCiclos(3), Pares.Prisma(), Pares.TresPorTres(),
            Pares.Completo(6), Pares.Petersen(), Pares.TresTriangulos(),
        })
        {
            Assert.True(Refinamento.EhRegular(grafo));
            Assert.Equal(1, Refinamento.Refinar(grafo).Quantas);
        }
    }

    /// <summary>
    /// E o MENOR par que ele nao separa tem seis vertices: o ciclo de seis contra
    /// dois triangulos.
    /// </summary>
    [Fact]
    public void OMenorParQueORefinamentoNaoSeparaTemSeisVertices()
    {
        var ciclo = Pares.Ciclo(6);
        var dois = Pares.DoisCiclos(3);

        Assert.True(Refinamento.Sobrevivem(ciclo, dois));
        Assert.False(ForcaBruta.SaoIsomorfos(ciclo, dois));
    }

    /// <summary>
    /// Em grafo SEM simetria nenhuma ele faz o oposto: deixa cada vertice com uma
    /// cor so dele.
    ///
    /// E ai o isomorfismo, se existir, e UNICO e sai de graca: cada vertice so pode
    /// corresponder ao de mesma cor no outro grafo. O refinamento nao e so um
    /// filtro, e tambem um atalho.
    /// </summary>
    [Fact]
    public void EmGrafoSemSimetriaEleFicaDiscreto()
    {
        var grafo = Pares.Sorteado(9, 0.3, 5);

        Assert.Equal(1, ForcaBruta.Automorfismos(grafo));
        Assert.True(Refinamento.EhDiscreto(grafo));
        Assert.Equal(grafo.Vertices, Refinamento.Refinar(grafo).Quantas);
    }

    /// <summary>
    /// O numero de cores NUNCA passa do numero de vertices, e o refinamento sempre
    /// para.
    ///
    /// Ele para porque o numero de cores so cresce, e ele nao pode crescer para
    /// sempre. O teste existe porque um laco que depende disso e um laco que trava
    /// se a conta de parada estiver um a menos.
    /// </summary>
    [Fact]
    public void ORefinamentoSempreParaENuncaPassaDoNumeroDeVertices()
    {
        foreach (var grafo in new[]
        {
            Pares.Ciclo(12), Pares.Petersen(), Pares.Grade(4, 4),
            Pares.Sorteado(14, 0.2, 3), Pares.Sorteado(14, 0.5, 9), new Grafo(6),
        })
        {
            var r = Refinamento.Refinar(grafo);

            Assert.InRange(r.Quantas, grafo.Vertices == 0 ? 0 : 1, grafo.Vertices);
            Assert.InRange(r.Rodadas, 1, grafo.Vertices + 1);
            Assert.Equal(r.Quantas, r.Cor.Distinct().Count());
        }
    }

    /// <summary>
    /// Dois grafos ISOMORFOS sempre sobrevivem ao refinamento, e isso nao e merito:
    /// e a definicao de invariante.
    ///
    /// Se este teste falhasse, o refinamento estaria dizendo "sao diferentes" sobre
    /// dois grafos iguais, que e o unico erro que um filtro nao pode cometer.
    /// </summary>
    [Fact]
    public void GrafosIsomorfosSempreSobrevivem()
    {
        for (ulong semente = 1; semente <= 100; semente++)
        {
            var grafo = Pares.ComTantasArestas(9, 14, semente);

            Assert.True(Refinamento.Sobrevivem(grafo, grafo.Embaralhar(semente + 500)));
        }
    }

    /// <summary>
    /// E em grafo SORTEADO ele separa quase tudo, que e por que ele e usado.
    /// </summary>
    [Fact]
    public void EmGrafoSorteadoEleSeparaQuaseTudo()
    {
        var sobreviveram = 0;

        for (ulong k = 1; k <= 200; k++)
        {
            var um = Pares.Sorteado(8, 0.4, k);
            var outro = Pares.Sorteado(8, 0.4, k + 7919);

            if (Refinamento.Sobrevivem(um, outro)) sobreviveram++;
        }

        Assert.True(sobreviveram < 20, $"sobreviveram {sobreviveram} de 200");
    }

    [Fact]
    public void OGrafoSemArestaTemUmaCorSo()
    {
        Assert.Equal(1, Refinamento.Refinar(new Grafo(6)).Quantas);
        Assert.True(Refinamento.EhRegular(new Grafo(6)));
    }
}
