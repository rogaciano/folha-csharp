# Plano de implementacao - Sistema RH e Folha

Data: 2026-06-04

## 1. Objetivo do plano

Transformar a especificacao aceita em um caminho pratico de implementacao, com MVP, fases, backlog inicial, entregaveis e criterios para escolha da stack.

Este plano ainda nao define tecnologia. A escolha da stack deve acontecer depois da validacao do modelo de dados, dos fluxos principais e das regras de calculo.

## 2. Escopo do MVP

O MVP deve entregar uma folha gerencial funcional para uso interno, com fechamento mensal auditavel.

Entram no MVP:

- Cadastro de empresa.
- Cadastro de setores.
- Cadastro de cargos.
- Cadastro completo de colaboradores em nivel operacional inicial.
- Cadastro de rubricas com incidencias basicas.
- Competencias mensais.
- Lancamentos fixos por colaborador.
- Lancamentos avulsos individuais.
- Lancamentos em massa.
- Adiantamento salarial.
- Lancamento de producao.
- Calculo preliminar da folha.
- Fechamento da folha por competencia.
- Snapshot dos valores fechados.
- Holerite simples.
- Relatorios basicos da competencia.
- Auditoria minima de acoes sensiveis.

Nao entram no MVP:

- eSocial nativo.
- Integracao com ERP.
- Integracao com biometria/ponto.
- Ferias completas.
- Afastamentos completos.
- Rescisao.
- 13o salario.
- Portal do colaborador.
- Assinatura digital.

Esses itens devem ser previstos no modelo, mas implementados em fases futuras.

## 3. Principios de arquitetura

- O calculo da folha deve ficar no backend/dominio.
- A interface nao deve calcular valores oficiais; apenas exibir simulacoes vindas da API.
- Folha fechada deve ser imutavel, com possibilidade de reabertura controlada.
- Valores historicos devem ser preservados por snapshot.
- Rubricas e tabelas legais devem ter vigencia.
- Dinheiro deve ser tratado com decimal.
- Toda operacao sensivel deve registrar usuario, data/hora, acao e motivo quando aplicavel.
- Lancamentos precisam guardar origem: manual, fixo, massa, producao, adiantamento, futuro ponto, futura ferias, futuro afastamento.
- Relatorios de folha fechada devem ler snapshots, nao recalcular historico.

## 4. Fases de implementacao

### Fase 0 - Preparacao e desenho

Objetivo: fechar desenho funcional e tecnico antes de codar.

Entregaveis:

- Modelo de dados inicial.
- Fluxo de fechamento da folha.
- Definicao das rubricas iniciais.
- Matriz de permissoes inicial.
- Criterios de escolha da stack.
- Decisao da stack.
- Estrutura inicial do projeto.

Criterio de pronto:

- Conseguimos explicar como um colaborador mensalista e um por producao viram uma folha fechada.
- Conseguimos explicar como um lancamento fixo, avulso, em massa e adiantamento entram na folha.
- Conseguimos saber quais tabelas gravam o snapshot do fechamento.

### Fase 1 - Fundacao do sistema

Objetivo: criar base tecnica e cadastros centrais.

Entregaveis:

- Backend inicial.
- Banco de dados com migrations.
- Autenticacao simples.
- Cadastro de empresa.
- Cadastro de setores.
- Cadastro de cargos.
- Cadastro de colaboradores.
- Cadastro de rubricas.
- Listagens, filtros e formularios principais.
- Auditoria minima.

Criterio de pronto:

- Usuario autenticado consegue cadastrar estrutura organizacional e colaboradores.
- Rubricas podem ser cadastradas com tipo, vigencia e incidencias basicas.

### Fase 2 - Lancamentos e competencias

Objetivo: permitir alimentar a folha.

Entregaveis:

- Cadastro de competencias.
- Lancamentos fixos por colaborador.
- Lancamentos avulsos individuais.
- Lancamentos em massa por filtro.
- Importacao simples por CSV, se aprovada para o MVP.
- Adiantamentos salariais.
- Producao por colaborador.
- Validacoes de competencia, duplicidade e rubrica.

Criterio de pronto:

- E possivel abrir uma competencia e registrar todos os tipos de entradas necessarias para uma folha simples.

### Fase 3 - Motor de calculo

Objetivo: calcular folha preliminar com rastreabilidade.

Entregaveis:

- Calculo de salario mensal.
- Calculo por producao.
- Aplicacao de lancamentos fixos vigentes.
- Aplicacao de lancamentos avulsos.
- Aplicacao de lancamentos em massa.
- Desconto de adiantamentos.
- Faltas e DSR em versao inicial.
- INSS.
- IRRF.
- FGTS informativo.
- Bases de calculo por rubrica.
- Memoria de calculo por colaborador.
- Testes automatizados do motor de calculo.

Criterio de pronto:

- Para uma competencia aberta, o sistema calcula folha preliminar por colaborador e apresenta memoria de calculo.

### Fase 4 - Fechamento e demonstrativos

Objetivo: fechar folha com snapshot e gerar saidas basicas.

Entregaveis:

- Validacao previa da folha.
- Fechamento por competencia.
- Snapshot de folha, folha por colaborador e itens.
- Bloqueio de alteracao de competencia fechada.
- Reabertura com permissao e motivo.
- Holerite simples.
- Relatorio resumo.
- Relatorio por colaborador.
- Relatorio por rubrica.
- Relatorio por setor/centro de custo.

Criterio de pronto:

- Competencia fechada preserva valores mesmo se cadastro/rubrica/tabela mudar depois.

### Fase 5 - Qualidade operacional

Objetivo: preparar o sistema para uso real com menor risco.

Entregaveis:

- Permissoes por perfil.
- Auditoria detalhada.
- Logs de processamento.
- Tratamento de erros.
- Backup/exportacao de dados.
- Melhorias de UX.
- Revisao de seguranca.
- Testes end-to-end dos fluxos principais.

Criterio de pronto:

- Fluxo completo de uma folha mensal pode ser executado por usuario operacional com rastreabilidade.

## 5. Backlog inicial

### Epico A - Cadastros base

- Criar cadastro de empresa.
- Criar cadastro de estabelecimentos, se necessario no MVP.
- Criar cadastro de setores.
- Criar cadastro de cargos.
- Criar cadastro de centros de custo.
- Criar cadastro de colaboradores.
- Criar historico de cargo e salario.
- Criar cadastro de dependentes.

### Epico B - Rubricas e tabelas

- Criar cadastro de rubricas.
- Definir incidencias INSS, FGTS e IRRF.
- Definir vigencia de rubricas.
- Criar rubricas padrao iniciais.
- Criar tabelas legais versionadas.
- Criar validacoes de rubrica ativa/inativa.

### Epico C - Competencias

- Criar competencia mensal.
- Abrir competencia.
- Bloquear competencia fechada.
- Reabrir competencia com motivo.
- Listar status das competencias.

### Epico D - Lancamentos

- Criar lancamento fixo.
- Encerrar/suspender lancamento fixo.
- Criar lancamento avulso.
- Cancelar lancamento avulso.
- Criar lote de lancamento em massa.
- Pre-visualizar lote.
- Aplicar lote.
- Cancelar lote antes do processamento.

### Epico E - Adiantamentos

- Registrar adiantamento.
- Definir competencia de desconto.
- Vincular desconto automaticamente na folha.
- Marcar adiantamento como descontado no fechamento.
- Cancelar adiantamento aberto.

### Epico F - Producao

- Criar tipos de producao.
- Definir valor por unidade.
- Lancar producao individual.
- Lancar producao em massa.
- Calcular producao na folha.

### Epico G - Calculo da folha

- Calcular salario mensal.
- Calcular producao.
- Aplicar proventos.
- Aplicar descontos.
- Calcular faltas.
- Calcular DSR.
- Calcular INSS.
- Calcular IRRF.
- Calcular FGTS informativo.
- Gerar memoria de calculo.
- Criar testes unitarios dos cenarios principais.

### Epico H - Fechamento

- Calcular folha preliminar.
- Validar inconsistencias.
- Aprovar folha.
- Fechar competencia.
- Gravar snapshot.
- Gerar holerite.
- Gerar relatorios.

### Epico I - Auditoria e seguranca

- Criar usuarios.
- Criar perfis.
- Criar permissoes.
- Registrar auditoria de cadastro.
- Registrar auditoria de lancamento.
- Registrar auditoria de fechamento/reabertura.

### Epico J - Futuras integracoes

- Preparar status de exportacao ERP.
- Preparar origem futura de ponto.
- Preparar origem futura de ferias.
- Preparar origem futura de afastamento.
- Preparar campos de rubrica para eSocial.

## 6. Modelo de dados - primeiro rascunho

Tabelas nucleares:

- `empresas`
- `estabelecimentos`
- `setores`
- `centros_custo`
- `cargos`
- `colaboradores`
- `contratos_trabalho`
- `historicos_cargo`
- `historicos_salario`
- `dependentes`
- `rubricas`
- `rubrica_vigencias`
- `tabelas_legais`
- `competencias`
- `lancamentos_fixos`
- `lancamentos_avulsos`
- `lotes_lancamento`
- `lote_lancamento_itens`
- `adiantamentos`
- `tipos_producao`
- `lancamentos_producao`
- `folhas`
- `folha_colaboradores`
- `folha_itens`
- `holerites`
- `usuarios`
- `perfis`
- `permissoes`
- `auditorias`

Tabelas futuras:

- `ferias`
- `afastamentos`
- `ponto_importacoes`
- `ponto_eventos`
- `exportacoes_erp`
- `eventos_esocial`

## 7. Fluxo principal da folha

1. Usuario abre ou seleciona uma competencia.
2. Sistema lista colaboradores ativos na competencia.
3. Usuario revisa cadastros pendentes.
4. Usuario registra producao.
5. Usuario registra adiantamentos.
6. Usuario registra lancamentos avulsos.
7. Usuario aplica lancamentos em massa.
8. Sistema identifica lancamentos fixos vigentes.
9. Usuario solicita calculo preliminar.
10. Sistema calcula folha por colaborador.
11. Sistema apresenta inconsistencias e totais.
12. Usuario corrige entradas, se necessario.
13. Usuario aprova fechamento.
14. Sistema grava snapshot da folha.
15. Sistema bloqueia alteracoes da competencia.
16. Sistema gera holerites e relatorios.
17. Futuramente, sistema gera exportacao para ERP/eSocial.

## 8. Stack - criterios de escolha

A stack deve ser escolhida com base nestes criterios:

- Facilidade de manter regras de negocio complexas.
- Boa modelagem relacional.
- Migrations confiaveis.
- Testes automatizados simples.
- Geracao de PDF/relatorios.
- Autenticacao e permissoes.
- Facilidade de deploy local ou cloud.
- Facilidade para integracao futura com ERP, ponto e eSocial.
- Curva de manutencao para o time.
- Custo de hospedagem e operacao.

Opcoes a comparar:

- Backend Node.js/TypeScript.
- Backend Python.
- Backend PHP/Laravel.
- Backend C#/.NET.
- Banco PostgreSQL.
- Frontend React/TypeScript.
- Aplicacao desktop/local com backend local, se houver requisito offline.

Decisao preliminar recomendada para avaliar primeiro:

- PostgreSQL como banco principal.
- Backend com dominio bem testado.
- Frontend separado do motor de calculo.

## 9. Perguntas de produto antes da stack

- Sera usado por uma unica empresa ou por varias empresas?
- Precisa rodar local/offline ou pode ser web/cloud?
- Quantos usuarios usarao o sistema?
- Quantos colaboradores a folha precisa suportar inicialmente?
- Havera escritorio/contador acessando varias empresas?
- Rescisao entra no horizonte curto?
- 13o entra no primeiro ano de uso?
- O ERP futuro ja existe? Qual?
- O ponto futuro sera equipamento biometrico, arquivo de importacao ou API?
- Holerite precisa ser PDF, impresso ou portal?

## 10. Proximas acoes

1. Validar MVP deste documento.
2. Desenhar modelo de dados com campos principais.
3. Desenhar fluxo de telas do MVP.
4. Comparar stacks candidatas.
5. Escolher stack.
6. Criar estrutura do projeto novo.
7. Implementar Fase 1.

## 11. Proximas melhorias aprovadas durante a implementacao

Depois da entrega dos relatorios basicos da folha, a proxima frente aprovada e evoluir o cadastro de colaboradores para uma visao mais completa de acompanhamento de RH.

Itens previstos:

- Foto do colaborador no cadastro e na ficha.
- Aba ou secao de acompanhamento do colaborador.
- Registro de eventos com data, tipo, descricao e responsavel.
- Filtros operacionais por nome, matricula, CPF, setor, cargo, responsavel, modelo e periodo de admissao.
- Responsavel direto opcional no cadastro do colaborador, apontando para outro colaborador da mesma empresa.
- Upload de foto para pasta do servidor, com caminho publico gravado no banco.
- Tipos iniciais de evento: curso, treinamento, advertencia, indisciplina, elogio, observacao, mudanca operacional e outro.
- Possibilidade de anexos futuramente, sem obrigar no primeiro passo.
- Preservacao historica dos eventos, sem exclusao fisica.
- Relatorio futuro de eventos por colaborador, periodo e tipo.

Modelo inicial sugerido:

- `colaborador_fotos`, caso as imagens sejam armazenadas com metadados.
- `colaborador_eventos`, para registrar acompanhamento historico.
- Campo `responsible_employee_id` em `colaboradores`, para representar o responsavel direto.
- Campo `photo_url` em `colaboradores`, gravando o caminho publico da imagem enviada ao servidor.

Campos iniciais de `colaborador_eventos`:

- `id`
- `empresa_id`
- `colaborador_id`
- `data_evento`
- `tipo`
- `titulo`
- `descricao`
- `responsavel`
- `status`: ativo, cancelado
- `created_at`
- `updated_at`

Regra operacional:

- Eventos de acompanhamento devem ser historicos. Quando houver erro de registro, cancelar/inativar o evento e manter rastreabilidade em vez de apagar fisicamente.
- Fotos devem ser enviadas para o servidor em `data/uploads/employees` e servidas pelo caminho publico `/uploads/employees`.
- Caminhos locais do computador do usuario nao devem ser usados como solucao final, pois nao aparecem corretamente para outros equipamentos.
- O responsavel direto do colaborador e opcional; futuramente o responsavel do setor pode ser usado como fallback quando esse campo estiver vazio.
