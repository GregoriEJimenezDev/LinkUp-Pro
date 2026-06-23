using LinkUpPro.Core.Application.Interfaces.IServices;
using LinkUpPro.Core.Application.ViewModel.Save;
using LinkUpPro.Core.Domain.Entities;
using LinkUpPro.Core.Domain.Interfaces;

namespace LinkUpPro.Core.Application.Interfaces.Services
{
    public class CommentService(ICommentRepository commentRepo) : ICommentService
    {
        private readonly ICommentRepository _commentRepo = commentRepo;

        public async Task<ServiceResult> AddAsync(SaveCommentViewModel vm, string userId)
        {
            var comment = new Comment
            {
                Content = vm.Content,
                PostId = vm.PostId,
                UserId = userId,
                ParentCommentId = vm.ParentCommentId,
                CreatedAt = DateTime.UtcNow
            };

            await _commentRepo.AddAsync(comment);
            return ServiceResult.Success();
        }
        public async Task<ServiceResult> DeleteAsync(int commentId, string userId)
        {
            var comment = await _commentRepo.GetByIdAsync(commentId);
            if (comment == null)
                return ServiceResult.Failure("Comentario no encontrado.");

            if (comment.UserId != userId)
                return ServiceResult.Failure("No estás autorizado para eliminar este comentario.");

            await _commentRepo.DeleteAsync(comment);
            return ServiceResult.Success();
        }
        public async Task<ServiceResult> UpdateAsync(SaveCommentViewModel vm, string userId)
        {
            var comment = await _commentRepo.GetByIdAsync(vm.Id);
            if (comment == null)
                return ServiceResult.Failure("Comentario no encontrado.");

            if (comment.UserId != userId)
                return ServiceResult.Failure("No estás autorizado para actualizar este comentario.");

            comment.Content = vm.Content;
            comment.UpdatedAt = DateTime.UtcNow;

            await _commentRepo.UpdateAsync(comment);

            return ServiceResult.Success();
        }
    }
}
