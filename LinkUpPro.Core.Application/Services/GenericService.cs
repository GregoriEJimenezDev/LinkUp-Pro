using AutoMapper;
using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Domain.Interfaces.IGeneric;
using LinkUpPro.Core.Domain.Interfaces;
using LinkUpPro.Core.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace LinkUpPro.Core.Application.Services
{
    public class GenericService<SaveViewModel, ViewModel, Entity>(
        IGenericRepository<Entity> repository,
        IMapper mapper,
        IUnitOfWork unitOfWork) : IGenericService<SaveViewModel, ViewModel, Entity>
        where SaveViewModel : class
        where ViewModel : class
        where Entity : class
    {
        protected readonly IGenericRepository<Entity> _repository = repository;
        protected readonly IMapper _mapper = mapper;
        protected readonly IUnitOfWork _unitOfWork = unitOfWork;

        public virtual async Task<ServiceResult> AddAsync(SaveViewModel vm)
        {
            try
            {
                var entity = _mapper.Map<Entity>(vm);
                await _repository.AddAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public virtual async Task<ServiceResult> UpdateAsync(SaveViewModel vm, int id)
        {
            try
            {
                var entity = _mapper.Map<Entity>(vm);
                await _repository.UpdateAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public virtual async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                    return ServiceResult.Failure("Entidad no encontrada.");
                
                await _repository.DeleteAsync(entity);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                return ServiceResult.Failure(ex.Message);
            }
        }

        public virtual async Task<SaveViewModel?> GetByIdSaveViewModelAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            if (entity == null) return null;
            return _mapper.Map<SaveViewModel>(entity);
        }

        public virtual async Task<List<ViewModel>> GetAllViewModelAsync()
        {
            var entities = await _repository.GetAllAsync();
            return _mapper.Map<List<ViewModel>>(entities);
        }
    }
}
