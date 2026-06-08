# Plano de producao e homologacao do MVP - Sistema RH e Folha

Data: 2026-06-08

## 1. Objetivo

Preparar o MVP aprovado pela equipe para uma homologacao controlada e, depois, para uso em producao em uma VPS Linux.

Este documento define o que precisa ser validado antes de colocar o sistema em uso real, quais riscos precisam ser tratados e qual ordem tecnica deve ser seguida para publicar com seguranca.

## 2. Situacao atual do MVP

O MVP ja possui fluxo operacional principal:

- cadastro de colaboradores;
- cadastro de setores e cargos;
- cadastro de rubricas;
- cadastro e manutencao de lancamentos fixos;
- lancamentos avulsos;
- lancamentos em massa;
- competencias mensais;
- calculo da folha;
- conferencia por colaborador;
- aprovacao e fechamento de competencia;
- holerite individual;
- impressao de holerites;
- ficha do colaborador com foto, responsavel e acompanhamento;
- manutencao manual de tabelas legais.

O sistema esta aprovado funcionalmente pela equipe para avancar para homologacao.

## 3. Decisao recomendada

Nao colocar diretamente em producao como sistema oficial antes de uma fase curta de homologacao.

Recomendacao:

1. Congelar o escopo funcional do MVP.
2. Preparar ambiente de homologacao na VPS ou em ambiente equivalente.
3. Rodar uma competencia real ou simulada com dados validados.
4. Corrigir apenas erros bloqueantes ou ajustes pequenos.
5. Preparar ambiente de producao.
6. Fazer entrada assistida em producao.

## 4. Checklist tecnico obrigatorio

### 4.1 Aplicacao

- [ ] Criar configuracao por ambiente: desenvolvimento, homologacao e producao.
- [ ] Remover dependencias de dados locais ou seeds para operacao real.
- [ ] Garantir que frontend use URL correta da API em producao.
- [ ] Configurar build de producao do frontend.
- [ ] Configurar publicacao da API .NET para Linux.
- [ ] Validar uploads de fotos em pasta persistente do servidor.
- [ ] Limitar tipos e tamanho dos arquivos de upload.
- [ ] Criar politica de armazenamento para `data/uploads`.

### 4.2 Banco de dados

- [ ] Criar banco PostgreSQL de homologacao.
- [ ] Criar banco PostgreSQL de producao.
- [ ] Configurar usuario especifico da aplicacao, sem usar superusuario.
- [ ] Aplicar migrations em homologacao.
- [ ] Aplicar migrations em producao somente depois da homologacao aprovada.
- [ ] Validar timezone do banco e da aplicacao.
- [ ] Validar precisao de campos monetarios.
- [ ] Criar backup automatico diario.
- [ ] Testar restauracao de backup.

### 4.3 Seguranca

- [ ] Implementar login antes de producao oficial.
- [ ] Criar perfis minimos: administrador, RH operacional, conferente e somente leitura.
- [ ] Proteger endpoints da API por autenticacao.
- [ ] Proteger uploads contra acesso indevido, quando necessario.
- [ ] Configurar HTTPS com certificado valido.
- [ ] Configurar CORS restrito ao dominio oficial.
- [ ] Remover mensagens tecnicas sensiveis da interface.
- [ ] Revisar variaveis de ambiente e segredos.

### 4.4 Infraestrutura

- [ ] Definir modelo de deploy: Docker Compose ou systemd + Nginx.
- [ ] Configurar Nginx como proxy reverso.
- [ ] Configurar HTTPS com Let's Encrypt.
- [ ] Configurar restart automatico da API.
- [ ] Configurar logs persistentes.
- [ ] Configurar rotacao de logs.
- [ ] Monitorar uso de disco, memoria e CPU.
- [ ] Criar rotina de atualizacao controlada.

### 4.5 Observabilidade

- [ ] Registrar erros da API em log.
- [ ] Registrar falhas de calculo, fechamento e upload.
- [ ] Criar endpoint de healthcheck para monitoramento.
- [ ] Definir rotina de verificacao diaria no inicio da producao.

## 5. Checklist operacional de homologacao

### 5.1 Cadastros

- [ ] Conferir empresa ativa.
- [ ] Conferir setores.
- [ ] Conferir cargos.
- [ ] Conferir colaboradores ativos.
- [ ] Conferir responsaveis diretos.
- [ ] Conferir fotos de colaboradores quando usadas.
- [ ] Conferir rubricas e incidencias.
- [ ] Conferir tabelas legais vigentes.

### 5.2 Lancamentos

- [ ] Criar lancamento avulso individual.
- [ ] Criar lancamento em massa filtrando por setor.
- [ ] Criar lancamento em massa filtrando por cargo.
- [ ] Criar lancamento em massa com valor padrao.
- [ ] Criar lancamento em massa com quantidade e observacao padrao.
- [ ] Criar lancamento fixo por colaborador.
- [ ] Editar lancamento fixo.
- [ ] Encerrar vigencia de lancamento fixo.
- [ ] Inativar e reativar lancamento fixo.

### 5.3 Folha

- [ ] Abrir competencia.
- [ ] Calcular folha.
- [ ] Conferir proventos.
- [ ] Conferir descontos.
- [ ] Conferir informativas, como FGTS.
- [ ] Conferir bases legais: FGTS, INSS e IRRF.
- [ ] Conferir liquido por colaborador.
- [ ] Aprovar folha.
- [ ] Fechar competencia.
- [ ] Validar que competencia fechada bloqueia alteracoes indevidas.
- [ ] Reabrir competencia apenas em teste controlado.

### 5.4 Holerite e relatorios

- [ ] Imprimir holerite individual.
- [ ] Imprimir todos os holerites.
- [ ] Validar layout de impressao.
- [ ] Validar relatorios/exportacoes existentes.
- [ ] Comparar totais da conferencia com totais esperados.

## 6. Pendencias obrigatorias antes da producao oficial

Estas pendencias devem ser tratadas antes de o sistema ser usado como base oficial de folha:

- Autenticacao de usuarios.
- Perfis de permissao.
- Auditoria com usuario, data, acao e entidade alterada.
- Backup automatico e restauracao testada.
- HTTPS.
- Banco de producao separado do banco de homologacao.
- Configuracao segura de variaveis de ambiente.
- Revisao das tabelas legais por fonte confiavel.
- Plano de suporte para reabertura/correcao de competencia.
- Protecao minima contra perda de arquivos enviados.

## 7. Pendencias importantes, mas nao bloqueantes para homologacao

Podem ser feitas durante ou logo apos a homologacao:

- Melhorar pesquisa global.
- Melhorar dashboards gerenciais.
- Exportacao PDF nativa do holerite.
- Auditoria detalhada de cada campo alterado.
- Historico completo de contratos do colaborador.
- Controle de ferias.
- Controle de afastamentos.
- Integracao futura com ponto.
- Integracao futura com contas a pagar no ERP.
- Portal do colaborador.

## 8. Riscos principais

### 8.1 Risco legal

Folha de pagamento depende de regras legais que mudam ao longo do tempo.

Mitigacao:

- manter tabelas legais com vigencia;
- revisar INSS, IRRF e FGTS antes de cada uso real;
- registrar fonte e responsavel pela manutencao das tabelas;
- evitar atualizacao automatica sem validacao humana.

### 8.2 Risco de calculo

Um erro de rubrica, incidencia ou tabela pode afetar todos os colaboradores.

Mitigacao:

- manter conferencia por colaborador;
- exigir aprovacao antes do fechamento;
- bloquear alteracoes apos fechamento;
- criar testes automatizados para regras criticas.

### 8.3 Risco de dados

Perda do banco ou da pasta de uploads pode comprometer historico e fichas.

Mitigacao:

- backup diario do banco;
- backup da pasta de uploads;
- teste periodico de restauracao;
- armazenamento persistente fora do container, caso use Docker.

### 8.4 Risco de acesso indevido

Dados de RH e folha sao sensiveis.

Mitigacao:

- login obrigatorio;
- perfis de acesso;
- HTTPS;
- senhas fortes;
- nao expor banco PostgreSQL publicamente;
- limitar portas abertas na VPS.

### 8.5 Risco operacional

Usuarios podem alterar lancamentos ou competencias sem perceber impacto.

Mitigacao:

- mensagens de confirmacao em operacoes sensiveis;
- status claros;
- trilha de auditoria;
- treinamento curto da equipe;
- rotina de fechamento bem definida.

## 9. Ordem recomendada de execucao

### Etapa 1 - Congelamento do MVP

Objetivo: parar inclusao de novas funcionalidades grandes.

Atividades:

- validar que o fluxo atual esta aprovado;
- listar somente bugs e ajustes pequenos;
- marcar pendencias futuras fora do MVP;
- revisar regras de UI e operacao.

Criterio de pronto:

- equipe concorda que o MVP aprovado sera a base de homologacao.

### Etapa 2 - Segurança minima

Objetivo: impedir acesso indevido antes de usar dados reais.

Atividades:

- implementar login;
- criar perfis;
- proteger API;
- proteger rotas da interface;
- configurar segredos por ambiente.

Criterio de pronto:

- nenhum usuario acessa dados sem autenticacao.

### Etapa 3 - Preparacao da VPS

Objetivo: criar ambiente Linux seguro para homologacao.

Atividades:

- atualizar Ubuntu;
- configurar firewall;
- instalar Docker ou runtime .NET;
- instalar/configurar PostgreSQL ou container PostgreSQL;
- configurar Nginx;
- configurar HTTPS;
- criar diretorios persistentes para banco, logs e uploads.

Criterio de pronto:

- ambiente responde por dominio ou IP com HTTPS.

### Etapa 4 - Deploy de homologacao

Objetivo: publicar uma versao validavel pela equipe.

Atividades:

- build do frontend;
- publish da API;
- aplicar migrations no banco de homologacao;
- configurar variaveis de ambiente;
- subir servico;
- testar healthcheck;
- testar login;
- testar upload de foto;
- testar ciclo de folha.

Criterio de pronto:

- equipe consegue executar o fluxo completo em homologacao.

### Etapa 5 - Homologacao operacional

Objetivo: validar com dados reais ou amostra realista.

Atividades:

- cadastrar base inicial;
- rodar uma competencia simulada;
- conferir holerites;
- validar totais;
- registrar divergencias;
- corrigir bloqueantes.

Criterio de pronto:

- folha simulada aprovada pela equipe responsavel.

### Etapa 6 - Preparacao da producao

Objetivo: criar ambiente final sem misturar com homologacao.

Atividades:

- criar banco de producao;
- configurar backups;
- testar restauracao;
- aplicar migrations;
- configurar dominio oficial;
- configurar HTTPS;
- configurar logs;
- revisar usuarios e permissoes.

Criterio de pronto:

- ambiente de producao esta pronto, vazio ou com carga inicial validada.

### Etapa 7 - Entrada assistida em producao

Objetivo: usar o sistema com acompanhamento tecnico e operacional.

Atividades:

- cadastrar dados oficiais;
- abrir primeira competencia;
- acompanhar lancamentos;
- calcular e conferir;
- aprovar somente apos validacao;
- fechar competencia;
- imprimir holerites;
- registrar ajustes para a proxima versao.

Criterio de pronto:

- primeira competencia processada com sucesso e backup validado.

## 10. Modelo de deploy recomendado para a VPS

Opcao recomendada para este projeto:

- Nginx na porta 80/443.
- Frontend React publicado como arquivos estaticos.
- API .NET rodando em `localhost` via systemd ou container.
- PostgreSQL sem porta publica externa.
- Pasta persistente para uploads.
- Backup automatico do banco e uploads.

Estrutura sugerida no servidor:

```text
/opt/rh-folha/
  api/
  frontend/
  data/
    uploads/
  logs/
  backups/
```

Portas recomendadas:

- 80 e 443 abertas publicamente para Nginx.
- Porta da API acessivel apenas localmente.
- Porta do PostgreSQL fechada para internet.

## 11. Criterios de pronto para producao

O sistema so deve ser considerado pronto para producao quando:

- login estiver implementado;
- permissoes basicas estiverem aplicadas;
- backup automatico estiver funcionando;
- restauracao de backup tiver sido testada;
- HTTPS estiver ativo;
- banco de producao estiver separado;
- uploads estiverem em pasta persistente;
- tabelas legais tiverem sido revisadas;
- uma competencia tiver sido homologada;
- equipe souber abrir, calcular, conferir, aprovar e fechar folha;
- houver responsavel definido para suporte e manutencao.

## 12. Proximo passo imediato

Implementar autenticacao e perfis de acesso.

Motivo:

- e o principal bloqueio para usar dados reais;
- protege informacoes sensiveis de RH e folha;
- permite iniciar auditoria por usuario;
- prepara o sistema para homologacao em VPS.

Ordem tecnica sugerida:

1. Criar entidade de usuario.
2. Criar login com senha criptografada.
3. Criar perfis/permissoes.
4. Proteger endpoints da API.
5. Proteger interface React.
6. Registrar usuario nas acoes sensiveis.
