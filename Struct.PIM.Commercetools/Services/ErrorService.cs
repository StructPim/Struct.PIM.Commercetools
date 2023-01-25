namespace Struct.PIM.Commercetools.Services;

public class ErrorService : IErrorService
{
    private readonly List<string> _errors = new();

    public bool HasErrors()
    {
        return _errors.Any();
    }

    public void AddError(string message)
    {
        _errors.Add(message);
    }

    public List<string> GetErrors()
    {
        return _errors;
    }

    public void Clear()
    {
        _errors.Clear();
    }
}

public interface IErrorService
{
    bool HasErrors();

    void AddError(string message);
    List<string> GetErrors();

    void Clear();
}