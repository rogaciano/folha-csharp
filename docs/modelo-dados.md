# Modelo de dados logico - Sistema RH e Folha

Data: 2026-06-04

## 1. Objetivo

Definir o modelo de dados logico do sistema de RH e folha antes da escolha da stack.

Este documento descreve entidades, campos principais, relacionamentos, status, regras de integridade e pontos de extensao para ERP, ponto, ferias, afastamentos e eSocial.

O modelo ainda nao e uma migration fisica. Depois da escolha da stack, ele sera convertido para tabelas, constraints, indices, migrations, models e testes.

## 2. Convencoes gerais

Campos comuns recomendados para quase todas as tabelas:

- `id`
- `created_at`
- `updated_at`
- `deleted_at`, quando for necessario exclusao logica
- `created_by`
- `updated_by`

Padroes:

- Dinheiro deve usar decimal.
- Percentuais devem ser armazenados como decimal, por exemplo `8.00` para 8%.
- Competencia deve seguir formato ano/mes, por exemplo `2026-06`.
- Datas de fato devem ser separadas de datas de processamento.
- Registros usados em folha fechada nao devem ser apagados fisicamente.
- Cadastros sensiveis devem ter historico ou snapshot.
- Status devem ser enums controlados.

## 3. Visao geral das entidades

Cadastros organizacionais:

- `Empresa`
- `Estabelecimento`
- `Setor`
- `CentroCusto`
- `Cargo`

Pessoas e contratos:

- `Colaborador`
- `ContratoTrabalho`
- `HistoricoCargo`
- `HistoricoSalario`
- `Dependente`

Folha e rubricas:

- `Rubrica`
- `RubricaVigencia`
- `TabelaLegal`
- `Competencia`
- `LancamentoFixo`
- `LancamentoAvulso`
- `LoteLancamento`
- `LoteLancamentoItem`
- `Adiantamento`
- `TipoProducao`
- `LancamentoProducao`
- `Folha`
- `FolhaColaborador`
- `FolhaItem`
- `Holerite`

Seguranca e auditoria:

- `Usuario`
- `Perfil`
- `Permissao`
- `Auditoria`

Futuro:

- `Ferias`
- `Afastamento`
- `PontoImportacao`
- `PontoEvento`
- `ExportacaoERP`
- `EventoESocial`

## 4. Cadastros organizacionais

### 4.1 Empresa

Representa a pessoa juridica principal que processa a folha.

Campos:

- `id`
- `razao_social`
- `nome_fantasia`
- `tipo_pessoa`: juridica, fisica
- `cpf_cnpj`
- `inscricao_estadual`
- `inscricao_municipal`
- `cnae_principal`
- `regime_tributario`
- `endereco_logradouro`
- `endereco_numero`
- `endereco_complemento`
- `endereco_bairro`
- `endereco_cidade`
- `endereco_uf`
- `endereco_cep`
- `telefone`
- `email`
- `ativa`

Relacionamentos:

- Empresa possui muitos estabelecimentos.
- Empresa possui muitos setores, centros de custo, cargos, colaboradores, rubricas e competencias.

Regras:

- `cpf_cnpj` deve ser unico por empresa cadastrada.
- Uma empresa inativa nao deve permitir abertura de nova competencia.

### 4.2 Estabelecimento

Representa matriz, filial, obra ou unidade operacional.

Campos:

- `id`
- `empresa_id`
- `nome`
- `tipo`: matriz, filial, obra, outro
- `cpf_cnpj`
- `codigo_interno`
- `endereco_logradouro`
- `endereco_numero`
- `endereco_complemento`
- `endereco_bairro`
- `endereco_cidade`
- `endereco_uf`
- `endereco_cep`
- `ativo`

Relacionamentos:

- Pertence a empresa.
- Pode estar vinculado ao contrato do colaborador.
- Pode ser usado em fechamento, relatorios e futura exportacao.

### 4.3 Setor

Representa departamento ou area.

Campos:

- `id`
- `empresa_id`
- `nome`
- `codigo_interno`
- `setor_pai_id`, opcional
- `ativo`

Relacionamentos:

- Pertence a empresa.
- Pode ter hierarquia.
- Colaboradores se vinculam por contrato/historico.

### 4.4 CentroCusto

Representa classificacao gerencial/contabil.

Campos:

- `id`
- `empresa_id`
- `nome`
- `codigo_interno`
- `ativo`

Relacionamentos:

- Pertence a empresa.
- Pode ser usado no contrato, folha, relatorios e futura integracao ERP.

### 4.5 Cargo

Representa cargo/função.

Campos:

- `id`
- `empresa_id`
- `nome`
- `codigo_interno`
- `cbo`
- `descricao`
- `ativo`

Relacionamentos:

- Pertence a empresa.
- Pode ser usado em historico de cargo e contrato.

Regras:

- Alterar nome/cbo de cargo nao deve alterar snapshots de folhas fechadas.

## 5. Colaboradores e contratos

### 5.1 Colaborador

Representa a pessoa trabalhadora.

Campos:

- `id`
- `empresa_id`
- `matricula`
- `nome`
- `nome_social`
- `cpf`
- `rg`
- `orgao_emissor_rg`
- `data_nascimento`
- `genero`
- `estado_civil`
- `email`
- `telefone`
- `celular`
- `endereco_logradouro`
- `endereco_numero`
- `endereco_complemento`
- `endereco_bairro`
- `endereco_cidade`
- `endereco_uf`
- `endereco_cep`
- `banco_codigo`
- `banco_agencia`
- `banco_conta`
- `banco_tipo_conta`
- `pix_chave`
- `status`: ativo, afastado, ferias, desligado, inativo
- `observacoes`

Relacionamentos:

- Pertence a empresa.
- Possui um ou mais contratos de trabalho.
- Possui dependentes.
- Possui lancamentos fixos, avulsos, producao e adiantamentos.
- Possui folhas por competencia.

Regras:

- `cpf` deve ser unico dentro da empresa.
- `matricula` deve ser unica dentro da empresa.
- Colaborador desligado nao entra em folha comum depois da data de desligamento, exceto se houver rotina futura de rescisao/complementar.

### 5.2 ContratoTrabalho

Representa o vinculo trabalhista do colaborador.

Campos:

- `id`
- `empresa_id`
- `colaborador_id`
- `estabelecimento_id`
- `setor_id`
- `centro_custo_id`
- `cargo_id`
- `data_admissao`
- `data_desligamento`
- `tipo_contrato`: clt_indeterminado, clt_determinado, experiencia, estagio, autonomo, pro_labore, outro
- `categoria_trabalhador`
- `modelo_remuneracao`: mensalista, horista, diarista, producao, misto
- `salario_base`
- `valor_hora`
- `valor_dia`
- `valor_producao_padrao`
- `garantia_minima`
- `jornada_mensal_horas`
- `jornada_semanal_horas`
- `dia_pagamento`
- `adiantamento_percentual_padrao`
- `status`: ativo, suspenso, encerrado

Relacionamentos:

- Pertence a colaborador.
- Aponta para cargo, setor, centro de custo e estabelecimento vigentes.

Regras:

- Um colaborador deve ter no maximo um contrato ativo por empresa no MVP.
- Folha deve considerar situacao do contrato na competencia.
- Alteracoes de salario, cargo, setor ou centro de custo devem gerar historico.

### 5.3 HistoricoCargo

Guarda mudancas de cargo, setor, centro de custo ou estabelecimento.

Campos:

- `id`
- `empresa_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `cargo_id`
- `setor_id`
- `centro_custo_id`
- `estabelecimento_id`
- `data_inicio`
- `data_fim`
- `motivo`

Regras:

- Periodos do mesmo contrato nao devem se sobrepor.
- Deve existir um registro vigente para contrato ativo.

### 5.4 HistoricoSalario

Guarda mudancas de remuneracao.

Campos:

- `id`
- `empresa_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `modelo_remuneracao`
- `salario_base`
- `valor_hora`
- `valor_dia`
- `valor_producao_padrao`
- `garantia_minima`
- `data_inicio`
- `data_fim`
- `motivo`

Regras:

- Periodos do mesmo contrato nao devem se sobrepor.
- Folha deve usar historico vigente na competencia ou data de referencia.

### 5.5 Dependente

Usado principalmente para IRRF e beneficios.

Campos:

- `id`
- `empresa_id`
- `colaborador_id`
- `nome`
- `cpf`
- `data_nascimento`
- `parentesco`
- `dependente_irrf`
- `dependente_salario_familia`
- `ativo`
- `data_inicio`
- `data_fim`

Regras:

- Dependentes de IRRF devem ser considerados por vigencia.

## 6. Rubricas e tabelas legais

### 6.1 Rubrica

Cadastro mestre de verbas de folha.

Campos:

- `id`
- `empresa_id`
- `codigo`
- `nome`
- `descricao`
- `tipo`: provento, desconto, informativa, base, patronal
- `natureza_esocial`
- `permite_lancamento_manual`
- `permite_lancamento_massa`
- `permite_lancamento_fixo`
- `ativa`

Relacionamentos:

- Possui vigencias.
- Usada em lancamentos e itens de folha.

Regras:

- `codigo` deve ser unico por empresa.
- Rubrica usada em folha fechada nao deve ser apagada.

### 6.2 RubricaVigencia

Define regras de calculo e incidencias da rubrica ao longo do tempo.

Campos:

- `id`
- `rubrica_id`
- `vigencia_inicio`
- `vigencia_fim`
- `incide_inss`
- `incide_fgts`
- `incide_irrf`
- `incide_dsr`
- `incide_ferias`
- `incide_decimo_terceiro`
- `incide_rescisao`
- `compoe_salario_base`
- `compoe_salario_bruto`
- `compoe_salario_liquido`
- `forma_calculo`: valor_fixo, percentual, quantidade_valor, formula, manual, sistema
- `base_calculo`: salario_base, salario_bruto, base_inss, base_irrf, base_fgts, outra, nenhuma
- `formula`, opcional
- `conta_financeira_padrao`, opcional para ERP futuro
- `ativa`

Regras:

- Vigencias da mesma rubrica nao devem se sobrepor.
- Folha deve usar a vigencia aplicavel a competencia.

### 6.3 TabelaLegal

Guarda parametros legais versionados.

Campos:

- `id`
- `empresa_id`, opcional para permitir tabela global ou customizada
- `tipo`: inss, irrf, fgts, salario_minimo, dependente_irrf, outro
- `competencia_inicio`
- `competencia_fim`
- `descricao`
- `dados`
- `fonte`
- `ativa`

Observacao:

- `dados` representa estrutura tabular/logica. No modelo fisico pode virar JSON ou tabelas normalizadas, conforme stack.

Regras:

- Tabela legal deve ser versionada por vigencia.
- Folha fechada deve registrar qual versao foi usada.

## 7. Competencias

### 7.1 Competencia

Representa o periodo mensal de processamento.

Campos:

- `id`
- `empresa_id`
- `ano`
- `mes`
- `codigo`: exemplo `2026-06`
- `data_inicio`
- `data_fim`
- `status`: aberta, em_calculo, calculada, aprovada, fechada, reaberta, cancelada
- `data_abertura`
- `data_fechamento`
- `fechada_por`
- `motivo_reabertura`

Relacionamentos:

- Possui folha.
- Agrupa lancamentos, producao e adiantamentos.

Regras:

- Uma empresa nao pode ter duas competencias com mesmo ano/mes.
- Lancamentos em competencia fechada devem ser bloqueados.
- Reabertura deve exigir permissao e auditoria.

## 8. Lancamentos

### 8.1 LancamentoFixo

Rubrica recorrente vinculada ao colaborador.

Campos:

- `id`
- `empresa_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `rubrica_id`
- `competencia_inicio`
- `competencia_fim`
- `tipo_recorrencia`: mensal, parcelado
- `valor`
- `quantidade`
- `percentual`
- `valor_total`
- `parcelas_total`
- `parcelas_processadas`
- `status`: ativo, suspenso, encerrado, cancelado
- `observacao`

Regras:

- Lancamento fixo ativo entra automaticamente no calculo da competencia vigente.
- Parcelado nao deve gerar mais parcelas do que `parcelas_total`.
- Encerrar nao deve remover parcelas ja processadas em folha fechada.

### 8.2 LancamentoAvulso

Lancamento individual de provento/desconto em uma competencia.

Campos:

- `id`
- `empresa_id`
- `competencia_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `rubrica_id`
- `origem`: manual, massa, importacao, producao, adiantamento, sistema, ponto, ferias, afastamento
- `origem_id`
- `data_fato`
- `valor`
- `quantidade`
- `percentual`
- `observacao`
- `status`: rascunho, validado, aprovado, processado, cancelado

Regras:

- Nao pode alterar lancamento processado em folha fechada.
- Lancamento cancelado nao entra no calculo.
- `origem` e `origem_id` permitem rastrear de onde o lancamento veio.

### 8.3 LoteLancamento

Cabecalho de lancamento em massa.

Campos:

- `id`
- `empresa_id`
- `competencia_id`
- `rubrica_id`
- `nome`
- `tipo_aplicacao`: valor_fixo, percentual, quantidade_valor, importacao
- `criterio_selecao`
- `valor`
- `quantidade`
- `percentual`
- `status`: rascunho, preview, aplicado, cancelado, erro
- `total_colaboradores`
- `total_valor`
- `observacao`

### 8.4 LoteLancamentoItem

Itens gerados pelo lote.

Campos:

- `id`
- `lote_lancamento_id`
- `lancamento_avulso_id`
- `colaborador_id`
- `valor`
- `quantidade`
- `percentual`
- `status`: gerado, aplicado, ignorado, erro
- `mensagem_erro`

Regras:

- Lote aplicado deve gerar lancamentos avulsos rastreaveis.
- Cancelamento de lote aplicado deve exigir regra especifica, pois pode afetar lancamentos ja processados.

## 9. Adiantamentos

### 9.1 Adiantamento

Controle de vale/adiantamento salarial.

Campos:

- `id`
- `empresa_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `competencia_origem_id`
- `competencia_desconto_id`
- `data_adiantamento`
- `valor`
- `forma_pagamento`: dinheiro, transferencia, pix, cheque, outro
- `status`: aberto, programado, descontado, cancelado
- `lancamento_desconto_id`
- `observacao`

Regras:

- Adiantamento aberto/programado deve gerar desconto na competencia definida.
- Ao fechar folha com desconto, status deve virar descontado.
- Nao permitir desconto duplicado do mesmo adiantamento.

## 10. Producao

### 10.1 TipoProducao

Cadastro de item/tipo produzido.

Campos:

- `id`
- `empresa_id`
- `codigo`
- `nome`
- `descricao`
- `valor_unitario_padrao`
- `ativo`

### 10.2 LancamentoProducao

Registro de producao por colaborador.

Campos:

- `id`
- `empresa_id`
- `competencia_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `tipo_producao_id`
- `data_producao`
- `quantidade`
- `valor_unitario`
- `valor_total`
- `status`: rascunho, aprovado, processado, cancelado
- `observacao`

Regras:

- Producao aprovada entra no calculo.
- Producao processada em folha fechada nao pode ser alterada.
- `valor_unitario` deve ser gravado no lancamento para preservar historico.

## 11. Folha

### 11.1 Folha

Cabecalho do processamento da folha da competencia.

Campos:

- `id`
- `empresa_id`
- `competencia_id`
- `tipo`: mensal, complementar, adiantamento, decimo_terceiro, ferias, rescisao
- `status`: rascunho, calculando, calculada, aprovada, fechada, reaberta, cancelada
- `data_calculo`
- `data_aprovacao`
- `data_fechamento`
- `calculada_por`
- `aprovada_por`
- `fechada_por`
- `total_proventos`
- `total_descontos`
- `total_liquido`
- `total_fgts`
- `total_inss`
- `total_irrf`
- `versao_motor_calculo`

Relacionamentos:

- Pertence a competencia.
- Possui muitos `FolhaColaborador`.

Regras:

- Uma competencia pode ter uma folha mensal principal no MVP.
- Folha fechada deve bloquear alteracoes nos itens.

### 11.2 FolhaColaborador

Snapshot do demonstrativo de um colaborador na folha.

Campos:

- `id`
- `folha_id`
- `empresa_id`
- `competencia_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `matricula_snapshot`
- `nome_snapshot`
- `cpf_snapshot`
- `cargo_snapshot`
- `setor_snapshot`
- `centro_custo_snapshot`
- `modelo_remuneracao_snapshot`
- `salario_base_snapshot`
- `base_inss`
- `base_fgts`
- `base_irrf`
- `total_proventos`
- `total_descontos`
- `liquido`
- `fgts`
- `inss`
- `irrf`
- `status`: calculado, aprovado, fechado, cancelado
- `memoria_calculo`

Regras:

- Campos snapshot devem preservar o cadastro usado no fechamento.
- `memoria_calculo` deve permitir auditoria do resultado.

### 11.3 FolhaItem

Itens calculados do demonstrativo.

Campos:

- `id`
- `folha_colaborador_id`
- `folha_id`
- `rubrica_id`
- `rubrica_codigo_snapshot`
- `rubrica_nome_snapshot`
- `rubrica_tipo_snapshot`
- `natureza_esocial_snapshot`
- `origem`: salario, fixo, avulso, massa, adiantamento, producao, sistema, ponto, ferias, afastamento
- `origem_id`
- `referencia`
- `quantidade`
- `valor_unitario`
- `percentual`
- `valor`
- `incide_inss_snapshot`
- `incide_fgts_snapshot`
- `incide_irrf_snapshot`
- `base_inss_item`
- `base_fgts_item`
- `base_irrf_item`
- `ordem_exibicao`

Regras:

- Deve guardar snapshots da rubrica.
- Deve apontar origem para rastreabilidade.
- Relatorios devem somar `FolhaItem` de folha fechada.

### 11.4 Holerite

Representa emissao/geracao de demonstrativo.

Campos:

- `id`
- `folha_colaborador_id`
- `colaborador_id`
- `competencia_id`
- `numero`
- `status`: gerado, enviado, visualizado, cancelado
- `arquivo_url`
- `data_geracao`
- `data_envio`
- `data_visualizacao`

Observacao:

- No MVP, o PDF pode ser gerado sob demanda. Ainda assim, a entidade ajuda a controlar emissao futura.

## 12. Usuarios, permissoes e auditoria

### 12.1 Usuario

Campos:

- `id`
- `empresa_id`
- `nome`
- `email`
- `senha_hash`
- `ativo`
- `ultimo_login_at`

### 12.2 Perfil

Campos:

- `id`
- `empresa_id`
- `nome`
- `descricao`
- `ativo`

### 12.3 Permissao

Campos:

- `id`
- `codigo`
- `descricao`

Relacionamentos:

- Usuario pode ter perfis.
- Perfil possui permissoes.

Permissoes iniciais:

- `colaborador.ler`
- `colaborador.criar`
- `colaborador.editar`
- `rubrica.gerenciar`
- `lancamento.criar`
- `lancamento.aprovar`
- `folha.calcular`
- `folha.aprovar`
- `folha.fechar`
- `folha.reabrir`
- `relatorio.ler`
- `auditoria.ler`

### 12.4 Auditoria

Campos:

- `id`
- `empresa_id`
- `usuario_id`
- `acao`
- `entidade`
- `entidade_id`
- `dados_anteriores`
- `dados_novos`
- `motivo`
- `ip`
- `user_agent`
- `created_at`

Regras:

- Fechamento, reabertura, cancelamento e alteracoes salariais sempre devem auditar.

## 13. Entidades futuras previstas

### 13.1 Ferias

Campos previstos:

- `id`
- `empresa_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `periodo_aquisitivo_inicio`
- `periodo_aquisitivo_fim`
- `periodo_concessivo_inicio`
- `periodo_concessivo_fim`
- `data_inicio_gozo`
- `data_fim_gozo`
- `dias_gozo`
- `dias_abono`
- `status`
- `folha_id`

### 13.2 Afastamento

Campos previstos:

- `id`
- `empresa_id`
- `colaborador_id`
- `contrato_trabalho_id`
- `tipo_afastamento`
- `data_inicio`
- `data_fim`
- `motivo`
- `status`
- `impacta_folha`

### 13.3 PontoImportacao

Campos previstos:

- `id`
- `empresa_id`
- `competencia_id`
- `origem`: arquivo, api, biometria
- `arquivo_url`
- `status`
- `total_registros`
- `total_erros`

### 13.4 PontoEvento

Campos previstos:

- `id`
- `empresa_id`
- `competencia_id`
- `colaborador_id`
- `tipo`: falta, atraso, hora_extra, adicional_noturno, banco_horas
- `data_evento`
- `quantidade_horas`
- `status`
- `lancamento_avulso_id`

### 13.5 ExportacaoERP

Campos previstos:

- `id`
- `empresa_id`
- `folha_id`
- `tipo`: contas_a_pagar, provisao, impostos
- `status`: pendente, enviado, confirmado, erro, cancelado
- `payload`
- `resposta`
- `data_envio`

### 13.6 EventoESocial

Campos previstos:

- `id`
- `empresa_id`
- `folha_id`
- `colaborador_id`
- `tipo_evento`: s1010, s1200, s1210, outro
- `status`: pendente, gerado, enviado, aceito, rejeitado, cancelado
- `payload`
- `recibo`
- `mensagem_retorno`

## 14. Regras de integridade essenciais

- Empresa nao pode ter duas competencias iguais.
- Colaborador nao pode ter CPF duplicado na mesma empresa.
- Matricula deve ser unica na empresa.
- Contrato ativo deve ter cargo, setor e centro de custo validos.
- Historicos de salario/cargo nao podem se sobrepor no mesmo contrato.
- Rubrica nao pode ter codigos duplicados na empresa.
- Vigencias da mesma rubrica nao podem se sobrepor.
- Lancamentos nao podem ser criados/alterados em competencia fechada.
- Folha fechada nao pode ser alterada sem reabertura formal.
- Reabertura exige motivo e auditoria.
- Adiantamento nao pode ser descontado mais de uma vez.
- Producao processada em folha fechada nao pode ser alterada.
- Relatorios de competencia fechada devem usar snapshots.

## 15. Snapshots obrigatorios no fechamento

No fechamento, o sistema deve congelar:

- dados basicos do colaborador;
- cargo;
- setor;
- centro de custo;
- modelo de remuneracao;
- salario/base vigente;
- rubricas usadas;
- incidencias das rubricas;
- tabelas legais usadas;
- lancamentos aplicados;
- bases de INSS, FGTS e IRRF;
- valores finais de proventos, descontos, liquido, INSS, IRRF e FGTS;
- memoria de calculo.

## 16. Pontos para validar antes da stack

- Se o MVP sera monoempresa ou multiempresa.
- Se `estabelecimentos` entram ja no MVP ou ficam preparados.
- Se centros de custo sao obrigatorios.
- Se salario por producao tera garantia minima.
- Se lancamento em massa por CSV entra no MVP.
- Se holerite PDF entra no MVP ou apenas tela/impressao.
- Se teremos aprovacao de lancamentos antes do calculo.
- Se reabertura de folha sera permitida no MVP.

