namespace Api.JsonNamingPolicy;
public class CamelCaseNamingPolicy : System.Text.Json.JsonNamingPolicy
{
    public static CamelCaseNamingPolicy Instance { get; } = new CamelCaseNamingPolicy();

    private CamelCaseNamingPolicy() { }

    public override string ConvertName(string name)
    {
        return string.Concat(name.Split('_')
            .Select((word, index) => index == 0 
                ? word.ToLower() 
                : char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }
}