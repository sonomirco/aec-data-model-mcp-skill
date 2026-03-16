using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace apsMcp.Tools.Models;

public class CachedProject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string DataManagementAPIProjectId { get; set; } = string.Empty;
    
    // Handle the nested alternativeIdentifiers during deserialization
    [JsonProperty("alternativeIdentifiers")]
    private JObject? AlternativeIdentifiers
    {
        set
        {
            if (value != null && value.TryGetValue("dataManagementAPIProjectId", out var projectIdToken))
            {
                DataManagementAPIProjectId = projectIdToken.ToString();
            }
        }
    }
}
