using Microsoft.AspNetCore.Http;

namespace FinalProject.DTOs
{
    public class UpdateUserProfilePicDTO
    {

        public IFormFile ImageFile { get; set; }

    }
}
