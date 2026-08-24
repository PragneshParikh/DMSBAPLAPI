using System.ComponentModel.DataAnnotations;

namespace DMS_BAPL_Utils.ViewModels
{
    public class LocationLoginRequestDto
    {
        [Required]
        public string LocationLoginId { get; set; } = null!;

        [Required]
        public string Password { get; set; } = null!;
        public string? LocationCode { get; set; }
    }
}