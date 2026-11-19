# Phase 2 plan: tablets, multiple customers, billing

Status: draft for discussion (2026-11). Effort is sized small / medium / large, not in dates; the order matters more than the sizes.

## Where we are

MowTrackr is a single-organisation app: a Blazor WebAssembly client (installable, phone-friendly), an ASP.NET Core API, Azure SQL, invitation-only sign-up with email and phone verification, and the security hardening in [security.md](security.md). The first customers use no software today, so the real competitor is paper and spreadsheets: the product has to be easier than a notebook.

## What phase 2 has to deliver

1. **Usable on a tablet in the field**, including with poor signal.
2. **More than one customer** on the same system without ever seeing each other's data.
3. **A way to charge** (see the pricing discussion: per active mower, unlimited users, free tier, no contract).
4. **Crew and time tracking** as the first step toward employee management.

Estimates and invoicing are deliberately **not** in this phase; see "Later".

## Recommended order

| Step | What | Size | Why this position |
| --- | --- | --- | --- |
| 1 | Tablet layout and workflow polish | S | No schema change, immediately useful to the first customers |
| 2 | Multi-tenant foundation | L | Every later feature touches the data model; far cheaper now than after more tables exist |
| 3 | Offline: read, then write and sync | L | Needs the stable multi-tenant model; the largest technical risk |
| 4 | Self-serve sign-up, trial, onboarding | M | Lets customer two onboard without you |
| 5 | Billing and plan limits | M | Build after a few design partners confirm what they will pay |
| 6 | Crew and time tracking | M | Ties labour cost to work orders and mowers |

Steps 1 and 2 can run in parallel. Step 5 should wait for evidence of willingness to pay.

## 1. Tablet layout and workflow (S)

- Landscape-first layouts: a two-pane work order screen (list left, tasks right) instead of the phone's stacked cards.
- Bigger touch targets and a technician home screen that shows today's work orders, not the manager dashboard.
- Hour-meter entry with a numeric keypad; checklist rows that can be ticked with one tap (partly done in the mobile work).
- QR code on each mower that opens its page (scan at the shop, start the right work order).
- Camera: attach a photo to a work order task (damage, worn blade).
- **Label capture** (customer request): a technician photographs the equipment's data plate or sticker and the manufacturer, model and serial number are read from it to fill in a new mower. See "Label capture" below.
- Pin down which tablets to support (iPad Safari, Android Chrome) and test on real devices.

### Label capture (S for photos, M for reading the label)

Requested by a customer: take a picture of the label with the serial number, manufacturer and model, and add it to the asset. Build it in two steps so value arrives early.

**Step A, photos (S).** "Add mower" gets a **Scan label** button that opens the tablet camera (`<input type="file" accept="image/*" capture="environment">` works on iPad and Android without a native app). The photo is compressed in the browser (phone photos are 5 to 10 MB), uploaded, and stored with the mower; the technician types the fields in with the photo beside the form. Useful on its own: the label photo stays on the record as proof and for parts ordering.

**Step B, read the label (M).** Send the photo to a service that reads text and returns manufacturer, model and serial number, and **prefill the form**. Always show the result for the technician to confirm or correct before saving: OCR confuses 0/O and 1/I and labels are messy, and a wrong serial number is worse than an empty one.

Ways to read the label, in order of how I would try them:

1. **A vision AI model with a structured answer** (a JSON object with the three fields plus a confidence per field). Best at free-form labels, since "serial" and "model" are placed differently on every brand. Cost is a small amount per photo; needs a per-customer usage limit.
2. **Azure OCR (Azure AI Vision / Document Intelligence) plus rules.** Stays inside Azure and is cheap, but needs brand-specific parsing to tell the serial from the model; weaker on unusual labels.
3. **Read the barcode or QR code** on the label, if there is one (many modern plates encode the serial). Works offline in the browser with a JavaScript library; a good extra, not a replacement.

Design points:

- **Storage**: a single tenant-scoped `Attachment` (photo) record linked to a mower or work order, files in a private Azure Blob container, served by short-lived links. Reuse it for the work order task photos above.
- **Upload safety**: accept only JPEG, PNG and HEIC (converted); reject SVG and anything else; size limit; strip location data from the photo; rate limit uploads and label reads (they cost money).
- **Privacy**: a hosted AI model means the photo leaves our system; say so in the privacy statement, and prefer a provider and region that fit the customers' expectations.
- **Offline**: the photo is queued with the other offline changes and read when the connection returns; the technician can still type the fields by hand at once.
- **Measure it**: record how often the technician corrects each field, so we know whether the reading is good enough to trust.

## 2. Multi-tenant foundation (L)

Today there is one implicit organisation. The rules to build in:

- **Tenant**: a new `Tenant` (name, plan, status). Every business table gets a `TenantId` (mowers, service templates, work orders, invitations, and so on).
- **Isolation in one place**: EF Core global query filters keyed on the current tenant, resolved from a claim in the JWT, plus a save-time check that new rows get the caller's tenant. No controller or service filters by tenant by hand. Add automated tests that try to read and write across tenants and must fail.
- **Membership and roles**: roles (Admin, Manager, Technician) become per tenant. Start with one tenant per user; keep the model ready for a user in several tenants (a contractor, or a dealer supporting customers).
- **Invitations** become tenant-scoped; the "first account is Admin and registration then closes" rule is replaced by "sign-up creates a tenant and its first Admin" (step 4).
- **Existing data** migrates into a default tenant; the current Admin becomes that tenant's Admin.
- **Platform admin**: a separate, tightly guarded role for you (see all tenants, suspend one, support). Not exposed in the customer UI.
- **Per-tenant settings**: language, theme default, mowing season dates, units.
- **Customer data rights**: export and delete a tenant's data; an audit trail of admin actions (invitations, role changes, deletions).
- **Database**: a shared database with a tenant column is the right start (cheap, simple). Revisit a database per customer only if a large customer needs hard isolation or their own backups.

Open decision: a shared database means the paid Azure SQL tier from the first paying customer on; the free offer will not do.

## 3. Offline (L)

Technicians work in yards and shops with weak signal, so offline is the feature they will judge the product on.

- **App shell**: a service worker caches the application so it opens with no signal (the manifest already lets it be installed).
- **Read**: cache the technician's assigned work orders, the mowers and service templates they reference, in IndexedDB; show them with a clear "offline" indicator and last-synced time.
- **Write**: ticking tasks, entering hours, adding notes and completing a work order are saved locally in a queue and sent when the connection returns.
- **Sync rules** (decide before building):
  - every queued change carries a client-generated id, so a retry never applies twice (idempotency);
  - per task, the last change wins, and the server still enforces the rules (hours never go below the starting hours, required tasks before completion);
  - a change that the server rejects (someone revoked the work order) is kept and shown to the technician, never silently dropped.
- **Sessions**: sign-in tokens last 8 hours and the app now signs the user out when one expires. That must not throw away queued work: keep the queue after expiry and ask the user to sign in again before syncing. Consider longer-lived device sessions (refresh tokens) for shared shop tablets.
- **Test plan**: a scripted offline test (airplane mode mid-checklist) on real tablets.

## 4. Sign-up, trial and onboarding (M)

- Public sign-up creates the tenant and first Admin; 14-day trial, no card.
- First-run wizard: add three mowers (or **import from CSV**), pick a template, schedule the first work order. A customer starting from paper should reach "first mower tracked" in minutes.
- Sample data option so a new account is not empty.
- Email: welcome and trial-ending messages (the email domain and templates already exist).

## 5. Billing and limits (M)

- Stripe Billing: plans by active mowers, unlimited users, monthly or annual, a pause option for winter, customer portal for self-service, Stripe Tax if needed.
- Plan limits are enforced when **adding** mowers; existing data is never locked away, and users are never blocked from their own records.
- Webhooks keep the tenant's plan state in step (trial ended, payment failed, cancelled); a grace period before restricting anything.
- Start prices to test (placeholders, validate with design partners): Free up to 3 mowers, Starter up to 15 around $19-29 a month, Pro up to 50 around $49-79 a month, Business quoted.

## 6. Crew and time tracking (M)

- Time entries on work orders (start/stop or manual), per technician.
- Labour rate per employee (optional) so each work order and mower shows labour plus parts cost.
- Crew members without a login (a seasonal worker who does not use the app) so time can still be recorded by a manager.
- Simple weekly timesheet and a labour-by-mower report.

## Cross-cutting work

- **Hosting**: move off the free App Service and SQL tiers before the first paying customer; set uptime alerts and a basic runbook. Backups and a tested restore.
- **Legal**: terms of service, privacy policy, and a data-processing statement.
- **Observability**: error and performance monitoring (Application Insights), plus product analytics for onboarding drop-off.
- **Support**: an in-app way to report a problem, a support inbox, and a status page.
- **Security**: keep [security.md](security.md) current; add tenant isolation tests to CI; review the long-lived device session design.

## Later (not phase 2)

- **Estimates** need job costing underneath (labour plus equipment hours), so they follow step 6.
- **Invoicing**: start with an export for QuickBooks (or similar) rather than building invoicing, payments and tax handling; build the real thing only if customers keep asking and will pay for it.
- Dealer / multi-location features, GPS, parts inventory.

## Decisions needed

1. Which tablets must work on day one (iPad, Android, both)?
2. How many mowers and employees does a typical first customer have?
3. How do the first customers schedule and invoice today (paper, messages, QuickBooks)?
4. Is the target customer a small landscaping crew, a municipality or golf course, or a dealer? This changes the roles, the pricing and the isolation needs.
5. What would the design partners pay, and do they need offline from day one or can step 1 ship first?
6. What do the labels look like (stamped metal plate, printed sticker, with or without a barcode), and are they often dirty or worn? A handful of real photos from the requesting customer would settle which reading approach to use.
