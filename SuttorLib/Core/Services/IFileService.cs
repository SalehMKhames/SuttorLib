using Microsoft.AspNetCore.Mvc;

namespace SuttorLibrary.Core.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, IFormFile coverPic);
        Task<FileContentResult> DownloadFileAsync(string fileName, string pathToFile, string bookId, string userId);
        Task<string> UploadUserPicAsync(IFormFile userPic, string username, bool IsAuthor);
        Task<bool> DeleteFileAsync(string fileName);
        Task<IFormFile?> GetPictureAsync(string pathToPicture);
        Task<string> UploadCategoryIcon(IFormFile icon, string category);
        public string ResolveStoragePath(string configuredPath);
        bool IsValidFileExtension(string fileName);
        bool IsValidPhotoExtension(string photoName);
        bool IsValidFileSize(long fileSizeBytes);
    }
}
