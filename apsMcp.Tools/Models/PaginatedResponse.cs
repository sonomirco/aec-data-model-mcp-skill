namespace apsMcp.Tools.Models;

public class PaginatedResponse<T>
{
    public PaginationInfo Pagination { get; set; } = new();
    public List<T> Results { get; set; } = new();
}