namespace Grapio.Server;

public class GrapioConfiguration
{
    public string ConnectionString { get; set; } = string.Empty;
    public string ControlServiceHost { get; set; } = "*:3280";
    public string ProviderServiceHost { get; set; } = "*:3278";
}
