# OrçaFácil

Sistema de gestão de orçamentos e serviços para pequenas empresas e prestadores
de serviço. Projeto de aprendizado/portfólio, construído com qualidade
suficiente para futuramente virar produto comercial.

**Fluxo principal:** Cliente → Orçamento → Envio → Aprovação → Serviço →
Conclusão → Histórico.

## ⚠️ Sobre esta entrega (FASE 1)

Este código foi escrito em um ambiente **sem o SDK do .NET instalado e sem
acesso à internet** — não foi possível rodar `dotnet restore`, `dotnet build`
nem subir um PostgreSQL para validar nada aqui. O código foi revisado
manualmente com cuidado, mas **a primeira coisa que você deve fazer na sua
máquina é `dotnet restore` + `dotnet build`** para confirmar que compila. Se
der erro, me manda a mensagem exata — principalmente se as versões dos
pacotes NuGet abaixo não existirem mais (elas podem ter sido descontinuadas
desde que este código foi escrito):

- `Microsoft.EntityFrameworkCore` / `Microsoft.EntityFrameworkCore.Sqlite` — 8.0.8
- `Swashbuckle.AspNetCore` — 6.6.2
- `FluentValidation` — 11.9.2
- `Microsoft.AspNetCore.Authentication.JwtBearer` — 8.0.8
- `System.IdentityModel.Tokens.Jwt` — 7.5.1
- `QuestPDF` — 2024.3.6 (ver observação de licença na seção FASE 7)

## FASE 3 — Autenticação

Endpoints novos, todos em `AuthController`:

| Método | Rota | Protegido? | Faz o quê |
|---|---|---|---|
| POST | `/api/auth/register` | Não (`[AllowAnonymous]`) | Cria uma Company nova + o primeiro usuário (Owner) |
| POST | `/api/auth/login` | Não (`[AllowAnonymous]`) | Autentica e devolve um JWT |
| POST | `/api/auth/logout` | Sim | Sem efeito no servidor (JWT é stateless) — existe para o frontend ter uma chamada simétrica ao login |
| GET | `/api/auth/me` | Sim | Devolve os dados do usuário autenticado, lidos do próprio token |

**A partir desta fase, toda rota exige um token válido por padrão** — um
filtro global (`AuthorizeFilter`) foi adicionado em `Program.cs`. Isso
significa que os controllers da FASE 4 em diante (Clientes, Serviços...) já
nascem protegidos sem precisar lembrar de anotar `[Authorize]` em cada um;
só rotas públicas precisam do `[AllowAnonymous]` explícito.

### Testando no Swagger

1. Suba a API (`dotnet run --project src/OrcaFacil.Api`) e abra `/swagger`.
2. Chame `POST /api/auth/register` com um corpo como:
   ```json
   {
     "companyName": "Minha Empresa LTDA",
     "userName": "Seu Nome",
     "email": "voce@exemplo.com",
     "password": "SenhaForte123!"
   }
   ```
3. Copie o `token` da resposta.
4. No topo do Swagger, clique em **Authorize** e cole `Bearer <token>`.
5. Chame `GET /api/auth/me` — deve devolver seus dados.
6. Chame de novo sem o header Authorization (ou com um token errado) — deve
   voltar `401 Unauthorized`. Isso confirma que a proteção de rota está
   funcionando.

### O que ficou de fora de propósito (FASE 3)

- **Recuperação de senha** ("esqueci minha senha") — só a interface
  `IEmailSender` foi preparada (sem implementação). Você pediu para deixar
  isso para uma fase futura.
- **Refresh tokens / blacklist de tokens** — o token expira em 60 minutos
  (configurável em `Jwt:ExpirationMinutes`) e não pode ser revogado antes
  disso. Para um MVP isso é aceitável; para produção real, vale considerar
  refresh tokens mais adiante.

## FASE 4 e 5 — Clientes e Serviços

Nenhuma migration nova é necessária além da `InitialCreate` que você já
aplicou — as tabelas `Customers` e `Services` já foram criadas na FASE 2,
junto com todo o resto do modelo. Se você não tiver mais o banco (ex.:
apagou o arquivo `.db`), gere de novo com os mesmos comandos da FASE 2.

Dois CRUDs completos, ambos seguindo o mesmo padrão:

| Método | Rota | O quê |
|---|---|---|
| GET | `/api/customers?search=` | Lista clientes da empresa autenticada, com busca opcional por nome/telefone/e-mail/documento |
| GET | `/api/customers/{id}` | Um cliente específico |
| POST | `/api/customers` | Cria cliente |
| PUT | `/api/customers/{id}` | Atualiza cliente |
| DELETE | `/api/customers/{id}` | Remove cliente |
| GET | `/api/services?search=&category=&status=` | Lista serviços da empresa, com filtros opcionais |
| GET | `/api/services/{id}` | Um serviço específico |
| POST | `/api/services` | Cria serviço |
| PUT | `/api/services/{id}` | Atualiza serviço (inclui status) |
| DELETE | `/api/services/{id}` | Remove serviço |

Todas as rotas exigem o header `Authorization: Bearer <token>` (herdado do
filtro global da FASE 3).

**O isolamento entre empresas acontece em duas camadas, não só uma:**
1. Toda consulta já sai filtrada por `CompanyId` (`ICustomerRepository`/`IServiceRepository`),
   lido do token via `ICurrentUserService` — nunca de um parâmetro que o
   cliente da API poderia manipular.
2. Buscar por Id usa `GetByIdForCompanyAsync(id, companyId)` — id sozinho
   nunca basta. Se você autenticado na Empresa A tentar `GET /api/customers/{id-da-empresa-B}`,
   a resposta é `404`, não `403` — de propósito, para não confirmar que
   aquele id existe em outra empresa.

### O que ficou de fora de propósito

- **"Visualizar histórico do cliente"** (orçamentos, serviços, valores) —
  depende de Orçamentos (FASE 6) e Ordens de Serviço (FASE 8) existirem.
  Volta como um endpoint dedicado quando essas fases estiverem prontas.
- Excluir um cliente ou serviço que já tenha orçamentos/ordens de serviço
  vinculados vai falhar no banco (as FKs são `Restrict`/`SetNull`, configuradas
  na FASE 2) — mas como essas tabelas ainda não têm dados, não tratamos esse
  erro especificamente ainda. Isso deve ser revisitado na FASE 6/8.

### Testando no Swagger

Com o token da FASE 3 já autorizado:
1. `POST /api/customers` com `{ "name": "Cliente Teste", "phone": "11999999999" }`.
2. `GET /api/customers` — deve listar o cliente criado.
3. `POST /api/services` com `{ "name": "Troca de óleo", "defaultPrice": 150 }`.
4. `GET /api/services?status=Active` — deve listar o serviço (status nasce `Active`).
5. Registre uma segunda empresa (`POST /api/auth/register` com outro e-mail),
   autorize com o token dela, e tente `GET /api/customers/{id-da-primeira-empresa}`
   — deve voltar `404`, confirmando o isolamento entre empresas.

## FASE 6 — Orçamentos

**Nenhuma migration nova é necessária.** As tabelas `Quotes` e `QuoteItems` já
existiam desde a `InitialCreate` da FASE 2 — esta fase só adicionou código de
aplicação (regras de negócio, endpoints), nenhuma mudança de schema.

| Método | Rota | O quê |
|---|---|---|
| GET | `/api/quotes?status=&customerId=` | Lista orçamentos da empresa (resumo, sem itens) |
| GET | `/api/quotes/{id}` | Detalhe completo, com itens |
| POST | `/api/quotes` | Cria orçamento (sempre nasce como `Draft`) |
| PUT | `/api/quotes/{id}` | Substitui itens/observações/validade — só funciona em `Draft` |
| PATCH | `/api/quotes/{id}/status` | Muda o status, respeitando a máquina de estados |

**Como montar um item do orçamento** (`POST /api/quotes` e `PUT /api/quotes/{id}`):
cada item precisa vir de UM dos dois jeitos:
- `{ "serviceId": "<guid>", "quantity": 2 }` — item do catálogo; nome e preço
  são copiados do `Service` automaticamente (pode informar `unitPrice` para
  sobrescrever o preço padrão nesse orçamento específico).
- `{ "description": "Cabo flexível 2,5mm", "quantity": 10, "unitPrice": 3.5 }`
  — produto/material avulso, sem cadastro prévio.

**Cálculo automático** (dentro da própria entidade `Quote`, não duplicado em
nenhum outro lugar): para cada item, `LineTotal = (Quantidade × Preço) − Desconto do item`.
O orçamento soma isso tudo: `Subtotal` = soma bruta (quantidade × preço, sem
desconto), `DiscountAmount` = soma dos descontos de cada item, `Total` = soma
dos `LineTotal`.

**Transição de status** segue a máquina de estados abaixo — tentar pular uma
etapa (ex.: `Draft` direto para `Approved`) devolve `409 Conflict`:

```
Draft → Sent → Approved → Cancelled
              → Rejected
              → Expired
Draft → Cancelled
```

Rascunho (`Draft`) e status finais (`Rejected`, `Expired`, `Cancelled`) não
aceitam mais nenhuma transição a partir de `Approved`/`Rejected`/etc.

### O que ficou de fora de propósito

- **Transformar orçamento aprovado em ordem de serviço** — é a FASE 8.
  Por enquanto, `Approved` é só um status; nada é criado automaticamente.
- **Geração de PDF e mensagem de WhatsApp** — FASE 7.
- **Expiração automática** — nada muda o status para `Expired` sozinho
  quando `ValidUntil` passa; por ora essa transição só acontece se alguém
  chamar `PATCH .../status` manualmente. Um job agendado para isso é uma
  melhoria natural mais adiante, fora do escopo do MVP.

### Testando no Swagger

1. Crie um cliente (`POST /api/customers`) e um serviço (`POST /api/services`), copie os ids.
2. `POST /api/quotes`:
   ```json
   {
     "customerId": "<id-do-cliente>",
     "validUntil": "2026-12-31",
     "items": [
       { "serviceId": "<id-do-serviço>", "quantity": 2 },
       { "description": "Material avulso", "quantity": 1, "unitPrice": 50 }
     ]
   }
   ```
3. Confira que `subtotal`/`discountAmount`/`total` vieram calculados na resposta.
4. `PATCH /api/quotes/{id}/status` com `{ "status": "Sent" }` — deve funcionar.
5. Tente `PATCH .../status` com `{ "status": "Draft" }` de novo — deve voltar `409`.
6. Tente `PUT /api/quotes/{id}` depois do passo 4 — deve voltar `409` (não editável fora de `Draft`).

## FASE 7 — PDF e mensagem de WhatsApp

**Nenhuma migration nova.** Esta fase só adiciona geração de arquivo e texto
a partir de dados que já existiam — nenhuma tabela ou coluna nova.

Biblioteca usada: **QuestPDF**. Duas coisas importantes:

1. **Licença**: a licença Community (grátis) vale para empresas com **menos
   de US$ 1 milhão de receita bruta anual**. Para o estágio atual do
   OrçaFácil está tudo certo — mas se o produto crescer de verdade, revise
   isso antes de continuar usando gratuitamente.
2. **Risco de build**: este é o pacote com a API mais rica do projeto até
   agora, e eu não consegui compilar para validar cada método. Se
   `dotnet build` falhar especificamente em `QuestPdfQuoteGenerator.cs`,
   comece consultando a documentação oficial (https://www.questpdf.com/) —
   é provável que seja um nome de método que mudou entre versões, não um
   erro de lógica.

| Método | Rota | O quê |
|---|---|---|
| GET | `/api/quotes/{id}/pdf` | Baixa o orçamento em PDF (`application/pdf`) |
| GET | `/api/quotes/{id}/whatsapp-message` | Devolve `{ message, whatsAppLink }` prontos para copiar/abrir |

**O que entra no PDF**: nome da empresa (+ CNPJ/telefone/e-mail/endereço se
cadastrados em `CompanySettings`), número e datas do orçamento, dados do
cliente, tabela de itens, subtotal/desconto/total, observações, um texto de
condições padrão, e duas linhas de assinatura (empresa/cliente).

**Sobre o logo da empresa**: `CompanySettings.LogoUrl` só é desenhado no PDF
se apontar para um **arquivo existente no disco do servidor** — ainda não
existe upload de logo (isso é da FASE 10, "Configurações"). Até lá, o
cabeçalho do PDF funciona normalmente, só sem imagem.

**Mensagem de WhatsApp**: gerada por uma função pura (`WhatsAppMessageBuilder`,
testável sem banco), no formato "Olá, [cliente]! Seu orçamento #0012 no valor
de R$ 350,50 está disponível...". Se o cliente tiver telefone cadastrado, a
resposta também traz um link `https://wa.me/<telefone>?text=<mensagem>` —
isso **não é uma integração oficial com a API do WhatsApp**, é só um link que
abre o WhatsApp do próprio usuário com o texto pré-preenchido, por pedido
explícito de deixar isso simples por enquanto. A normalização do telefone é
básica (só remove caracteres não numéricos) — se o número cadastrado não
tiver o DDI (ex.: 55 do Brasil), o link pode não abrir a conversa certa.

Os botões "Compartilhar" / "Baixar PDF" / "Copiar mensagem" da FASE 7 do
enunciado original mapeiam assim no frontend (que ainda não existe — isso é
da FASE 11): "Baixar PDF" chama o primeiro endpoint acima; "Copiar mensagem"
e "Compartilhar" chamam o segundo e usam `message`/`whatsAppLink` conforme
disponíveis.

### Testando

1. Garanta que o cliente do orçamento tem telefone cadastrado (para testar o link).
2. `GET /api/quotes/{id}/pdf` no Swagger — deve baixar um PDF com todos os dados.
3. Abra o PDF e confira: nome da empresa, dados do cliente, tabela de itens,
   totais, texto de condições, linhas de assinatura.
4. `GET /api/quotes/{id}/whatsapp-message` — confira que a mensagem tem os
   valores certos e que `whatsAppLink` foi preenchido.
5. Edite o cliente removendo o telefone e repita o passo 4 — `whatsAppLink`
   deve vir `null`.

## FASE 8 — Ordens de Serviço

**Nenhuma migration nova.** `WorkOrder`/`WorkOrderItem` já existiam desde a
FASE 2 — só código de aplicação novo.

| Método | Rota | O quê |
|---|---|---|
| GET | `/api/work-orders?status=&customerId=&assignedUserId=` | Lista (resumo) |
| GET | `/api/work-orders/{id}` | Detalhe completo, com itens |
| POST | `/api/work-orders` | Cria uma O.S. diretamente, sem passar por orçamento |
| POST | `/api/work-orders/from-quote/{quoteId}` | Transforma um orçamento **aprovado** em O.S., copiando os itens |
| PUT | `/api/work-orders/{id}` | Edita responsável/data prevista/observações/itens — só fora de `Completed`/`Cancelled` |
| PATCH | `/api/work-orders/{id}/status` | Muda o status, respeitando a máquina de estados |

**Duas formas de criar uma O.S.**, porque o enunciado original permite as
duas: convertendo um orçamento aprovado (`POST .../from-quote/{quoteId}`,
que falha com `409` se o orçamento não estiver `Approved` ou já tiver sido
convertido antes) ou criando direto (`POST /api/work-orders`), para quando
não existe orçamento prévio.

**Responsável (`assignedUserId`)** precisa ser um usuário da mesma empresa —
validado a cada criação/edição; usuário de outra empresa (ou inexistente)
devolve `404`, seguindo o mesmo princípio de isolamento das fases anteriores.

**Máquina de estados:**

```
Awaiting → Scheduled → InProgress → Completed
Awaiting → InProgress
Awaiting/Scheduled/InProgress → Cancelled
```

`Completed` e `Cancelled` são finais — nenhuma transição a partir deles.

### O que ficou de fora de propósito

- **Total não é uma coluna no banco** — é calculado on-the-fly (soma dos
  `LineTotal` dos itens) toda vez que uma O.S. é lida, então não existe risco
  de ficar desatualizado, mas também não dá para filtrar/ordenar por ele
  numa consulta SQL sem calcular primeiro. Se isso virar necessidade real
  (ex.: relatório de faturamento), aí sim vale adicionar como coluna.
- **Excluir uma ordem de serviço** — o enunciado original não pede isso para
  O.S. (diferente de Clientes/Serviços), então não implementei. Avise se
  precisar.
- **Desconto do orçamento não é copiado na conversão** — a O.S. registra
  `UnitPrice` do item tal como estava no orçamento, mas não carrega o
  `DiscountAmount` daquela negociação. Isso é intencional: a O.S. é sobre
  execução, não sobre a parte comercial já fechada.

### Testando

1. Aprove um orçamento (`PATCH /api/quotes/{id}/status` com `{ "status": "Sent" }`, depois `{ "status": "Approved" }`).
2. `POST /api/work-orders/from-quote/{id-do-orçamento}` — confira que os itens vieram copiados.
3. Tente chamar o mesmo endpoint de novo com o mesmo orçamento — deve voltar `409`.
4. `PATCH /api/work-orders/{id}/status` com `{ "status": "Scheduled" }`, depois `{ "status": "InProgress" }`, depois `{ "status": "Completed" }`.
5. Tente `PUT /api/work-orders/{id}` depois de `Completed` — deve voltar `409`.

## FASE 9 — Dashboard e Métricas

**Nenhuma migration nova.** Dashboard é 100% leitura agregada sobre tabelas
que já existiam — nenhuma entidade ou coluna nova.

| Método | Rota | O quê |
|---|---|---|
| GET | `/api/dashboard/summary` | Visão geral: orçamentos totais/pendentes/aprovados, O.S. em andamento/concluídas, faturamento estimado/realizado |
| GET | `/api/dashboard/revenue?months=` | Série de faturamento mensal (padrão: 6 meses) |
| GET | `/api/dashboard/quote-conversion?months=` | Contagem por status + taxa de conversão (sem `months`, considera todo o histórico) |
| GET | `/api/dashboard/service-ranking?take=` | Ranking de serviços por faturamento (padrão: top 10) |

**Sobre o isolamento**: como no resto do sistema, é por **empresa**
(`CompanyId`, lido do token do usuário logado via `ICurrentUserService`) — um
usuário nunca vê números de outra empresa, mas todos os usuários da mesma
empresa veem os mesmos números. Esta é a única fase com um teste de
**integração** (banco SQLite real, em memória) em vez de só testes puros —
isolamento entre empresas é uma garantia que só se prova de verdade
observando o SQL filtrar corretamente, não só a lógica em C#.

### As três métricas que você pediu, e como cada uma foi calculada

**1. Faturamento mensal** (`/revenue`) — dois valores por mês: `realizedRevenue`
(soma de O.S. **concluídas** naquele mês) e `estimatedRevenue` (soma de
orçamentos **aprovados** naquele mês — podem nunca virar O.S., ou virar em
outro mês). Meses sem nenhum dado aparecem com `0`, não somem da lista — sem
isso, um gráfico de comparação mês a mês ficaria com buracos difíceis de
interpretar. **Limitação que vale registrar**: não existe um campo
`CompletedAt`/`ApprovedAt` explícito no banco — uso `UpdatedAt` como proxy
(confiável porque editar depois de `Completed`/`Approved` já é bloqueado
pelas regras da FASE 6/8, então `UpdatedAt` reflete o momento real da
mudança de status). Se um dia for preciso auditar histórico completo de
mudanças de status, vale adicionar um campo dedicado.

**2. Taxa de conversão de orçamentos** (`/quote-conversion`) — contagem por
todos os status (`Draft`, `Sent`, `Approved`, `Rejected`, `Expired`,
`Cancelled`) mais `conversionRate`. A taxa considera só orçamentos **já
decididos** (aprovados + rejeitados) como denominador — orçamentos ainda em
rascunho/enviado/expirado/cancelado não entram nela, porque misturá-los
distorceria a resposta a "dos orçamentos que o cliente decidiu, quantos ele
aceitou". `months` (opcional) filtra por orçamentos **emitidos** nesse
período, não por quando foram decididos.

**3. Ranking de serviços** (`/service-ranking`) — cada serviço do catálogo
aparece com `executionCount` (quantas vezes apareceu em uma O.S. concluída),
`totalQuantity` (soma das quantidades) e `totalRevenue` (soma do faturamento
gerado), ordenado por `totalRevenue` decrescente. Só conta itens de O.S.
**concluídas** e vinculados a um `Service` do catálogo — um item avulso
(produto/material sem cadastro prévio) não tem "serviço" para ranquear.
Sobre "Labor/Parts": o schema atual não distingue mão de obra de peças — só
existe `Service.Category`, que já pode ser usada informalmente para separar
isso (ex.: categoria "Mão de obra" vs. "Peças") se fizer sentido no seu caso
de uso. Adicionar essa distinção como campo formal é uma mudança pequena,
me avise se for necessária.

### Uma decisão de risco técnico que vale explicar

Para agrupar por mês, a forma "correta" seria fazer o banco de dados agrupar
(`GROUP BY` em ano/mês). Não tenho como testar se o provider SQLite do EF
Core traduz `DateTime.Year`/`.Month` corretamente dentro de uma consulta sem
rodar o projeto — e um erro de tradução aí quebraria o endpoint inteiro. Por
segurança, busco os dados já filtrados por empresa/status/período do banco
(isso sim é uma tradução simples e segura) e agrupo por mês **em memória**,
em C# puro. Para o volume de dados de um MVP isso é irrelevante em
performance; se o volume crescer muito (milhares de O.S. por mês), aí sim
vale mover a agregação para o banco.

### Testando

1. Com orçamentos e O.S. já criados nas fases anteriores, chame `GET /api/dashboard/summary` e confira os números batendo com o que você criou manualmente.
2. `GET /api/dashboard/revenue?months=3` — confira que vêm exatamente 3 meses, mesmo que só um tenha dado.
3. `GET /api/dashboard/quote-conversion` — confira que `conversionRate` bate com aprovados/(aprovados+rejeitados) × 100.
4. `GET /api/dashboard/service-ranking` — confira que só aparecem serviços de O.S. já `Completed`.
5. Registre uma segunda empresa, crie um orçamento nela, e confirme que `/summary` da primeira empresa não mudou.

## Correção pós-Fase 9 + Campo de Observações / Ressalvas Técnicas

**1. Bug corrigido**: `MonthlyRevenueBuilder.cs` usava o tipo `MonthlyAmount`
sem o `using OrcaFacil.Application.Interfaces;` correspondente — erro
`CS0246` no build. Corrigido.

**2. Novo campo `TechnicalObservations`** em `Quote` e `WorkOrder` — **nova
migration necessária**:

```bash
cd backend
dotnet ef migrations add AddTechnicalObservations \
  --project src/OrcaFacil.Infrastructure \
  --startup-project src/OrcaFacil.Api

dotnet ef database update \
  --project src/OrcaFacil.Infrastructure \
  --startup-project src/OrcaFacil.Api
```

**Por que um campo novo, e não reaproveitar `Notes`?** `Notes` já existia
para observações gerais (ex.: "cliente pediu entrega até sexta").
`TechnicalObservations` é conceitualmente diferente: existe especificamente
para registrar ressalvas que protegem a empresa juridicamente (ex.: "peça X
está desgastada, cliente optou por não autorizar a troca agora"). Misturar
os dois faria uma ressalva importante se perder no meio de observações
soltas — por isso é um campo à parte, com tratamento visual próprio no PDF.

**Onde entrou**:
- `Quote.TechnicalObservations` / `WorkOrder.TechnicalObservations`
  (`string?`, até 1000 caracteres, mapeado via Fluent API)
- `CreateQuoteDto`, `UpdateQuoteDto`, `QuoteResponseDto` e os equivalentes de
  `WorkOrder` — todos com o novo campo
- Validado com `MaximumLength(1000)` nos validadores de criação/edição de
  ambos
- **Conversão de orçamento em O.S.** (`POST /api/work-orders/from-quote/{id}`)
  copia `TechnicalObservations` do orçamento para a O.S. gerada — faz
  sentido que a ressalva registrada no orçamento continue visível na O.S.
  que nasce dele

**No PDF** (orçamento e O.S. — ambos ganharam essa seção): se o campo tiver
texto, aparece um bloco com fundo amarelo claro, título **"Observações /
Ressalvas Técnicas:"**, logo depois de observações gerais e antes do texto de
condições. A legenda da assinatura do cliente muda automaticamente quando
existe uma ressalva: em vez de "Assinatura do Cliente", aparece **"Declaro
estar ciente dos serviços realizados e das ressalvas acima descritas."** —
sem ressalva preenchida, a legenda volta a ser a original.

**PDF de Ordem de Serviço criado do zero** — até esta rodada, só existia
gerador de PDF para Orçamento (FASE 7). Adicionei um novo (`GET
/api/work-orders/{id}/pdf`), mesma estrutura e mesmas ressalvas de risco já
registradas para o PDF de orçamento (API do QuestPDF rica, não testável aqui).

### Testando

1. Rode a migration acima.
2. Crie um orçamento com `technicalObservations` preenchido.
3. `GET /api/quotes/{id}/pdf` — confira o bloco amarelo e a legenda de ciência no lugar de "Assinatura do Cliente".
4. Aprove o orçamento e converta em O.S. (`POST /api/work-orders/from-quote/{id}`) — confira que `technicalObservations` veio junto.
5. `GET /api/work-orders/{id}/pdf` — confira que o PDF novo também mostra o bloco.
6. Crie um orçamento **sem** `technicalObservations` e confirme que o bloco não aparece e a legenda volta a ser "Assinatura do Cliente".

## Correção pós-Fase 9 (2) — `GetSummaryAsync` falhando em teste

Um dos 76 testes falhou: `DashboardRepositoryTests.GetSummaryAsync_NaoDeveMisturarDadosDeOutraEmpresa`,
erro em `ToListAsync()` dentro de `GetSummaryAsync`. A causa provável é o
padrão `w.Items.Sum(i => i.LineTotal)` **dentro** de um `.Select()` — uma
agregação sobre coleção de navegação embutida numa projeção, que exige uma
subquery correlacionada por linha. Eu tinha registrado esse trecho como
"seguro" num comentário anterior sem conseguir testar de verdade — o erro
mostrou que essa confiança não era justificada.

**Correção**: reescrevi `GetSummaryAsync` e `GetCompletedWorkOrderRevenueByMonthAsync`
para nunca agregar uma coleção de navegação dentro de `Select()`. Em vez
disso, uso `CountAsync`/`SumAsync` direto contra os `DbSet`s com filtro
(`Where` + agregado, sem projeção aninhada) ou busco direto em
`WorkOrderItems` (uma linha por item, com `i.WorkOrder.CompanyId`/`i.WorkOrder.Status`
— navegação simples, não coleção) — o mesmo padrão que `GetServiceRankingAsync`
já usava com sucesso. `DashboardRepositoryTests.cs` não precisou de nenhuma
mudança: os testes validam o comportamento do repositório, não como ele
está implementado por dentro.

**Grau de confiança**: não consigo rodar `dotnet test` aqui para confirmar
que os 76 testes passam agora — mas a mudança elimina o padrão específico
apontado pelo erro, sem alterar o resultado esperado de nenhuma consulta
(são reescritas matematicamente equivalentes, só numa forma mais simples de
traduzir para SQL).

## Correção pós-Fase 9 (3) — pacote nativo do SQLite faltando no projeto de testes

O mesmo teste continuou falhando depois da correção acima, agora numa linha
diferente (`SumAsync` em vez de `ToListAsync`) — sinal de que a consulta em
si nunca foi o problema. A causa real: `OrcaFacil.Tests.csproj` só recebia
`Microsoft.EntityFrameworkCore.Sqlite` **transitivamente**, via a referência
a `OrcaFacil.Infrastructure`. O pacote nativo do SQLite (`SQLitePCLRaw`, que
faz a ponte com a biblioteca C de verdade) nem sempre é copiado para a pasta
de saída de um projeto quando a referência é só transitiva — e como esse
pacote só é "usado" de fato na primeira operação real contra o banco, o erro
aparece ali, mesmo a causa sendo outra.

**Correção**: adicionei `Microsoft.EntityFrameworkCore.Sqlite` como
referência **direta** em `OrcaFacil.Tests.csproj` (mesma versão, 8.0.8, já
usada em `Infrastructure` — sem conflito). Nenhuma mudança em
`DashboardRepositoryTests.cs` ou em `DashboardRepository.cs` foi necessária
desta vez — o problema nunca esteve no código, estava no `.csproj`.

## Correção pós-Fase 9 (4) — causa raiz real: SQLite não agrega `decimal`

Depois da correção do `.csproj`, o teste continuou falhando — e desta vez
veio a mensagem de erro exata: `SQLite cannot apply aggregate operator
'Sum' on expressions of type 'decimal'`. Essa é uma limitação real e
documentada do provider SQLite do EF Core — ele não sabe traduzir `SUM()`
sobre uma coluna `decimal` para SQL (outro provider, como PostgreSQL, não
teria esse problema). Nenhuma das minhas duas hipóteses anteriores
(subquery correlacionada, pacote nativo faltando) era a causa raiz — as
duas eram plausíveis e uma delas (o pacote nativo) provavelmente era um
problema real também, só que não o que estava quebrando *esse* teste
especificamente.

**Correção**: os dois `SumAsync(x => x.CampoDecimal)` em `GetSummaryAsync`
viraram "busca só a coluna para memória, depois `.Sum()` em LINQ-to-Objects"
— mesmo padrão que o resto do arquivo já usava (`GetCompletedWorkOrderRevenueByMonthAsync`,
`GetApprovedQuoteRevenueByMonthAsync`, `GetServiceRankingAsync` nunca usaram
`SumAsync`, só `Sum()` depois de um `ToListAsync()` — só o `GetSummaryAsync`
reescrito na rodada anterior introduziu o problema, tentando ser mais
"eficiente" com agregação no banco).

## Arquitetura

```
orcafacil/
  backend/                   → API em .NET (Clean Architecture)
    src/
      OrcaFacil.Domain/       → entidades puras, zero dependências externas
      OrcaFacil.Application/  → regras de negócio, DTOs, interfaces, validadores
      OrcaFacil.Infrastructure/ → EF Core, SQLite (dev) / PostgreSQL (produção), autenticação (implementa Application)
      OrcaFacil.Api/          → Controllers, Program.cs (composition root)
    tests/
      OrcaFacil.Tests/        → testes automatizados (xUnit + Moq)
  frontend/                  → Next.js (App Router) + TypeScript + Tailwind
  docker-compose.yml         → sobe o PostgreSQL local (opcional — não usado com SQLite)
```

**Regra de dependência**: `Api → Infrastructure → Application → Domain`. O
Domain nunca conhece Infrastructure. Isso é o que torna a regra de negócio
testável sem precisar de um banco de dados real — veja
`backend/tests/OrcaFacil.Tests/Domain/BaseEntityTests.cs` para um exemplo.

## Como rodar — Backend

Pré-requisitos: [.NET 8 SDK](https://dotnet.microsoft.com/download). **Nenhum
Docker ou servidor de banco de dados é necessário** — o projeto usa SQLite
(um arquivo local, `orcafacil.db`), justamente para funcionar sem
virtualização habilitada na BIOS.

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project src/OrcaFacil.Api
```

A API sobe em `http://localhost:5000`, com Swagger em `http://localhost:5000/swagger`.
Teste `GET /api/health` — deve responder `{ "status": "ok", "database": "connected" }`.

Nesta FASE 1 não existem entidades nem migrations ainda — isso é da FASE 2.

## FASE 2 — Gerar e aplicar a primeira migration

As entidades e o mapeamento já estão prontos, mas **a migration em si não foi
gerada aqui** — ela precisa ser criada pela ferramenta `dotnet-ef`, que
reflete sobre o código já compilado; escrevê-la à mão seria muito frágil.

```bash
cd backend

# instale a ferramenta uma vez (se ainda não tiver)
dotnet tool install --global dotnet-ef

dotnet ef migrations add InitialCreate \
  --project src/OrcaFacil.Infrastructure \
  --startup-project src/OrcaFacil.Api

dotnet ef database update \
  --project src/OrcaFacil.Infrastructure \
  --startup-project src/OrcaFacil.Api
```

Isso cria o arquivo `src/OrcaFacil.Api/orcafacil.db` na primeira execução (o
SQLite não precisa que o arquivo exista antes). Depois disso, `GET /api/health`
deve continuar respondendo `"database": "connected"`.

Para inspecionar as tabelas geradas sem instalar nada além do que você já tem:

```bash
dotnet tool install --global dotnet-ef # se ainda não instalou
sqlite3 src/OrcaFacil.Api/orcafacil.db ".tables"
```

(Se não tiver o `sqlite3` CLI, o próprio Visual Studio/Rider/VS Code com a
extensão "SQLite Viewer" abre o arquivo `.db` normalmente.)

### Sobre a troca de PostgreSQL para SQLite

Troquei porque sua BIOS está com virtualização desativada e o Docker não sobe
sem ela — SQLite não precisa de servidor nenhum, é só um arquivo. **Nada nas
entidades ou nas regras de negócio muda** — só o provider do EF Core
(`UseSqlite` em vez de `UseNpgsql`) e a connection string. Quando quiser
voltar para PostgreSQL (por exemplo antes de colocar em produção, já que
SQLite não é recomendado para um backend multiusuário real), a mudança é a
mesma no sentido contrário: trocar o pacote NuGet, `UseSqlite` → `UseNpgsql`,
e gerar uma nova migration a partir do zero (migrations de SQLite e de
PostgreSQL não são intercambiáveis, cada provider gera SQL diferente).

## Como rodar — Frontend

Pré-requisitos: Node.js 18.18+.

```bash
cd frontend
npm install
cp .env.local.example .env.local
npm run dev
```

Abra `http://localhost:3000`.

## Ordem de implementação

| Fase | Conteúdo | Status |
|---|---|---|
| 1 | Estrutura do projeto e arquitetura | ✅ Validado por você |
| 2 | Banco de dados e entidades | ✅ Validado por você (SQLite) |
| 3 | Autenticação (JWT) | ✅ Validado por você |
| 4 | Clientes (CRUD) | ✅ Validado por você |
| 5 | Serviços | ✅ Validado por você |
| 6 | Orçamentos | ✅ Validado por você (46 testes passando) |
| 7 | Geração de PDF | ✅ Validado por você |
| 8 | Ordens de serviço | ✅ Validado por você (62 testes passando) |
| 9 | Dashboard | ✅ Este commit |
| 10 | Configurações da empresa | Próxima |
| 11 | UX/UI e responsividade | — |
| 12 | Testes | — |
| 13 | README e documentação final | — |
| 14 | Preparação para deploy | — |

## Segurança — regras que valem desde já

- Nenhum secret vai para `appsettings.json` — connection strings e chaves JWT
  reais entram via variável de ambiente ou `dotnet user-secrets`.
- `appsettings.Development.json` está versionado só com uma senha placeholder
  (`CHANGE_ME`) para desenvolvimento local — troque antes de usar.
- CORS já está restrito à URL do frontend (não usa `AllowAnyOrigin`).
