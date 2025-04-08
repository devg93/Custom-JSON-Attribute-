# Custom JSON Attribute Serializer in C#

This project demonstrates how to implement a custom serializer in C# with support for:

- `[MyJsonProperty("custom_name")]` for overriding property names.
- `[MyJsonIgnore]` for excluding properties.
- `MaxDepth` parameter to avoid infinite recursion.
- Optionally: masking fields like passwords.

## Example

```csharp
public class User
{
    [MyJsonProperty("user_name")]
    public string Username { get; set; }

    [MyJsonIgnore]
    public string Password { get; set; }

    public Profile Profile { get; set; }
}
