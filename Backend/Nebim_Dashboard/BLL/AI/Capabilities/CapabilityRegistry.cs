using Entity.DTOs.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BLL.AI.Capabilities;

/// <summary>
/// Tüm capability'leri yöneten registry.
/// Version bazlı capability çözümleme yapar.
/// </summary>
public interface ICapabilityRegistry
{
    /// <summary>
    /// Capability'yi isim ve versiyonla getirir.
    /// </summary>
    ICapability? GetCapability(string name, string? version = null);

    /// <summary>
    /// Tüm aktif capability'leri listeler.
    /// </summary>
    IEnumerable<ICapability> GetAllCapabilities();

    /// <summary>
    /// Belirli bir tier için erişilebilir capability'leri listeler.
    /// </summary>
    IEnumerable<ICapability> GetCapabilitiesForTier(string tier);

    /// <summary>
    /// Kategoriye göre capability'leri listeler.
    /// </summary>
    IEnumerable<ICapability> GetCapabilitiesByCategory(string category);

    /// <summary>
    /// Tüm capability bilgilerini DTO olarak döndürür.
    /// </summary>
    List<CapabilityInfoDto> GetCapabilityInfos();

    /// <summary>
    /// AI prompt'u için capability açıklamalarını döndürür.
    /// </summary>
    string GetCapabilityDescriptionsForPrompt();
}

/// <summary>
/// ICapabilityRegistry implementasyonu.
/// </summary>
public class CapabilityRegistry : ICapabilityRegistry
{
    private readonly Dictionary<string, Dictionary<string, ICapability>> _capabilities = new();
    private readonly ILogger<CapabilityRegistry> _logger;

    private static readonly string[] TierHierarchy = { "Free", "Professional", "Enterprise" };

    public CapabilityRegistry(IServiceProvider serviceProvider, ILogger<CapabilityRegistry> logger)
    {
        _logger = logger;
        RegisterCapabilities(serviceProvider);
    }

    private void RegisterCapabilities(IServiceProvider serviceProvider)
    {
        // Get all registered ICapability implementations
        var capabilities = serviceProvider.GetServices<ICapability>();

        foreach (var capability in capabilities)
        {
            if (!_capabilities.ContainsKey(capability.Name))
            {
                _capabilities[capability.Name] = new Dictionary<string, ICapability>();
            }

            _capabilities[capability.Name][capability.Version] = capability;
            _logger.LogInformation(
                "Registered capability: {Name} v{Version}",
                capability.Name, capability.Version);
        }

        _logger.LogInformation(
            "CapabilityRegistry initialized with {Count} capabilities",
            _capabilities.Count);
    }

    public ICapability? GetCapability(string name, string? version = null)
    {
        if (!_capabilities.TryGetValue(name, out var versions))
        {
            _logger.LogWarning("Capability not found: {Name}", name);
            return null;
        }

        if (string.IsNullOrEmpty(version))
        {
            // Return latest version
            return versions.Values.OrderByDescending(c => c.Version).FirstOrDefault();
        }

        if (versions.TryGetValue(version, out var capability))
        {
            return capability;
        }

        _logger.LogWarning("Capability version not found: {Name} v{Version}", name, version);
        return null;
    }

    public IEnumerable<ICapability> GetAllCapabilities()
    {
        return _capabilities.Values
            .SelectMany(v => v.Values)
            .GroupBy(c => c.Name)
            .Select(g => g.OrderByDescending(c => c.Version).First());
    }

    public IEnumerable<ICapability> GetCapabilitiesForTier(string tier)
    {
        var tierIndex = Array.IndexOf(TierHierarchy, tier);
        if (tierIndex < 0) tierIndex = 0;

        return GetAllCapabilities()
            .Where(c =>
            {
                var requiredTierIndex = Array.IndexOf(TierHierarchy, c.RequiredTier);
                return requiredTierIndex <= tierIndex;
            });
    }

    public IEnumerable<ICapability> GetCapabilitiesByCategory(string category)
    {
        return GetAllCapabilities()
            .Where(c => c.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    public List<CapabilityInfoDto> GetCapabilityInfos()
    {
        return GetAllCapabilities()
            .Select(c => c.GetInfo())
            .OrderBy(c => c.Category)
            .ThenBy(c => c.Name)
            .ToList();
    }

    public string GetCapabilityDescriptionsForPrompt()
    {
        var capabilities = GetAllCapabilities().ToList();
        var lines = new List<string>
        {
            "Available Capabilities:",
            ""
        };

        foreach (var category in capabilities.GroupBy(c => c.Category))
        {
            lines.Add($"## {category.Key}");
            lines.Add("");

            foreach (var cap in category)
            {
                lines.Add($"### {cap.Name} (v{cap.Version})");
                lines.Add($"Description: {cap.Description}");
                lines.Add("Parameters:");
                
                foreach (var param in cap.Parameters)
                {
                    var required = param.IsRequired ? "(required)" : "(optional)";
                    var defaultVal = !string.IsNullOrEmpty(param.DefaultValue) 
                        ? $", default: {param.DefaultValue}" 
                        : "";
                    lines.Add($"  - {param.Name}: {param.Type} {required}{defaultVal} - {param.Description}");
                }

                lines.Add("Example queries:");
                foreach (var example in cap.ExampleQueries)
                {
                    lines.Add($"  - \"{example}\"");
                }
                lines.Add("");
            }
        }

        return string.Join("\n", lines);
    }
}
