using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

[AttributeUsage(AttributeTargets.Class)]
public class MyCustomJsonConverterAttribute : Attribute { }



public class CustomConverter<T> : JsonConverter<T>
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        var instance = Activator.CreateInstance<T>();

        foreach (var prop in typeof(T).GetProperties())
        {
            if (!root.TryGetProperty(prop.Name, out var jsonProp))
                continue;

            var value = JsonSerializer.Deserialize(jsonProp.GetRawText(), prop.PropertyType, options);
            prop.SetValue(instance, value);
        }

        return instance;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("type", typeof(T).Name);
        writer.WriteString("assembly", typeof(T).Assembly.FullName);

        foreach (var prop in typeof(T).GetProperties())
        {
            var propValue = prop.GetValue(value);
            if (propValue != null)
            {
                writer.WritePropertyName(prop.Name);
                JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
            }
        }

        writer.WriteEndObject();
    }
}
public class MyCustomConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.GetCustomAttribute<MyCustomJsonConverterAttribute>() != null;
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var converterType = typeof(CustomConverter<>).MakeGenericType(typeToConvert);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}


[MyCustomJsonConverter]
public class MyModel
{
    public string? Name { get; set; }
    public int Age { get; set; }
}



public class Program
{
    public static void Main(string[] args)
    {

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        options.Converters.Add(new MyCustomConverterFactory());

        var model = new MyModel { Name = "Gio", Age = 25 };

        var json = JsonSerializer.Serialize(model, options);
        Console.WriteLine(json);

        // Output will include type + assembly + all properties

    }
}


