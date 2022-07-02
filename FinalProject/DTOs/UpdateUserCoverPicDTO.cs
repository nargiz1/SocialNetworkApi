using Microsoft.AspNetCore.Http;

namespace FinalProject.DTOs
{
    public class UpdateUserCoverPicDTO
    {
        public IFormFile CoverPicFile { get; set; }
    }
}
