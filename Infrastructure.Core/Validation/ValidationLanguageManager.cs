using System.Collections.Concurrent;
using System.Globalization;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Validation;

public class KeyValidationLanguageManager : ValidationLanguageManager
{
    public override string GetString(string key, CultureInfo culture = null)
    {
        var value = _languages.GetOrAdd(key, k => KeyLanguage.GetTranslation(key));

        return value ?? string.Empty;
    }
}

public class ValidationLanguageManager : FluentValidation.Resources.LanguageManager
{
    protected readonly ConcurrentDictionary<string, string> _languages = new ConcurrentDictionary<string, string>();

    public ValidationLanguageManager()
    {
        Enabled = true;

        //AddTranslation("en", "NotNullValidator", "'{PropertyName}' is required.");
        //AddTranslation("en-US", "NotNullValidator", "'{PropertyName}' is required.");
        //AddTranslation("en-GB", "NotNullValidator", "'{PropertyName}' is required.");
    }

    public override string GetString(string key, CultureInfo culture = null)
    {
        string value = null;

        if (Enabled)
        {
            culture = culture ?? Culture ?? CultureInfo.CurrentUICulture;

            var currentCultureKey = culture.Name + ":" + key;
            value = _languages.GetOrAdd(currentCultureKey, k => GetTranslation(culture.Name, key));

            // If the value couldn't be found, try the parent culture.
            var currentCulture = culture;
            while (value == null && currentCulture.Parent != CultureInfo.InvariantCulture)
            {
                currentCulture = currentCulture.Parent;
                var parentCultureKey = currentCulture.Name + ":" + key;
                value = _languages.GetOrAdd(parentCultureKey, k => GetTranslation(currentCulture.Name, key));
            }
        }

        if (value == null)
        {
            value = _languages.GetOrAdd("Key:" + key, k => KeyLanguage.GetTranslation(key));
        }

        return value ?? string.Empty;
    }

    public new void AddTranslation(string language, string key, string message)
    {
        if (string.IsNullOrEmpty(language)) throw new ArgumentNullException(nameof(language));
        if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
        if (string.IsNullOrEmpty(message)) throw new ArgumentNullException(nameof(message));

        _languages[language + ":" + key] = message;
    }

    private static string GetTranslation(string culture, string key)
    {
        return culture switch
        {
            EnglishLanguage.AmericanCulture => EnglishLanguage.GetTranslation(key),
            EnglishLanguage.BritishCulture => EnglishLanguage.GetTranslation(key),
            EnglishLanguage.Culture => EnglishLanguage.GetTranslation(key),
            GermanLanguage.Culture => GermanLanguage.GetTranslation(key),
            _ => KeyLanguage.GetTranslation(key),
        };
    }

    internal class KeyLanguage
    {
        public static string GetTranslation(string key) => key switch
        {
            "EmailValidator" => "EmailValidator",
            "GreaterThanOrEqualValidator" => "GreaterThanOrEqualValidator",
            "GreaterThanValidator" => "GreaterThanValidator",
            "LengthValidator" => "LengthValidator",
            "MinimumLengthValidator" => "MinimumLengthValidator",
            "MaximumLengthValidator" => "MaximumLengthValidator",
            "LessThanOrEqualValidator" => "LessThanOrEqualValidator",
            "LessThanValidator" => "LessThanValidator",
            "NotEmptyValidator" => "NotEmptyValidator",
            "NotEqualValidator" => "NotEqualValidator",
            "NotNullValidator" => "NotNullValidator",
            "PredicateValidator" => "PredicateValidator",
            "AsyncPredicateValidator" => "AsyncPredicateValidator",
            "RegularExpressionValidator" => "RegularExpressionValidator",
            "EqualValidator" => "EqualValidator",
            "ExactLengthValidator" => "ExactLengthValidator",
            "InclusiveBetweenValidator" => "InclusiveBetweenValidator",
            "ExclusiveBetweenValidator" => "ExclusiveBetweenValidator",
            "CreditCardValidator" => "CreditCardValidator",
            "ScalePrecisionValidator" => "ScalePrecisionValidator",
            "EmptyValidator" => "EmptyValidator",
            "NullValidator" => "NullValidator",
            "EnumValidator" => "EnumValidator",
            "Length_Simple" => "Length_Simple",
            "MinimumLength_Simple" => "MinimumLength_Simple",
            "MaximumLength_Simple" => "MaximumLength_Simple",
            "ExactLength_Simple" => "ExactLength_Simple",
            "InclusiveBetween_Simple" => "InclusiveBetween_Simple",
            _ => null,
        };
    }

    internal class EnglishLanguage
    {
        public const string Culture = "en";
        public const string AmericanCulture = "en-US";
        public const string BritishCulture = "en-GB";

        public static string GetTranslation(string key) => key switch
        {
            "EmailValidator" => "Invalid email address.",
            "GreaterThanOrEqualValidator" => "Must be greater than or equal to '{ComparisonValue}'.",
            "GreaterThanValidator" => "Must be greater than '{ComparisonValue}'.",
            "LengthValidator" => "Must be between {MinLength} and {MaxLength} characters. You entered {TotalLength} characters.",
            "MinimumLengthValidator" => "The length must be at least {MinLength} characters. You entered {TotalLength} characters.",
            "MaximumLengthValidator" => "The length must be {MaxLength} characters or fewer. You entered {TotalLength} characters.",
            "LessThanOrEqualValidator" => "Must be less than or equal to '{ComparisonValue}'.",
            "LessThanValidator" => "Must be less than '{ComparisonValue}'.",
            "NotEmptyValidator" => "Can not be empty.",
            "NotEqualValidator" => "Can not be equal to '{ComparisonValue}'.",
            "NotNullValidator" => "Can not be empty.",
            "PredicateValidator" => "The specified condition was not met.",
            "AsyncPredicateValidator" => "The specified condition was not met.",
            "RegularExpressionValidator" => "Is not in the correct format.",
            "EqualValidator" => "Must be equal to '{ComparisonValue}'.",
            "ExactLengthValidator" => "Must be {MaxLength} characters in length. You entered {TotalLength} characters.",
            "InclusiveBetweenValidator" => "Must be between {From} and {To}. You entered {PropertyValue}.",
            "ExclusiveBetweenValidator" => "Must be between {From} and {To} (exclusive). You entered {PropertyValue}.",
            "CreditCardValidator" => "Is not a valid credit card number.",
            "ScalePrecisionValidator" => "Must not be more than {ExpectedPrecision} digits in total, with allowance for {ExpectedScale} decimals. {Digits} digits and {ActualScale} decimals were found.",
            "EmptyValidator" => "Must be empty.",
            "NullValidator" => "Must be empty.",
            "EnumValidator" => "Has a range of values which does not include '{PropertyValue}'.",
            // Additional fallback messages used by clientside validation integration.
            "Length_Simple" => "Must be between {MinLength} and {MaxLength} characters.",
            "MinimumLength_Simple" => "The length must be at least {MinLength} characters.",
            "MaximumLength_Simple" => "The length must be {MaxLength} characters or fewer.",
            "ExactLength_Simple" => "Must be {MaxLength} characters in length.",
            "InclusiveBetween_Simple" => "Must be between {From} and {To}.",
            _ => null,
        };
    }

    internal class GermanLanguage
    {
        public const string Culture = "de";

        public static string GetTranslation(string key) => key switch
        {
            "EmailValidator" => "Keine gültige E-Mail-Adresse.",
            "GreaterThanOrEqualValidator" => "Muss grösser oder gleich '{ComparisonValue}' sein.",
            "GreaterThanValidator" => "Muss grösser sein als '{ComparisonValue}'.",
            "LengthValidator" => "Muss zwischen {MinLength} und {MaxLength} Zeichen liegen. Es wurden {TotalLength} Zeichen eingetragen.",
            "MinimumLengthValidator" => "Die Länge muss größer oder gleich {MinLength} sein. Sie haben {TotalLength} Zeichen eingegeben.",
            "MaximumLengthValidator" => "Die Länge muss kleiner oder gleich {MaxLength} sein. Sie haben {TotalLength} Zeichen eingegeben.",
            "LessThanOrEqualValidator" => "Muss kleiner oder gleich '{ComparisonValue}' sein.",
            "LessThanValidator" => "Muss kleiner sein als '{ComparisonValue}'.",
            "NotEmptyValidator" => "Darf nicht leer sein.",
            "NotEqualValidator" => "Darf nicht '{ComparisonValue}' sein.",
            "NotNullValidator" => "Darf nicht leer sein.",
            "PredicateValidator" => "Entspricht nicht der festgelegten Bedingung.",
            "AsyncPredicateValidator" => "Entspricht nicht der festgelegten Bedingung.",
            "RegularExpressionValidator" => "Weist ein ungültiges Format auf.",
            "EqualValidator" => "Muss gleich '{ComparisonValue}' sein.",
            "ExactLengthValidator" => "Muss genau {MaxLength} lang sein. Es wurden {TotalLength} eingegeben.",
            "ExclusiveBetweenValidator" => "Muss zwischen {From} und {To} sein (exklusiv). Es wurde {PropertyValue} eingegeben.",
            "InclusiveBetweenValidator" => "Muss zwischen {From} and {To} sein. Es wurde {PropertyValue} eingegeben.",
            "CreditCardValidator" => "Ist keine gültige Kreditkartennummer.",
            "ScalePrecisionValidator" => "Darf insgesamt nicht mehr als {ExpectedPrecision} Ziffern enthalten, mit Berücksichtigung von {ExpectedScale} Dezimalstellen. Es wurden {Digits} Ziffern und {ActualScale} Dezimalstellen gefunden.",
            "EmptyValidator" => "Sollte leer sein.",
            "NullValidator" => "Sollte leer sein.",
            "EnumValidator" => "Hat einen Wertebereich, der '{PropertyValue}' nicht enthält.",
            // Zusätzliche Rückfallmeldungen, die von der Clientseitenvalidierung verwendet werden.
            "Length_Simple" => "Die Länge muss zwischen {MinLength} und {MaxLength} Zeichen liegen.",
            "MinimumLength_Simple" => "Die Länge muss größer oder gleich {MinLength} sein.",
            "MaximumLength_Simple" => "Die Länge muss kleiner oder gleich {MaxLength} sein.",
            "ExactLength_Simple" => "Muss genau {MaxLength} lang sein.",
            "InclusiveBetween_Simple" => "Muss zwischen {From} and {To} sein.",
            _ => null,
        };
    }
}