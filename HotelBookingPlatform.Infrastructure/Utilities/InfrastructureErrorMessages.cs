namespace HotelBookingPlatform.Infrastructure.Utilities;

public static class InfrastructureErrorMessages
{
    public const string EmptyImageError = "The uploaded image is empty.";
    public const string InvalidImageTypeError = "The uploaded image type is not supported.";
    public const string NullToken = "Null token.";
    public const string InvalidRole = "Invalid role: {0}.";
    public const string SecretKeyIsMissing = "Secret key is missing.";
    public const string JwtSettingsSectionIsMissing = "JwtSettings section is missing.";
    public const string PasswordsDoNotMatch = "Passwords Do Not Match.";
    public const string JwtKeyMissing = "Jwt Key is Missing.";
    public const string JwtIssuerMissing = "Jwt Issuer is Missing.";
    public const string InvalidUser = "Invalid User";
    public const string RolesMissing = "Roles is Missing";
    public const string NoSmailStrategyFound = "No email strategy found for {0}";
    public const string NoPdfGeneratorStrategyFound = "No PDF generation strategy found for {0}";
}
