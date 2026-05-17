using CloudinaryDotNet.Actions;

namespace API.Interfaces;

public interface IPhotoService
{
    Task<ImageUploadResult> UploadAvatarAsync(IFormFile file);
    Task<DeletionResult> DeletePhotoAsync(string publicId);
}
