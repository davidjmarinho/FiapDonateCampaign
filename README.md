# FiapDonateCampaign

API .NET 8 para gestão de campanhas e registro de intenções de doação na plataforma FiapDonate.

> O projeto é responsável pelas campanhas e doações locais. Usuários, roles e o schema do ASP.NET Identity são consultados neste serviço, mas pertencem ao serviço responsável por autenticação.

## Visão geral

A solução está organizada em camadas:

```text
FiapDonateCampaign.API             Host HTTP, controllers e pipeline
FiapDonateCampaign.Application     Casos de uso, DTOs e validações
FiapDonateCampaign.Domain          Entidades e regras de domínio
FiapDonateCampaign.Infrastructure  EF Core, SQL Server, Identity e repositórios
```

### Tecnologias

- .NET 8 e ASP.NET Core
- Entity Framework Core 8 com SQL Server
- ASP.NET Core Identity
- JWT Bearer Authentication
- FluentValidation
- Swagger/OpenAPI em ambiente `Development`
- xUnit para os projetos de teste

## Requisitos

- .NET SDK 8
- SQL Server 2022 ou compatível
- Docker Desktop (opcional, para execução em containers)
- Kubernetes e `kubectl` (opcional, para deploy no cluster)

## Configuração

A API usa as seguintes chaves de configuração:

| Chave | Descrição |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | Connection string do SQL Server. |
| `Jwt__Key` | Chave de assinatura JWT. Deve ser secreta e ter ao menos 32 caracteres. |
| `Jwt__Issuer` | Emissor esperado do token. |
| `Jwt__Audience` | Audiência esperada do token. |
| `RabbitMq__Host` | Host do broker RabbitMQ (compartilhado com o Worker). |
| `RabbitMq__VirtualHost` | Virtual host do RabbitMQ (padrão `/`). |
| `RabbitMq__Username` / `RabbitMq__Password` | Credenciais do RabbitMQ. |

Em desenvolvimento, o arquivo [FiapDonateCampaign.API/appsettings.json](FiapDonateCampaign.API/appsettings.json) contém valores locais. Para ambientes compartilhados, use variáveis de ambiente ou Secrets; não versione credenciais reais.

Em execução Docker integrada, a API também aceita Docker Secrets montados em `/run/secrets`. Os nomes dos arquivos devem ser `ConnectionStrings__DefaultConnection` e `Jwt__Key`. Essa fonte tem precedência sobre `appsettings` e variáveis de ambiente.

Exemplo de connection string para SQL Server em container:

```text
Server=sqlserver,1433;Database=FiapDonateDb;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True;
```

## Banco de dados e Identity

A aplicação usa dois contextos que apontam para o mesmo banco físico:

- `AppDbContext`: é responsável pelas tabelas `Campaigns` e `Donation` e pelas migrations deste repositório.
- `IdentityStoreDbContext`: acessa usuários e roles do ASP.NET Identity. Este projeto **não** é dono das migrations nem do seed de usuários e roles.

As migrations não são aplicadas automaticamente quando a API inicia. Antes de atender tráfego contra um banco vazio, aplique as migrations de `AppDbContext` pelo processo de deploy adotado pela equipe.

## Execução local

### Com .NET SDK

1. Configure o SQL Server e as chaves JWT por `appsettings` ou variáveis de ambiente.
2. Aplique as migrations necessárias.
3. Execute a API:

```bash
dotnet run --project FiapDonateCampaign.API
```

O perfil HTTP de desenvolvimento utiliza `http://localhost:5296`. Swagger fica disponível em [http://localhost:5296/swagger](http://localhost:5296/swagger) enquanto o ambiente for `Development`.

### Com Docker Compose

1. Copie [.env.example](.env.example) para `.env`.
2. Substitua `MSSQL_SA_PASSWORD` e `JWT_KEY` por valores seguros.
3. Execute:

```bash
docker compose up --build
```

A API fica disponível em `http://localhost:8080`, e o SQL Server é publicado na porta `1433`.

Consulte [docs/CONTAINERIZATION_STATUS.md](docs/CONTAINERIZATION_STATUS.md) para limitações e detalhes de operação.

## API HTTP

Todos os endpoints, exceto o login, exigem um token Bearer válido.

| Método | Rota | Regra de autorização |
| --- | --- | --- |
| `POST` | `/api/auth/login` | Público |
| `POST` | `/api/campaign` | Role `GestorONG` |
| `PUT` | `/api/campaign/{id}` | Role `GestorONG` |
| `GET` | `/api/campaign/ativas` | Qualquer usuário autenticado |
| `GET` | `/api/campaign/{id}` | Qualquer usuário autenticado |
| `POST` | `/api/doacoes` | Role `Doador` |

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "senha": "SenhaForte123!"
}
```

A API consulta o usuário no store Identity compartilhado e devolve um objeto com o token JWT quando as credenciais são válidas.

### Criar campanha

```http
POST /api/campaign
Authorization: Bearer {token}
Content-Type: application/json

{
  "titulo": "Campanha de Verão",
  "descricao": "Arrecadação para projeto social",
  "dataInicio": "2026-09-01T00:00:00Z",
  "dataFim": "2026-12-01T00:00:00Z",
  "metaFinanceira": 10000.00
}
```

### Registrar intenção de doação

```http
POST /api/doacoes
Authorization: Bearer {token}
Content-Type: application/json

{
  "idCampanha": "00000000-0000-0000-0000-000000000000",
  "valorDoacao": 50.00
}
```

A campanha precisa estar apta a receber doações. O identificador do doador é extraído do claim `NameIdentifier` do JWT.

## Mensageria

O caso de uso de doação (`DonationService.RegistrarIntencaoAsync`) **não** atualiza `ValorArrecadado` diretamente: ele publica `DoacaoRecebidaEvent` no RabbitMQ via `MassTransitEventPublisher` (implementação real de `IEventPublisher`, usando `IPublishEndpoint` do MassTransit).

Arquitetura database-per-service: esta API e o repositório `FiapDonateWorker` têm cada um o seu próprio banco (a tabela `Campaigns` não é compartilhada). A integração é feita **100% por eventos**:

1. Esta API publica `DoacaoRecebidaEvent` (só para campanhas ativas — a validação de atividade acontece no intake, antes de publicar).
2. O Worker consome o evento, credita o valor no seu próprio banco (`conexao_solidaria`) e publica de volta `ValorArrecadadoAtualizadoEvent`.
3. Esta API consome esse evento (`ValorArrecadadoAtualizadoConsumer`) e atualiza `Campaigns.ValorArrecadado` com o valor absoluto recebido — é esse valor que o Painel de Transparência (`GET /api/campaign/ativas`) expõe.

> **Contrato entre repositórios:** os tipos `DoacaoRecebidaEvent` e `ValorArrecadadoAtualizadoEvent` desta API têm o namespace `FiapDonateWorker.Api.Events` — igual ao do repositório `FiapDonateWorker` — de propósito. O MassTransit roteia mensagens pela identidade do tipo CLR (namespace + nome); como os dois serviços estão em repositórios distintos sem um pacote de contratos compartilhado, o namespace precisa ser idêntico nos dois lados para a mensagem chegar à fila correta. Veja os comentários em `FiapDonateCampaign.Application/Event/DoacaoRecebidaEvent.cs` e `FiapDonateCampaign.API/Events/ValorArrecadadoAtualizadoEvent.cs`.

A configuração do RabbitMQ é lida de `RabbitMq:Host`/`VirtualHost`/`Username`/`Password` (`appsettings`, variáveis de ambiente ou `RabbitMq__*` no Compose/Kubernetes).

## Kubernetes

Os manifestos ficam em [k8s](k8s):

- [k8s/configmap.yaml](k8s/configmap.yaml): ambiente, URL HTTP e configuração JWT não sensível;
- [k8s/secret.example.yaml](k8s/secret.example.yaml): modelo do Secret para a connection string e chave JWT;
- [k8s/deployment.yaml](k8s/deployment.yaml): uma réplica da imagem `fiapdonatecampaign:local` na porta `8080`;
- [k8s/service.yaml](k8s/service.yaml): Service `ClusterIP` com o DNS `fiapdonatecampaign`.

Para aplicar no cluster:

1. Publique a imagem em um registry acessível ou disponibilize `fiapdonatecampaign:local` no cluster local.
2. Copie [k8s/secret.example.yaml](k8s/secret.example.yaml) para `k8s/secret.yaml`, preencha valores seguros e não versione esse arquivo.
3. Ajuste a connection string para o DNS real do SQL Server no cluster.
4. Aplique os manifests:

```bash
kubectl apply -f k8s/configmap.yaml -f k8s/secret.yaml -f k8s/deployment.yaml -f k8s/service.yaml
```

A comunicação interna com a API ocorre por `http://fiapdonatecampaign:8080`.

A API expõe `/health/live`, `/health/ready`, `/health` e `/metrics`. O Kubernetes já pode usar probes HTTP em `/health/live` e `/health/ready`.

## Testes e validação

Execute todos os testes:

```bash
dotnet test FiapDonateCampaign.slnx
```

Valide o build:

```bash
dotnet build FiapDonateCampaign.slnx
```

A imagem pode ser construída isoladamente com:

```bash
docker build --tag fiapdonatecampaign:local .
```

## Integração com FiapDonateServices

O guia operacional para o repositório central de infraestrutura está em [docs/fiapdonatecampaign-integracao.md](docs/fiapdonatecampaign-integracao.md). Ele descreve DNS, variáveis de ambiente, Secrets, banco de dados, JWT, Kubernetes e pendências conhecidas.

## Estado atual e pendências

- O SQL Server, o RabbitMQ e o JWT já podem ser configurados por variáveis de ambiente, Compose e ConfigMap/Secret Kubernetes.
- A publicação de `DoacaoRecebidaEvent` e o consumo de `ValorArrecadadoAtualizadoEvent` já são reais (MassTransit + RabbitMQ), fechando o fluxo assíncrono com o Worker.
- A aplicação depende do schema e dos usuários provisionados pelo serviço proprietário do Identity.
- Migrations precisam ser executadas externamente.
- A API já expõe health checks HTTP e `/metrics` para observabilidade.
- Health checks e métricas agora estão implementados.
