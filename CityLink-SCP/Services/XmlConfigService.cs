using CityLink_SCP.Models;
using System.Xml.Serialization;
using CityLink_SCP.Database;
using CityLink_SCP.DbModels;
using CityLink_SCP.Extensions;
using System.Xml.Linq;
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
        var typeName = typeof(T).Name;
        var record = _db.XML_Configurations
            .Where(r => r.Type == typeName && r.IsActive)
            .OrderByDescending(r => r.UploadedAt)
            .FirstOrDefault();

        if (record == null) return Activator.CreateInstance<T>();

        var serializer = new XmlSerializer(typeof(T));
        using var reader = new StringReader(record.XmlContent);
        return (T?)serializer.Deserialize(reader);
    }
    // Get the full viewmodel from the db by Id 
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

    // Get raw XML by Id (for editing/rollback)
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
            Type = typeof(T).Name,
            XmlContent = ToXml(viewModel),
            UploadedAt = DateTime.UtcNow,
            IsActive = true,
            Label = ""
        };
    }

    // Save new version; deactivate all previous for that type
    public async Task SaveNewVersionAsync(Staff staff, XmlConfigDto xmlConfig)
    {
        // Deactivate old
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
            Label = xmlConfig.Label,
            Staff = staff
        });

        await _db.SaveChangesAsync();
    }

    // Roll back to a specific version by Id
    public async Task<bool> ActivateVersionAsync(int recordId)
    {
        var target = await _db.XML_Configurations.FindAsync(recordId);
        if (target == null)
        {
            return false;
        }

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

    // Generate an empty/template XML for a given ViewModel
    public string GenerateTemplate(string typeName)
    {
        var vmType = GetViewModelType(typeName);
        if (vmType == null) return string.Empty;
        var instance = Activator.CreateInstance(vmType);
        var serializer = new XmlSerializer(vmType);
        using var sw = new StringWriter();
        serializer.Serialize(sw, instance);
        return sw.ToString();
    }

    // Central registry — add new ViewModels here
    public static Type? GetViewModelType(string typeName) => typeName switch
    {
        "FooterModel" => typeof(FooterModel),
        "IndexViewModel" => typeof(IndexViewModel),
        _ => null
    };
}