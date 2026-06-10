# Regras de UI e Operacao - RH Folha

Este documento define o padrao visual e operacional que deve orientar novas telas, ajustes e refatoracoes do sistema RH Folha.

## Principios gerais

- O sistema e operacional, nao promocional. Priorizar leitura rapida, densidade moderada, consistencia e baixo ruido visual.
- Cada tela deve resolver uma tarefa real do usuario, com formulario, lista, filtros, acoes e estados necessarios.
- Evitar telas explicativas ou decorativas. O primeiro contato da tela deve ser a propria operacao.
- Toda acao que altera dados deve dar retorno visual por mensagem de sucesso ou erro.
- Nenhuma exclusao fisica deve ser oferecida ao usuario. Usar inativacao/reativacao e preservar historico.
- O usuario deve conseguir entender o status de um registro pela listagem, sem abrir detalhe.

## Layout

- Manter a estrutura atual:
  - menu lateral fixo no desktop;
  - area de conteudo com topbar;
  - paineis brancos sobre fundo cinza claro;
  - tabelas com rolagem horizontal quando necessario.
- Usar `Panel` para blocos funcionais: cadastro, filtros, listagens, resumo e manutencao.
- Evitar cards dentro de cards.
- Formularios simples podem usar 3 colunas no desktop e 1 coluna no mobile.
- Tabelas operacionais podem ser largas, mas devem ficar dentro de `.table-scroll`.
- Campos relacionados devem ficar proximos: competencia, rubrica, data, valor, quantidade e observacao.

## Cores

Paleta base:

- Fundo da aplicacao: `#eef2f5`
- Sidebar: `#10233f`
- Sidebar hover/ativo: `#1c355b`
- Texto principal: `#17202a`
- Texto secundario: `#64748b`
- Borda: `#d8dee6`
- Borda leve: `#e3e8ef`
- Superficie: `#ffffff`
- Acao primaria: `#256f5c`
- Verde de destaque/status positivo: `#1f6c4d`
- Erro: `#9f1d1d`
- Alerta/calculo: `#835600`

Regras:

- Verde deve indicar acao primaria, sucesso ou ativo.
- Cinza deve indicar neutro, inativo ou fechado.
- Vermelho deve ser reservado para erro, bloqueio e risco.
- Amarelo/ambar deve indicar calculo, pendencia ou atencao.
- Nao usar gradientes, fundos decorativos, sombras fortes ou paletas muito saturadas.

## Tipografia

- Manter fonte do sistema/Inter.
- Nao escalar fonte com viewport.
- Titulos de pagina devem ser curtos e objetivos.
- Labels de campos devem ser claros e diretos.
- Evitar textos longos dentro da interface.
- Usar texto em ASCII nos arquivos por enquanto: `Lancamentos`, `Competencias`, `Configuracoes`.

## Botoes e acoes

- Botao primario:
  - fundo `#256f5c`;
  - usado para salvar, atualizar, aplicar, abrir competencia e executar acoes principais.
- Botao de tabela:
  - classe `.table-action`;
  - menor, direto, com verbo curto: `Fechar`, `Reabrir`, `Inativar`, `Reativar`.
- Acoes destrutivas nao devem existir como delete fisico.
- Inativacao deve ser bloqueada quando houver vinculo ativo que comprometa consistencia.
- Acoes irreversiveis ou sensiveis devem usar confirmacao.
- Sempre recarregar dados apos acao bem-sucedida.

## Icones

- Quando introduzirmos icones, usar `lucide-react` como biblioteca padrao.
- Icones devem complementar o texto, nao substituir comandos criticos sem tooltip.
- Usar icones consistentes:
  - salvar: `Save`
  - atualizar: `RefreshCw`
  - filtros: `Filter`
  - adicionar: `Plus`
  - editar: `Pencil`
  - inativar: `CircleOff`
  - reativar: `RotateCcw`
  - fechar competencia: `Lock`
  - reabrir competencia: `Unlock`
  - lancamentos: `ReceiptText`
  - colaboradores: `Users`
  - estrutura: `Network`
  - rubricas: `ListChecks`
  - competencias: `CalendarDays`
  - configuracoes: `Settings`
- Icones em menu e botoes devem ter tamanho entre 16 e 18px.

## Formularios

- Todo formulario deve:
  - validar campos obrigatorios no frontend quando possivel;
  - enviar payload limpo para a API;
  - resetar apos sucesso;
  - mostrar mensagem de sucesso/erro;
  - desabilitar botao quando faltarem pre-condicoes.
- Campos numericos monetarios:
  - `type="number"`;
  - `min="0"` ou `min="0.01"`;
  - `step="0.01"`.
- Quantidades:
  - `step="0.0001"`;
  - opcionais devem virar `null` quando zero/vazio.
- Datas:
  - usar `type="date"`;
  - competencias devem sugerir o primeiro dia do periodo quando aplicavel.
- Campos `select` obrigatorios devem seguir a regra 0/1/N:
  - com 0 opcoes validas: mostrar placeholder vazio/desabilitado e impedir envio;
  - com 1 opcao valida: pode pre-selecionar automaticamente, pois nao ha ambiguidade operacional;
  - com 2 ou mais opcoes validas: iniciar sem valor selecionado, com uma primeira opcao desabilitada como `Selecione...`, obrigando o usuario a escolher.
- O sistema nao deve pre-selecionar colaborador, rubrica, competencia, cargo, setor ou qualquer entidade de negocio sensivel quando houver mais de uma opcao valida e a escolha alterar o resultado operacional.
- A pre-selecao tambem e aceitavel quando for uma preferencia neutra e explicita, por exemplo filtro `Todos`, ou quando a regra de negocio realmente definir um padrao seguro.
- Selects de entidades com potencial de muitos registros, como colaboradores, rubricas, cargos, setores, empresas, centros de custo ou responsaveis, devem evoluir para componente pesquisavel no padrao Select2.
- Em React, evitar jQuery Select2 diretamente. Preferir um componente React reutilizavel de busca/selecao, com comportamento equivalente ao Select2: busca por texto, placeholder, limpar selecao quando aplicavel, navegacao por teclado e exibicao consistente.

## Componentes reutilizaveis

- Componentizar ao maximo elementos repetidos para reduzir divergencia visual e operacional.
- Componentes padrao recomendados:
  - `Modal`;
  - `Panel`;
  - `DataTable`;
  - `Metric`;
  - `ActionButton`;
  - `StatusBadge`;
  - `SearchableSelect`;
  - `FormActions`;
  - `ToolbarActions`.
- Novas telas nao devem copiar blocos grandes de formulario, tabela, modal ou select quando ja existir componente reutilizavel equivalente.
- Componentes compartilhados devem concentrar:
  - estilo visual;
  - estados desabilitados;
  - placeholder padrao;
  - mensagens de vazio;
  - comportamento de limpeza;
  - acessibilidade basica.
- Antes de criar um novo padrao visual, verificar se `App.tsx`, `App.css` ou componentes existentes ja resolvem o caso.
- Quando um trecho for repetido em duas ou mais telas, avaliar extrair componente antes de continuar expandindo a duplicacao.

## Tabelas

- Toda tabela deve ter:
  - titulo claro;
  - colunas estaveis;
  - status visivel quando houver ciclo de vida;
  - acoes na ultima coluna;
  - rolagem horizontal para telas estreitas.
- Nao esconder registros inativos automaticamente em telas de manutencao. Mostrar `Ativo/Inativo`.
- Em telas operacionais de lancamento, filtrar por padrao apenas itens ativos para selecao.

## Status e badges

Padroes atuais:

- `status-active`: ativo
- `status-inactive`: inativo
- `status-aberta`: competencia aberta
- `status-reaberta`: competencia reaberta
- `status-fechada`: competencia fechada
- `status-em_calculo`, `status-calculada`, `status-aprovada`: ciclo de calculo
- `status-cancelada`: cancelado/erro operacional

Regras:

- Status deve aparecer como badge, nao como texto solto.
- Status de competencia deve ser tratado como regra operacional, nao apenas visual.
- Competencia fechada nao deve aceitar novos lancamentos.

## Mensagens e feedback ao usuario

Padrao obrigatorio:

- Mensagem positiva/sucesso: verde.
- Mensagem de erro/falha: vermelho.
- Mensagem de aviso, bloqueio preventivo ou pre-condicao: amarelo/laranja.

Regras:

- Nao exibir mensagem de erro com estilo verde.
- Mensagens iniciadas por `Nao foi possivel...` devem usar vermelho.
- Mensagens como `Informe...`, `Nao pode...`, `Precisa...` ou bloqueios operacionais devem usar amarelo/laranja.
- Mensagens de cadastro, atualizacao, inativacao, reativacao, calculo, aprovacao e fechamento concluidos devem usar verde.
- O backend continua sendo a camada oficial de validacao; o frontend deve apenas comunicar o resultado de forma clara.

## Operacao de cadastros

- Cadastros principais devem ter:
  - criar;
  - listar;
  - visualizar status;
  - inativar;
  - reativar.
- Edicao completa pode vir depois, mas deve preservar historico quando impactar folha.
- Setor/cargo com colaborador ativo nao deve ser inativado.
- Rubrica com lancamento fixo ativo nao deve ser inativada.
- Rubrica com historico de lancamento pode ser inativada, preservando lancamentos antigos.

## Operacao de folha

- Lancamento individual:
  - usado para ajuste pontual;
  - origem `manual`.
- Lancamento em massa:
  - usado para aplicar uma rubrica a varios colaboradores;
  - origem `massa`;
  - deve permitir filtros por setor e cargo;
  - deve permitir valor fixo e percentual sobre salario.
- Lancamento fixo:
  - usado para recorrencias por colaborador;
  - deve ter inicio e fim opcional de vigencia;
  - sera consumido no calculo/fechamento da folha.

## Backend e regras de API

- API deve validar as mesmas regras criticas que o frontend sugere.
- Frontend nunca deve ser a unica camada de seguranca.
- Endpoints de manutencao devem preferir verbos operacionais:
  - `POST /{id}/activate`
  - `POST /{id}/deactivate`
  - `POST /{id}/close`
  - `POST /{id}/reopen`
- Evitar `DELETE` para dados de negocio.
- Retornar `400` com mensagem clara quando uma regra impedir a operacao.
- Retornar `404` quando o registro nao existir.

## Qualidade e testes

A partir deste ponto do projeto, cada bloco funcional relevante deve seguir o padrao minimo de testes abaixo.

Regras obrigatorias:

- Regra de negocio deve ter teste unitario.
- Endpoint critico deve ter teste de integracao.
- Permissao, bloqueio operacional e competencia fechada/reaberta devem ter teste obrigatorio.
- Frontend deve passar em `npm run build` antes de concluir a entrega.
- Backend deve passar em `dotnet build` e `dotnet test` quando houver alteracao em API, dominio, infraestrutura ou testes.

Escopo recomendado por tipo de recurso:

- CRUD operacional:
  - criar registro valido;
  - rejeitar payload invalido;
  - listar com dados essenciais;
  - inativar/reativar quando existir ciclo de vida;
  - bloquear exclusao fisica.
- Fluxos de folha:
  - bloquear lancamento em competencia fechada;
  - permitir lancamento em competencia aberta/reaberta;
  - preservar snapshots usados no calculo;
  - impedir alteracao de registros integrados em folha fechada.
- Permissoes:
  - perfil leitura nao pode abrir formularios operacionais nem salvar dados;
  - perfil sem permissao recebe bloqueio na API;
  - a interface deve esconder ou desabilitar acoes nao permitidas.
- Producao:
  - regra de valor deve ser resolvida pela pontuacao documentada;
  - empate de regra deve bloquear o apontamento/calculo;
  - ausencia de regra aplicavel deve bloquear o apontamento/calculo;
  - apontamento aprovado deve ser rastreavel ate a folha.

Testes end-to-end:

- Playwright fica como padrao futuro para fluxos principais, especialmente:
  - login;
  - criar colaborador;
  - configurar tabela de producao;
  - registrar/aprovar apontamento;
  - calcular/conferir/fechar folha;
  - emitir holerite.

Pendencia tecnica registrada:

- Criar projeto/base de testes de integracao da API com banco isolado de teste.
- Primeiros endpoints a cobrir: autenticacao, tabelas de producao, apontamentos de producao, competencias e permissoes por perfil.

## Responsividade

- Desktop: usar grades de 2 ou 3 colunas conforme densidade.
- Mobile/tablet: cair para 1 coluna.
- Tabelas devem rolar horizontalmente.
- Botoes devem manter altura minima de 40px, exceto acoes de tabela.

## Checklist antes de finalizar uma tela

- A tela tem estado de carregamento/erro global funcionando?
- Os botoes ficam desabilitados quando faltam pre-condicoes?
- A API valida a regra de negocio?
- Existe feedback de sucesso/erro?
- A tabela mostra status e acoes?
- Registros historicos foram preservados?
- `npm run build` passa?
- Regra de negocio nova tem teste unitario?
- Endpoint critico novo tem teste de integracao ou pendencia registrada?
- Bloqueios/permissoes foram testados?
- `dotnet build` e `dotnet test` passam quando houver mudanca no backend?
