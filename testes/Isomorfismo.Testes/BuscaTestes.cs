using Conde.Isomorfismo;
using Xunit;

namespace Conde.Isomorfismo.Testes;

/// <summary>
/// O juiz tambem precisa de juiz.
///
/// A busca com poda e usada como resposta certa nas tabelas grandes, porque a
/// forca bruta nao chega la. Entao ela precisa ser conferida contra a forca bruta
/// onde as duas cabem, em muitos pares, e e isso que este arquivo faz.
/// </summary>
public class BuscaTestes
{
    /// <summary>
    /// A BUSCA COM PODA da a mesma resposta que a forca bruta, em todo par.
    ///
    /// Sao seiscentos pares de feitios diferentes. Se as duas discordassem num so,
    /// todas as tabelas grandes deste repositorio perderiam o chao.
    /// </summary>
    [Fact]
    public void ABuscaConcordaComAForcaBrutaEmTodoPar()
    {
        foreach (var (caso, montar) in Medidas.Casos())
            for (var k = 1UL; k <= 120; k++)
            {
                var (um, outro) = montar(k);
                var pelaForcaBruta = ForcaBruta.SaoIsomorfos(um, outro);

                Assert.Equal(pelaForcaBruta, Busca.Procurar(um, outro).Achou);
                Assert.Equal(pelaForcaBruta, Busca.Procurar(um, outro, false).Achou);
            }
    }

    /// <summary>
    /// E a renomeacao que ela devolve FUNCIONA: aplicada a um grafo, da o outro.
    ///
    /// Devolver o sim sozinho nao da para auditar. Com a renomeacao na mao, quem
    /// duvida aplica e olha, e e isso que este teste faz.
    /// </summary>
    [Fact]
    public void ARenomeacaoDevolvidaFuncionaDeVerdade()
    {
        foreach (var par in Pares.Conhecidos().Concat(Pares.Regulares()))
        {
            var r = Busca.Procurar(par.Um, par.Outro);

            Assert.Equal(par.Isomorfos, r.Achou);

            if (!r.Achou) continue;

            Assert.True(par.Um.Renomear(r.Isomorfismo!).MesmasArestas(par.Outro), par.Nome);
        }
    }

    [Fact]
    public void AForcaBrutaTambemDevolveUmaRenomeacaoQueFunciona()
    {
        foreach (var par in Pares.Conhecidos().Where(p => p.Isomorfos))
        {
            var nome = ForcaBruta.UmIsomorfismo(par.Um, par.Outro);

            Assert.NotNull(nome);
            Assert.True(par.Um.Renomear(nome).MesmasArestas(par.Outro), par.Nome);
        }
    }

    /// <summary>
    /// O ACHADO DO CUSTO: a poda corta o trabalho por varias ordens de grandeza, e
    /// a resposta e exatamente a mesma.
    ///
    /// No ciclo de nove contra tres triangulos, a forca bruta considera as 362.880
    /// renomeacoes e a busca com poda considera 252. Mesma definicao, mesma
    /// resposta, ordem das operacoes trocada.
    /// </summary>
    [Fact]
    public void APodaCortaVariasOrdensDeGrandeza()
    {
        var ciclo = Pares.Ciclo(9);
        var tres = Pares.TresTriangulos();

        var semPoda = Busca.SemPoda(ciclo, tres);
        var comPoda = Busca.Procurar(ciclo, tres);

        Assert.False(semPoda.Achou);
        Assert.False(comPoda.Achou);
        Assert.Equal(362_880, semPoda.Nos);
        Assert.True(comPoda.Nos < semPoda.Nos / 1_000, $"com poda considerou {comPoda.Nos}");
    }

    /// <summary>
    /// O ACHADO QUE ME CORRIGIU: a poda POR COR quase nao acrescenta nada por cima
    /// da poda por consistencia.
    ///
    /// Eu esperava que ela fosse a parte importante, porque o refinamento e o
    /// metodo de que todo mundo fala. Em quinze pares, ela so muda o numero em UM
    /// deles, e nos regulares ela nao pode mudar nada por construcao: la o
    /// refinamento da uma cor so para todo mundo.
    ///
    /// A poda que faz o trabalho e a mais simples das duas: conferir no MEIO, e nao
    /// no fim, se a aresta vira aresta.
    /// </summary>
    [Fact]
    public void APodaPorCorQuaseNaoAcrescenta()
    {
        var mudou = 0;
        var pares = Pares.Conhecidos().Concat(Pares.Regulares()).ToList();

        foreach (var par in pares)
        {
            var semCor = Busca.Procurar(par.Um, par.Outro, false);
            var comCor = Busca.Procurar(par.Um, par.Outro);

            Assert.Equal(semCor.Achou, comCor.Achou);
            Assert.True(comCor.Nos <= semCor.Nos, par.Nome);

            if (comCor.Nos != semCor.Nos) mudou++;
        }

        Assert.Equal(15, pares.Count);
        Assert.Equal(1, mudou);
    }

    /// <summary>
    /// E nos grafos REGULARES ela nao pode mudar nada: o refinamento da uma cor so
    /// para todo mundo, entao a comparacao de cores aceita qualquer par.
    /// </summary>
    [Fact]
    public void NosRegularesAPodaPorCorNaoMudaNada()
    {
        foreach (var par in Pares.Regulares())
        {
            Assert.True(Refinamento.EhRegular(par.Um));
            Assert.Equal(
                Busca.Procurar(par.Um, par.Outro, false).Nos,
                Busca.Procurar(par.Um, par.Outro).Nos);
        }
    }

    [Fact]
    public void ComNumeroDeArestasDiferenteNinguemGastaNada()
    {
        var r = Busca.Procurar(Pares.Completo(6), Pares.Ciclo(6));

        Assert.False(r.Achou);
        Assert.Equal(0, r.Nos);
    }

    /// <summary>
    /// As PERMUTACOES do algoritmo de Heap sao todas, e sao diferentes entre si.
    ///
    /// A forca bruta inteira se apoia nisso: uma permutacao faltando faria o juiz
    /// dizer "nao sao isomorfos" sobre grafos que sao, e nada avisaria.
    /// </summary>
    [Fact]
    public void AsPermutacoesSaoTodasEDiferentes()
    {
        for (var n = 1; n <= 7; n++)
        {
            var todas = ForcaBruta.Permutacoes(n).Select(p => string.Join(",", p)).ToList();

            Assert.Equal(ForcaBruta.Fatorial(n), todas.Count);
            Assert.Equal(ForcaBruta.Fatorial(n), todas.Distinct().Count());
        }
    }

    [Fact]
    public void AForcaBrutaRecusaGrafoGrandeDemaisEmVezDeDemorarPraSempre()
    {
        Assert.Throws<ArgumentException>(() => ForcaBruta.UmIsomorfismo(new Grafo(12), new Grafo(12)));
        Assert.Throws<ArgumentException>(() => ForcaBruta.Quantos(new Grafo(10), new Grafo(10)));
    }
}
