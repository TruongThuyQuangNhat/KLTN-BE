using Microsoft.AspNetCore.Http;

namespace ManageUser.Helpers
{
    public interface IFileValidator
    {
        bool IsValid(IFormFile file);
    }
}
