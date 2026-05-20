namespace CityLink_SCP.Models
{
    public interface IXmlViewModel
    {
        // Returns the ViewComponent name to invoke (e.g. "Footer")
        string PartialName { get; }
    }
}
