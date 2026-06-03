namespace CityLink_SCP.Common;

public class QueryParameters
{
    private const int _maxSize = 100;
    private int _size = 10;
    public int Size
    {
        get => _size;
        set => _size = Math.Min(_maxSize, value); // prevents ?size=999999
    }
    public int Page { get; set; } = 1;
    public string SortBy { get; set; } = "Id";

    private string _sortOrder = "asc";
    public string SortOrder
    {
        get => _sortOrder;
        set => _sortOrder = (value == "asc" || value == "desc") ? value : _sortOrder;
    }
}