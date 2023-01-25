using commercetools.Base.Client.Error;

namespace Struct.PIM.Commercetools.Helpers;

public static class ExceptionHelper
{
    public static string ResolveMessage(this Exception exception)
    {
        return exception is BadRequestException badRequestException ? badRequestException.Body : exception.Message;
    }
}