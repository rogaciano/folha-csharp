# Fluxo de telas do MVP - Sistema RH e Folha

Data: 2026-06-04

## 1. Objetivo

Definir as telas, fluxos, acoes e estados do MVP do sistema de RH e folha.

Este documento conecta o modelo de dados aprovado com a experiencia operacional do usuario. Ainda nao define stack, rotas tecnicas ou componentes finais.

## 2. Perfil do MVP

O MVP deve permitir que um usuario administrativo cadastre a estrutura da empresa, cadastre colaboradores, configure rubricas, alimente uma competencia, calcule a folha, revise os resultados, feche a competencia e emita demonstrativos/relatorios basicos.

Usuario principal:

- operador/administrador de RH/DP.

Usuarios previstos:

- administrador do sistema;
- operador de folha;
- gestor/visualizador, em fase posterior do MVP se necessario.

## 3. Menu principal

Menu sugerido:

- Dashboard
- Colaboradores
- Estrutura
- Rubricas
- Competencias
- Lancamentos
- Adiantamentos
- Producao
- Folha
- Holerites
- Relatorios
- Configuracoes
- Auditoria

Agrupamento alternativo para simplificar MVP:

- Dashboard
- Cadastros
- Lancamentos
- Folha
- Relatorios
- Configuracoes

Recomendacao para primeira versao: usar menu completo se a UI ficar clara; caso contrario agrupar em secoes.

## 4. Dashboard

Objetivo:

- Dar visao rapida da competencia atual e pendencias antes do fechamento.

Informacoes:

- Competencia ativa.
- Status da competencia: aberta, em calculo, calculada, aprovada, fechada.
- Total de colaboradores ativos.
- Total bruto preliminar.
- Total de descontos preliminar.
- Total liquido preliminar.
- Total FGTS informativo.
- Adiantamentos abertos.
- Lancamentos pendentes.
- Producoes pendentes.
- Inconsistencias da folha.

Acoes:

- Selecionar competencia ativa.
- Ir para lancamentos.
- Ir para producao.
- Ir para calculo da folha.
- Ir para fechamento.

Estados:

- Sem competencia aberta.
- Competencia aberta sem dados.
- Competencia com pendencias.
- Competencia calculada.
- Competencia fechada.

## 5. Cadastros

### 5.1 Empresas

Objetivo:

- Cadastrar e manter dados da empresa.

Campos principais:

- razao social;
- nome fantasia;
- CPF/CNPJ;
- inscricoes;
- CNAE;
- endereco;
- telefone;
- email;
- status.

Acoes:

- criar empresa;
- editar empresa;
- ativar/inativar;
- visualizar detalhes.

Observacao:

- Se o MVP for monoempresa, essa tela pode aparecer apenas em Configuracoes.

### 5.2 Estabelecimentos

Objetivo:

- Cadastrar matriz/filial/unidade.

Campos principais:

- empresa;
- nome;
- tipo;
- CPF/CNPJ;
- codigo interno;
- endereco;
- status.

Acoes:

- criar;
- editar;
- inativar;
- listar colaboradores vinculados.

MVP:

- Pode ficar opcional se a operacao inicial for empresa unica sem filiais.

### 5.3 Setores

Objetivo:

- Cadastrar departamentos.

Campos principais:

- nome;
- codigo interno;
- setor pai;
- status.

Acoes:

- criar;
- editar;
- inativar;
- visualizar colaboradores.

### 5.4 Centros de custo

Objetivo:

- Classificacao gerencial e futura integracao com ERP.

Campos principais:

- nome;
- codigo interno;
- status.

Acoes:

- criar;
- editar;
- inativar.

### 5.5 Cargos

Objetivo:

- Cadastrar cargos/funcoes.

Campos principais:

- nome;
- codigo interno;
- CBO;
- descricao;
- status.

Acoes:

- criar;
- editar;
- inativar;
- visualizar colaboradores no cargo.

## 6. Colaboradores

### 6.1 Listagem de colaboradores

Objetivo:

- Encontrar, filtrar e acessar colaboradores.

Filtros:

- nome;
- CPF;
- matricula;
- status;
- setor;
- cargo;
- centro de custo;
- modelo de remuneracao.

Colunas:

- matricula;
- nome;
- CPF;
- cargo;
- setor;
- modelo de remuneracao;
- salario/base;
- status.

Acoes:

- novo colaborador;
- editar;
- visualizar;
- inativar/desligar futuramente;
- abrir lancamentos do colaborador;
- abrir historico;

Estados:

- vazio;
- carregando;
- com filtros sem resultado;
- erro.

### 6.2 Cadastro/edicao de colaborador

Abas sugeridas:

- Dados pessoais
- Contrato
- Remuneracao
- Dependentes
- Dados bancarios
- Historico

Campos de dados pessoais:

- matricula;
- nome;
- nome social;
- CPF;
- RG;
- data de nascimento;
- genero;
- estado civil;
- telefone;
- email;
- endereco;
- status.

Campos de contrato:

- data de admissao;
- estabelecimento;
- setor;
- centro de custo;
- cargo;
- tipo de contrato;
- categoria;
- jornada mensal;
- jornada semanal.

Campos de remuneracao:

- modelo: mensalista, horista, diarista, producao, misto;
- salario base;
- valor hora;
- valor dia;
- valor producao padrao;
- garantia minima;
- percentual padrao de adiantamento.

Campos bancarios:

- banco;
- agencia;
- conta;
- tipo de conta;
- chave Pix.

Acoes:

- salvar;
- cancelar;
- gerar historico ao mudar cargo/setor/salario;
- visualizar lancamentos fixos do colaborador.

Validacoes:

- CPF obrigatorio e unico;
- matricula obrigatoria e unica;
- cargo/setor obrigatorios para contrato ativo;
- salario/base obrigatorio conforme modelo de remuneracao.

## 7. Rubricas

### 7.1 Listagem de rubricas

Objetivo:

- Administrar proventos, descontos e rubricas informativas.

Filtros:

- codigo;
- nome;
- tipo;
- ativa;
- incidencia INSS;
- incidencia FGTS;
- incidencia IRRF.

Colunas:

- codigo;
- nome;
- tipo;
- INSS;
- FGTS;
- IRRF;
- forma de calculo;
- status.

Acoes:

- nova rubrica;
- editar;
- inativar;
- duplicar rubrica;
- abrir vigencias.

### 7.2 Cadastro/edicao de rubrica

Abas sugeridas:

- Identificacao
- Incidencias
- Calculo
- Vigencia
- Integracoes futuras

Campos de identificacao:

- codigo;
- nome;
- descricao;
- tipo: provento, desconto, informativa, base, patronal;
- natureza eSocial;
- ativa.

Campos de permissoes de uso:

- permite lancamento manual;
- permite lancamento em massa;
- permite lancamento fixo.

Campos de incidencia:

- incide INSS;
- incide FGTS;
- incide IRRF;
- incide DSR;
- incide ferias;
- incide 13o;
- incide rescisao;
- compoe salario bruto;
- compoe salario liquido.

Campos de calculo:

- forma de calculo;
- base de calculo;
- formula opcional;
- ordem de exibicao.

Campos de vigencia:

- inicio;
- fim;
- status.

Acoes:

- salvar;
- criar nova vigencia;
- encerrar vigencia;
- inativar rubrica.

Validacoes:

- codigo unico;
- vigencias sem sobreposicao;
- rubrica usada em folha fechada nao pode ser removida.

## 8. Competencias

### 8.1 Listagem de competencias

Objetivo:

- Gerenciar periodos mensais da folha.

Colunas:

- competencia;
- status;
- data abertura;
- data fechamento;
- total liquido;
- responsavel fechamento.

Acoes:

- abrir nova competencia;
- selecionar competencia ativa;
- visualizar detalhes;
- reabrir, se permitido;
- acessar folha.

Estados:

- aberta;
- em calculo;
- calculada;
- aprovada;
- fechada;
- reaberta;
- cancelada.

### 8.2 Detalhe da competencia

Informacoes:

- periodo;
- status;
- quantidade de colaboradores;
- lancamentos avulsos;
- lancamentos fixos aplicaveis;
- adiantamentos;
- producao;
- inconsistencias;
- totais da folha, quando calculada.

Acoes:

- calcular folha;
- aprovar;
- fechar;
- reabrir;
- gerar relatorios.

## 9. Lancamentos

### 9.1 Visao geral de lancamentos

Objetivo:

- Centralizar entradas manuais, fixas e em massa.

Abas:

- Lancamentos avulsos
- Lancamentos fixos
- Lotes em massa

Filtros:

- competencia;
- colaborador;
- rubrica;
- origem;
- status;
- setor;
- cargo.

### 9.2 Lancamento avulso individual

Campos:

- competencia;
- colaborador;
- rubrica;
- data do fato;
- valor;
- quantidade;
- percentual;
- observacao.

Acoes:

- salvar rascunho;
- validar;
- aprovar;
- cancelar.

Validacoes:

- competencia aberta;
- rubrica ativa;
- rubrica permite lancamento manual;
- colaborador ativo na competencia.

### 9.3 Lancamento fixo por colaborador

Pode ser acessado por:

- tela de Lancamentos;
- detalhe do Colaborador.

Campos:

- colaborador;
- rubrica;
- competencia inicio;
- competencia fim;
- tipo: mensal ou parcelado;
- valor;
- quantidade;
- percentual;
- valor total;
- parcelas;
- observacao;
- status.

Acoes:

- criar;
- suspender;
- encerrar;
- cancelar;
- visualizar parcelas processadas.

Validacoes:

- rubrica permite lancamento fixo;
- competencia inicio obrigatoria;
- parcelado exige numero de parcelas ou valor total.

### 9.4 Lancamento em massa

Fluxo:

1. Escolher competencia.
2. Escolher rubrica.
3. Escolher criterio de selecao.
4. Informar valor/percentual/quantidade.
5. Gerar preview.
6. Revisar colaboradores afetados.
7. Aplicar lote.
8. Gerar lancamentos avulsos rastreaveis.

Criterios de selecao:

- todos os ativos;
- por setor;
- por cargo;
- por centro de custo;
- por modelo de remuneracao;
- lista manual de colaboradores.

Campos:

- nome do lote;
- competencia;
- rubrica;
- criterio;
- tipo de aplicacao;
- valor;
- percentual;
- quantidade;
- observacao.

Tela de preview:

- colaborador;
- setor;
- cargo;
- valor calculado;
- status;
- mensagem de erro.

Acoes:

- voltar;
- aplicar lote;
- cancelar lote.

Validacoes:

- competencia aberta;
- rubrica permite lancamento em massa;
- preview obrigatorio antes de aplicar.

## 10. Adiantamentos

### 10.1 Listagem de adiantamentos

Filtros:

- competencia origem;
- competencia desconto;
- colaborador;
- status.

Colunas:

- data;
- colaborador;
- valor;
- competencia desconto;
- status;
- forma de pagamento.

Acoes:

- novo adiantamento;
- cancelar;
- visualizar desconto gerado.

### 10.2 Cadastro de adiantamento

Campos:

- colaborador;
- competencia origem;
- competencia desconto;
- data;
- valor;
- forma de pagamento;
- observacao.

Acoes:

- salvar;
- cancelar.

Regras:

- Ao salvar, o sistema deve programar desconto para a competencia definida.
- No calculo da folha, o adiantamento vira item de desconto.
- Ao fechar a folha, o adiantamento vira descontado.

## 11. Producao

### 11.1 Tipos de producao

Campos:

- codigo;
- nome;
- descricao;
- valor unitario padrao;
- status.

Acoes:

- criar;
- editar;
- inativar.

### 11.2 Lancamento de producao

Filtros:

- competencia;
- colaborador;
- setor;
- status.

Colunas:

- data;
- colaborador;
- tipo;
- quantidade;
- valor unitario;
- valor total;
- status.

Acoes:

- novo lancamento;
- importar/lancar em massa futuramente;
- aprovar;
- cancelar.

Campos do formulario:

- competencia;
- colaborador;
- tipo de producao;
- data;
- quantidade;
- valor unitario;
- observacao.

Validacoes:

- colaborador deve aceitar remuneracao por producao ou mista;
- competencia aberta;
- quantidade maior que zero.

## 12. Folha

### 12.1 Tela principal da folha

Objetivo:

- Processar, revisar, aprovar e fechar a folha.

Cabecalho:

- competencia;
- status;
- periodo;
- quantidade de colaboradores;
- total proventos;
- total descontos;
- total liquido;
- total INSS;
- total IRRF;
- total FGTS.

Abas:

- Resumo
- Colaboradores
- Inconsistencias
- Lancamentos aplicados
- Bases e tributos

Acoes:

- calcular/recalcular;
- aprovar;
- fechar;
- reabrir;
- gerar holerites;
- gerar relatorios.

Estados:

- sem calculo;
- calculando;
- calculada com inconsistencias;
- calculada sem inconsistencias;
- aprovada;
- fechada.

### 12.2 Resumo da folha

Informacoes:

- total de colaboradores;
- total proventos;
- total descontos;
- liquido;
- FGTS informativo;
- INSS;
- IRRF;
- total por setor;
- total por rubrica.

### 12.3 Colaboradores da folha

Colunas:

- matricula;
- nome;
- setor;
- cargo;
- bruto;
- descontos;
- liquido;
- status;
- alertas.

Acoes:

- abrir demonstrativo;
- abrir memoria de calculo;
- revisar lancamentos do colaborador.

### 12.4 Demonstrativo do colaborador

Informacoes:

- dados snapshot do colaborador;
- salario/base;
- proventos;
- descontos;
- bases INSS/FGTS/IRRF;
- INSS;
- IRRF;
- FGTS;
- liquido;
- origem de cada item.

Acoes:

- imprimir;
- gerar PDF, se entrar no MVP;
- abrir memoria de calculo;
- voltar para folha.

### 12.5 Inconsistencias

Exemplos:

- colaborador ativo sem contrato;
- contrato sem cargo/setor;
- mensalista sem salario;
- producao sem valor unitario;
- rubrica sem vigencia;
- lancamento com rubrica inativa;
- adiantamento sem competencia de desconto;
- dependencia de tabela legal ausente.

Colunas:

- severidade;
- colaborador;
- origem;
- mensagem;
- acao sugerida.

Acoes:

- abrir cadastro relacionado;
- ignorar aviso nao bloqueante, se permitido.

Regra:

- Erros bloqueiam fechamento.
- Avisos permitem fechamento com confirmacao e auditoria, se configurado.

### 12.6 Fechamento da folha

Fluxo:

1. Usuario clica em fechar.
2. Sistema verifica inconsistencias.
3. Sistema exibe resumo final.
4. Usuario confirma.
5. Sistema grava snapshot.
6. Sistema bloqueia competencia.
7. Sistema marca adiantamentos descontados.
8. Sistema marca producao/lancamentos como processados.
9. Sistema libera holerites e relatorios.

Confirmacao deve exibir:

- competencia;
- total colaboradores;
- total proventos;
- total descontos;
- total liquido;
- aviso de bloqueio;
- campo de observacao opcional.

## 13. Holerites

### 13.1 Listagem de holerites

Filtros:

- competencia;
- colaborador;
- setor;
- status.

Colunas:

- competencia;
- colaborador;
- liquido;
- status;
- data geracao.

Acoes:

- visualizar;
- imprimir;
- gerar PDF, se aplicavel;
- gerar todos da competencia.

### 13.2 Visualizacao do holerite

Conteudo:

- empresa;
- colaborador;
- competencia;
- cargo/setor;
- tabela de proventos;
- tabela de descontos;
- bases;
- liquido;
- mensagem/observacao.

MVP:

- Pode iniciar com tela/impressao.
- PDF pode ser fase seguinte se nao for essencial.

## 14. Relatorios

Relatorios MVP:

- resumo da folha;
- folha analitica por colaborador;
- total por rubrica;
- total por setor;
- adiantamentos abertos/descontados;
- producao por colaborador;
- lancamentos por competencia.

Filtros comuns:

- competencia;
- setor;
- cargo;
- centro de custo;
- colaborador;
- rubrica.

Acoes:

- visualizar;
- exportar CSV;
- imprimir;
- exportar PDF futuramente.

## 15. Configuracoes

Telas:

- dados da empresa;
- parametros gerais de folha;
- tabelas legais;
- usuarios;
- perfis e permissoes.

Parametros gerais iniciais:

- competencia padrao;
- regra de adiantamento;
- regra de DSR;
- arredondamento;
- permitir reabertura;
- exigir aprovacao antes do fechamento.

Tabelas legais:

- INSS;
- IRRF;
- FGTS;
- deducao por dependente;
- salario minimo, se necessario.

## 16. Auditoria

Objetivo:

- Consultar acoes sensiveis.

Filtros:

- periodo;
- usuario;
- entidade;
- acao;
- colaborador;
- competencia.

Colunas:

- data/hora;
- usuario;
- acao;
- entidade;
- identificador;
- motivo.

Acoes:

- visualizar detalhes;
- comparar antes/depois.

Eventos obrigatorios:

- alteracao salarial;
- alteracao de cargo/setor;
- criacao/cancelamento de lancamento;
- aplicacao de lote em massa;
- calculo da folha;
- aprovacao;
- fechamento;
- reabertura;
- cancelamento.

## 17. Permissoes por tela

Permissoes minimas:

- visualizar cadastros;
- editar cadastros;
- gerenciar rubricas;
- criar lancamentos;
- aprovar lancamentos;
- calcular folha;
- aprovar folha;
- fechar folha;
- reabrir folha;
- visualizar relatorios;
- administrar usuarios;
- visualizar auditoria.

Regra:

- Botoes e acoes devem respeitar permissao e status da competencia.

## 18. Fluxo operacional recomendado

Fluxo mensal:

1. Abrir competencia.
2. Conferir colaboradores ativos.
3. Registrar producao.
4. Registrar adiantamentos.
5. Registrar lancamentos avulsos.
6. Aplicar lancamentos em massa.
7. Conferir lancamentos fixos vigentes.
8. Calcular folha.
9. Resolver inconsistencias.
10. Revisar demonstrativos.
11. Aprovar folha.
12. Fechar folha.
13. Emitir holerites.
14. Consultar relatorios.

## 19. Fora do MVP

Nao implementar na primeira versao:

- portal do colaborador;
- envio automatico de holerite;
- integracao bancaria;
- eSocial;
- ERP;
- ponto/biometria;
- ferias completas;
- afastamentos completos;
- 13o;
- rescisao;
- app mobile.

O modelo de dados ja deixa espaco para esses recursos.

## 20. Proximas decisoes

Antes da escolha da stack, validar:

- menu completo ou agrupado;
- PDF de holerite entra no MVP ou fica depois;
- importacao CSV de lancamento em massa entra no MVP;
- aprovacao de lancamento antes da folha sera obrigatoria;
- reabertura de folha entra no MVP;
- sistema sera monoempresa ou multiempresa desde o inicio.

