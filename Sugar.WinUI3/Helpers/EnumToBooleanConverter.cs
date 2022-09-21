using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Windows.Storage.Search;

namespace Sugar.WinUI3.Helpers;

public class EnumToBooleanConverter<T> : IValueConverter
{
    public EnumToBooleanConverter()
    {
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            if (!Enum.IsDefined(typeof(T), value))
            {
                throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
            }

            var enumValue = Enum.Parse(typeof(T), enumString);

            return enumValue.Equals(value);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse(typeof(T), enumString);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
}

public class ElementThemeEnumToBooleanConverter : EnumToBooleanConverter<ElementTheme>
{
}

public class FileSortEnumToBooleanConverter : EnumToBooleanConverter<CommonFileQuery>
{
}