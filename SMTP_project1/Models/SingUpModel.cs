using Microsoft.Extensions.FileProviders;

namespace SMTP_project1.Models
{
    public class SingUpModel
    {
        public int? id {  get; set; }
        public string? name { get; set; }
        public string? email { get; set; }
        public string? pass { get; set; }
        public IFormFile? img { get; set; }
    }
}
