using CityLink_SCP.Models;
using System.Xml.Serialization;
using CityLink_SCP.Database;
using CityLink_SCP.DbModels;
using CityLink_SCP.Common;
using System.Xml.Linq;
using CityLink_SCP.PageModels;
using System.Collections.ObjectModel;

namespace CityLink_SCP.Services;

public class XmlConfigService
{
    private readonly CityLinksContext _db;

    public XmlConfigService(CityLinksContext db)
    {
        _db = db;
    }

    // Deserialize active XML for a given ViewModel type
    public T? GetActive<T>()
    {
        var typeName = DbInitialiser.GetFriendlyName(typeof(T));
        var record = _db.XML_Configurations
            .Where(r => r.Type == typeName && r.IsActive)
            .OrderByDescending(r => r.UploadedAt)
            .FirstOrDefault();

        if (record == null) return Activator.CreateInstance<T>();

        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(record.XmlContent);
        return (T?)serializer.Deserialize(reader);
    }
    public object? GetActive(Type type)
    {
        var typeName = DbInitialiser.GetFriendlyName(type);
        var record = _db.XML_Configurations
            .Where(r => r.Type == typeName && r.IsActive)
            .OrderByDescending(r => r.UploadedAt)
            .FirstOrDefault();

        if (record == null) return null;

        var serializer = new XmlSerializer(type);
        using var reader = new StringReader(record.XmlContent);
        return serializer.Deserialize(reader);
    }

    // Get the full deserialized ViewModel from the db by Id
    public object? GetXmlViewModel(int id)
    {
        var record = _db.XML_Configurations.Where(r => r.Id == id).FirstOrDefault();
        if (record == null) return null;

        Type? type = GetViewModelType(record.Type);
        if (type == null) return null;

        var serializer = new XmlSerializer(type);
        using var reader = new StringReader(record.XmlContent);
        return serializer.Deserialize(reader);
    }

    // Get raw pretty-printed XML by Id (for the editor)
    public string? GetXmlContentById(int id)
    {
        var record = _db.XML_Configurations.Where(r => r.Id == id).FirstOrDefault();
        if (record == null) return null;
        return XDocument.Parse(record.XmlContent).ToString();
    }

    // Convert a ViewModel instance to XML string for saving/updating
    public string ToXml<T>(T viewModel)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var writer = new StringWriter();
        serializer.Serialize(writer, viewModel);
        return writer.ToString();
    }

    public T? ToViewModel<T>(string xml)
    {
        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(xml);
        return (T?)serializer.Deserialize(reader);
    }

    public XmlConfig FromViewModel<T>(T viewModel)
    {
        return new XmlConfig
        {
            Type = DbInitialiser.GetFriendlyName(typeof(T)),
            XmlContent = ToXml(viewModel),
            UploadedAt = DateTime.UtcNow,
            IsActive = true,
            Label = ""
        };
    }

    // Save new version; deactivate all previous for that type
    public async Task SaveNewVersionAsync(ApplicationStaff staff, XmlConfigDto xmlConfig)
    {
        // Deactivate existing active records for this type
        var existing = _db.XML_Configurations.Where(r => r.Type == xmlConfig.Type && r.IsActive);
        foreach (var r in existing)
        {
            r.IsActive = false;
        }

        // Insert new active record
        _db.XML_Configurations.Add(new XmlConfig
        {
            Type = xmlConfig.Type,
            XmlContent = xmlConfig.XmlContent,
            UploadedAt = DateTime.UtcNow,
            IsActive = true,
            Label = xmlConfig.Label ?? string.Empty,
            Staff = staff
        });

        await _db.SaveChangesAsync();
    }

    // Roll back / activate a specific version by Id
    public async Task<bool> ActivateVersionAsync(int recordId)
    {
        var target = await _db.XML_Configurations.FindAsync(recordId);
        if (target == null) return false;

        var existing = _db.XML_Configurations.Where(r => r.Type == target.Type && r.IsActive);
        foreach (var r in existing)
        {
            r.IsActive = false;
        }

        target.IsActive = true;
        await _db.SaveChangesAsync();
        return true;
    }

    // Validate XML against a ViewModel type by attempting deserialization
    public (bool valid, string? error) Validate(string typeName, string xmlContent)
    {
        try
        {
            var vmType = GetViewModelType(typeName);
            if (vmType == null) return (false, "Unknown ViewModel type.");
            var serializer = new XmlSerializer(vmType);
            using var reader = new StringReader(xmlContent);
            serializer.Deserialize(reader);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.InnerException?.Message ?? ex.Message);
        }
    }

    // Generate an empty/template XML for a given ViewModel type
    public string GenerateTemplate(string typeName)
    {
        var vmType = GetViewModelType(typeName);
        if (vmType == null) return string.Empty;
        var xml = XmlTypeSerializer.ToXml(vmType);
        return XDocument.Parse(xml).ToString(); // pretty-print
    }

	public static string GetFriendlyName(Type type)
	{
		if (type.IsGenericType)
		{
			// Get the name without the `1 arity suffix
			string name = type.Name.Split('`')[0];
			// Get the friendly names of the generic arguments
			var args = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyName));
			return $"{name}<{args}>";
		}
		return type.Name;
	}

	// -----------------------------------------------------------------------
	// Central type registry — add new ViewModel types here as the site grows
	// -----------------------------------------------------------------------

	public static Type? GetViewModelType(string typeName) => typeName switch
    {
        "AnnouncementsViewModel" => typeof(AnnouncementsViewModel),
        "FAQViewModel"           => typeof(FAQViewModel),
        "EventsViewModel"        => typeof(EventsViewModel),
        "ServicesViewModel"      => typeof(ServicesViewModel),
        "FooterModel"            => typeof(FooterModel),
        // Legacy entries kept for existing DB records
        "IndexViewModel"         => typeof(IndexViewModel),
        "List<FAQViewModel>"     => typeof(List<FAQViewModel>),
        "List<CardViewModel>"    => typeof(List<CardViewModel>),
		_ => null
    };

    // Human-readable labels shown to admins in the editor dropdown and table.
    private static readonly IReadOnlyDictionary<string, string> TypeLabels =
        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
        {
            ["AnnouncementsViewModel"] = "Announcements",
            ["FAQViewModel"]           = "FAQ Section",
            ["EventsViewModel"]        = "Events Carousel",
            ["ServicesViewModel"]      = "Services Carousel",
            ["FooterModel"]            = "Site Footer",
        });

    /// <summary>Returns the human-readable label for a stored type name, falling back to the raw name.</summary>
    public static string GetFriendlyLabel(string typeName) =>
        TypeLabels.TryGetValue(typeName, out var label) ? label : typeName;

    /// <summary>Returns the editable config types for the admin UI dropdown.</summary>
    public static List<XmlConfigTypeOption> GetAvailableTypes() =>
        new()
        {
            new("AnnouncementsViewModel", "Announcements"),
            new("FAQViewModel",           "FAQ Section"),
            new("EventsViewModel",        "Events Carousel"),
            new("ServicesViewModel",      "Services Carousel"),
            new("FooterModel",            "Site Footer"),
        };
}