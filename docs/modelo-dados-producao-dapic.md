# Modelo de dados - Producao e integracao Dapic

Data: 2026-06-09

## 1. Objetivo

Definir o modelo de dados do modulo de producao por colaborador e da integracao com o Dapic/Webpic.

Este modelo complementa o modelo geral de RH e folha. A ideia e permitir:

- sincronizar dados operacionais do Dapic;
- vincular funcionarios do Dapic aos colaboradores do RH Folha;
- registrar apontamentos de producao no RH Folha;
- conferir producao antes da folha;
- gerar proventos de producao na competencia;
- manter rastreabilidade e auditoria.

## 2. Principios

- O Dapic e fonte de dados operacionais de apoio.
- O RH Folha e fonte do apontamento validado para pagamento.
- Dados importados devem ter `external_id`.
- Dados usados em folha fechada nao devem ser apagados fisicamente.
- Toda sincronizacao deve gerar log.
- Toda producao integrada na folha deve ter origem rastreavel.
- A integracao inicial deve ser manual/sob demanda; agendamento fica para fase futura.

## 3. Entidades de integracao

### 3.1 ExternalIntegration

Configura uma integracao externa por empresa.

Campos:

- `id`
- `company_id`
- `provider`: `Dapic`
- `name`
- `base_url`
- `external_company_identifier`
- `integration_token_secret`
- `access_token`
- `access_token_expires_at`
- `last_sync_at`
- `status`: `Active`, `Inactive`, `Error`
- `last_error`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- Deve existir no maximo uma integracao Dapic ativa por empresa no MVP.
- `integration_token_secret` nao deve aparecer em logs ou respostas de API.
- `access_token` pode ser renovado automaticamente, pois expira em 1 dia.
- Integracao inativa nao pode executar sincronizacao.

Indices recomendados:

- unico parcial por `company_id`, `provider`, `status = Active`.
- indice por `provider`.

### 3.2 ExternalSyncLog

Registra cada execucao de sincronizacao.

Campos:

- `id`
- `company_id`
- `external_integration_id`
- `provider`
- `resource`: `Employees`, `Products`, `ProductionOrders`, `ProductionOrderProducts`, `ProductionOperations`, `ProductionCells`, `ProductTechnicalSheets`, `LaborOutputs`
- `started_at`
- `finished_at`
- `status`: `Running`, `Success`, `PartialSuccess`, `Error`, `Canceled`
- `requested_from`
- `requested_to`
- `page_count`
- `records_read`
- `records_created`
- `records_updated`
- `records_ignored`
- `error_message`
- `created_by_user_id`

Regras:

- Sincronizacao iniciada deve criar log com `Running`.
- Ao final, atualizar status e totais.
- Erro 429 deve ser registrado como erro de limite ou tentativa adiada.
- Nao armazenar payload inteiro por padrao; se necessario, criar log tecnico separado com retencao curta.

Indices recomendados:

- `company_id`, `resource`, `started_at`.
- `external_integration_id`, `status`.

### 3.3 ExternalEntityMap

Mapeia entidades externas para entidades locais quando o relacionamento nao for direto.

Campos:

- `id`
- `company_id`
- `provider`
- `external_entity_type`
- `external_id`
- `local_entity_type`
- `local_entity_id`
- `external_display_name`
- `status`: `Linked`, `Ignored`, `Conflict`, `Inactive`
- `linked_at`
- `linked_by_user_id`
- `notes`
- `created_at`
- `updated_at`

Uso inicial:

- vincular funcionario Dapic ao colaborador do RH Folha.

Regras:

- Um `external_id` de funcionario Dapic nao deve apontar para dois colaboradores ativos na mesma empresa.
- Um colaborador pode ter no maximo um vinculo ativo com funcionario Dapic no MVP.
- Conflitos devem ficar visiveis para ajuste manual.

Indices recomendados:

- unico por `company_id`, `provider`, `external_entity_type`, `external_id`, `status = Linked`.
- unico por `company_id`, `local_entity_type`, `local_entity_id`, `provider`, `status = Linked`.

## 4. Entidades sincronizadas do Dapic

### 4.1 DapicEmployee

Replica local resumida do funcionario Dapic.

Campos:

- `id`
- `company_id`
- `external_id`
- `name`
- `fantasy_name`
- `display_name`
- `status`: `Active`, `Inactive`, `Unknown`
- `raw_updated_at`
- `last_synced_at`
- `is_ignored`
- `created_at`
- `updated_at`
- `deleted_at`

Relacionamentos:

- Pode estar vinculado a um `Employee` via `ExternalEntityMap`.

Regras:

- Nao cria colaborador automaticamente sem confirmacao.
- Se ja houver colaborador com nome/matricula equivalente, sugerir vinculo.

Indices recomendados:

- unico por `company_id`, `external_id`.
- indice por `name`.

### 4.2 ProductionProduct

Produto/referencia importado do Dapic.

Campos:

- `id`
- `company_id`
- `external_id`
- `reference`
- `factory_description`
- `status`: `Active`, `Inactive`, `Unknown`
- `external_created_at`
- `external_updated_at`
- `last_synced_at`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- `reference` deve ser pesquisavel.
- Produto usado em apontamento nao deve ser apagado fisicamente.
- Alteracao de descricao no Dapic atualiza o cadastro local, mas apontamentos ja feitos devem manter snapshot.

Indices recomendados:

- unico por `company_id`, `external_id`.
- indice por `reference`.
- indice por `status`.

### 4.3 ProductionOperation

Operacao de producao importada do Dapic.

Campos:

- `id`
- `company_id`
- `external_id`
- `name`
- `description`
- `status`: `Active`, `Inactive`, `Unknown`
- `last_synced_at`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- Operacao usada em producao deve permanecer disponivel para historico, mesmo se ficar inativa.
- A tabela de valores pode usar operacao como criterio.

Indices recomendados:

- unico por `company_id`, `external_id`.
- indice por `name`.

### 4.4 ProductionCell

Celula/linha/equipe de producao importada do Dapic.

Campos:

- `id`
- `company_id`
- `external_id`
- `name`
- `description`
- `status`: `Active`, `Inactive`, `Unknown`
- `department_id`, opcional
- `last_synced_at`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- No MVP, celula nao substitui setor de RH.
- `department_id` serve apenas como mapeamento opcional para relatorios.

Indices recomendados:

- unico por `company_id`, `external_id`.
- indice por `department_id`.

### 4.5 ProductionOrder

Ordem de producao importada do Dapic.

Campos:

- `id`
- `company_id`
- `external_id`
- `number`
- `description`
- `status`: `Open`, `InProgress`, `Completed`, `Canceled`, `Unknown`
- `issue_date`
- `start_date`
- `end_date`
- `external_updated_at`
- `last_synced_at`
- `raw_status`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- `raw_status` guarda o status original do Dapic.
- O mapeamento para status interno pode ser ajustado depois de testar com dados reais.
- Ordem usada em apontamento nao deve ser apagada.

Indices recomendados:

- unico por `company_id`, `external_id`.
- indice por `number`.
- indice por `status`.
- indice por `issue_date`.

### 4.6 ProductionOrderProduct

Produto/referencia dentro de uma ordem de producao.

Campos:

- `id`
- `company_id`
- `production_order_id`
- `production_product_id`
- `external_id`, opcional se o Dapic retornar id proprio do item
- `reference_snapshot`
- `description_snapshot`
- `color`
- `size`
- `grade`
- `planned_quantity`
- `produced_quantity`
- `status`: `Open`, `Completed`, `Canceled`, `Unknown`
- `last_synced_at`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- Deve guardar snapshot de referencia e descricao.
- Se o Dapic nao retornar item id, usar combinacao ordem + produto + grade/cor/tamanho.
- `produced_quantity` pode ser calculada pelos apontamentos locais.

Indices recomendados:

- `production_order_id`.
- `production_product_id`.
- `company_id`, `reference_snapshot`.

### 4.7 ProductTechnicalSheet

Ficha tecnica resumida de um produto.

Campos:

- `id`
- `company_id`
- `production_product_id`
- `external_product_id`
- `version_label`
- `last_synced_at`
- `raw_hash`
- `status`: `Active`, `Inactive`, `Unknown`
- `created_at`
- `updated_at`

Regras:

- A ficha tecnica pode mudar; o apontamento deve guardar snapshot da operacao e valor usado.
- `raw_hash` ajuda a detectar mudanca sem depender de todos os campos.

### 4.8 ProductTechnicalSheetOperation

Operacoes encontradas na ficha tecnica.

Campos:

- `id`
- `company_id`
- `product_technical_sheet_id`
- `production_operation_id`
- `operation_name_snapshot`
- `sequence`
- `standard_quantity`
- `standard_time`
- `notes`
- `created_at`
- `updated_at`

Regras:

- Se a operacao nao existir na tabela local, criar com status `Unknown` ou exigir revisao.
- Pode servir como sugestao para apontamento, nao como regra obrigatoria.

## 5. Tabelas de valor de producao

### 5.1 ProductionRateTable

Tabela de valores para producao.

Campos:

- `id`
- `company_id`
- `name`
- `effective_from`
- `effective_to`
- `status`: `Draft`, `Active`, `Inactive`, `Closed`
- `notes`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- Apenas uma tabela ativa deve valer para a mesma data e empresa, salvo se houver regra especifica por setor/cargo.
- Tabela usada em folha fechada nao deve ser alterada; criar nova vigencia.

### 5.2 ProductionRate

Valor unitario por criterio.

Campos:

- `id`
- `company_id`
- `production_rate_table_id`
- `production_product_id`, opcional
- `production_operation_id`, opcional
- `job_position_id`, opcional
- `department_id`, opcional
- `production_cell_id`, opcional
- `unit_value`
- `calculation_type`: `QuantityTimesUnitValue`, `FixedAmount`, `Percentage`
- `minimum_quantity`
- `maximum_quantity`
- `status`: `Active`, `Inactive`
- `notes`
- `created_at`
- `updated_at`
- `deleted_at`

Regras:

- Uma regra com campo opcional vazio significa "todos" ou "qualquer" para aquele criterio.
- Uma regra so pode ser candidata quando todos os criterios preenchidos batem com o apontamento e com o cadastro do colaborador.
- Criterios avaliados: produto, operacao, celula, setor, cargo e faixa de quantidade.
- A faixa de quantidade deve obedecer:
  - `minimum_quantity` nulo: sem limite minimo;
  - `maximum_quantity` nulo: sem limite maximo;
  - quando informados, a quantidade apontada deve estar dentro dos limites.
- Entre as regras candidatas, usar pontuacao de especificidade:
  - produto informado: `+32`;
  - operacao informada: `+16`;
  - celula informada: `+8`;
  - setor informado: `+4`;
  - cargo informado: `+2`;
  - faixa de quantidade informada: `+1`.
- A regra com maior pontuacao vence.
- A ordem de cadastro nao pode ser usada como criterio de escolha.
- Em caso de empate, aplicar desempate nesta ordem:
  1. regra com produto informado;
  2. regra com operacao informada;
  3. regra com celula informada;
  4. regra com setor informado;
  5. regra com cargo informado;
  6. maior `minimum_quantity`;
  7. menor `maximum_quantity`;
  8. se ainda houver empate, bloquear o apontamento/calculo e exigir ajuste da tabela.
- Se nenhuma regra for encontrada, no MVP o apontamento/calculo deve ser bloqueado para evitar pagamento sem parametro.
- Valor manual futuro deve exigir permissao e auditoria.
- Ao calcular o apontamento, gravar `production_rate_id`, `unit_value`, `total_amount` e snapshots dos dados usados.

Indices recomendados:

- `production_rate_table_id`.
- `production_product_id`, `production_operation_id`.
- `department_id`, `job_position_id`, `production_cell_id`.

## 6. Apontamento de producao

### 6.1 EmployeeProductionEntry

Lancamento de producao por colaborador.

Campos:

- `id`
- `company_id`
- `payroll_period_id`
- `employee_id`
- `production_date`
- `production_order_id`
- `production_order_product_id`
- `production_product_id`
- `production_operation_id`
- `production_cell_id`
- `quantity`
- `unit_value`
- `total_amount`
- `rate_source`: `Manual`, `RateTable`, `EmployeeDefault`, `Imported`
- `production_rate_id`
- `origin`: `Manual`, `MassEntry`, `Import`, `Adjustment`
- `status`: `Draft`, `PendingReview`, `Approved`, `Rejected`, `IntegratedIntoPayroll`, `Canceled`
- `notes`
- `approved_at`
- `approved_by_user_id`
- `integrated_payroll_calculation_id`
- `integrated_payroll_calculation_item_id`
- `created_at`
- `updated_at`
- `deleted_at`

Snapshots obrigatorios:

- `employee_registration_snapshot`
- `employee_name_snapshot`
- `order_number_snapshot`
- `product_reference_snapshot`
- `product_description_snapshot`
- `operation_name_snapshot`
- `cell_name_snapshot`

Regras:

- `quantity` deve ser maior que zero.
- `unit_value` deve ser maior ou igual a zero.
- `total_amount = quantity * unit_value`, exceto quando `calculation_type` permitir valor fixo.
- Somente status `Approved` entra na folha.
- Ao integrar na folha, status vira `IntegratedIntoPayroll`.
- Apontamento integrado em folha fechada nao pode ser alterado.
- Cancelamento de apontamento aprovado exige auditoria.
- Competencia fechada bloqueia criacao/edicao/cancelamento.

Indices recomendados:

- `payroll_period_id`, `employee_id`.
- `production_date`.
- `production_order_id`.
- `production_product_id`.
- `production_operation_id`.
- `status`.

### 6.2 EmployeeProductionBatch

Cabecalho para lancamento em massa de producao.

Campos:

- `id`
- `company_id`
- `payroll_period_id`
- `name`
- `production_date`
- `production_order_id`
- `production_product_id`
- `production_operation_id`
- `production_cell_id`
- `default_quantity`
- `default_unit_value`
- `default_notes`
- `status`: `Draft`, `Preview`, `Applied`, `Canceled`, `Error`
- `total_employees`
- `total_quantity`
- `total_amount`
- `created_by_user_id`
- `created_at`
- `updated_at`

Regras:

- Deve seguir o mesmo padrao 0/1/N de selects do projeto.
- Pode aplicar padroes de quantidade, valor e observacao.
- Deve gerar `EmployeeProductionEntry` rastreaveis.

### 6.3 EmployeeProductionBatchItem

Itens do lote de producao.

Campos:

- `id`
- `company_id`
- `employee_production_batch_id`
- `employee_production_entry_id`
- `employee_id`
- `quantity`
- `unit_value`
- `total_amount`
- `status`: `Generated`, `Applied`, `Ignored`, `Error`
- `error_message`

## 7. Integracao com folha

### 7.1 Regra de calculo

No calculo da competencia:

1. localizar producoes `Approved` da competencia;
2. agrupar por colaborador;
3. gerar item de provento com rubrica de producao;
4. registrar origem `Production`;
5. vincular `EmployeeProductionEntry` ao item calculado;
6. marcar apontamentos como `IntegratedIntoPayroll` quando a folha for aprovada/fechada, conforme regra final de processamento.

### 7.2 Rubrica de producao

Recomendacao:

- criar rubrica propria, exemplo `150 - Producao`;
- tipo `Provento`;
- origem de sistema/manual;
- incidencias configuraveis conforme regra da empresa e validacao contavel.

### 7.3 Rastreabilidade

O holerite pode mostrar apenas o total da producao, mas a conferencia deve permitir abrir detalhe:

- data;
- ordem;
- produto/referencia;
- operacao;
- quantidade;
- valor unitario;
- valor total.

## 8. Telas previstas

### 8.1 Integracao Dapic

Funcoes:

- configurar identificador da empresa;
- configurar token de integracao;
- testar conexao;
- ver status do ultimo token;
- executar sincronizacoes manuais;
- ver logs.

### 8.2 Vinculo de funcionarios

Funcoes:

- listar funcionarios Dapic;
- pesquisar colaborador RH;
- vincular/desvincular;
- marcar funcionario Dapic como ignorado;
- mostrar conflitos.

### 8.3 Cadastros sincronizados

Funcoes:

- consultar produtos/referencias;
- consultar operacoes;
- consultar celulas;
- consultar ordens;
- consultar produtos das ordens.

Inicialmente podem ser telas de consulta, sem edicao.

### 8.4 Tabela de valores de producao

Funcoes:

- criar vigencia;
- definir valor por operacao;
- definir valor por produto + operacao;
- inativar regra;
- copiar tabela anterior.

### 8.5 Apontamento de producao

Funcoes:

- lancamento individual;
- lancamento em massa;
- filtros por colaborador, setor, cargo, ordem, produto, operacao e celula;
- aplicar quantidade/valor/observacao padrao;
- conferir e aprovar.

### 8.6 Conferencia de producao

Funcoes:

- total por colaborador;
- total por operacao;
- total por produto;
- total por ordem;
- divergencias sem valor;
- apontamentos pendentes.

## 9. Permissoes

Perfis sugeridos:

- Administrador: configura integracao e tabela de valores.
- RH operacional: vincula funcionarios e lanca producao.
- Conferente: aprova/rejeita apontamentos.
- Leitura: consulta producao e sincronizacoes.

Permissoes futuras:

- `production.read`
- `production.create`
- `production.edit`
- `production.approve`
- `production.cancel`
- `production.rate.manage`
- `integration.dapic.manage`
- `integration.dapic.sync`

## 10. Auditoria obrigatoria

Auditar:

- configuracao de token Dapic;
- teste de conexao;
- sincronizacao iniciada/finalizada;
- vinculo/desvinculo de funcionario;
- alteracao de tabela de valores;
- criacao/edicao/cancelamento/aprovacao de producao;
- integracao da producao na folha.

## 11. Pendencias para validar antes da migration

- Confirmar campos reais retornados por ordens de producao.
- Confirmar campos reais dos produtos da ordem.
- Confirmar campos reais da ficha tecnica.
- Confirmar se saida para mao de obra ajuda no apontamento.
- Definir se valor por producao sera por operacao, produto + operacao ou colaborador.
- Definir rubrica padrao de producao.
- Definir se producao entra com incidencia em INSS/FGTS/IRRF conforme regra contavel da empresa.

## 12. Ordem de implementacao recomendada

1. Criar entidades e migrations de integracao/sincronizacao.
2. Criar entidades e migrations de producao.
3. Criar servico cliente Dapic com autenticacao.
4. Criar endpoints de teste de conexao e sincronizacao.
5. Criar telas de integracao e logs.
6. Criar vinculo Dapic funcionario x colaborador.
7. Criar consultas de produtos, operacoes, celulas e ordens.
8. Criar tabela de valores.
9. Criar apontamento individual e em massa.
10. Criar conferencia.
11. Integrar producao aprovada ao calculo da folha.
