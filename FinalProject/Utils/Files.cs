using Microsoft.AspNetCore.Http;
using System.IO;
using System.Net.Http.Headers;

namespace FinalProject.Utils
{
    public static class Files
    {
        public static bool IsImage(this IFormFile file)
        {
            return file.ContentType.Contains("image/");
        }
        public static bool IsVideo(this IFormFile file)
        {
            return file.ContentType.Contains("video/");
        }
        public static bool IsvalidSize(this IFormFile file, int kb)
        {
            return file.Length / 1024 < kb;
        }
        public static string Upload(this IFormFile image, string folder)
        {
            var file = image;
            var folderName = Path.Combine("Resources", folder);
            var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);
            if (file.Length > 0)
            {
                var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                var fullPath = Path.Combine(pathToSave, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }
                return fileName;
            }
            return "invalid";
        }
        public static void Delete(string folder1, string folder2, string file)
        {
            string filePath = Path.Combine(folder1, folder2, file);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
