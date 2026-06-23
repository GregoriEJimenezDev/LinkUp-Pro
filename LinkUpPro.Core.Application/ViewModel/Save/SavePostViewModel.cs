using LinkUpPro.Core.Domain.Enum;
using System.ComponentModel.DataAnnotations;

namespace LinkUpPro.Core.Application.ViewModel.Save
{
    public class SavePostViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El contenido es obligatorio.")]
        public string Content { get; set; } = string.Empty;
        [Required(ErrorMessage = "Selecciona uno: tipo de medio o video.")]
        public MediaType MediaType { get; set; }
        public byte[]? ImageFile { get; set; }
        public string? VideoUrl { get; set; }
    }
}
