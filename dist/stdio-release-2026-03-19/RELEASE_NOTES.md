# Release notes: stdio-v2026-03-19

- Date: 2026-03-19
- Source build commit: 54f987d
- Component: apsMcp.StdioServer
- Build config: Release, self-contained, single-file

## Artifacts

- win-x64/apsMcp.StdioServer.exe (77569919 bytes)
- osx-x64/apsMcp.StdioServer (76665232 bytes)
- apsMcp.StdioServer-win-x64.zip
- apsMcp.StdioServer-osx-x64.zip
- SHA256SUMS.txt

## Build commands used

```bash
dotnet publish src/apsMcp.StdioServer/apsMcp.StdioServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o artifacts/releases/stdio-v2026-03-19/win-x64

dotnet publish src/apsMcp.StdioServer/apsMcp.StdioServer.csproj -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true -o artifacts/releases/stdio-v2026-03-19/osx-x64
```

## Notes

- Windows artifact is distributed as .exe.
- macOS artifact is an executable binary without .exe extension.
- Verify integrity with: shasum -a 256 win-x64/apsMcp.StdioServer.exe osx-x64/apsMcp.StdioServer apsMcp.StdioServer-win-x64.zip apsMcp.StdioServer-osx-x64.zip
