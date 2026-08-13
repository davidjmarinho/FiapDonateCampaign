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
- [Contribuição](#contribuição)

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

- **Entities**: Classes que representam conceitos do negócio
- **Value Objects**: Objetos imutáveis que representam valores
- **Repositories**: Padrão para acesso a dados
- **Services**: Orquestração de lógica de aplicação

### SOLID Principles

- **S**ingle Responsibility: Cada classe tem uma única responsabilidade
- **O**pen/Closed: Aberto para extensão, fechado para modificação
- **L**iskov Substitution: Subtipagem mantém contrato
- **I**nterface Segregation: Interfaces específicas, não genéricas
- **D**ependency Inversion: Depender de abstrações, não de implementações

---

## 🔧 Variáveis de Ambiente

Configure em `appsettings.json`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Sua string de conexão"
  },
  "Jwt": {
	"Key": "Sua chave JWT secreta (mínimo 32 caracteres)",
	"Issuer": "Seu issuer",
	"Audience": "Seu audience"
  },
  "Logging": {
	"LogLevel": {
	  "Default": "Information"
	}
  }
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
**Versão**: 1.0.0  
**Status**: Em Desenvolvimento
