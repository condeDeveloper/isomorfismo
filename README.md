# isomorfismo

Isomorfismo de grafos do zero em C# e .NET 8: quatro invariantes, o refinamento
de cores (Weisfeiler e Leman de dimensão um), uma busca com poda, e a
**definição** no papel de juiz — existe uma renomeação dos vértices que leva um
grafo no outro, e o jeito de saber é tentar todas as n! renomeações.

O problema vive num lugar estranho da teoria: ninguém conhece algoritmo rápido
para ele, e ninguém provou que ele é difícil. O que existe são invariantes, e
eles funcionam numa direção só.

```
$ dotnet medidor.dll filtro

  caso                       pares   isomorfos   diferentes   nao separou: graus   assinatura   distancias   refinamento
  sorteados soltos              500           0          500                    4            0            7            17
  mesmo numero de arestas       500           1          499                   22            0           73           175
  um embaralhado do outro       500         500            0                    0            0            0             0
  tres-regulares de 8           500         177          323                  323            5           12           323
  dois-regulares de 9           500         245          255                  255          115            0           255
```

As quatro últimas colunas contam só os pares **diferentes**: um par isomorfo
passa por todo filtro, sempre, e isso é a definição de invariante, não mérito.

Nas duas primeiras linhas o refinamento de cores é o filtro mais fraco e ainda
assim resolve quase tudo. Nas duas últimas, ele não resolve **nada**.

## Em grafo regular, o refinamento não enxerga nada

O refinamento de cores é o método que quase todo programa de isomorfismo usa por
baixo. Comece com todo mundo da mesma cor; a cada rodada, troque a cor de cada
vértice pelo par (cor antiga, cores dos vizinhos em ordem); pare quando o número
de cores parar de crescer.

Num grafo **regular**, todo vértice tem o mesmo grau. A primeira rodada dá a
mesma chave para todo mundo, e a segunda também. Ele para com **uma cor só** e
não tem por onde começar.

```
$ dotnet medidor.dll simetria

  grafo               vertices   arestas   automorfismos   cores do refinamento   regular
  caminho de 6               6         5               2                      3       NAO
  estrela de 5               6         5             120                      2       NAO
  ciclo de 6                 6         6              12                      1       sim
  dois triangulos            6         6              72                      1       sim
  prisma                     6         9              12                      1       sim
  tres por tres              6         9              72                      1       sim
  completo de 6              6        15             720                      1       sim
  grade 2x3                  6         7               4                      2       NAO
  sorteado 8 (40%)           8         9               2                      7       NAO
  sorteado 9 (30%)           9        16               1                      9       NAO
```

Quanto mais simetrias um grafo tem, menos o refinamento consegue ver. No extremo
de baixo, um grafo sorteado sem simetria nenhuma tem **um** automorfismo e fica
**discreto**: cada vértice com uma cor só dele, e aí o isomorfismo, se existir, é
único e sai de graça.

O menor par que ele não separa tem seis vértices: o ciclo de seis contra dois
triângulos. Mesmo número de vértices, mesmo número de arestas, todo mundo de grau
dois, e são grafos diferentes.

## Nenhum filtro domina os outros

```
$ dotnet medidor.dll conhecido

  par                                  vertices   isomorfos   forca bruta   mesmos graus   mesma assinatura   mesmas distancias   sobrevive ao refinamento
  ciclo de 6 contra dois triangulos           6         NAO           NAO            sim                NAO                 NAO                        sim
  prisma contra tres por tres                 6         NAO           NAO            sim                NAO                 sim                        sim
  ciclo de 8 contra dois quadrados            8         NAO           NAO            sim                sim                 NAO                        sim
  caminho de 7 contra estrela de 6            7         NAO           NAO            NAO                NAO                 NAO                        NAO
  Petersen embaralhado                       10         sim           sim            sim                sim                 sim                        sim
```

O **prisma** contra o **três por três**: dois grafos três-regulares de seis
vértices com nove arestas. As distâncias entre todos os pares são idênticas nos
dois. O que os separa é o triângulo: o prisma tem dois, o outro é bipartido e não
tem nenhum.

O **ciclo de oito** contra **dois quadrados**: a assinatura inteira bate, graus,
triângulos, graus dos vizinhos. O que os separa é a distância, porque num ciclo
de oito há pares a distância quatro e em dois quadrados separados metade dos
pares nem se alcança.

Cada um resolve o que o outro não resolve.

## A poda, e o que ela realmente corta

```
$ dotnet medidor.dll poda

  par                                  vertices   renomeacoes que existem   sem poda   com poda   com poda e cor
  ciclo de 8 embaralhado                      8                   40,320      2,936         36               36
  Petersen embaralhado                       10                3,628,800     14,997         55               55
  grade 3x3 embaralhada                       9                  362,880     50,440         45               45
  sorteado embaralhado                        9                  362,880    339,892         72               45
  ciclo de 9 contra tres triangulos           9                  362,880    362,880        252              252
  ciclo de 8 contra dois quadrados            8                   40,320     40,320        328              328
  prisma contra tres por tres                 6                      720        720        150              150
```

A força bruta monta a renomeação inteira e só depois confere. A busca monta um
vértice de cada vez e confere a cada passo: se v foi mandado para w, todo vizinho
de v que já tem destino precisa ser vizinho de w. Uma escolha incompatível é
abandonada ali, sem que as outras sejam sequer geradas.

Mesma definição, mesma resposta, ordem das operações trocada: 362.880 contra 252.

## Como rodar

```
dotnet test testes/Isomorfismo.Testes/Isomorfismo.Testes.csproj
dotnet run --project ferramentas/Medidor/Medidor.csproj -c Release -- tudo
```

Os comandos do medidor são `conhecido`, `filtro`, `poda`, `simetria` e `tudo`.

Nada aqui é medido em segundos: a unidade é **quantas escolhas de destino foram
consideradas**, porque segundo depende de máquina e de quem mais está rodando.

## O que as medidas me corrigiram

**O método com nome na literatura é o pior dos três onde importa.** Eu esperava
que o refinamento de cores dominasse os invariantes feitos à mão, porque ele é o
método de que todo mundo fala e porque ele é, num sentido preciso, o limite do
que se consegue olhando vizinhança.

Em 323 pares de grafos três-regulares genuinamente diferentes, ele não separou
**nenhum**. Contar triângulos, que é uma linha de código, falhou em **cinco**. A
lista de distâncias falhou em doze. Nos dois-regulares de nove vértices, a
distância separou **todos** os 255 e o refinamento, nenhum.

Faz sentido depois de ver: contar triângulos e medir distância olham coisas que o
refinamento, por construção, não consegue ver. Mas eu não teria apostado nisso.

**A poda que faz o trabalho é a mais simples das duas.** Eu tinha certeza de que a
poda por cor seria a parte importante, de novo porque o refinamento é o que tem
nome. Em quinze pares, ela muda o número em **um** deles.

E nos regulares ela não pode mudar nada, por construção: lá o refinamento dá uma
cor só para todo mundo, então comparar cores aceita qualquer par. A poda que
corta de 362.880 para 252 é a de conferir no **meio** em vez de no fim.

**A resposta certa não podia vir de como o par foi montado.** A primeira versão da
tabela dos filtros marcava "dois grafos três-regulares sorteados" como não
isomorfos, porque eu os montei de sementes diferentes. Eles são isomorfos em
**177 de 500 casos**: com oito vértices e grau três, não há tantos grafos assim.
A tabela inteira teria sido ficção.

A resposta passou a vir da busca exata, e que a busca exata concorda com a força
bruta é um teste separado, em seiscentos pares.

## Licença

MIT.
