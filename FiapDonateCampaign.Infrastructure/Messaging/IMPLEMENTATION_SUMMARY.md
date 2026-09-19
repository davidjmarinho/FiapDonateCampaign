# ✅ Integração RabbitMQ - Resumo da Implementação

## 🎯 O que foi feito

Adicionamos o contrato `DoacaoRecebidaEvent` e a infraestrutura básica para publicação de eventos via RabbitMQ no seu projeto FiapDonateCampaign.

---

## 📂 Arquivos Criados

### 1️⃣ Contrato do Evento (Em Application - Respeitando Clean Architecture)
```
📄 FiapDonateCampaign.Application/Event/DoacaoRecebidaEvent.cs
```
**O que é:** Record imutável que define a estrutura do evento de doação
```csharp
public record DoacaoRecebidaEvent(
	Guid DoacaoId,                    // ID da doação
	Guid IdCampanha,                  // ID da campanha
	decimal ValorDoacao,              // Valor doado
	DateTimeOffset DataHoraRecebida);  // Data/hora com timezone (UTC)
```

### 2️⃣ Publicador RabbitMQ (Em Infrastructure - Para Implementação Real)
```
📄 FiapDonateCampaign.Infrastructure/Messaging/RabbitMqEventPublisher.cs
```
**O que é:** Classe scaffold com TODO tags para implementação futura do RabbitMQ
- Contém documentação de como implementar
- Segue a mesma interface `IEventPublisher`
- Pronto para receber as dependências do RabbitMQ

### 3️⃣ Guia de Integração Completo
```
📄 FiapDonateCampaign.Infrastructure/Messaging/RABBITMQ_INTEGRATION_GUIDE.md
```
**O que é:** Documentação detalhada com:
- Estrutura e fluxo de dados
- Configuração em appsettings.json
- Exemplos práticos
- Diagrama de Produtor/Subscriber
- Boas práticas de segurança
- Setup com Docker

---

## 🔄 Como Funciona (Fluxo Completo)

```
1. Cliente HTTP POST /api/donations
					↓
2. DonationController recebe a requisição
					↓
3. DonationService.RegistrarIntencaoAsync()
   ├─ Valida campanha
   ├─ Cria entidade Donation
   ├─ Salva no banco de dados
   ├─ ✅ Cria DoacaoRecebidaEvent
   │  └─ DoacaoId: ID da doação
   │  └─ IdCampanha: ID da campanha
   │  └─ ValorDoacao: Valor doado
   │  └─ DataHoraRecebida: Data/hora com timezone
   │
   ├─ Publica via IEventPublisher.PublishAsync(evento, "campanha.doacao.recebida")
   │  └─ Atualmente: LogEventPublisher (apenas loga)
   │  └─ Futuramente: RabbitMqEventPublisher (envia para RabbitMQ)
   │
   └─ Retorna IntentionDonateResponseDto
					↓
4. RabbitMQ entrega para subscribers interessados
```

---

## 💡 Por Que Usar DateTimeOffset?

O campo `DataHoraRecebida` usa `DateTimeOffset` em vez de `DateTime` porque:

- ✅ **Timezone**: Inclui informação de zona horária
- ✅ **Distribuído**: Essencial para eventos em diferentes regiões
- ✅ **Serialização JSON**: Preserva offset (ex: `2025-01-15T10:30:00+00:00`)
- ✅ **Comparação**: Facilita comparações de eventos globais

```csharp
// ✅ Recomendado
DateTimeOffset.UtcNow  // 2025-01-15T10:30:00+00:00

// ❌ Evitar para eventos distribuídos
DateTime.UtcNow  // Perde informação de timezone
```

---

## 🏗️ Arquitetura (Clean Architecture)

```
┌─────────────────────────────────────────────────────────┐
│ FiapDonateCampaign.Application (Camada de Aplicação)   │
│ ─────────────────────────────────────────────────────   │
│                                                         │
│  Event/                                                │
│  └─ DoacaoRecebidaEvent.cs  ← Contrato do evento      │
│                                                         │
│  Services/                                             │
│  └─ DonationService.cs      ← Pública o evento       │
│                                                         │
│  Interface/                                            │
│  └─ IEventPublisher.cs      ← Define o contrato     │
└─────────────────────────────────────────────────────────┘
			  ▲                          ▲
			  │ implementa               │ usa
			  │                          │
┌─────────────┴──────────────────────────┴────────────────┐
│ FiapDonateCampaign.Infrastructure (Camada Infra)      │
│ ─────────────────────────────────────────────────────   │
│                                                         │
│  Messaging/                                            │
│  ├─ LogEventPublisher.cs       ← Teste (apenas logs)  │
│  ├─ RabbitMqEventPublisher.cs  ← Produção (RabbitMQ) │
│  └─ RabbitMqSettings.cs        ← Configuração        │
└─────────────────────────────────────────────────────────┘
```

### ✅ Por que Application não vê Infrastructure?
- Garante que camadas superiores não dependem das inferiores
- Permite trocar LogEventPublisher por RabbitMqEventPublisher sem alterar Application
- Facilita testes (injetar mock)

---

## 🚀 Próximos Passos (Implementação Real)

### Passo 1: Instalar NuGet Package
```bash
cd FiapDonateCampaign.Infrastructure
dotnet add package RabbitMQ.Client
```

### Passo 2: Configurar appsettings.json
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
	"AutomaticRecovery": true
  }
}
```

### Passo 3: Implementar RabbitMqEventPublisher
Complete os TODOs no arquivo `RabbitMqEventPublisher.cs`:
- [ ] Criar ConnectionFactory
- [ ] Estabelecer conexão
- [ ] Publicar mensagens
- [ ] Implementar retry policy

### Passo 4: Trocar a Implementação
Em `DependencyInjection.cs`:
```csharp
// De:
services.AddScoped<IEventPublisher, LogEventPublisher>();

// Para:
services.AddScoped<IEventPublisher, RabbitMqEventPublisher>();
```

### Passo 5: Criar Consumer (Microserviço Externo)
Qualquer serviço pode se inscrever no evento:
```csharp
// Em outro projeto (.NET, Node.js, Python, etc.)
Subscribe("campanha.doacao.recebida")
  .OnMessage(async (envelope) => {
	var evento = JsonSerializer.Deserialize<DoacaoRecebidaEvent>(envelope.Body);
	// Processar evento (enviar email, atualizar cache, etc.)
  });
```

---

## 📊 Exemplo de JSON Serializado

Quando o evento é publicado para RabbitMQ, fica assim:

```json
{
  "doacaoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "idCampanha": "8f0ae4d4-c001-4e6b-9e1f-d1c5c6c7c8c9",
  "valorDoacao": 250.50,
  "dataHoraRecebida": "2025-01-15T10:30:45.1234567+00:00"
}
```

---

## 🧪 Testar Localmente com Docker

```bash
# Iniciar RabbitMQ
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3.12-management

# Acessar Management UI
# Abra: http://localhost:15672
# Login: guest / guest
```

---

## 🔐 Segurança em Produção

1. **Use variáveis de ambiente** para credenciais
2. **Configure SSL/TLS** para RabbitMQ
3. **Isolamento**: Use vhosts separados (dev, staging, prod)
4. **Validação**: Valide eventos no consumer

---

## ✨ Resumo Visual

```
┌─────────────────────────────────┐
│   DoacaoRecebidaEvent           │
│  (Novo Contrato)                │
│                                 │
│  Guid DoacaoId                  │
│  Guid IdCampanha                │
│  decimal ValorDoacao            │
│  DateTimeOffset DataHoraRecebida│
└────────────────┬────────────────┘
				 │
				 ├─ Definido em:
				 │  Application/Event/
				 │
				 ├─ Usado por:
				 │  DonationService
				 │
				 ├─ Publicado para:
				 │  RabbitMQ
				 │  routing_key: campanha.doacao.recebida
				 │
				 └─ Consumido por:
					Qualquer microserviço interessado
```

---

## 📚 Documentação Completa

Para detalhes sobre configuração, exemplos e boas práticas, consulte:
```
📄 FiapDonateCampaign.Infrastructure/Messaging/RABBITMQ_INTEGRATION_GUIDE.md
```

---

## ✅ Status Atual

- ✅ Contrato `DoacaoRecebidaEvent` criado
- ✅ `DonationService` integrado ao novo contrato
- ✅ Scaffold `RabbitMqEventPublisher` criado
- ✅ Documentação completa fornecida
- ✅ Build compila sem erros
- ⏳ Implementação real do RabbitMQ (próximos passos)

---

## 🎓 FAQ Rápido

**P: Onde o evento é definido?**
- R: `FiapDonateCampaign.Application/Event/DoacaoRecebidaEvent.cs`

**P: Quando é publicado?**
- R: Imediatamente após salvar a Donation no banco

**P: Para onde vai?**
- R: RabbitMQ fila com routing key "campanha.doacao.recebida"

**P: Como um serviço consome?**
- R: Se inscrevendo no padrão de routing key (ex: "campanha.*" ou "campanha.doacao.*")

**P: O que acontece se RabbitMQ não estiver rodando?**
- R: Atual (LogEventPublisher): apenas loga | Futuro (RabbitMqEventPublisher): implementar retry/circuit breaker

---

**Criado em:** Janeiro 2025  
**Status:** ✅ Pronto para produção (com implementação RabbitMQ real)
