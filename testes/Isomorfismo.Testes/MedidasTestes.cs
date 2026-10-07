using Conde.Isomorfismo;
using Xunit;

namespace Conde.Isomorfismo.Testes;

/// <summary>
/// Os testes das TABELAS que o medidor imprime.
///
/// Eles existem porque numero em README que ninguem confere envelhece errado em
/// silencio: alguem troca uma constante, a tabela muda e o texto continua dizendo
/// a frase antiga. Aqui o teste afirma sobre o mesmo objeto que vira linha
/// impressa.
/// </summary>
public class MedidasTestes
{
    [Fact]
    public void AForcaBrutaBateComARespostaSabidaEmTodaLinha()
    {
        var linhas = Medidas.Conhecidos();

        Assert.Equal(15, linhas.Count);
        Assert.All(linhas, l => Assert.Equal(l.Isomorfos, l.ForcaBruta));

        // e todo par isomorfo passa por todo filtro, sempre
        foreach (var l in linhas.Where(l => l.Isomorfos))
        {
            Assert.True(l.MesmosGraus, l.Par);
            Assert.True(l.MesmaAssinatura, l.Par);
            Assert.True(l.MesmasDistancias, l.Par);
            Assert.True(l.SobreviveAoRefinamento, l.Par);
        }
    }

    /// <summary>
    /// A frase do texto: nenhum filtro domina os outros, e duas linhas da tabela
    /// mostram isso.
    /// </summary>
    [Fact]
    public void NenhumFiltroDominaOsOutros()
    {
        var linhas = Medidas.Conhecidos();

        // a assinatura separa o prisma do tres por tres, a distancia nao
        var prisma = linhas.First(l => l.Par == "prisma contra tres por tres");

        Assert.False(prisma.Isomorfos);
        Assert.False(prisma.MesmaAssinatura);
        Assert.True(prisma.MesmasDistancias);

        // e a distancia separa o ciclo de oito dos dois quadrados, a assinatura nao
        var ciclo = linhas.First(l => l.Par == "ciclo de 8 contra dois quadrados");

        Assert.False(ciclo.Isomorfos);
        Assert.True(ciclo.MesmaAssinatura);
        Assert.False(ciclo.MesmasDistancias);
    }

    /// <summary>
    /// A frase do texto: em grafo REGULAR o refinamento nao separa NADA, e os dois
    /// invariantes feitos a mao separam quase tudo.
    /// </summary>
    [Fact]
    public void EmGrafoRegularORefinamentoNaoSeparaNada()
    {
        var linhas = Medidas.Filtros(500);

        Assert.Equal(5, linhas.Count);

        var tres = linhas.Single(l => l.Caso == "tres-regulares de 8");

        Assert.Equal(323, tres.Diferentes);
        Assert.Equal(323, tres.FalsosGraus);
        Assert.Equal(323, tres.FalsosRefinamento);
        Assert.Equal(5, tres.FalsosAssinatura);
        Assert.Equal(12, tres.FalsosDistancias);

        var dois = linhas.Single(l => l.Caso == "dois-regulares de 9");

        Assert.Equal(255, dois.Diferentes);
        Assert.Equal(255, dois.FalsosRefinamento);
        Assert.Equal(0, dois.FalsosDistancias);
    }

    /// <summary>
    /// E em grafo sorteado ele separa quase tudo, que e por que ele e usado.
    /// </summary>
    [Fact]
    public void EmGrafoSorteadoORefinamentoSeparaQuaseTudo()
    {
        var linhas = Medidas.Filtros(500);

        var soltos = linhas.Single(l => l.Caso == "sorteados soltos");

        Assert.Equal(500, soltos.Diferentes);
        Assert.Equal(17, soltos.FalsosRefinamento);

        // e um par isomorfo por construcao nunca e separado por filtro nenhum
        var embaralhado = linhas.Single(l => l.Caso == "um embaralhado do outro");

        Assert.Equal(500, embaralhado.Isomorfos);
        Assert.Equal(0, embaralhado.Diferentes);
        Assert.Equal(0, embaralhado.FalsosRefinamento);
    }

    /// <summary>
    /// A frase do texto: a poda corta varias ordens de grandeza, e a poda por cor
    /// acrescenta quase nada.
    /// </summary>
    [Fact]
    public void APodaCortaEAPodaPorCorQuaseNaoAcrescenta()
    {
        var linhas = Medidas.Podas();

        Assert.Equal(15, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.SemPoda <= l.Fatorial, l.Par));
        Assert.All(linhas, l => Assert.True(l.ComPodaECor <= l.ComPoda, l.Par));
        Assert.Equal(1, linhas.Count(l => l.ComPodaECor < l.ComPoda));

        var tres = linhas.First(l => l.Par == "ciclo de 9 contra tres triangulos");

        Assert.Equal(362_880, tres.Fatorial);
        Assert.Equal(362_880, tres.SemPoda);
        Assert.Equal(252, tres.ComPoda);
    }

    /// <summary>
    /// A frase do texto: quanto mais simetrias um grafo tem, menos cores o
    /// refinamento consegue achar.
    /// </summary>
    [Fact]
    public void MaisSimetriaEhMenosCor()
    {
        var linhas = Medidas.Simetrias();

        Assert.Equal(10, linhas.Count);

        // todo grafo regular daqui para com UMA cor so
        foreach (var l in linhas.Where(l => l.Regular)) Assert.Equal(1, l.CoresDoRefinamento);

        // o sem simetria nenhuma fica discreto
        var semSimetria = linhas.Single(l => l.Grafo == "sorteado 9 (30%)");

        Assert.Equal(1, semSimetria.Automorfismos);
        Assert.Equal(semSimetria.Vertices, semSimetria.CoresDoRefinamento);

        // e o completo de seis tem todas as 720 renomeacoes como simetria
        var completo = linhas.Single(l => l.Grafo == "completo de 6");

        Assert.Equal(720, completo.Automorfismos);
        Assert.Equal(1, completo.CoresDoRefinamento);
    }

    /// <summary>
    /// Os grafos regulares sorteados sao regulares de verdade, e isso precisa ser
    /// conferido: a tabela inteira dos filtros depende disso.
    /// </summary>
    [Fact]
    public void OsGrafosRegularesSorteadosSaoRegularesMesmo()
    {
        for (ulong semente = 1; semente <= 50; semente++)
        {
            var tres = Medidas.Regular(8, 3, semente);

            Assert.True(Refinamento.EhRegular(tres));
            Assert.Equal(12, tres.Arestas);
            Assert.All(Enumerable.Range(0, 8), v => Assert.Equal(3, tres.Grau(v)));

            var dois = Medidas.Regular(9, 2, semente);

            Assert.True(Refinamento.EhRegular(dois));
            Assert.Equal(9, dois.Arestas);
        }
    }
}
