# 📝 DonationService - Antes e Depois

## ❌ ANTES (Usando DonationReceivedEvent genérico)

```csharp
using FiapDonateCampaign.Application.DTOs;
using FiapDonateCampaign.Application.Event;
using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Exceptions;
using FiapDonateCampaign.Domain.Interfaces;

namespace FiapDonateCampaign.Application.Services;

public class DonationService : IDonationService
{
	private readonly IDonationRepository _doacaoRepository;
	private readonly ICampaignRepository _campanhaRepository;
	private readonly IEventPublisher _eventPublisher;

	public DonationService(
		IDonationRepository doacaoRepository,
		ICampaignRepository campanhaRepository,
		IEventPublisher eventPublisher)
	{
		_doacaoRepository = doacaoRepository;
		_campanhaRepository = campanhaRepository;
		_eventPublisher = eventPublisher;
	}

	public async Task<IntentionDonateResponseDto> RegistrarIntencaoAsync(
		string doadorId, 
		IntentionDonateRequestDto dto)
	{
		var campanha = await _campanhaRepository.ObterPorIdAsync(dto.IdCampanha)
			?? throw new DomainException("Campanha não encontrada.");

		campanha.ValidarPodeReceberDoacao();

		var doacao = new Donation(dto.IdCampanha, doadorId, dto.ValorDoacao);
		await _doacaoRepository.AdicionarAsync(doacao);

		// ❌ PROBLEMA: Evento genérico com campos diferentes do esperado
		var evento = new DonationReceivedEvent(
			doacao.Id, 
			doacao.CampanhaId, 
			doacao.DoadorId,           // ← Campo não esperado no RabbitMQ
			doacao.ValorDoacao, 
			doacao.DataDoacao);        // ← DateTime, não DateTimeOffset

		await _eventPublisher.PublishAsync(evento, "campanha.doacao.recebida");

		return new IntentionDonateResponseDto(
			doacao.Id, 
			doacao.CampanhaId, 
			doacao.ValorDoacao, 
			doacao.DataDoacao);
	}
}
```

### Problemas:
- ❌ Evento contém `DoadorId` que não é necessário no contrato RabbitMQ
- ❌ `DateTime` em vez de `DateTimeOffset` (perde timezone)
- ❌ Nomenclatura em padrão English (DonationReceivedEvent)
- ❌ Não é específico para RabbitMQ

---

## ✅ DEPOIS (Usando DoacaoRecebidaEvent específico para RabbitMQ)

```csharp
using FiapDonateCampaign.Application.DTOs;
using FiapDonateCampaign.Application.Event;
using FiapDonateCampaign.Application.Interface;
using FiapDonateCampaign.Domain.Entities;
using FiapDonateCampaign.Domain.Exceptions;
using FiapDonateCampaign.Domain.Interfaces;

namespace FiapDonateCampaign.Application.Services;

public class DonationService : IDonationService
{
	private readonly IDonationRepository _doacaoRepository;
	private readonly ICampaignRepository _campanhaRepository;
	private readonly IEventPublisher _eventPublisher;

	public DonationService(
		IDonationRepository doacaoRepository,
		ICampaignRepository campanhaRepository,
		IEventPublisher eventPublisher)
	{
		_doacaoRepository = doacaoRepository;
		_campanhaRepository = campanhaRepository;
		_eventPublisher = eventPublisher;
	}

	public async Task<IntentionDonateResponseDto> RegistrarIntencaoAsync(
		string doadorId, 
		IntentionDonateRequestDto dto)
	{
		var campanha = await _campanhaRepository.ObterPorIdAsync(dto.IdCampanha)
			?? throw new DomainException("Campanha não encontrada.");

		campanha.ValidarPodeReceberDoacao();

		var doacao = new Donation(dto.IdCampanha, doadorId, dto.ValorDoacao);
		await _doacaoRepository.AdicionarAsync(doacao);

		// ✅ MELHOR: Evento específico para RabbitMQ
		var evento = new DoacaoRecebidaEvent(
			DoacaoId: doacao.Id,                      // ✅ ID da doação
			IdCampanha: doacao.CampanhaId,            // ✅ ID da campanha
			ValorDoacao: doacao.ValorDoacao,          // ✅ Valor doado
			DataHoraRecebida: doacao.DataDoacao);     // ✅ DateTimeOffset com timezone

		// ✅ Limpo e clara intenção de publicar para fila específica
		await _eventPublisher.PublishAsync(evento, "campanha.doacao.recebida");

		return new IntentionDonateResponseDto(
			doacao.Id, 
			doacao.CampanhaId, 
			doacao.ValorDoacao, 
			doacao.DataDoacao);
	}
}
```

### Melhorias:
- ✅ Evento com APENAS os campos necessários para RabbitMQ
- ✅ `DateTimeOffset` garante timezone (UTC)
- ✅ Nomenclatura em português (alinhada com projeto)
- ✅ Nomes de parâmetros nomeados (mais legível)
- ✅ Específico para RabbitMQ (contrato claro)

---

## 📊 Comparação de Estruturas

### DonationReceivedEvent (ANTES)
```csharp
public record DonationReceivedEvent(
	Guid DoacaoId,        // ✅ Necessário
	Guid CampanhaId,      // ✅ Necessário
	string DoadorId,      // ❌ NÃO necessário (privado)
	decimal ValorDoacao,  // ✅ Necessário
	DateTime DataDoacao); // ❌ DateTime (sem timezone)
```

### DoacaoRecebidaEvent (DEPOIS)
```csharp
public record DoacaoRecebidaEvent(
	Guid DoacaoId,                    // ✅ ID da doação
	Guid IdCampanha,                  // ✅ ID da campanha
	decimal ValorDoacao,              // ✅ Valor doado
	DateTimeOffset DataHoraRecebida); // ✅ Com timezone (UTC)
```

## JSON Serializado

### Antes
```json
{
  "doacaoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "campanhaId": "8f0ae4d4-c001-4e6b-9e1f-d1c5c6c7c8c9",
  "doadorId": "user-123",                    // ← Privado, não deve há RabbitMQ
  "valorDoacao": 250.50,
  "dataDoacao": "2025-01-15T10:30:45.1234567" // ← Sem timezone
}
```

### Depois
```json
{
  "doacaoId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "idCampanha": "8f0ae4d4-c001-4e6b-9e1f-d1c5c6c7c8c9",
  "valorDoacao": 250.50,
  "dataHoraRecebida": "2025-01-15T10:30:45.1234567+00:00" // ← Com timezone
}
```

---

## 🎯 Por Que Isso Importa?

### 1. **Contrato Claro com RabbitMQ**
- Consumer sabe EXATAMENTE qual estrutura esperar
- Sem campos extras que podem causar confusão

### 2. **Timezone Correto**
- `DateTimeOffset` preserva fuso horário
- Essencial para eventos distribuídos globalmente
- `DateTime.UtcNow` sozinho perde essa informação

### 3. **Domain-Driven Design**
- Contrato específico para cada contexto
- RabbitMQ é um detalhe de delivery, não de domínio
- Mas o evento deve ser bem estruturado

### 4. **Segurança**
- `DoadorId` é privado (não deve ir para fila pública)
- Apenas informações necessárias são expostas

### 5. **Manutenção**
- Fácil adicionar novos campos sem quebrar consumers
- Versionar eventos é simples (criar `DoacaoRecebidaEventV2`)

---

## 🚀 Próximo Passo: Implementar RabbitMqEventPublisher

Agora que o contrato está definido e o DonationService está integrado, o próximo passo é:

1. **Instalar RabbitMQ.Client**
2. **Implementar RabbitMqEventPublisher**
3. **Configurar em appsettings.json**
4. **Trocar LogEventPublisher por RabbitMqEventPublisher** em DependencyInjection.cs

Ver: `IMPLEMENTATION_SUMMARY.md` para próximos passos!

---

**Versão:** 1.0  
**Atualizado:** Janeiro 2025  
**Status:** ✅ Implementação de Contrato Completa
