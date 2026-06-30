# Integration Patterns Analysis

## API Design Patterns

The Life OS backend uses a **REST-first** approach for all external integrations and mobile clients:

- **Minimal API endpoints** for mobile ↔ backend communication (Flutter → ASP.NET Core)
- **Webhook receivers** for inbound events (WhatsApp messages, payment confirmations)
- **HTTP Client factory** for outbound calls to Claude, Razorpay, Bhagavad Gita, WhatsApp Cloud APIs
- **OpenAPI 3.1** (built into .NET 10) auto-documents all endpoints — used for mobile client codegen

Pattern recommendation: **REST + JSON** for synchronous requests; **Azure Service Bus / background jobs** (Hangfire) for async workloads (AI analysis, transcription, email notifications). No GraphQL needed at Phase 1 complexity.

---

## WhatsApp Business Cloud API Integration

**Status: Production-ready C# library exists**

A mature C# wrapper for the WhatsApp Business Cloud API is available:
- **`WhatsappBusinessCloudApi`** NuGet package (github.com/gabrieldwight/Whatsapp-Business-Cloud-Api-Net) — DI-friendly, supports sending text, image, PDF, video, template messages
- Webhook endpoint receives inbound messages and status updates via `POST /webhook`
- Meta developer account required; phone number ID and permanent access token provisioned via Meta Business Suite

**Integration architecture for Life OS:**

```csharp
// Program.cs
builder.Services.AddWhatsAppBusinessCloudApiService(options => {
    options.WhatsAppBusinessPhoneNumberId = config["WhatsApp:PhoneNumberId"];
    options.WhatsAppBusinessAccountId    = config["WhatsApp:AccountId"];
    options.WhatsAppBusinessId           = config["WhatsApp:BusinessId"];
    options.AccessToken                  = config["WhatsApp:AccessToken"];
});

// NotificationsModule: Send group event reminder
await _whatsAppClient.SendTextMessageAsync(new TextMessageRequest {
    To      = contact.Phone,
    Text    = new WhatsAppText { Body = $"Satsang reminder: {session.Title} at {session.Time}" }
});

// WebhookController: Receive inbound messages
[HttpPost("/webhook")]
public IActionResult ReceiveMessage([FromBody] WhatsAppNotification notification) {
    // Process inbound message → publish domain event → AI analysis
}
```

**Use cases in Life OS:**
- Send event reminders to group members (Satsang, travel bookings)
- Receive RSVP replies via WhatsApp → update group attendance
- Share daily Bhagavad Gita verse to opted-in contacts
- Trigger AI coaching nudges based on interaction reminders

**Constraints:**
- Template messages required for first-contact outreach (must be pre-approved by Meta)
- 24-hour session window for free-form replies after user initiates contact
- Use BSP (AiSensy, Gupshup) for volumes exceeding direct Meta Cloud API limits

_Source: [WhatsApp API C# .NET — SendZen](https://www.sendzen.io/docs/whatsapp-api-csharp-dotnet), [WhatsApp Business Cloud API .NET — GitHub gabrieldwight](https://github.com/gabrieldwight/Whatsapp-Business-Cloud-Api-Net)_

---

## Claude API (Anthropic) Integration

**Status: Official .NET SDK now available (2026)**

Anthropic shipped an official C# SDK in 2026:
- **`Anthropic`** NuGet package — official Anthropic SDK for .NET, targets .NET Standard 2.0+ and .NET 10.0
- Supports streaming, built-in retries, timeouts, IChatClient integration (Microsoft.Extensions.AI compatible)
- Also available: community `Anthropic.SDK` by tghamm (github.com/tghamm/Anthropic.SDK) targeting .NET 10.0 explicitly

**Integration pattern for Life OS AI coaching:**

```csharp
// AIModule/Services/RelationshipCoachService.cs
public class RelationshipCoachService(AnthropicClient claude) {

    public async Task<string> GenerateInsightAsync(Contact contact, List<InteractionLog> history) {
        var systemPrompt = """
            You are a relationship intelligence assistant. Analyse the interaction 
            history and suggest how to strengthen this relationship. Be concise, 
            warm, and culturally sensitive to Indian family and community dynamics.
            """;
        var userPrompt = BuildContactContext(contact, history);

        var response = await claude.Messages.CreateAsync(new MessageRequest {
            Model    = "claude-sonnet-4-6",
            MaxTokens = 500,
            System   = systemPrompt,
            Messages = [new Message { Role = "user", Content = userPrompt }]
        });
        return response.Content[0].Text;
    }
}
```

**Key capability: Persistent memory (Dreaming API)**
The Claude Managed Agents API "Dreaming" feature (preview May 2026, GA expected late 2026) consolidates persistent memory between sessions. For the Life OS, this means:
- AI relationship coach remembers the last 3 coaching conversations per contact
- Bhagavad Gita reflection context persists across Satsang sessions
- No need to re-send full history on every API call — Claude manages memory server-side

**Important limitation:** Claude Agent SDK is currently Python/TypeScript only — not .NET. For .NET, use the official `Anthropic` NuGet package directly (no agent orchestration framework). Implement tool use and multi-turn conversations manually using the Messages API.

_Source: [Official Claude C# SDK — platform.claude.com](https://platform.claude.com/docs/en/api/sdks/csharp), [Claude is a First-Class .NET Citizen — Medium](https://medium.com/@mikhail.petrusheuski/claude-is-now-a-first-class-net-citizen-and-that-changes-the-ai-stack-73eaef7224fd)_

---

## Razorpay UPI Payment Integration

**Status: REST API, RBI-authorised, PCI DSS Level 1**

Razorpay is the recommended payment provider for India:
- **RBI-authorised Payment Aggregator** + PCI DSS Level 1 certification
- Supports UPI, cards, net banking, wallets, EMI, BNPL — single integration
- **0% MDR for UPI** (government-mandated) — no transaction cost for UPI payments
- 21 billion UPI transactions processed January 2026 alone

**Integration pattern (HttpClient, no official .NET SDK):**

```csharp
// PaymentsModule/Services/RazorpayService.cs
public class RazorpayService(HttpClient http, IConfiguration config) {

    public async Task<RazorpayOrder> CreateOrderAsync(decimal amount, string currency = "INR") {
        var auth = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{config["Razorpay:KeyId"]}:{config["Razorpay:KeySecret"]}"));
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", auth);

        var body = new { amount = (int)(amount * 100), currency, receipt = Guid.NewGuid().ToString() };
        var response = await http.PostAsJsonAsync("https://api.razorpay.com/v1/orders", body);
        return await response.Content.ReadFromJsonAsync<RazorpayOrder>();
    }
}
```

**Webhook for payment confirmation:**
```csharp
[HttpPost("/payments/webhook")]
public IActionResult RazorpayWebhook([FromBody] JsonDocument payload,
    [FromHeader(Name = "X-Razorpay-Signature")] string signature) {
    // Verify HMAC-SHA256 signature using Razorpay webhook secret
    // Publish PaymentConfirmed domain event → unlock premium features
}
```

**September 2025 compliance note:** RBI Master Directions for Payment Aggregators tightened requirements — Razorpay satisfies all of them as a licensed PA. No additional merchant compliance action needed beyond standard KYC.

_Source: [Razorpay UPI Payment Gateway India — Razorpay](https://razorpay.com/upi-payment-gateway-india/), [Razorpay Integration Guide 2026 — AnandhishTech](https://anandhishtech.com/blogs/razorpay-payment-gateway-integration-india.html)_

---

## Whisper Voice Transcription Integration

**Architecture: Faster-Whisper FastAPI microservice**

**Recommended approach:** Deploy Faster-Whisper as a dedicated Python FastAPI microservice; ASP.NET Core calls it via REST.

```
[Flutter App]
  └── POST /api/interactions/transcribe (audio file)
        └── [ASP.NET Core]
              └── POST http://whisper-service:8080/transcribe
                    └── [Faster-Whisper FastAPI]
                          └── Returns { text, language, segments }
              └── POST /claude/analyse → relationship insight
              └── Saves InteractionLog + AI summary to PostgreSQL
```

**Faster-Whisper FastAPI service (Python):**
```python
# whisper_service/main.py
from fastapi import FastAPI, UploadFile
from faster_whisper import WhisperModel

app   = FastAPI()
model = WhisperModel("medium", device="cpu", compute_type="int8")  # ~1GB RAM

@app.post("/transcribe")
async def transcribe(file: UploadFile, language: str = "hi"):
    audio_bytes = await file.read()
    segments, info = model.transcribe(audio_bytes, language=language)
    return {"text": " ".join(s.text for s in segments), "language": info.language}
```

**Production considerations:**
- Use `medium` model for balance of accuracy vs speed; `large-v3` for higher accuracy (requires more RAM)
- Latency: 1–5s for a 2-minute call recording on CPU; acceptable for post-call transcription (not real-time)
- **DPDP compliance**: audio files processed on-server, never sent to external cloud → full data sovereignty
- **Alternative**: AssemblyAI API if team lacks ML engineering capacity — supports Hindi, handles accents, $0.37/hour; trade-off is data leaving India server

**WhisperLive** (collabora/WhisperLive) for real-time streaming use cases (Phase 2).

_Source: [Build Speech-to-Text with Faster Whisper — Medium](https://medium.com/@johnidouglasmarangon/build-a-speech-to-text-service-in-python-with-faster-whisper-39ad3b1e2305), [Faster Whisper — GitHub SYSTRAN](https://github.com/SYSTRAN/faster-whisper)_

---

## System Interoperability and Security Patterns

**Authentication: JWT + Refresh Tokens**
- ASP.NET Core Identity with JWT bearer authentication
- Refresh token rotation stored in PostgreSQL (encrypted)
- OAuth 2.0 social login for Google (Phase 1) and LinkedIn (Phase 2)
- All API endpoints require `[Authorize]` — no anonymous access to personal data

**DPDP Consent as API Gate:**
```csharp
// ConsentMiddleware — runs on every request touching personal data
public class ConsentMiddleware(RequestDelegate next) {
    public async Task InvokeAsync(HttpContext context, IConsentService consent) {
        var userId     = context.User.GetUserId();
        var dataScope  = context.GetRequestedDataScope();  // e.g. "contacts.ai_analysis"
        if (!await consent.HasActiveConsentAsync(userId, dataScope))
            context.Response.StatusCode = 403;  // "Consent required"
        else
            await next(context);
    }
}
```

**Outbound HTTP: IHttpClientFactory**
All external API calls (Claude, WhatsApp, Razorpay, Bhagavad Gita) use named `IHttpClientFactory` clients with:
- Polly retry policies (3 retries with exponential backoff)
- Circuit breakers (open after 5 failures in 30s)
- Request/response logging via Serilog (audit trail for DPDP)

**Event-driven internal coordination:**
- MediatR domain events for cross-module communication (not a message broker at Phase 1 scale)
- Example: `ContactInteractionLogged` event → `AIModule` analyses tone → `NotificationsModule` schedules follow-up reminder

_Source: [Anthropic.SDK GitHub — tghamm](https://github.com/tghamm/Anthropic.SDK), [Faster Whisper HTTP API — GitHub joshuaboniface](https://github.com/joshuaboniface/remote-faster-whisper)_
