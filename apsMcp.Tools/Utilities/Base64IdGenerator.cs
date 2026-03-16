using System;
using System.Text;

namespace apsMcp.Tools.Utilities
{
    /// <summary>
    /// Utility class for generating base64-encoded IDs required for Autodesk APS data exchange operations.
    /// These IDs follow specific patterns that concatenate hub, project, folder, and item information
    /// before base64 encoding for API compatibility.
    /// </summary>
    public static class Base64IdGenerator
    {
        /// <summary>
        /// Generates a base64-encoded source file ID for data exchange operations.
        /// Pattern: item~{hubId}~{projectId}~{folderId}~{itemId}
        /// </summary>
        /// <param name="hubId">The hub identifier (e.g., "b.768cae14-76b3-4531-9030-25212dab4e48")</param>
        /// <param name="projectId">The project identifier (e.g., "b.22a5dc47-ca19-4231-9f3e-157ebd4842ff")</param>
        /// <param name="folderId">The folder URN (e.g., "urn:adsk.wipprod:fs.folder:co.N7mdSNSHTw6O5ucnjXlJ2Q")</param>
        /// <param name="itemId">The item/lineage URN (e.g., "urn:adsk.wipprod:dm.lineage:BTbwzunJQQSL0MPllXMT6A")</param>
        /// <returns>Base64-encoded source file ID for use in CreateExchange mutations</returns>
        public static string GenerateSourceFileId(string hubId, string projectId, string folderId, string itemId)
        {
            var concatenatedString = $"item~{hubId}~{projectId}~{folderId}~{itemId}";
            var encodedBytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(concatenatedString));
            return encodedBytes;
        }
        
        /// <summary>
        /// Generates a base64-encoded target folder ID for data exchange operations.
        /// Pattern: fold~{hubId}~{projectId}~{folderId}
        /// </summary>
        /// <param name="hubId">The hub identifier (e.g., "b.768cae14-76b3-4531-9030-25212dab4e48")</param>
        /// <param name="projectId">The project identifier (e.g., "b.22a5dc47-ca19-4231-9f3e-157ebd4842ff")</param>
        /// <param name="folderId">The folder URN (e.g., "urn:adsk.wipprod:fs.folder:co.N7mdSNSHTw6O5ucnjXlJ2Q")</param>
        /// <returns>Base64-encoded target folder ID for use in CreateExchange mutations</returns>
        public static string GenerateTargetFolderId(string hubId, string projectId, string folderId)
        {
            var concatenatedString = $"fold~{hubId}~{projectId}~{folderId}";
            var encodedBytes = Convert.ToBase64String(Encoding.UTF8.GetBytes(concatenatedString));
            return encodedBytes;
        }
        
        /// <summary>
        /// Generates a meaningful exchange name from a filter string.
        /// If no filter is provided, returns a default name.
        /// </summary>
        /// <param name="filter">The filter string (e.g., "(category=='Windows')")</param>
        /// <returns>A descriptive name for the exchange operation</returns>
        public static string GenerateExchangeName(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
            {
                return "AEC Exchange - No Filter";
            }
            
            var cleanFilter = filter.Replace("(", "").Replace(")", "").Replace("'", "").Trim();
            return $"AEC Filter based Exchange - {cleanFilter}";
        }
    }
}