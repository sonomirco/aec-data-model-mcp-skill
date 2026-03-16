using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace apsMcp.Tools.Models;

public class CachedHub
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string DataManagementAPIHubId { get; set; } = string.Empty;
    
    // Handle the nested alternativeIdentifiers during deserialization
    [JsonProperty("alternativeIdentifiers")]
    private JObject? AlternativeIdentifiers
    {
        set
        {
            if (value != null && value.TryGetValue("dataManagementAPIHubId", out var hubIdToken))
            {
                DataManagementAPIHubId = hubIdToken.ToString();
            }
        }
    }
}