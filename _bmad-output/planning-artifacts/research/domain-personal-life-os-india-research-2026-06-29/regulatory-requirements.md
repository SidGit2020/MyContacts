# Regulatory Requirements

## Applicable Regulations

The Personal Life OS must comply with four regulatory frameworks simultaneously:

| Regulation | Authority | Status | Impact on Life OS |
|---|---|---|---|
| **DPDP Act 2023 + Rules 2025** | MeitY | Phase 1 active (Nov 2025) | 🔴 Critical — governs all personal data processing |
| **IT Act 2000 + IT Rules 2021/2025** | MeitY | Active | 🟡 Medium — content moderation, grievance redressal |
| **UPI/NPCI Regulations** | RBI/NPCI | Active | 🟡 Medium — payment processing compliance |
| **India AI Governance Guidelines** | MeitY/IndiaAI | Voluntary (Nov 2025) | 🟢 Advisory — best practices for AI features |

_Source: [DPDP Act Mobile App Compliance — Respectlytics](https://respectlytics.com/blog/india-dpdp-act-mobile-app-compliance/), [India AI Governance Guidelines — PIB](https://www.pib.gov.in/PressReleasePage.aspx?PRID=2228315&reg=48&lang=2)_

---

## Data Protection and Privacy — DPDP Act Deep Dive

**This is the single most critical regulatory obligation for the Life OS.**

The Digital Personal Data Protection Act 2023, with Rules notified in November 2025, is India's comprehensive data protection law. Key provisions:

### Consent Requirements
- **No "legitimate interests" basis** — unlike GDPR, India has no such exception. Every data processing activity requires explicit, informed consent
- Consent must be **free, specific, informed, unconditional**, and based on clear affirmative action
- "Accept or don't use the app" consent patterns are explicitly risky — consent cannot be conditioned on service access
- **Consent notices required in 22 Indian languages** — a significant implementation requirement
- Users can withdraw consent at any time; data must be deleted within 7 days of withdrawal request

### Children's Data (Critical for Family Contact Category)
- Children defined as **under 18** (stricter than GDPR's 13–16)
- **Tracking and profiling of children is expressly banned**
- Verifiable parental consent required before processing any child's data
- The Life OS Contact Category system must handle "Family" contacts carefully — any contact tagged as a child must not be profiled

### Implementation Timeline
```
Nov 2025  → Phase 1: DPBI (Data Protection Board of India) established
Nov 2026  → Phase 2: Consent Manager framework activates
May 2027  → Phase 3: Full enforcement begins (18-month window from Nov 2025)
```

### Penalties
- Up to **₹250 crore (~$30M) per violation category**
- 72-hour breach notification to DPBI mandatory
- Companies must implement automated data deletion workflows

### Life OS Compliance Checklist
- [ ] Granular consent per data category (contacts, location, interaction history, spiritual data, financial interests)
- [ ] In-app consent manager with withdrawal capability
- [ ] Consent notices in Hindi + at least 2 regional languages at launch
- [ ] Data localisation — store sensitive personal data on India-based servers
- [ ] 72-hour breach notification protocol
- [ ] Automated deletion workflow (7-day SLA)
- [ ] Child-safe design for family contact categories
- [ ] Appoint Data Protection Officer (DPO) when user base crosses thresholds

_Source: [DPDP Rules 2025 Compliance Guide — Seclore](https://www.seclore.com/fundamentals/dpdp-rules-2025-compliance-guide/), [India DPDPA 2025 Updated Guide — CookieYes](https://www.cookieyes.com/blog/india-digital-personal-data-protection-act-dpdpa/)_

---

## IT Act / Content Moderation Compliance

**When the Life OS crosses 5M registered users**, it becomes a **Significant Social Media Intermediary (SSMI)** with enhanced obligations:
- Appoint India-resident Chief Compliance Officer, Nodal Contact Person, and Resident Grievance Officer
- Monthly compliance reports to government
- Faster content removal timelines for unlawful content
- AI-generated content must be **labelled as AI-generated** (IT Amendment Rules 2025)
- Grievance redressal mechanism mandatory from Day 1 (even below SSMI threshold)

**Religious Content:**
- India's IT Rules use an age-based content classification system for religious content
- No special licensing required for spiritual content apps at current scale
- Community Satsang sessions, scripture study groups, and prayer coordination are not regulated activities beyond standard IT Act provisions
- Avoid content that could be interpreted as promoting religious disharmony (IPC Section 153A risk)

**AI-Generated Content (IT Amendment Rules 2025):**
- Any AI-generated spiritual quotes, relationship coaching text, or financial suggestions must be labelled
- Platforms must verify and label synthetic/AI-generated media
- This applies to the AI coaching features of the Life OS

_Source: [IT Amendment Rules 2025 — PMF IAS](https://www.pmfias.com/it-amendment-rules-2025/), [Telecoms Media Internet Laws India 2026 — ICLG](https://iclg.com/practice-areas/telecoms-media-and-internet-laws-and-regulations/india/)_

---

## Payment and Booking Compliance

**UPI Integration (Razorpay / PhonePe):**
- UPI is RBI-regulated; apps integrating UPI must use licensed Payment Service Providers (PSPs) — direct NPCI API access requires RBI licence; use Razorpay, PhonePe, or PayU as intermediaries
- Transaction limit: ₹1 lakh–₹2 lakh per transaction (bank-dependent)
- GST invoicing required for all paid transactions
- No IRCTC convenience fee for UPI payments; standard charges apply for AC/non-AC bookings

**IRCTC Travel Booking API:**
- IRCTC API available via authorised third-party providers (not direct to public)
- Compliance requirements: authorised agent registration, data security per Indian Railways standards, passenger data handling guidelines, GST invoicing, tatkal rule adherence
- Use third-party IRCTC API aggregators (ZuelPay, BOS Center) for travel booking features — avoids direct IRCTC authorisation complexity
- **Recommendation:** Phase 2/3 feature; integrate via established aggregator to avoid regulatory overhead at launch

_Source: [IRCTC API Integration Guide 2026 — BOS Center](https://bos.center/irctc-api-integration-guide-travel-agents), [UPI Policy IRCTC — IRCTC Official](https://contents.irctc.co.in/en/Policy_for_Unified_Payment_Interface_Integration_on_e-ticketing_Website_and_Mobile_App.pdf)_

---

## AI Governance Compliance

India's AI Governance Guidelines (MeitY, November 2025) are **voluntary, not enforceable law**. They represent India's "light-touch" innovation-first approach:

- **No separate AI law** — existing DPDPA, IT Act, and Consumer Protection Act govern AI
- Key voluntary commitments: transparency in AI use, grievance redressal, privacy-enhancing technologies, algorithmic audits
- **DPDPA consent regime is the binding constraint** — AI training on personal relationship data requires explicit consent
- Children under 18 cannot be profiled or tracked by AI features
- AI-generated content (coaching suggestions, spiritual quotes) must be labelled

**Life OS AI Compliance Actions:**
- All AI coaching outputs labelled: "AI suggestion — powered by Claude"
- Relationship intelligence features require explicit opt-in consent separate from basic app consent
- No AI profiling of contacts tagged as children/minors
- Grievance mechanism for AI-generated suggestions ("this recommendation was wrong")
- Publish quarterly AI transparency report once SSMI threshold approached

_Source: [AI Laws and Regulations India 2026 — Prashant Mali](https://www.prashantmali.com/cyber-law-blog-india/ai-laws-and-regulations-in-india-as-of-2026), [India AI Governance Guidelines EY](https://www.ey.com/en_in/insights/ai/ai-governance-guidelines-a-bet-on-innovation)_

---

## Risk Assessment

| Regulatory Risk | Severity | Timeline | Mitigation |
|---|---|---|---|
| DPDP consent non-compliance | 🔴 Critical (₹250 Cr) | Phase 3: May 2027 | Build consent-first architecture from Day 1; 22-language consent UI |
| SSMI threshold triggers enhanced obligations | 🟡 Medium | When 5M+ users | Plan CCO/Nodal officer appointments in advance; build compliance dashboard |
| IRCTC/travel booking unlicensed | 🟡 Medium | Phase 2 feature | Use authorised API aggregators only; never direct IRCTC API |
| AI-generated content unlabelled | 🟡 Medium | IT Rules 2025 active | Label all AI outputs at UI level from Day 1 |
| Child data profiling violation | 🔴 High | DPDPA active | Implement age-gating and child-safe design for family categories |
| UPI payment without PSP licence | 🔴 High | Payment feature launch | Mandatory: use Razorpay/PhonePe; never direct NPCI API |
