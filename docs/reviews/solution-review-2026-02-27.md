# solution review - 2026-02-27 (updated 2026-03-18)

## current verification snapshot

Validation commands:
- `dotnet build src/apsMcp.sln`
- `dotnet test src/apsMcp.sln --no-build`

Current status:
- Build succeeded with `0` warnings.
- Tests passed: `93/93`.

## status of original findings

### resolved

1. Replace non-cryptographic PKCE verifier generation in auth flow.
- Status: resolved.
- Evidence: `AuthService.GenerateCodeVerifier` now uses `RandomNumberGenerator`.

2. Harden callback URL handling before registering the HTTP listener prefix.
- Status: resolved.
- Evidence: callback URL validation and listener-prefix normalization now run in `AuthService`.

3. Add lifecycle control for viewer-side HTTP/WebSocket background loops.
- Status: resolved.
- Evidence: `ViewerRuntimeService` is a managed hosted service with cancellation and disposal handling.

4. Re-enable service defaults or align docs with runtime behavior.
- Status: resolved.
- Evidence: `builder.AddServiceDefaults()` is active in `src/apsMcp.SseServer/Program.cs`.

5. Eliminate nullable warnings in GraphQL template parameter handling.
- Status: resolved.
- Evidence: build now runs warning-free.

6. Narrow broad exception swallowing in authentication continuity logic.
- Status: resolved.
- Evidence: `EnsureAuthenticatedAsync` now catches specific exception types before a final unexpected-error branch.

7. Remove or justify legacy SignalR package dependency.
- Status: resolved.
- Evidence: no `Microsoft.AspNetCore.SignalR.Core` reference remains in `apsMcp.SseServer.csproj`.

8. Expand tests beyond template mapping to cover service integration risks.
- Status: partially resolved.
- Evidence: dedicated tests now exist for auth callback/prefix logic and viewer runtime lifecycle.

9. Fix character encoding artifacts in prompt markdown export.
- Status: resolved.
- Evidence: prior mojibake markers are no longer present in `src/apsMcp.Tools/prompt.md`.

10. Resolve naming and casing inconsistencies in source layout.
- Status: resolved.
- Evidence: namespace casing is normalized to `apsMcp.*` in source and tests.

11. Align contributor entry-point docs with current guidance files.
- Status: resolved.
- Evidence: `README.md` points contributors to `AGENTS.md`.

12. Add the referenced ExecPlan template source.
- Status: resolved.
- Evidence: `.agents/plans.md` exists and documents the expected template.

## remaining high-value improvements

1. Expand auth tests to cover token exchange and refresh failure paths with HTTP stubbing.
2. Keep transport documentation explicit about streamable HTTP (`/`) vs legacy SSE (`/sse`) usage by client type.
