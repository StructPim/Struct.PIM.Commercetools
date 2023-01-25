namespace Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

public interface IImportService
{
    bool RollBackOnFailure { get; set; }
    void AddRollBackStep(Func<Task> t);
    Task Execute<T>(Func<Task<T?>> crudFunc, Func<Task>? rollbackFunc);
    Task Execute(Func<Task> crudFunc, Func<Task>? rollbackFunc);
    Task CleanUp();
}