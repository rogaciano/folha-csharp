# Integracao Dapic para modulo de producao

Data: 2026-06-09

Fonte analisada:

- Documentacao publica: `https://docs.dapic.app/`
- Base URL informada pela documentacao: `https://api.dapic.app`
- Colecao publica Postman carregada pela documentacao Dapic.

## Objetivo

Planejar a integracao entre o sistema RH Folha e o Dapic usado pela Webpic, com foco em colaboradores que recebem por producao.

O objetivo inicial nao e escrever dados no Dapic, mas consumir informacoes de apoio para:

- importar funcionarios;
- importar referencias/produtos;
- importar ordens de producao;
- importar produtos das ordens;
- importar operacoes de producao;
- importar celulas de producao;
- usar ficha tecnica do produto como apoio ao apontamento;
- lancar producao por colaborador no RH Folha;
- calcular pagamento por producao dentro da competencia da folha.

## Observacoes gerais da API

A documentacao informa:

- API REST;
- respostas em JSON;
- autenticacao por Bearer Token;
- obtencao de token com `Empresa` e `TokenIntegracao`;
- token com duracao de 1 dia;
- limite de 100 requisicoes por minuto por endpoint/chave;
- paginacao por `Pagina` e `RegistrosPorPagina`;
- limite maximo de 200 registros por pagina;
- API focada em consulta, sem inclusao/modificacao geral de dados.

Isso indica que a primeira fase da integracao deve ser uma sincronizacao controlada do Dapic para o RH Folha.

## Endpoints relevantes encontrados

### Autenticacao

```http
POST https://api.dapic.app/autenticacao/v1/login
```

Uso esperado:

- gerar `access_token`;
- armazenar o token temporariamente na aplicacao;
- renovar quando expirar.

Payload documentado:

```json
{
  "Empresa": "identificador_da_empresa",
  "TokenIntegracao": "token_de_integracao"
}
```

### Funcionarios

```http
GET https://api.dapic.app/v1/funcionarios?RegistrosPorPagina=50
```

Utilidade:

- importar funcionarios cadastrados no Dapic;
- permitir vincular `Employee` do RH Folha com `FuncionarioId` do Dapic;
- evitar digitar nomes manualmente quando a base ja existir no ERP.

Exemplo de campos observados:

```json
{
  "Id": 1,
  "Nome": "Maria Oliveira",
  "Fantasia": "Maria",
  "NomeExibicao": "Maria Oliveira"
}
```

Limite:

- a listagem de funcionarios parece simples; nao foram observados campos trabalhistas completos como matricula CLT, cargo, setor, admissao ou salario.
- portanto, a importacao deve ser usada como apoio/cadastro inicial, nao como fonte completa de folha.

### Produtos / referencias

```http
GET https://api.dapic.app/v1/produtos?RegistrosPorPagina=50
GET https://api.dapic.app/v1/produtos/id_produto?incluirFichaTecnica=false
```

Utilidade:

- importar referencias dos produtos;
- usar referencia/descricao na tela de lancamento de producao;
- relacionar produto com ordem de producao e operacao.

Campos observados na listagem:

```json
{
  "Id": 1,
  "Referencia": "CMC 001",
  "DescricaoFabrica": "Camisa manga longa",
  "DataCadastro": "2024-01-10T10:00:00",
  "DataModificacao": "2026-05-10T09:00:00",
  "Status": "Ativo"
}
```

### Ficha tecnica do produto

```http
GET https://api.dapic.app/v1/produtos/id_produto/ficha-tecnica
```

Utilidade:

- consultar operacoes e consumos associados ao produto;
- apoiar tabela de valores por operacao/produto;
- sugerir operacoes disponiveis no apontamento de producao.

Observacao:

- a documentacao indica que esse endpoint retorna detalhes, operacoes e consumos da ficha tecnica;
- fichas tecnicas particulares de cliente nao sao retornadas.

### Ordens de producao

```http
GET https://api.dapic.app/v1/ordensproducao
GET https://api.dapic.app/v1/ordensproducao/id_ordem_producao
GET https://api.dapic.app/v1/ordensproducao/produtos?DataInicial=2024-01-01&DataFinal=2024-06-01&Pagina=2&RegistrosPorPagina=100
```

Utilidade:

- importar ordens de producao;
- vincular apontamentos de producao a uma ordem;
- filtrar por periodo;
- importar produtos/referencias presentes nas ordens.

Recomendacao:

- no RH Folha, criar uma entidade local de `ProductionOrder` com `ExternalId`;
- criar `ProductionOrderProduct` para os produtos/referencias da ordem;
- guardar numero/codigo/status/data quando disponiveis na resposta real.

### Operacoes de producao

```http
GET https://api.dapic.app/v1/operacoesproducao?RegistrosPorPagina=50
```

Utilidade:

- importar operacoes como corte, costura, acabamento, embalagem etc.;
- usar operacao no lancamento de producao;
- permitir tabela de valor por operacao, produto ou cargo.

Recomendacao:

- criar `ProductionOperation` local com `ExternalId`, nome, status e metadados.

### Celulas de producao

```http
GET https://api.dapic.app/v1/celulasproducao?RegistrosPorPagina=50
```

Utilidade:

- importar celulas/linhas/equipes de producao;
- apoiar filtros e relatorios por celula;
- possivel relacao futura com setor ou responsavel.

Recomendacao:

- no MVP, tratar celula Dapic como classificacao operacional separada de setor de RH;
- depois podemos mapear celula para setor, se fizer sentido na Webpic.

### Saida para mao de obra

```http
GET https://api.dapic.app/v1/saidasmaodeobra
GET https://api.dapic.app/v1/saidasmaodeobra/id_saida_mao_obra
```

Utilidade provavel:

- pode indicar distribuicao/envio de itens para producao ou terceiro;
- pode ser importante para apontamento por lote ou ordem.

Ponto a confirmar:

- a documentacao lista o endpoint, mas precisamos testar com credenciais reais para entender campos e relacao com funcionarios, operacoes e ordens.

## O que serve diretamente para o RH Folha

Serve diretamente:

- funcionarios;
- produtos/referencias;
- ordens de producao;
- produtos das ordens de producao;
- operacoes de producao;
- celulas de producao;
- ficha tecnica do produto.

Serve como apoio futuro:

- movimentacoes de estoque;
- saidas para mao de obra;
- composicoes de produtos;
- grades de produtos;
- grades de tamanhos;
- linhas e grupos de produtos.

Nao parece atender diretamente:

- lancamento de producao por funcionario no Dapic;
- envio de producao calculada pelo RH Folha para o Dapic;
- folha de pagamento;
- contratos/cargos/setores trabalhistas completos.

## Proposta de desenho para nosso sistema

### Entidades locais sugeridas

`ExternalIntegration`

- provedor: `Dapic`;
- empresa;
- identificador da empresa;
- token de integracao criptografado ou armazenado como segredo;
- status;
- data da ultima sincronizacao.

`ExternalSyncLog`

- provedor;
- recurso sincronizado;
- inicio/fim;
- status;
- quantidade importada;
- mensagem de erro.

`DapicEmployeeLink`

- `EmployeeId`;
- `DapicFuncionarioId`;
- nome importado;
- data da vinculacao.

`ProductionProduct`

- `ExternalId`;
- referencia;
- descricao;
- status;
- data de cadastro/modificacao no Dapic.

`ProductionOrder`

- `ExternalId`;
- numero/codigo quando existir;
- data;
- status;
- observacao;
- periodo/importacao.

`ProductionOrderProduct`

- `ProductionOrderId`;
- `ProductionProductId`;
- quantidade planejada quando existir;
- grade/tamanho/cor quando existir.

`ProductionOperation`

- `ExternalId`;
- nome;
- status.

`ProductionCell`

- `ExternalId`;
- nome;
- status.

`EmployeeProductionEntry`

- colaborador;
- competencia;
- data;
- ordem de producao;
- produto/referencia;
- operacao;
- celula;
- quantidade;
- valor unitario;
- valor total;
- origem: manual, importado, ajuste;
- observacao;
- status: aberto, conferido, integrado na folha, cancelado.

### Regra de folha

No fechamento/calculo da competencia:

1. buscar producoes conferidas dentro da competencia;
2. somar por colaborador;
3. gerar item de provento de producao;
4. manter rastreabilidade dos apontamentos que formaram o total;
5. bloquear alteracao depois da folha fechada, salvo reabertura controlada.

## Fluxo operacional recomendado

1. Configurar credenciais Dapic.
2. Sincronizar funcionarios.
3. Vincular funcionarios Dapic aos colaboradores do RH Folha.
4. Sincronizar produtos/referencias.
5. Sincronizar operacoes de producao.
6. Sincronizar celulas de producao.
7. Sincronizar ordens de producao e produtos das ordens.
8. Lancar producao por colaborador usando dados sincronizados.
9. Conferir producao.
10. Calcular folha considerando producao conferida.

## Regra de prioridade da tabela de valores de producao

Ao registrar um apontamento de producao, o sistema deve buscar uma tabela de valores ativa na data do apontamento e escolher a regra aplicavel mais especifica.

Dados considerados no apontamento:

- produto/referencia;
- operacao;
- celula;
- setor do colaborador;
- cargo do colaborador;
- quantidade produzida;
- data do apontamento.

### Filtro inicial

Uma regra de valor so pode ser candidata quando:

- pertence a uma tabela ativa;
- a data do apontamento esta dentro da vigencia da tabela;
- todos os criterios preenchidos na regra batem com o apontamento;
- criterios vazios na regra significam "todos";
- a quantidade do apontamento respeita `minimum_quantity` e `maximum_quantity`, quando informados;
- a regra esta ativa e nao possui exclusao virtual.

Exemplo:

- regra com apenas operacao vale para qualquer produto/celula/setor/cargo daquela operacao;
- regra com produto + operacao vale somente para aquele produto naquela operacao;
- regra com quantidade minima 101 vale apenas para apontamentos com quantidade igual ou maior que 101.

### Pontuacao de especificidade

Entre as regras candidatas, o sistema deve calcular uma pontuacao:

- produto informado: `+32`;
- operacao informada: `+16`;
- celula informada: `+8`;
- setor informado: `+4`;
- cargo informado: `+2`;
- faixa de quantidade informada: `+1`.

A regra com maior pontuacao vence.

Essa pontuacao evita depender da ordem de cadastro e protege a folha contra escolha manual inconsistente.

### Ordem conceitual de prioridade

A pontuacao acima representa, na pratica, a seguinte ordem:

1. produto + operacao + celula + setor + cargo;
2. produto + operacao + celula;
3. produto + operacao;
4. operacao + celula;
5. operacao;
6. produto;
7. setor + cargo;
8. cargo;
9. setor;
10. regra geral.

### Desempate

Se duas ou mais regras tiverem a mesma pontuacao, aplicar desempate nesta ordem:

1. regra com produto informado vence;
2. depois regra com operacao informada;
3. depois regra com celula informada;
4. depois regra com setor informado;
5. depois regra com cargo informado;
6. depois regra com maior `minimum_quantity`;
7. depois regra com menor `maximum_quantity`;
8. se ainda houver empate, bloquear o calculo e exigir ajuste da tabela.

O sistema nao deve escolher uma regra ambigua silenciosamente.

### Vigencia e historico

- Tabela em `Draft` pode ser editada.
- Tabela `Active` ou `Inactive` nao deve ter valores editados diretamente.
- Alteracao de valor vigente deve ser feita por `Nova vigencia`, copiando a tabela anterior para rascunho.
- Tabelas e regras usadas em folha fechada devem permanecer rastreaveis.
- Exclusao de regra deve ser virtual.

### Exemplos

Exemplo 1:

- regra A: operacao costura, valor `R$ 1,00`;
- regra B: produto camisa + operacao costura, valor `R$ 1,30`.

Apontamento de camisa/costura usa a regra B, pois ela e mais especifica.

Exemplo 2:

- regra A: operacao embalagem, quantidade 1 a 100, valor `R$ 0,50`;
- regra B: operacao embalagem, quantidade minima 101, valor `R$ 0,60`.

Apontamento com quantidade 120 usa a regra B.

Exemplo 3:

- duas regras ativas com produto + operacao e mesma faixa de quantidade.

O sistema deve bloquear o calculo e pedir ajuste da tabela para remover a ambiguidade.

## Cuidados tecnicos

- Respeitar limite de 100 requisicoes/minuto.
- Usar paginacao com ate 200 registros por pagina.
- Salvar `ExternalId` de todos os registros importados.
- Nao sobrescrever dados manuais sem confirmacao.
- Criar rotina de sincronizacao manual no MVP; automatizar depois.
- Registrar auditoria das sincronizacoes e vinculos.
- Tratar erro 401 renovando token.
- Tratar erro 429 com espera e nova tentativa.
- Evitar sincronizar endpoints volumosos sem filtro de data.

## Duvidas para validar com a Webpic/Dapic

1. O funcionario do Dapic representa exatamente o colaborador de producao da folha?
2. A ordem de producao possui status que indique aberta, em andamento e concluida?
3. Os produtos da ordem trazem quantidade planejada, grade, cor e referencia?
4. A ficha tecnica traz valor de operacao ou somente operacao/consumo?
5. O endpoint de saida para mao de obra relaciona funcionario, ordem, produto e quantidade?
6. Existe endpoint de apontamento/producao realizada por funcionario?
7. Existe webhook ou somente consulta via polling?
8. A Webpic usa celula de producao como setor/equipe?
9. Qual campo deve ser usado como chave estavel: `Id`, referencia ou outro codigo?
10. Podemos obter ambiente de teste/token para validar respostas reais?

## Recomendacao

Fazer a integracao em fases.

Fase 1:

- tela de configuracao Dapic;
- teste de conexao;
- sincronizacao manual de funcionarios, produtos, operacoes, celulas e ordens;
- vinculo manual entre funcionario Dapic e colaborador RH Folha.

Fase 2:

- modulo de lancamento de producao por colaborador;
- filtros por competencia, colaborador, ordem, produto, operacao e celula;
- conferencia e bloqueio apos integracao na folha.

Fase 3:

- gerar provento de producao automaticamente no calculo da folha;
- relatorio de producao por colaborador;
- auditoria completa.

Fase 4:

- sincronizacao automatica agendada;
- tratamento avancado de divergencias;
- possivel integracao com saida para mao de obra, se confirmado que o endpoint atende ao processo.
