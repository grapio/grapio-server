using System.Globalization;

namespace Grapio.Server.Tests;

public class TypeDetectorTests
{
    [Fact]
    public void Detect_should_detect_an_integer()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("1");
        Assert.Equal(OpenFeatureType.Integer, type);
    }
    
    [Fact]
    public void Detect_should_detect_a_double()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("123.2568");
        Assert.Equal(OpenFeatureType.Double, type);
    }
    
    [Fact]
    public void Detect_should_detect_a_true_bool()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("true");
        Assert.Equal(OpenFeatureType.Boolean, type);
    }
    
    [Fact]
    public void Detect_should_detect_a_false_bool()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("false");
        Assert.Equal(OpenFeatureType.Boolean, type);
    }
    
    [Fact]
    public void Detect_should_detect_a_string()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("hello, world");
        Assert.Equal(OpenFeatureType.String, type);
    }
    
    [Fact]
    public void Detect_should_detect_a_json_object_structure()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("{}");
        Assert.Equal(OpenFeatureType.Structured, type);
    }
    
    [Fact]
    public void Detect_should_detect_a_json_array_structure()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("[]");
        Assert.Equal(OpenFeatureType.Structured, type);
    }
    
    [Fact]
    public void Detect_should_detect_an_xml_structure()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("<Document></Document>");
        Assert.Equal(OpenFeatureType.Structured, type);
    }
    
    [Fact]
    public void Detect_should_detect_a_yaml_structure()
    {
        TypeDetector.Instance.Culture = CultureInfo.InvariantCulture;
        var type = TypeDetector.Instance.Detect("---");
        Assert.Equal(OpenFeatureType.Structured, type);
    }
}