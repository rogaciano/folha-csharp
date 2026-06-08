# Especificacao inicial - Sistema RH e Folha

Data da analise: 2026-06-03

## 1. Objetivo

Construir um sistema gerencial completo de RH e folha de pagamento para empresas CLT, partindo do prototipo atual `rh-folha-clt`.

O sistema deve permitir cadastrar estrutura organizacional, colaboradores, cargos, setores, rubricas de proventos e descontos, lancamentos fixos, lancamentos avulsos, lancamentos em massa, adiantamentos salariais, producao, fechamento mensal da folha, demonstrativos, relatorios e futuras integracoes com ERP, ponto, ferias e afastamentos.

## 2. Situacao atual do prototipo

O prototipo atual e uma SPA simples em HTML/JavaScript, servida localmente por Python.

Arquivos principais:

- `rh-folha-clt/index.html`: contem praticamente toda a aplicacao, estado, renderizacao, formularios, calculo da folha, localStorage e impressao.
- `rh-folha-clt/App.jsx`: contem uma copia/parcial do codigo da aplicacao.
- `rh-folha-clt/iniciar_sistema.py`: servidor HTTP local na porta 8000.
- `rh-folha-clt/js/*`: dependencias frontend baixadas localmente.

Funcionalidades existentes:

- Cadastro simples de colaboradores.
- Colaborador mensalista ou por producao.
- Tipos basicos de lancamentos, separados apenas entre provento e desconto.
- Vales/adiantamentos simples.
- Recorrencias por colaborador.
- Edicao manual de alguns dados da competencia, como producao e faltas.
- Calculo basico de folha no frontend.
- Fechamento/congelamento da competencia em `localStorage`.
- Impressao de holerites.

Limitacoes atuais:

- Sem backend, banco de dados, usuarios, permissoes ou auditoria.
- Regra de folha fica acoplada a tela.
- Tabelas legais estao fixas no codigo.
- Provento/desconto nao possui incidencias formais de INSS, FGTS, IRRF ou natureza eSocial.
- Nao ha cadastro de empresa, estabelecimentos, setores, cargos, contratos, historico salarial ou lotacao.
- Nao ha processamento em lote com validacao, aprovacao, estorno ou reabertura controlada.
- Nao ha trilha clara para integracao futura com eSocial, ERP financeiro, controle de ponto, ferias e afastamentos.

## 3. Referencias oficiais consideradas

Fontes oficiais consultadas em 2026-06-03:

- INSS: tabela de contribuicao mensal valida a partir da competencia janeiro/2026, com faixas e aliquotas para empregado, empregado domestico e trabalhador avulso.
  URL: https://www.gov.br/inss/pt-br/direitos-e-deveres/inscricao-e-contribuicao/tabela-de-contribuicao-mensal
- Receita Federal: tributacao de 2026, tabela de incidencia mensal, deducao por dependente e reducao mensal.
  URL: https://www.gov.br/receitafederal/pt-br/assuntos/meu-imposto-de-renda/tabelas/2026
- eSocial: leiautes S-1.2, incluindo S-1010 Tabela de Rubricas, S-1200 Remuneracao e S-1210 Pagamentos.
  URL: https://www.gov.br/esocial/pt-br/documentacao-tecnica/leiautes-esocial-v-1-2-versao-s-1-2-nt-04-2024/index.html
- Manual de Orientacao do eSocial S-1.3: exemplos de rubricas, incidencias de CP, FGTS e IRRF, ferias e eventos periodicos.
  URL: https://www.gov.br/esocial/pt-br/documentacao-tecnica/manuais/mos-s-1-3-consolidada-ate-a-no-s-1-3-07-2026.pdf

Consequencia para o produto: rubricas precisam ser tratadas como cadastros ricos, versionados e com incidencia legal, nao apenas como "provento" ou "desconto".

## 4. Modulos funcionais propostos

### 4.1 Empresas e parametros

Cadastro de empresa e estabelecimentos:

- Razao social, nome fantasia, CNPJ/CPF, inscricoes, CNAE, endereco.
- Regime tributario e parametros trabalhistas/previdenciarios.
- Estabelecimentos/filiais.
- Competencia ativa.
- Calendario de fechamento.
- Parametros de arredondamento.
- Parametros de DSR, faltas, adiantamento e pagamento.

### 4.2 Estrutura organizacional

Cadastros:

- Setores/departamentos.
- Centros de custo.
- Cargos/funcoes.
- CBO.
- Jornada padrao.
- Tabelas de producao, quando aplicavel.

Requisitos:

- Historico de lotacao do colaborador.
- Historico de cargo.
- Historico de salario/base de producao.
- Permitir filtros gerenciais por setor, cargo, centro de custo e estabelecimento.

### 4.3 Colaboradores

Cadastro completo:

- Dados pessoais: nome, CPF, RG, data de nascimento, genero, estado civil, contatos, endereco.
- Dados trabalhistas: matricula, data de admissao, tipo de contrato, categoria, cargo, setor, centro de custo, jornada, banco/agencia/conta/Pix.
- Dados salariais: mensalista, horista, diarista ou producao; salario base; valor hora; valor por peca/producao; garantia minima, quando houver.
- Dependentes para IRRF e salario-familia, se aplicavel.
- Beneficios e descontos vinculados.
- Situacao: ativo, afastado, ferias, desligado.
- Documentos/anexos futuramente.

Requisitos:

- Alteracoes sensiveis devem gerar historico.
- Fechamentos antigos devem preservar os dados usados no calculo, mesmo que o cadastro mude depois.

### 4.4 Rubricas, proventos e descontos

Substituir "tipos de lancamento" por cadastro de rubricas.

Campos minimos:

- Codigo interno.
- Nome.
- Tipo: vencimento/provento, desconto, informativa, base, patronal.
- Natureza eSocial.
- Incidencia INSS/CP.
- Incidencia FGTS.
- Incidencia IRRF.
- Incidencia DSR, ferias, 13o, rescisao, medias.
- Forma de calculo: valor fixo, percentual, quantidade x valor, formula, manual.
- Base de calculo.
- Vigencia inicial/final.
- Ativa/inativa.
- Permite lancamento manual.
- Permite lancamento em massa.
- Permite recorrencia.
- Classificacao financeira futura para contas a pagar.

Rubricas iniciais sugeridas:

- Salario mensal.
- Producao.
- Hora extra.
- Adicional noturno.
- Insalubridade.
- Periculosidade.
- Gratificacao.
- Comissao.
- Vale alimentacao/refeicao.
- Vale transporte.
- Plano de saude.
- Plano odontologico.
- Adiantamento salarial.
- Desconto de adiantamento.
- Falta.
- DSR sobre falta.
- INSS.
- IRRF.
- FGTS informativo.
- Ferias.
- 1/3 ferias.
- 13o salario.
- Afastamento.

### 4.5 Lancamentos fixos por colaborador

Permitir vincular rubricas recorrentes ao colaborador:

- Valor fixo mensal.
- Percentual.
- Parcelado por valor total ou por numero de parcelas.
- Inicio e fim por competencia.
- Suspensao temporaria.
- Aplicacao somente em competencias especificas.
- Observacao e origem.

Exemplos:

- Plano de saude mensal.
- Vale alimentacao mensal.
- Desconto de emprestimo parcelado.
- Gratificacao fixa.
- Ajuda de custo recorrente.

### 4.6 Lancamentos avulsos e em massa

Lancamento individual:

- Colaborador.
- Rubrica.
- Competencia.
- Data do fato.
- Valor, quantidade, percentual ou formula.
- Observacao.
- Anexo opcional.
- Status: rascunho, validado, aprovado, processado, cancelado.

Lancamento em massa:

- Por filtro de colaboradores: setor, cargo, centro de custo, todos ativos.
- Por importacao CSV/XLSX.
- Por regra: percentual sobre salario, valor fixo, quantidade x valor.
- Pre-visualizacao antes de gravar.
- Validacao de rubrica, competencia e duplicidade.
- Log da operacao em lote.

### 4.7 Adiantamento salarial

Fluxo proprio:

- Registrar adiantamento/vale.
- Definir competencia de desconto.
- Permitir pagamento em dinheiro, banco ou futura integracao com contas a pagar.
- Gerar rubrica de desconto automaticamente na folha da competencia indicada.
- Controlar status: aberto, descontado, cancelado.
- Evitar desconto duplicado.

### 4.8 Producao

Para colaboradores por producao:

- Cadastro de itens/tipos de producao.
- Valor por unidade por colaborador, cargo ou tabela geral.
- Lancamento individual ou em massa.
- Competencia e data da producao.
- Quantidade aprovada, rejeitada, ajustada.
- Calculo de vencimento por quantidade x valor.
- Possibilidade de salario minimo/garantia, se adotado pela empresa.

### 4.9 Processamento e fechamento da folha

Fluxo mensal sugerido:

1. Abrir competencia.
2. Importar/registrar producao, faltas, adiantamentos, lancamentos avulsos e em massa.
3. Aplicar lancamentos fixos vigentes.
4. Calcular folha preliminar.
5. Validar inconsistencias.
6. Revisar por colaborador, setor e totais.
7. Aprovar folha.
8. Fechar competencia.
9. Gerar holerites, relatorios, provisoes e exportacoes.
10. Futuramente gerar contas a pagar e eventos eSocial.

Regras de fechamento:

- Folha fechada deve ser imutavel.
- Reabertura deve exigir permissao, motivo e gerar auditoria.
- Valores calculados devem ser gravados como snapshot.
- Tabelas legais e rubricas usadas devem ser versionadas no calculo.
- Cada demonstrativo deve guardar origem dos lancamentos.

### 4.10 Ferias

Fase futura, mas o modelo deve prever:

- Periodo aquisitivo.
- Periodo concessivo.
- Programacao de ferias.
- Abono pecuniario.
- 1/3 constitucional.
- Adiantamento de ferias.
- Ferias partidas.
- Impacto na folha mensal.
- Relatorios de vencimento.

### 4.11 Afastamentos

Fase futura:

- Tipo de afastamento.
- Datas de inicio/fim.
- Motivo.
- Impacto na remuneracao.
- Impacto em ferias/13o, quando aplicavel.
- Retorno ao trabalho.
- Base futura para eventos eSocial.

### 4.12 Controle de ponto

Fase futura:

- Importacao de ponto.
- Integracao com biometria.
- Jornada, escalas e horarios.
- Faltas, atrasos, horas extras, adicional noturno e banco de horas.
- Fechamento do ponto antes da folha.
- Geracao automatica de rubricas para a folha.

### 4.13 Integracao com ERP financeiro

Fase futura:

- Gerar contas a pagar por fechamento de folha.
- Separar liquido por colaborador, tributos, beneficios e fornecedores.
- Exportar lote para ERP.
- Controlar status de envio, retorno e erro.
- Conciliar pagamento.

## 5. Modelo conceitual inicial

Entidades principais:

- Empresa
- Estabelecimento
- Setor
- CentroCusto
- Cargo
- Colaborador
- ContratoTrabalho
- HistoricoCargo
- HistoricoSalario
- Dependente
- Rubrica
- TabelaLegal
- ParametroFolha
- Competencia
- LancamentoFixo
- LancamentoAvulso
- LoteLancamento
- LancamentoProducao
- Adiantamento
- Folha
- FolhaColaborador
- FolhaItem
- Holerite
- Ferias
- Afastamento
- Auditoria
- Usuario
- PerfilPermissao
- ExportacaoERP

Relacionamentos centrais:

- Empresa possui estabelecimentos, setores, cargos e colaboradores.
- Colaborador possui contrato, historicos, dependentes, lancamentos fixos, avulsos, producao, adiantamentos, ferias e afastamentos.
- Competencia possui uma folha por empresa/estabelecimento.
- Folha possui varios demonstrativos por colaborador.
- FolhaItem aponta para uma rubrica e guarda valor calculado, origem e bases.
- Rubrica carrega incidencias e vigencia.

## 6. Regras tecnicas importantes

- Calculo de folha deve ficar no backend/dominio, nao na UI.
- Usar decimal para dinheiro, nunca ponto flutuante binario no banco/calculo final.
- Folha fechada deve ser snapshot auditavel.
- Tabelas legais devem ter vigencia.
- Rubricas devem ter versao/vigencia.
- Lancamentos devem preservar origem: manual, fixo, massa, producao, ponto, ferias, afastamento, importacao.
- Toda alteracao sensivel deve gerar auditoria.
- Permissoes devem separar cadastro, lancamento, calculo, aprovacao, fechamento, reabertura e exportacao.
- Relatorios precisam bater com os demonstrativos fechados, nao recalcular historico com regra nova.

## 7. Relatorios e telas esperadas

Telas operacionais:

- Dashboard gerencial.
- Colaboradores.
- Cargos.
- Setores.
- Centros de custo.
- Rubricas.
- Tabelas legais.
- Lancamentos fixos.
- Lancamentos avulsos.
- Lancamento em massa.
- Producao.
- Adiantamentos.
- Processamento da folha.
- Fechamento da folha.
- Holerites.
- Ferias.
- Afastamentos.
- Auditoria.
- Configuracoes.

Relatorios:

- Resumo da folha por competencia.
- Folha analitica por colaborador.
- Totais por rubrica.
- Totais por setor/centro de custo.
- Proventos e descontos.
- INSS, IRRF e FGTS.
- Adiantamentos abertos/descontados.
- Lancamentos em massa por lote.
- Producao por colaborador.
- Ferias a vencer/vencidas.
- Afastamentos.
- Exportacoes para ERP.

## 8. Fases sugeridas

### Fase 1 - Fundacao gerencial

- Backend, banco e autenticacao.
- Empresa, setores, cargos, colaboradores.
- Rubricas completas com incidencias.
- Lancamentos fixos, avulsos e em massa.
- Competencias e fechamento com snapshot.
- Holerite e relatorios basicos.

### Fase 2 - Folha mais robusta

- Tabelas legais versionadas.
- Motor de calculo testado.
- Adiantamento salarial completo.
- Producao completa.
- Auditoria e permissoes.
- Importacao CSV/XLSX.
- Relatorios gerenciais.

### Fase 3 - Rotinas de DP

- Ferias.
- Afastamentos.
- 13o salario.
- Rescisao, se for escopo.
- Provisoes.

### Fase 4 - Integracoes

- ERP contas a pagar.
- Controle de ponto.
- Biometria/importacao de ponto.
- eSocial.

## 9. Decisoes pendentes antes da stack

- O sistema sera usado por uma empresa unica ou multiempresa/multitenant?
- Precisa funcionar offline/local ou sera web/cloud?
- Quantos usuarios simultaneos?
- Havera escritorio contabilidade usando para varios clientes?
- eSocial sera integracao nativa ou apenas preparo/exportacao inicialmente?
- Controle de ponto sera interno ou integracao com equipamento/software externo?
- ERP futuro ja existe? Tem API?
- Precisa emitir recibos/holerites em PDF com assinatura?
- Folha inclui apenas CLT ou tambem pro-labore, autonomos, estagiarios e domesticos?
- Havera rescisao na primeira versao?

## 10. Recomendacao inicial

Nao evoluir o prototipo atual diretamente para producao. Ele deve servir como referencia visual e de fluxo.

O proximo passo recomendado e desenhar:

1. modelo de dados inicial;
2. fluxo de fechamento de competencia;
3. cadastro de rubricas;
4. casos de uso prioritarios;
5. criterios para escolha da stack.

