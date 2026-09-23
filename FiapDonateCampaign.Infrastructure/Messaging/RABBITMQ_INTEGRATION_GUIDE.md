# 📦 Guia de Integração RabbitMQ - DoacaoRecebidaEvent

## 🎯 Visão Geral

O contrato `DoacaoRecebidaEvent` foi criado para permitir a comunicação assíncrona entre a API de campanhas e possíveis consumidores de eventos (microserviços, workers, etc.) através do RabbitMQ.

### Estrutura do Evento

```csharp
public record DoacaoRecebidaEvent(
	Guid DoacaoId,              // ID único da doação
	Guid IdCampanha,            // ID da campanha que recebeu a doação
	decimal ValorDoacao,        // Valor doado
	DateTimeOffset DataHoraRecebida);  // Data/hora com timezone
```

---

## 📂 Onde os Arquivos Estão

### Contrato (Estrutura do Evento)
```
FiapDonateCampaign.Infrastructure/
└── Messaging/
	└── Events/
		└── DoacaoRecebidaEvent.cs  ← CONTRATO DO EVENTO
```

### Publicador (Responsável pela Entrega)
```
FiapDonateCampaign.Infrastructure/
└── Messaging/
	├── LogEventPublisher.cs        ← Implementação de teste (logs)
	└── RabbitMqEventPublisher.cs   ← Implementação RabbitMQ (TODO)
```

### Uso (Onde é Publicado)
```
FiapDonateCampaign.Application/
└── Services/
	└── DonationService.cs          ← Publica o evento após salvar doação
```

### Interface (Contrato da Implementação)
```
FiapDonateCampaign.Application/
└── Interface/
	└── IEventPublisher.cs          ← Interface genérica
```

---

## 🔄 Fluxo de uma Doação

```
1. Cliente HTTP chama POST /api/donations
					↓
2. DonationController.Create([FromBody] IntentionDonateRequestDto dto)
					↓
3. DonationService.RegistrarIntencaoAsync(doadorId, dto)
	   ├── Valida campanha
	   ├── Cria entidade Donation
	   ├── Salva no banco (PostgreSQL/SQL Server)
	   ├── ✅ Cria DoacaoRecebidaEvent com dados da doação
	   ├── Publica no IEventPublisher (LogEventPublisher ou RabbitMqEventPublisher)
	   │   └── Serializa para JSON
	   │   └── Envia para RabbitMQ com routing key: "campanha.doacao.recebida"
	   └── Retorna IntentionDonateResponseDto
					↓
4. RabbitMQ entrega mensagem para subscribers/consumers
					↓
5. Consumers processam o evento (ex: atualizar cache, enviar email, etc.)
```

---

## 🚀 Como Usar - Exemplo Prático

### Antes (Sem Evento)
```csharp
public async Task RegistrarIntencaoAsync(string doadorId, IntentionDonateRequestDto dto)
{
	var doacao = new Donation(dto.IdCampanha, doadorId, dto.ValorDoacao);
	await _doacaoRepository.AdicionarAsync(doacao);

	// Pronto! Mas quem quer saber que teve uma doação?
}
```

### Depois (Com Evento RabbitMQ)
```csharp
public async Task RegistrarIntencaoAsync(string doadorId, IntentionDonateRequestDto dto)
{
	var doacao = new Donation(dto.IdCampanha, doadorId, dto.ValorDoacao);
	await _doacaoRepository.AdicionarAsync(doacao);

	// ✅ Publicar evento para fila "campanha.doacao.recebida"
	var evento = new DoacaoRecebidaEvent(
		DoacaoId: doacao.Id,
		IdCampanha: doacao.CampanhaId,
		ValorDoacao: doacao.ValorDoacao,
		DataHoraRecebida: doacao.DataDoacao);

	await _eventPublisher.PublishAsync(evento, "campanha.doacao.recebida");

	// Agora qualquer service que esteja ouvindo saberá do evento!
}
```

---

## ⚙️ Configuração Necessária

### 1. Instalar NuGet Package

```bash
cd FiapDonateCampaign.Infrastructure
dotnet add package RabbitMQ.Client --version 6.8.1
```

### 2. Adicionar Configuração em `appsettings.json`

```json
{
  "RabbitMQ": {
	"HostName": "localhost",
	"Port": 5672,
	"UserName": "guest",
	"Password": "guest",
	"ExchangeName": "donations.exchange",
	"QueueName": "donations.queue",
	"RoutingKey": "campanha.doacao.recebida",
	"AutomaticRecovery": true,
	"ConnectionRetryCount": 3,
	"ConnectionRetryDelayMs": 5000
  }
}
```

### 3. Adicionar Configuração em `appsettings.Production.json`

```json
{
  "RabbitMQ": {
	"HostName": "${RABBITMQ_HOST}",
	"Port": 5672,
	"UserName": "${RABBITMQ_USER}",
	"Password": "${RABBITMQ_PASSWORD}",
	"ExchangeName": "donations.exchange",
	"QueueName": "donations.queue",
	"RoutingKey": "campanha.doacao.recebida",
	"AutomaticRecovery": true,
	"ConnectionRetryCount": 5,
	"ConnectionRetryDelayMs": 10000
  }
}
```

### 4. Criar Classe de Configuração

Arquivo: `FiapDonateCampaign.Infrastructure/Messaging/RabbitMqSettings.cs`

```csharp
namespace FiapDonateCampaign.Infrastructure.Messaging;

public class RabbitMqSettings
{
	public string HostName { get; set; } = "localhost";
	public int Port { get; set; } = 5672;
	public string UserName { get; set; } = "guest";
	public string Password { get; set; } = "guest";
	public string ExchangeName { get; set; } = "donations.exchange";
	public string QueueName { get; set; } = "donations.queue";
	public string RoutingKey { get; set; } = "campanha.doacao.recebida";
	public bool AutomaticRecovery { get; set; } = true;
	public int ConnectionRetryCount { get; set; } = 3;
	public int ConnectionRetryDelayMs { get; set; } = 5000;
}
```

### 5. Atualizar `DependencyInjection.cs`

```csharp
// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
	this IServiceCollection services, 
	IConfiguration configuration)
{
	// ... código existente ...

	// Registrar RabbitMQ
	services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMQ"));

	// ESCOLHA UMA IMPLEMENTAÇÃO:
	// Para desenvolvimento (apenas logs):
	services.AddScoped<IEventPublisher, LogEventPublisher>();

	// Para produção (RabbitMQ real):
	// services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();

	return services;
}
```

---

## 🔗 Routing Keys (Topic-Based)

O RabbitMQ usa routing keys para direcionar mensagens. Recomenda-se seguir um padrão hierárquico:

```
campanha.doacao.recebida          ← Novo evento de doação
campanha.atualizada               ← Campanha foi modificada
campanha.finalizada               ← Campanha terminou
notifications.email.enviar        ← Email para enviar
report.financeiro.update          ← Atualizar relatório financeiro
```

Qualquer serviço pode se inscrever no padrão que desejar:
- `campanha.*` - Ouça todos os eventos de campanha
- `campanha.doacao.*` - Ouça todos os eventos de doação
- `*.email.*` - Ouça todos os eventos de email

---

## 📊 Diagrama de Publicador/Subscriber

```
┌──────────────────────────────────────────────────────┐
│  FiapDonateCampaign.API (Produtor)                  │
│  ────────────────────────────────────────────────    │
│  POST /api/donations                                 │
│    └─> DonationService                               │
│         └─> Publica DoacaoRecebidaEvent             │
│             └─> IEventPublisher.PublishAsync()      │
└──────────────────────────────────────────────────────┘
					   │
					   │ (via RabbitMQ)
					   │ routing_key: "campanha.doacao.recebida"
					   ├─────────────────────────────────────┐
					   │                                     │
		┌──────────────▼──────────┐        ┌───────────────▼────────┐
		│   Microservice A        │        │  Microservice B        │
		│ (Email Notifications)   │        │  (Analytics Service)   │
		│ ─────────────────────   │        │  ────────────────────  │
		│ Subscribe to:           │        │ Subscribe to:          │
		│ campanha.doacao.*       │        │ campanha.doacao.recebida
		│                         │        │                        │
		│ Process: Enviar email   │        │ Process: Log stats     │
		│ de confirmação          │        │ de campanha            │
		└─────────────────────────┘        └────────────────────────┘
```

---

## ✅ Implementação Futura (TODO)

O arquivo `RabbitMqEventPublisher.cs` contém TODOs para as próximas etapas:

1. **Implementar ConnectionFactory**
   - Criar conexão com RabbitMQ
   - Configurar retry policy
   - Implementar heartbeat

2. **Declarar Exchange e Filas**
   - Exchange type: `topic` (para suportar wildcards)
   - Fila: `donations.queue`
   - Vinculação com routing keys

3. **Publicar Mensagens**
   - Serializar evento
   - Adicionar headers (timestamp, type, version)
   - Publicar com confirmação

4. **Tratamento de Erros**
   - Circuit breaker
   - Dead Letter Queue (DLQ)
   - Retry exponencial

5. **Monitoramento**
   - Logs estruturados
   - Métricas de publicação
   - Rastreamento de eventos

---

## 🧪 Teste Local com RabbitMQ Docker

```bash
# Iniciar RabbitMQ localmente
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  -e RABBITMQ_DEFAULT_USER=guest \
  -e RABBITMQ_DEFAULT_PASS=guest \
  rabbitmq:3.12-management

# Acessar Management UI
# http://localhost:15672
# Login: guest / guest
```

---

## 🔐 Segurança

- **Autenticação**: Use variáveis de ambiente para credenciais
- **Encriptação**: Configure SSL/TLS em produção
- **Isolamento**: Use vhosts separados por ambiente
- **Validação**: Valide a estrutura do evento no consumer

---

## 📚 Referências

- [RabbitMQ Official Documentation](https://www.rabbitmq.com/documentation.html)
- [RabbitMQ.Client NuGet](https://www.nuget.org/packages/RabbitMQ.Client/)
- [Topic Exchange Pattern](https://www.rabbitmq.com/tutorials/tutorial-five-dotnet.html)
- [Async/Await Best Practices](https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/march/async-await-best-practices-in-asynchronous-programming)

---

## 🎓 Perguntas Frequentes

**P: Por que usar DateTimeOffset em vez de DateTime?**
- R: `DateTimeOffset` inclui informação de timezone, essencial para eventos distribuídos.

**P: O que acontece se RabbitMQ cair?**
- R: A mensagem será enfileirada localmente e retentada. Implementar Circuit Breaker é recomendado.

**P: Posso usar o mesmo evento em múltiplos serviços?**
- R: Sim! Este é o objetivo de separar o contrato em uma camada específica.

**P: Como versionar eventos?**
- R: Adicione um campo `Version` no record ou crie `DoacaoRecebidaEventV2`.
