# Security

Last reviewed: 2026-11-19 (whole app: API, identity, client, Azure infrastructure, CI/CD, dependencies).

## What the review covered

Authorization on every endpoint, object-level access (technicians see only their own work orders), authentication and
token handling, registration and invitation flows, external sign-in, input limits, error handling, response headers,
rate limiting, secrets, dependency vulnerabilities (`dotnet list package --vulnerable`: none), the Bicep (network,
identity, Key Vault, SQL) and the GitHub Actions workflows.

## Fixed in this review

| Issue | Fix |
| --- | --- |
| The dev API ran in **Development** mode in Azure (stack traces, API docs and loopback CORS exposed to the internet) | Dev now runs as `Staging`; prod as `Production`. Development is local only. |
| **No rate limiting** on sign-in, registration, invitation and email links | Per-address limits (10/min on auth routes, 240/min overall), per-user limits on SMS (3 per 10 min) and on code entry (8 per 10 min). Forwarded headers are enabled so the limit sees the real client address behind Azure. |
| **No security headers** | API: `nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy`, a locked-down CSP, `Cache-Control: no-store`, HSTS outside Development. Client (Static Web App): the same family plus HSTS and a **Content-Security-Policy** that allows only our own scripts. |
| A fixed CSP would need `unsafe-inline` because Blazor's published page has an inline import map that changes every build | `infra/scripts/apply-csp.py` hashes the inline script in the actual published page at deploy time and allows exactly that. Checked in a browser: the app runs, fonts load, injected inline scripts are blocked. |
| **Account takeover** through external sign-in: a provider identity was attached to any existing account with the same email, verified or not | A provider is only linked to an account whose email is confirmed. New external users still need an invitation. |
| JWT signing key length was not checked, and any HMAC algorithm was accepted | Startup fails unless the key is at least 32 bytes; only HS256 is accepted. |
| Sign-in revealed whether an account was **locked** (and an unknown email looked different in timing and wording) | One message for unknown email, wrong password and locked account. Only someone who knows the password learns the email is unconfirmed. |
| Unbounded password and email length (cost of hashing, storage abuse) | Length limits on every credential field (password max 128). |
| ASP.NET **data-protection keys** were in the container's disk, lost on every restart (email-confirmation tokens and the external sign-in cookie would break) | Keys are stored in the database. |
| Prod Key Vault could be purged | Purge protection is on for prod. |
| Dependencies were never checked | Dependabot (NuGet and GitHub Actions, weekly). |

## New protections

- **Password policy** (`PasswordPolicy`, shared by the API and the client's live checklist): 12 to 128 characters, upper
  case, lower case, a digit and a symbol; must not contain the user's name or email; must not be a common password,
  including look-alike spellings such as `P@ssw0rd` and padded ones such as `Password2027!`.
- **Email verification**: the first account must confirm its email link before it can sign in. Invited users are
  verified by the invitation email itself; external users by their provider.
- **Phone verification**: a number is only saved after a texted code is entered. Profile updates cannot set a phone
  number directly. Texting needs a Communication Services phone number (see below).

## Known risks and recommendations

| Risk | Notes and recommendation |
| --- | --- |
| The JWT is kept in `localStorage` and lasts 8 hours with no refresh or revocation | Script injection could steal it; the CSP is the mitigation. Role changes and removals do not take effect until the token expires. Consider a shorter lifetime with refresh tokens, or cookie-based sessions through a backend-for-frontend. |
| Prod has no required reviewers | Add them under Settings > Environments > prod so a deploy needs approval. |
| GitHub Actions are pinned by version tag, not commit SHA | A compromised tag would run in the pipeline with Azure access. Pin to SHAs (Dependabot can then update them). |
| SQL allows "Azure services" through its firewall | Needed for the free App Service tier, and every login still needs Entra authentication. Private endpoints need paid tiers. |
| Key Vault allows public network access | Every read needs an Azure role; restrict the network when the plan allows. |
| Data-protection keys are stored unencrypted in the database | They only protect short-lived email and cookie tokens. Encrypt them with a Key Vault key if the data is ever sensitive enough. |
| No audit trail of admin actions (invitations, role changes, deletions) | Add structured audit events before the user base grows. |
| `AllowedHosts` is `*` | Low risk behind Azure; restrict to the real host names. |
| Accounts created before email verification existed are unconfirmed | They cannot sign in until they confirm. Locally, request a new link from the sign-in page and copy it from the API log. |

## Setting up phone verification in Azure

Text messaging needs a phone number on the Communication Services resource, which cannot be created from Bicep:
in the portal, open the `mowtrackr-acs-<env>-001` resource > Phone numbers > Get, choose a toll-free (or short code)
number for SMS, and complete the carrier verification it asks for (toll-free numbers need it before messages are
delivered, which can take days). Then set the GitHub environment variable `SMS_SENDER_NUMBER` to the number in
international format (for example `+18885550100`) and run the Deploy workflow. Until then the Verify phone dialog tells
the user that texting is not set up.