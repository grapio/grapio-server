using System.Globalization;

namespace Grapio.Server;

internal enum OpenFeatureType
{
    None,
    Boolean,
    Integer,
    Double,
    String,
    Structured
}

internal class TypeDetector
{
    private readonly IList<Func<string, OpenFeatureType>> _detectors = [];

    public static TypeDetector Instance { get; } = new();
    public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

    private TypeDetector()
    {
        AddDetector(value => bool.TryParse(value, out _) ? OpenFeatureType.Boolean : OpenFeatureType.None);
        AddDetector(value => int.TryParse(value, NumberStyles.Integer, Instance.Culture, out _) ? OpenFeatureType.Integer : OpenFeatureType.None);
        AddDetector(value => double.TryParse(value, NumberStyles.Number, Instance.Culture, out _) ? OpenFeatureType.Double : OpenFeatureType.None);
        AddDetector(value =>
        {
            var isJson = value.Trim().StartsWith('{') && value.EndsWith('}') || 
                         value.StartsWith('[') && value.EndsWith(']'); 
            return isJson ? OpenFeatureType.Structured : OpenFeatureType.None;
        });
        AddDetector(value =>
        {
            var isXml = value.Trim().StartsWith('<') && value.EndsWith('>'); 
            return isXml ? OpenFeatureType.Structured : OpenFeatureType.None;
        });
        AddDetector(value =>
        {
            var isYaml = value.Trim().StartsWith("---"); 
            return isYaml ? OpenFeatureType.Structured : OpenFeatureType.None;
        });
        AddDetector(_ => OpenFeatureType.String);
    }
    
    public OpenFeatureType Detect(string value)
    {
        foreach (var detector in _detectors)
        {
            var type = detector(value);
            if (type == OpenFeatureType.None)
                continue;
            return type;
        }

        throw new NotSupportedException($"Type could not be detected for {value}");
    }

    private void AddDetector(Func<string, OpenFeatureType> detector) => _detectors.Add(detector);
}