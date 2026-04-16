using Microsoft.AspNetCore.Mvc;

namespace SuttorLibrary.Core.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, IFormFile coverPic);
        Task<FileContentResult> DownloadFileAsync(string fileName, string pathToFile, Guid bookId, string userId);
        Task<bool> DeleteFileAsync(string fileName);

        bool IsValidFileExtension(string fileName);
        bool IsValidPhotoExtension(string photoName);
        bool IsValidFileSize(long fileSizeBytes);
    }
}
