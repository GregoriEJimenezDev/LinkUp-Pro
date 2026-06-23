using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Save
{
    public class SaveCommentViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El comentario no puede estar vacío.")]
        public string Content { get; set; } = string.Empty;
        [Required]
        public int PostId { get; set; }
        public int? ParentCommentId { get; set; }
    }
}
