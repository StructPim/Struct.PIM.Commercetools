using Struct.PIM.Commercetools.Services.Commercetools.Interfaces;

namespace Struct.PIM.Commercetools.Services.Commercetools;

public class ImportService : IImportService
{
    private readonly IErrorService _errorService;
    private readonly List<Func<Task>>? _rollBacks = new();

    public ImportService(IErrorService errorService)
    {

        _errorService = errorService ?? throw new ArgumentNullException(nameof(errorService));
    }

    public bool RollBackOnFailure { get; set; }

    public void AddRollBackStep(Func<Task> t)
    {
        _rollBacks?.Insert(0, t);
    }


    public async Task Execute<T>(Func<Task<T?>> crudFunc, Func<Task>? rollbackFunc)
    {
        if(rollbackFunc!=null){
            AddRollBackStep(rollbackFunc);
        }
        _errorService.Clear();
        await crudFunc();
        if (_errorService.HasErrors() && RollBackOnFailure)
        {
            await CleanUp();
        }

        if (_errorService.HasErrors())
        {
            throw new Exception(string.Join(",", _errorService.GetErrors()));
        }
    }

    public async Task Execute(Func<Task> crudFunc, Func<Task>? rollbackFunc)
    {
        if(rollbackFunc!=null){
            AddRollBackStep(rollbackFunc);
        }
        _errorService.Clear();
        await crudFunc();
        if (_errorService.HasErrors() && RollBackOnFailure)
        {
            await CleanUp();
        }

        if (_errorService.HasErrors())
        {
            throw new Exception(string.Join(",", _errorService.GetErrors()));
        }
    }

    public async Task CleanUp()
    {
        if (_rollBacks != null)
        {
            foreach (var rollBack in _rollBacks)
            {
                await rollBack();
            }
        }
    }
}