using Conde.Isomorfismo;

var pedido = args.Length > 0 ? args[0] : "tudo";
var tudo = pedido == "tudo";

if (tudo || pedido == "conhecido") Conhecidos();
if (tudo || pedido == "filtro") Filtros();
if (tudo || pedido == "poda") Podas();
if (tudo || pedido == "simetria") Simetrias();

static void Conhecidos()
{
    Console.WriteLine();
    Console.WriteLine("  OS PARES DE RESPOSTA SABIDA, CONTRA A FORCA BRUTA E CONTRA CADA FILTRO");
    Console.WriteLine();
    Console.WriteLine("  par                                  vertices   isomorfos   forca bruta   mesmos graus   mesma assinatura   mesmas distancias   sobrevive ao refinamento");

    foreach (var l in Medidas.Conhecidos())
        Console.WriteLine($"  {l.Par,-36} {l.Vertices,8} {Marca(l.Isomorfos),11} {Marca(l.ForcaBruta),13} {Marca(l.MesmosGraus),14} {Marca(l.MesmaAssinatura),18} {Marca(l.MesmasDistancias),19} {Marca(l.SobreviveAoRefinamento),26}");

    Console.WriteLine();
    Console.WriteLine("  as quatro ultimas colunas sao FILTROS: um sim ali quer dizer \"nao sei\", e so");
    Console.WriteLine("  um nao e resposta. A coluna da forca bruta e a unica que responde sim ou nao");
    Console.WriteLine();
}

static void Filtros()
{
    Console.WriteLine();
    Console.WriteLine("  QUANTOS PARES CADA FILTRO DEIXA PASSAR, EM 500 PARES SORTEADOS");
    Console.WriteLine();
    Console.WriteLine("  caso                       pares   isomorfos   diferentes   nao separou: graus   assinatura   distancias   refinamento");

    foreach (var l in Medidas.Filtros())
        Console.WriteLine($"  {l.Caso,-26} {l.Pares,6:N0} {l.Isomorfos,11:N0} {l.Diferentes,12:N0} {l.FalsosGraus,20:N0} {l.FalsosAssinatura,12:N0} {l.FalsosDistancias,12:N0} {l.FalsosRefinamento,13:N0}");

    Console.WriteLine();
    Console.WriteLine("  as quatro ultimas colunas contam so os pares DIFERENTES: um par isomorfo");
    Console.WriteLine("  passa por todo filtro sempre, e isso e a definicao de invariante, nao merito");
    Console.WriteLine();
}

static void Podas()
{
    Console.WriteLine();
    Console.WriteLine("  O CUSTO DA BUSCA, EM ESCOLHAS CONSIDERADAS");
    Console.WriteLine();
    Console.WriteLine("  par                                  vertices   renomeacoes que existem   sem poda   com poda   com poda e cor");

    foreach (var l in Medidas.Podas())
        Console.WriteLine($"  {l.Par,-36} {l.Vertices,8} {l.Fatorial,24:N0} {l.SemPoda,10:N0} {l.ComPoda,10:N0} {l.ComPodaECor,16:N0}");

    Console.WriteLine();
    Console.WriteLine("  a mesma resposta, pela mesma definicao, com a ordem das operacoes trocada");
    Console.WriteLine();
}

static void Simetrias()
{
    Console.WriteLine();
    Console.WriteLine("  AS SIMETRIAS DE CADA GRAFO, E O QUE O REFINAMENTO CONSEGUE VER");
    Console.WriteLine();
    Console.WriteLine("  grafo               vertices   arestas   automorfismos   cores do refinamento   regular");

    foreach (var l in Medidas.Simetrias())
        Console.WriteLine($"  {l.Grafo,-19} {l.Vertices,8} {l.Arestas,9} {l.Automorfismos,15:N0} {l.CoresDoRefinamento,22} {Marca(l.Regular),9}");

    Console.WriteLine();
    Console.WriteLine("  grafo sem simetria nenhuma tem UM automorfismo e fica discreto no");
    Console.WriteLine("  refinamento: a resposta sai de graca. Grafo muito simetrico tem muitos, o");
    Console.WriteLine("  refinamento para com uma cor so, e sobra a busca");
    Console.WriteLine();
}

static string Marca(bool valor) => valor ? "sim" : "NAO";
