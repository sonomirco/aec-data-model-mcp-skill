using Newtonsoft.Json;

namespace apsMcp.Tools.Models;

public class CachedFileInfo
{
    [JsonProperty("properties")]
    public PropertyResults Properties { get; set; } = new();

    /// <summary>
    /// Filters properties to keep only those starting with "Project" and having non-null values
    /// </summary>
    public void FilterProjectProperties()
    {
        if (Properties?.Results != null)
        {
            Properties.Results = Properties.Results
                .Where(prop => !string.IsNullOrWhiteSpace(prop.Name) && 
                              prop.Name.StartsWith("Project", StringComparison.OrdinalIgnoreCase) &&
                              prop.Value != null &&
                              !string.IsNullOrWhiteSpace(prop.Value.ToString()))
                .ToList();
        }
    }
}