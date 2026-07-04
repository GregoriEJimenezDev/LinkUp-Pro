using System.Collections.Generic;
using System.Threading.Tasks;

namespace LinkUpPro.Core.Application.Interfaces.IServices
{
    public interface IGenericService<SaveViewModel, ViewModel, Entity>
        where SaveViewModel : class
        where ViewModel : class
        where Entity : class
    {
        Task<Services.ServiceResult> AddAsync(SaveViewModel vm);
        Task<Services.ServiceResult> UpdateAsync(SaveViewModel vm, int id);
        Task<Services.ServiceResult> DeleteAsync(int id);
        Task<SaveViewModel?> GetByIdSaveViewModelAsync(int id);
        Task<List<ViewModel>> GetAllViewModelAsync();
    }
}
