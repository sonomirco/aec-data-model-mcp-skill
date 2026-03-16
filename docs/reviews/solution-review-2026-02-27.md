# Solution review - 2026-02-27

## Scope and verification

Review scope:
- Architecture and startup wiring.
- Authentication and viewer workflow services.
- Core GraphQL template plumbing.
- Documentation and contributor guidance.

Validation commands run:
- `dotnet build src/apsMcp.sln`
- `dotnet test src/apsMcp.sln --no-build`

Current status:
- Build succeeded with 2 nullable warnings in `GraphQLTemplate.cs`.
- Tests passed: 86/86.

## Critical

1. Replace non-cryptographic PKCE verifier generation in auth flow.
Evidence: `src/apsMcp.Tools/Services/AuthService.cs:109` uses `new Random()` for PKCE verifier generation.
Impact: PKCE verifier entropy source is predictable relative to cryptographic RNG expectations.
Address: use `RandomNumberGenerator` and RFC 7636-compatible verifier generation.

2. Harden callback URL handling before registering the HTTP listener prefix.
Evidence: `src/apsMcp.Tools/Services/AuthService.cs:70` calls `listener.Prefixes.Add(_callbackUrl)` directly.
Impact: callback URLs without required prefix formatting (for example missing trailing slash) can break login at runtime.
Address: normalize and validate callback URL format before listener registration, and fail fast with a clear error.

3. Add lifecycle control for viewer-side HTTP/WebSocket background loops.
Evidence: `src/apsMcp.Tools/ViewerTools.cs:172` and `src/apsMcp.Tools/ViewerTools.cs:191` run infinite loops; listeners are created in `src/apsMcp.Tools/ViewerTools.cs:166` without disposal path.
Impact: long-running process resource leakage risk and fragile behavior across repeated viewer sessions.
Address: move listener/websocket lifecycle to a managed hosted service with cancellation tokens and proper disposal.

## Good to have

1. Re-enable service defaults or align docs with current runtime behavior.
Evidence: `builder.AddServiceDefaults()` is commented at `src/apsMcp.SseServer/Program.cs:21`.
Impact: health checks, standard resilience, and OpenTelemetry defaults documented for the stack are not applied in SSE server startup.
Address: either enable defaults and endpoint mapping or explicitly document that they are intentionally disabled.

2. Eliminate nullable warnings in GraphQL template parameter handling.
Evidence: warning-producing paths at `src/apsMcp.Tools/Models/GraphQLTemplate.cs:69` and `src/apsMcp.Tools/Models/GraphQLTemplate.cs:98`.
Impact: nullable contract drift and potential runtime exceptions under template misconfiguration.
Address: add null guards around semantic wrapper parameter names and avoid null-forgiving where not guaranteed.

3. Narrow broad exception swallowing in authentication continuity logic.
Evidence: `catch (Exception)` in `src/apsMcp.Tools/Services/AuthService.cs:208`.
Impact: operational failures become hard to diagnose because unrelated errors trigger silent re-authentication fallback.
Address: catch specific exception types and add structured logging for unexpected failures.

4. Remove or justify legacy SignalR package dependency.
Evidence: `Microsoft.AspNetCore.SignalR.Core` is referenced at `src/apsMcp.SseServer/apsMcp.SseServer.csproj:12`, with no source usage in the server code.
Impact: unnecessary dependency footprint and maintenance risk.
Address: remove unused dependency or document the expected near-term usage.

5. Expand tests beyond template mapping to cover service integration risks.
Evidence: test project currently includes only `PaginationModelTests.cs`, `ParameterConsistencyTests.cs`, and `TemplateQueryGenerationTests.cs`.
Impact: no regression coverage for auth listener behavior, token refresh, or viewer lifecycle orchestration.
Address: add targeted integration/unit tests for auth callbacks, token refresh failures, and viewer loop lifecycle.

## Nice to have

1. Fix character encoding artifacts in prompt markdown export.
Evidence: `src/apsMcp.Tools/prompt.md:150` and `src/apsMcp.Tools/prompt.md:184` contain mojibake (`â†’`).
Impact: degraded prompt readability and maintenance friction.
Address: regenerate or normalize file encoding as UTF-8 consistently.

2. Resolve naming and casing inconsistencies in source layout.
Evidence: `src/apsMcp.Tools/graphQlTools.cs` file casing differs from class name `GraphQlTools`; mixed namespace casing appears in service imports.
Impact: readability and portability friction, especially on case-sensitive environments.
Address: standardize file names and namespace casing conventions.

3. Align contributor entry-point docs with current guidance files.
Evidence: `README.md:46` points contributors to `CLAUDE.md`, which is not present as a separate file in this repository.
Impact: onboarding confusion.
Address: point README to `AGENTS.md` (or add a dedicated `CLAUDE.md` that mirrors current guidance).

4. Add the referenced ExecPlan template source.
Evidence: process guidance expects `.agents/plans.md`, but that path is currently missing.
Impact: process guidance cannot be followed literally.
Address: add `.agents/plans.md` or update guidance to a valid existing path.

