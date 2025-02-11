namespace HotelBookingPlatform.Application.Utilities.ErrorMessages;

public class ExceptionsErrorMessages
{
    public const string UserIsUnauthenticated = "User is unauthenticated.";
    public const string InvalidEmailOrPassword = "Invalid Email or Password.";
    public const string UnauthorizedRoleAccess = "The user with Id {0} has no role {1}.";
    public const string UnauthorizedGuestAccess = "The user with Id {0} has no access to guest with Id {1}.";
    public const string UnauthorizedEntityAccess = "The user with Id {0} has no access to Entity '{1}' ({2}).";
    public const string NoRolesForUser = "No Roles For the user with Id {0}";
    public const string EntityWithKeyNotFound = "Entity '{0}' ({1}) was not found.";
    public const string EntityNotFound = "Entity '{0}' was not found.";
    public const string PriceErrorWithId = "Error while checking Room Price, roomId: {0} ";
    public const string PriceError = "Error while checking Room Price.";
    public const string UnavailableRoom = "Room with id: '{0}' is Unavailable from {1} to {2}.";
    public const string InvalidNumberOfGuests = "Requested Rooms cannot accommodate {0} Adults and {1} Children.";
}
