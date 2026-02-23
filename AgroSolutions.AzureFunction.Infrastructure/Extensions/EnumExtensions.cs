using System.ComponentModel;
using System.Reflection;

namespace AgroSolutions.AzureFunction.Infrastructure.Extensions;

public static class EnumExtensions
{
    extension(Enum @enum)
    {
        public string GetDescription()
        {
            FieldInfo? field = @enum.GetType().GetField(@enum.ToString());
            DescriptionAttribute? attribute = field?.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? @enum.ToString();
        }
    }
}
