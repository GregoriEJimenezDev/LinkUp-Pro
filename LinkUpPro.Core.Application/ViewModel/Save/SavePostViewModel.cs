using LinkUpPro.Core.Domain.Enum;
using System.ComponentModel.DataAnnotations;
using LinkUpPro.Core.Application.Validation;

namespace LinkUpPro.Core.Application.ViewModel.Save
{
    public class SavePostViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El contenido es obligatorio.")]
        public string Content { get; set; } = string.Empty;
        [Required(ErrorMessage = "Selecciona uno: tipo de medio o video.")]
        public MediaType MediaType { get; set; }
        
        [MaxFileSize(5 * 1024 * 1024)]
        public byte[]? ImageFile { get; set; }
        public string? ImageFileName { get; set; }
        
        public string? VideoUrl { get; set; }
        
        [Required(ErrorMessage = "Debes seleccionar la privacidad de la publicación.")]
        public PostPrivacy Privacy { get; set; } = PostPrivacy.FriendsOnly;
        public bool AllowComments { get; set; } = true;
    }
}
