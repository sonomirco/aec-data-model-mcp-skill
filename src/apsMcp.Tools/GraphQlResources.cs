using System;
using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;

namespace apsMcp.Tools;

public static class GraphQlResources
{

  [McpServerResourceType]
  public static class ClientDocumentation
  {
      [McpServerResource(UriTemplate = "prompt://documentation", Name = "Prompt Documentation", MimeType = "text/markdown")]
      [Description("External prompt documentation file for APS GraphQL MCP tools.")]
      public static string PromptDocumentation()
      {
          var assembly = Assembly.GetExecutingAssembly();
          var resourceName = "apsMcp.Tools.prompt.md";

          using var stream = assembly.GetManifestResourceStream(resourceName);
          if (stream == null)
          {
              throw new InvalidOperationException($"Embedded resource '{resourceName}' not found in assembly.");
          }

          using var reader = new StreamReader(stream);
          return reader.ReadToEnd();
      }
  }
}
