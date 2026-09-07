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
| 4 | Clientes (CRUD) | ✅ Este commit |
| 5 | Serviços | ✅ Validado por você |
| 6 | Orçamentos | ✅ Validado por você (46 testes passando) |
| 7 | Geração de PDF | ✅ Este commit |
| 8 | Ordens de serviço | Próxima |
| 9 | Dashboard | — |
| 10 | Configurações da empresa | — |
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
