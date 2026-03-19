using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace apsMcp.Tools.Models;

public class CachedElementGroup
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string FileUrn { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string FileVersionUrn { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string ParentFolderId { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string ParentFolderName { get; set; } = string.Empty;
    
    [JsonIgnore]
    public string GrandparentFolderName { get; set; } = string.Empty;
    
    // Handle the nested alternativeIdentifiers during deserialization
    [JsonProperty("alternativeIdentifiers")]
    private JObject? AlternativeIdentifiers
    {
        set
        {
            if (value != null)
            {
                if (value.TryGetValue("fileUrn", out var fileUrnToken))
                {
                    FileUrn = fileUrnToken.ToString();
                }
                if (value.TryGetValue("fileVersionUrn", out var fileVersionUrnToken))
                {
                    FileVersionUrn = fileVersionUrnToken.ToString();
                }
            }
        }
    }
    
    // Handle the nested parentFolder during deserialization
    [JsonProperty("parentFolder")]
    private JObject? ParentFolder
    {
        set
        {
            if (value != null)
            {
                if (value.TryGetValue("id", out var parentFolderIdToken))
                {
                    ParentFolderId = parentFolderIdToken.ToString();
                }
                if (value.TryGetValue("name", out var parentFolderNameToken))
                {
                    ParentFolderName = parentFolderNameToken.ToString();
                }
                
                // Handle nested grandparent folder
                if (value.TryGetValue("parentFolder", out var grandparentToken) && grandparentToken is JObject grandparent)
                {
                    if (grandparent.TryGetValue("name", out var grandparentNameToken))
                    {
                        GrandparentFolderName = grandparentNameToken.ToString();
                    }
                }
            }
        }
    }
}