# 🎯 Fiap Donate Campaign

Uma plataforma robusta e escalável para gerenciamento de campanhas de doação desenvolvida com **.NET 8** e arquitetura em camadas.

## 📋 Sumário

- [Visão Geral](#visão-geral)
- [Arquitetura](#arquitetura)
- [Stack Tecnológico](#stack-tecnológico)
- [Estrutura do Projeto](#estrutura-do-projeto)
- [Requisitos](#requisitos)
- [Instalação & Configuração](#instalação--configuração)
- [Como Executar](#como-executar)
- [API Endpoints](#api-endpoints)
- [Testes](#testes)
- [Segurança](#segurança)
- [Padrões de Código](#padrões-de-código)
- [Regras Aplicadas](#regras-aplicadas-ao-projeto)
- [Convenções de Nomenclatura](#convenções-de-nomenclatura)
- [Fluxo de Requisição](#fluxo-de-requisição-típico)
- [Variáveis de Ambiente](#variáveis-de-ambiente-e-configurações)
- [Boas Práticas](#boas-práticas-de-desenvolvimento)
- [Referências](#referências)
- [Contribuição](#contribuição)
- [Licença](#licença)
- [Contato](#contato)

---

## 🎯 Visão Geral

**FiapDonateCampaign** é uma solução completa para gerenciar campanhas de arrecadação de doações. O sistema oferece:

- ✅ Autenticação e autorização com JWT Bearer Token
- ✅ Gestão de campanhas com validações de negócio
- ✅ Banco de dados relacional com SQL Server
- ✅ Documentação automática com Swagger/OpenAPI
- ✅ Tratamento centralizado de exceções
- ✅ Validação fluente de dados de entrada
- ✅ Estrutura pronta para testes (Integração e Unitários)

---

## 🏗️ Arquitetura

O projeto utiliza uma **arquitetura em camadas (Clean Architecture)** com separação clara de responsabilidades:

```
┌─────────────────────────────────────────────────┐
│          API Layer (Controllers)                │  ← Entrada HTTP
├─────────────────────────────────────────────────┤
│          Application Layer (Services)           │  ← Lógica de Aplicação
├─────────────────────────────────────────────────┤
│          Domain Layer (Entities & Rules)        │  ← Regras de Negócio
├─────────────────────────────────────────────────┤
│   Infrastructure (DB, Auth, Repositories)      │  ← Persistência
└─────────────────────────────────────────────────┘
```

### 📦 Camadas

#### **1. FiapDonateCampaign.Domain**
- **Responsabilidade**: Define as entidades e regras de negócio centrais
- **Componentes principais**:
  - `Campaign` - Entidade principal que representa uma campanha de doação
  - `StatusCampaign` - Enum com estados possíveis de uma campanha
  - `ICampaignRepository` - Contrato para persistência
  - `DomainException` - Exceções específicas do domínio

#### **2. FiapDonateCampaign.Application**
- **Responsabilidade**: Orquestra a lógica de aplicação entre controller e domínio
- **Componentes principais**:
  - `CampaignService` - Serviço que implementa casos de uso
  - `CampaignRequestDto` / `CampaignResponseDto` - DTOs para comunicação
  - `CampaignRequestValidator` - Validação usando FluentValidation
  - `ICampaignService` - Interface do serviço

#### **3. FiapDonateCampaign.Infrastructure**
- **Responsabilidade**: Implementa detalhes técnicos (banco de dados, autenticação, repositórios)
- **Componentes principais**:
  - `AppDbContext` - Contexto do Entity Framework Core
  - `CampaignRepository` - Implementação do repositório
  - `ApplicationUser` - Usuário da aplicação com Identity
  - `TokenService` - Geração e validação de JWT
  - `RoleSeeder` - Inicialização de papéis (roles)
  - Migrations do banco de dados

#### **4. FiapDonateCampaign.API**
- **Responsabilidade**: Expõe os endpoints HTTP e configura a aplicação
- **Componentes principais**:
  - `CampaignController` - Endpoints de campanhas
  - `AuthController` - Endpoints de autenticação
  - `ExceptionMiddleware` - Middleware para tratamento global de erros
  - `Program.cs` - Configuração da DI (Dependency Injection) e pipeline

#### **5. FiapDonateCampaign.UnitTests**
- Testes unitários das camadas de aplicação e domínio

#### **6. FiapDonateCampaign.IntegrationTests**
- Testes de integração da API completa

---

## 💻 Stack Tecnológico

| Componente | Versão | Descrição |
|-----------|--------|-----------|
| **.NET** | 8.0 | Runtime e framework principal |
| **ASP.NET Core** | 8.0 | Framework web |
| **Entity Framework Core** | 8.0 | ORM para acesso a dados |
| **SQL Server** | - | Banco de dados |
| **JWT Bearer** | Latest | Autenticação stateless |
| **FluentValidation** | 12.1.1 | Validação de dados |
| **Swagger/OpenAPI** | Latest | Documentação de API |
| **Identity** | 8.0 | Gerenciamento de usuários e roles |

---

## 📁 Estrutura do Projeto

```
FiapDonateCampaign/
├── FiapDonateCampaign.Domain/                    # Camada de Domínio
│   ├── Entities/
│   │   └── Campaign.cs                           # Entidade de Campanha
│   ├── Enums/
│   │   └── StatusCampaign.cs                     # Estados da campanha
│   ├── Exceptions/
│   │   └── DomainException.cs                    # Exceções de negócio
│   └── Interfaces/
│       └── ICampaignRepository.cs                # Contrato do repositório
│
├── FiapDonateCampaign.Application/               # Camada de Aplicação
│   ├── DTOs/
│   │   ├── CampaignRequestDto.cs                 # DTO de entrada
│   │   └── CampaignResponseDto.cs                # DTO de saída
│   ├── Interfaces/
│   │   └── ICampaignService.cs                   # Contrato do serviço
│   ├── Services/
│   │   └── CampaignService.cs                    # Implementação do serviço
│   └── Validators/
│       └── CampaignRequestValidator.cs           # Validação de DTO
│
├── FiapDonateCampaign.Infrastructure/            # Camada de Infra
│   ├── Auth/
│   │   └── TokenService.cs                       # Serviço de JWT
│   ├── Data/
│   │   └── AppDbContext.cs                       # Contexto EF Core
│   ├── Identity/
│   │   ├── ApplicationUser.cs                    # Usuário da app
│   │   └── RoleSeeder.cs                         # Inicialização de roles
│   ├── Repositories/
│   │   └── CampaignRepository.cs                 # Repositório
│   ├── Migrations/                               # Migrations do banco
│   └── DependencyInjection.cs                    # Registro de serviços
│
├── FiapDonateCampaign.API/                       # Camada de Apresentação
│   ├── Controllers/
│   │   ├── CampaignController.cs                 # Endpoints de campanhas
│   │   └── AuthController.cs                     # Endpoints de auth
│   ├── Middlewares/
│   │   └── ExceptionMiddleware.cs                # Tratamento de erros
│   ├── Program.cs                                # Configuração principal
│   ├── appsettings.json                          # Configurações [dados sensíveis]
│   ├── appsettings.Development.json              # Config. de desenvolvimento
│   └── FiapDonateCampaign.API.http               # Requests de teste
│
├── FiapDonateCampaign.UnitTests/                 # Testes Unitários
├── FiapDonateCampaign.IntegrationTests/          # Testes de Integração
│
├── FiapDonateCampaign.slnx                       # Arquivo da solução
└── README.md                                      # Este arquivo
```

---

## ⚙️ Requisitos

- **.NET 8 SDK** ou superior
- **SQL Server** 2019+ (ou SQL Server Express)
- **Visual Studio 2026+** (Community/Professional/Enterprise)
- **PowerShell** ou linha de comando

---

## 🚀 Instalação & Configuração

### 1. Clonar o Repositório

```bash
git clone https://github.com/davidjmarinho/FiapDonateCampaign.git
cd FiapDonateCampaign
```

### 2. Configurar o Banco de Dados

Edite o arquivo `appsettings.json` na pasta `FiapDonateCampaign.API`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR_SERVER;Database=FiapDonateCampaignDb;Trusted_Connection=true;"
  },
  "Jwt": {
	"Key": "sua-chave-secreta-super-longa-aqui", // Mínimo 32 caracteres
	"Issuer": "FiapDonate",
	"Audience": "FiapDonateUsers"
  }
}
```

### 3. Executar Migrations

```bash
cd FiapDonateCampaign.API
dotnet ef database update
```

Isso irá:
- Criar o banco de dados
- Criar as tabelas necessárias
- Seedar os roles padrão (Admin, User, etc.)

### 4. Restaurar Dependências

```bash
dotnet restore
```

---

## ▶️ Como Executar

### Modo de Desenvolvimento

```bash
cd FiapDonateCampaign.API
dotnet run
```

A API estará disponível em:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger/index.html`

### Modo de Produção

```bash
dotnet build -c Release
dotnet run --configuration Release
```

---

## 📡 API Endpoints

### Autenticação

#### Registrar Usuário
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SenhaForte123!"
}
```

#### Fazer Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SenhaForte123!"
}
```

**Resposta**:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

### Campanhas

#### Listar Campanhas
```http
GET /api/campaigns
Authorization: Bearer {token}
```

#### Obter Campanha por ID
```http
GET /api/campaigns/{id}
Authorization: Bearer {token}
```

#### Criar Campanha
```http
POST /api/campaigns
Authorization: Bearer {token}
Content-Type: application/json

{
  "titulo": "Campanha de Verão",
  "descricao": "Arrecadação para projeto social",
  "dataInicio": "2025-01-01T00:00:00Z",
  "dataFim": "2025-03-01T00:00:00Z",
  "metaFinanceira": 10000.00
}
```

#### Atualizar Campanha
```http
PUT /api/campaigns/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "titulo": "Novo Título",
  "descricao": "Nova Descrição",
  "dataInicio": "2025-01-01T00:00:00Z",
  "dataFim": "2025-03-01T00:00:00Z",
  "metaFinanceira": 15000.00
}
```

#### Deletar Campanha
```http
DELETE /api/campaigns/{id}
Authorization: Bearer {token}
```

---

## 🧪 Testes

### Executar Testes Unitários

```bash
dotnet test FiapDonateCampaign.UnitTests
```

### Executar Testes de Integração

```bash
dotnet test FiapDonateCampaign.IntegrationTests
```

### Executar Todos os Testes

```bash
dotnet test
```

---

## 🔐 Segurança

- **Autenticação**: JWT Bearer Token com validação
- **Autorização**: Baseada em roles (Admin, User)
- **HTTPS**: Obrigatório em produção
- **CORS**: Configurável em `appsettings.json`
- **Validação**: Validação de entrada com FluentValidation
- **Tratamento de Erros**: Middleware de exceção global

---

## 📝 Padrões de Código

### Domain-Driven Design (DDD)

- **Entities**: Classes que representam conceitos do negócio com lógica de validação integrada
- **Value Objects**: Objetos imutáveis que representam valores
- **Repositories**: Padrão para acesso a dados, implementados na camada de Infrastructure
- **Services**: Orquestração de lógica de aplicação entre camadas
- **Domain Exceptions**: Exceções específicas do domínio para regras de negócio violadas

### SOLID Principles

- **S**ingle Responsibility: Cada classe tem uma única responsabilidade
- **O**pen/Closed: Aberto para extensão, fechado para modificação
- **L**iskov Substitution: Subtipagem mantém contrato
- **I**nterface Segregation: Interfaces específicas, não genéricas
- **D**ependency Inversion: Depender de abstrações, não de implementações

---

## ⚙️ Regras Aplicadas ao Projeto

### 🔐 Arquitetura de Dependências

1. **Inversão de Controle (IoC) via Dependency Injection**
   - Todas as dependências são registradas no `DependencyInjection.cs` (Infrastructure) e `Program.cs` (API)
   - Use `AddScoped` para serviços e repositórios (ciclo de vida por requisição)
   - Injetar sempre via construtor, não via properties

2. **Dois DbContexts para Separação de Responsabilidades**
   - `AppDbContext`: Gerencia entidades da API (Campaign, Donation) e **propriedade das migrations**
   - `IdentityStoreDbContext`: Apenas leitura do Identity (usuários e roles) - **sem migrations**
   - Ambos apontam para o mesmo banco de dados físico
   - Este padrão previne conflitos de schema entre Identity e entidades de negócio

```csharp
// Infrastructure/DependencyInjection.cs
services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
services.AddDbContext<IdentityStoreDbContext>(options => options.UseSqlServer(connectionString));
```

### 🎯 Regras de Domínio (Domain-Driven Design)

1. **Lógica de Negócio na Entidade**
   - Validações críticas devem estar no construtor da entidade ou em métodos de domínio
   - Use `DomainException` para violações de regra de negócio
   - Exemplo: `Campaign.ValidarPodeReceberDoacao()` valida estado antes de aceitar doação

```csharp
// Domain/Entities/Campaign.cs
public void ValidarPodeReceberDoacao()
{
    if (Status == StatusCampaign.Concluida || Status == StatusCampaign.Cancelada)
        throw new DomainException("Não é possível doar para uma campanha encerrada ou cancelada.");
}
```

2. **Propriedades Privadas com Inicialização Controlada**
   - Apenas getters públicos para propriedades de entidade
   - Setters privados para controlar mudanças de estado
   - Sempre que necessário alteração de estado, use métodos específicos (ex: `AdicionarValorArrecadado()`)

```csharp
public class Campaign
{
    public string Titulo { get; private set; }  // ✅ Protegido
    public decimal ValorArrecadado { get; private set; }  // ✅ Nunca muda via setter

    // ✅ Método específico para adicionar valor
    public void AdicionarValorArrecadado(decimal valor) { /* ... */ }
}
```

3. **Construtor Sem Parâmetros Protegido**
   - Entity Framework Core requer um construtor sem parâmetros
   - Marcar como `protected` (nunca `public`) para prevenir instanciação direta

```csharp
protected Campaign() { } // EF Core requires a parameterless constructor
```

### 📦 Regras de Repositórios

1. **Interfaces em Domain, Implementações em Infrastructure**
   - Defina `ICampaignRepository` em `Domain.Interfaces`
   - Implemente em `Infrastructure.Repositories.CampaignRepository`
   - Controllers e Services dependem da interface, não da implementação

2. **Métodos Assíncronos Obrigatórios**
   - Todo acesso a banco de dados deve ser async (I/O não-bloqueante)
   - Nomenclatura: `GetAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `GetByIdAsync`, `ListarTodosAsync`

```csharp
public interface ICampaignRepository
{
    Task<Campaign?> ObterPorIdAsync(Guid id);
    Task<List<Campaign>> ListarTodosAsync();
    Task AdicionarAsync(Campaign campaign);
}
```

### 🛡️ Validação de Dados

1. **FluentValidation para DTOs**
   - Crie um `*Validator` para cada DTO de entrada
   - Registre automaticamente via `AddValidatorsFromAssemblyContaining<T>()`
   - Validação ocorre automaticamente no pipeline (via Action Filter)

```csharp
public class CampaignRequestValidator : AbstractValidator<CampaignRequestDto>
{
    public CampaignRequestValidator()
    {
        RuleFor(c => c.Titulo).NotEmpty().MaximumLength(100);
        RuleFor(c => c.MetaFinanceira).GreaterThan(0);
    }
}
```

2. **Validações de Regra de Negócio no Domínio**
   - Validações que envolvem lógica complexa devem estar na entidade
   - DTOs são validados por estrutura e tipos; entidades validam regras

### 🔒 Segurança

1. **JWT Bearer Token com Validação Completa**
   - Validar: Issuer, Audience, Lifetime, Signing Key
   - Configuração no `Program.cs` com chave do `appsettings.json`

2. **Identity & Roles**
   - Use `ApplicationUser` (estende `IdentityUser`) para usuários da app
   - Roles pré-definidos seeded no `RoleSeeder`
   - Exemplo: Admin, User

3. **HTTPS Obrigatório em Produção**
   - Redirecionar HTTP → HTTPS
   - Usar `UseHttpsRedirection()` no pipeline

### 🎛️ Exceções

1. **DomainException para Negócio**
   - Use quando regra de negócio for violada
   - Exemplo: Tentativa de doar para campanha cancelada

2. **Middleware Global de Exceção**
   - `ExceptionMiddleware` centraliza tratamento
   - Mapeia exceções para respostas HTTP apropriadas
   - Registre via `app.UseMiddleware<ExceptionMiddleware>()`

```csharp
try
{
    var campanha = await _campanhaRepository.ObterPorIdAsync(id)
        ?? throw new DomainException("Campanha não encontrada.");
    campanha.ValidarPodeReceberDoacao(); // Pode lançar DomainException
}
catch (DomainException ex)
{
    // Middleware converte para 400 Bad Request
}
```

### 📡 Controllers

1. **Controllers devem ser Thin (Finos)**
   - Apenas recebem requisição, chamam serviço, retornam resposta
   - Nenhuma lógica de negócio diretamente no controller

2. **Injetar Interface do Serviço**
   - Dependência do serviço apenas, não da implementação

```csharp
[ApiController]
[Route("api/[controller]")]
public class CampaignController : ControllerBase
{
    private readonly ICampaignService _service;

    public CampaignController(ICampaignService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CampaignRequestDto dto)
    {
        var resultado = await _service.CriarAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = resultado.Id }, resultado);
    }
}
```

### 🧪 Testes

1. **Unit Tests**: Testes de lógica de domínio e serviços isolados
2. **Integration Tests**: Testes da API completa com banco in-memory ou real

### 📝 DTOs

1. **DTOs Separados por Operação**
   - `CampaignRequestDto`: Entrada de dados (POST/PUT)
   - `CampaignResponseDto`: Saída de dados (GET)
   - Previne exposição excessiva de informações

2. **Mapeamento Manual ou AutoMapper**
   - Atualmente sem AutoMapper; considere adicionar para escalar
   - Ex: `new CampaignResponseDto { Id = campaign.Id, ... }`

---

## 📋 Convenções de Nomenclatura

| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| **Namespaces** | PascalCase com pontos | `FiapDonateCampaign.Domain.Entities` |
| **Classes** | PascalCase | `Campaign`, `CampaignService`, `CampaignValidator` |
| **Interfaces** | I + PascalCase | `ICampaignRepository`, `ICampaignService` |
| **Métodos** | PascalCase + Async suffix | `GetByIdAsync`, `CreateAsync` |
| **Propriedades** | PascalCase | `Id`, `Titulo`, `Status` |
| **Campos privados** | _camelCase | `_repository`, `_logger` |
| **Enums** | PascalCase | `StatusCampaign`, `UserRole` |
| **Constants** | UPPER_SNAKE_CASE | `MAX_CAMPAIGN_TITLE_LENGTH` |

---

## 🔄 Fluxo de Requisição Típico

```
1. [HTTP Request] → CampaignController
2. CampaignController → FluentValidator (automático)
3. CampaignController → ICampaignService
4. CampaignService → ICampaignRepository
5. CampaignRepository → AppDbContext → SQL Server
6. [Domain Validation] → Campaign Entity (lança DomainException se necessário)
7. ExceptionMiddleware → [HTTP Response] ✅ ou ❌
```

---

## 🔧 Variáveis de Ambiente e Configurações

Configure em `appsettings.json` (raiz da pasta FiapDonateCampaign.API):

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=YOUR_SERVER;Database=FiapDonateCampaignDb;Trusted_Connection=true;"
  },
  "Jwt": {
	"Key": "sua-chave-secreta-super-longa-com-minimo-32-caracteres",
	"Issuer": "FiapDonate",
	"Audience": "FiapDonateUsers"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.EntityFrameworkCore": "Warning"
	}
  }
}
```

### 📌 Regras de Configuração

1. **Connection String**
   - Deve apontar para um SQL Server válido
   - Usar `Trusted_Connection=true` para Windows Authentication (desenvolvimento)
   - Usar user/password em produção com variáveis de ambiente

2. **JWT Key**
   - **Mínimo de 32 caracteres** (256 bits para HS256)
   - **NUNCA commitar** chaves reais no repositório
   - Use environment variables ou Azure Key Vault em produção

3. **Environments**
   - `Development`: `appsettings.Development.json` (valores de teste)
   - `Production`: Use variáveis de ambiente do servidor

---

## ✅ Boas Práticas de Desenvolvimento

1. **Sempre use async/await** em operações de I/O
2. **Valide entrada de dados** com FluentValidation
3. **Lance DomainException** para regras de negócio violadas
4. **Não coloque lógica no controller** - use services
5. **Interfaces para toda abstração** que será injetada
6. **Testes para lógica crítica** de domínio
7. **Commit mensagens em inglês** ou português claro
8. **Code review antes de merge** para main/production

### 🚫 Anti-padrões a Evitar

```csharp
// ❌ ERRADO: Serviço instanciado
var service = new CampaignService(repo); 

// ✅ CORRETO: Injetado via DI
public CampaignController(ICampaignService service) => _service = service;

// ❌ ERRADO: Setter público em entidade
public class Campaign { public string Titulo { get; set; } }

// ✅ CORRETO: Setter privado
public class Campaign { public string Titulo { get; private set; } }

// ❌ ERRADO: Lógica de negócio no controller
[HttpPost]
public async Task<IActionResult> Create(CampaignRequestDto dto)
{
	if (dto.MetaFinanceira <= 0) return BadRequest(); // LÓGICA NO CONTROLLER!
}

// ✅ CORRETO: Validação no DTO + lógica na entidade
[HttpPost]
public async Task<IActionResult> Create(CampaignRequestDto dto)
{
	var resultado = await _service.CriarAsync(dto); // Service orquestra
}
```

---

## 📚 Referências

- [Microsoft .NET 8 Documentation](https://learn.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)
- [JWT Guide](https://jwt.io/introduction)
- [FluentValidation](https://docs.fluentvalidation.net/)

---

## 👥 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 📄 Licença

Este projeto está sob licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

---

## 📧 Contato

- **GitHub**: [davidjmarinho/FiapDonateCampaign](https://github.com/davidjmarinho/FiapDonateCampaign)
- **Branch de Desenvolvimento**: `Lucas-Development`

---

**Última Atualização**: Janeiro 2025  
**Versão**: 1.0.1  
**Status**: Em Desenvolvimento  
**Nova Seção**: ✅ Regras Aplicadas ao Projeto
