using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using SuttorLibrary.Data;
using SuttorLibrary.Models;

namespace SuttorLibrary.Core.Services
{
    public class FileService(
        IConfiguration configuration, 
        ILogger<FileService> logger,
        AppDbContext context,
        IWebHostEnvironment env
        ) : IFileService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<FileService> _logger = logger;
        private readonly AppDbContext _context = context;
        private readonly IWebHostEnvironment _env = env;

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("File deleted: {FileName}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", filePath);
                throw;
            }
        }

        public async Task<FileContentResult> DownloadFileAsync(
            string fileName, 
            string pathToFile, 
            string bookId, 
            string? userId
            )
        {
            try
            {
                if (!File.Exists(pathToFile))
                    throw new FileNotFoundException($"File '{fileName}' not found.");

                // If a user id was provided, try to record the download
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var user = await _context.AppUsers.FindAsync(userId);
                        if (user is not null)
                        {
                            var download = new Download
                            {
                                Id = Guid.NewGuid().ToString(),
                                BookID = bookId,
                                UserID = userId,
                                DownloadedAt = DateTime.UtcNow,
                                IsFinishReading = false
                            };

                            _context.Downloads.Add(download);
                            await _context.SaveChangesAsync();
                            _logger.LogInformation("Recorded download for user {UserId} and book {BookId}", userId, bookId);
                        }
                        else
                        {
                            _logger.LogWarning("Download attempted by non-existing user {UserId} for book {BookId}", userId, bookId);
                        }
                    }
                    catch (Exception dbEx)
                    {
                        // Log but do not block actual file delivery
                        _logger.LogError(dbEx, "Failed to record download for user {UserId} and book {BookId}", userId, bookId);
                    }
                }

                _logger.LogInformation("File download initiated: {FileName}", fileName);
               
                var fileBytes = await File.ReadAllBytesAsync(pathToFile);
                var fileContent = new FileContentResult(fileBytes, "application/octet-stream")
                {
                    FileDownloadName = fileName
                };

                return fileContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, IFormFile coverPic)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            if (!IsValidFileExtension(file.FileName))
                throw new InvalidOperationException($"File extension not allowed. Allowed: {GetAllowedExtensions()}");
            
            if(!IsValidPhotoExtension(coverPic.FileName))
                throw new InvalidOperationException($"File extension not allowed. Allowed: {GetAllowedPictureExtensions()}");

            if (!IsValidFileSize(file.Length))
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {GetMaxFileSize()}MB.");

            try
            {
                var storagePath = _configuration["FileStorage:Path"] ?? "private\\Books";
                storagePath = ResolveStoragePath(storagePath!);
                
                if (!Directory.Exists(storagePath))
                    Directory.CreateDirectory(storagePath);

                var PhotoPath = Path.Combine(storagePath, "Photos");
                if (!Directory.Exists(PhotoPath))
                    Directory.CreateDirectory(PhotoPath);

                var uniqueFileName = file.FileName;
                var uniqueCoverName = coverPic.FileName;

                var filePath = Path.Combine(storagePath, uniqueFileName);

                // Use CreateNew to automatically fail if file already exists
                try
                {
                    await using (var stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                catch (IOException ioEx) when (File.Exists(filePath))
                {
                    _logger.LogWarning(ioEx, "Attempted to create a file that already exists: {FilePath}", filePath);
                    throw new InvalidOperationException("A file with the same name already exists.");
                }

                //Save the cover picture
                if(coverPic is not null && coverPic.Length >0)
                {
                    var coverExtension = Path.GetExtension(uniqueCoverName);
                    uniqueCoverName = $"{Path.GetFileNameWithoutExtension(uniqueFileName)}_cover{(string.IsNullOrEmpty(coverExtension) ? string.Empty : coverExtension)}";
                    var coverPath = Path.Combine(PhotoPath, uniqueCoverName);

                    // Overwrite cover if it exists
                    await using (var coverStream = new FileStream(coverPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous))
                    {
                        await coverPic.CopyToAsync(coverStream);
                    }
                }

                _logger.LogInformation("File uploaded successfully: {FileName} at {FilePath}", file.FileName, filePath);
                return uniqueFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
                throw;
            }
        }

        public async Task<string> UploadUserPicAsync(IFormFile userPic, string username)
        {
            if (userPic is null || userPic.Length == 0)
                throw new ArgumentException("File is empty.");

            if (!IsValidPhotoExtension(userPic.FileName))
                throw new InvalidOperationException($"File extension not allowed. Allowed: {GetAllowedPictureExtensions()}");

            try {
                var storagePath = _configuration["FileStorage:UsersPicsPath"] ?? "private\\Users";
                storagePath = ResolveStoragePath(storagePath!);
                if (!Directory.Exists(storagePath))
                    Directory.CreateDirectory(storagePath);

                var uniquePicName = userPic.FileName;

                if (userPic is not null && userPic.Length > 0)
                {
                    //Renaming the photo to {username_picture with the extension}
                    var coverExtension = Path.GetExtension(uniquePicName);
                    var coverFileName = $"{username}_picture{(string.IsNullOrEmpty(coverExtension) ? string.Empty : coverExtension)}";

                    var coverPath = Path.Combine(storagePath, coverFileName);

                    // Overwrite cover if it exists
                    await using (var coverStream = new FileStream(coverPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous))
                    {
                        await userPic.CopyToAsync(coverStream);
                    }
                }

                _logger.LogInformation("File uploaded successfully: {FileName} at {FilePath}", userPic!.FileName, storagePath);
                return uniquePicName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", userPic.FileName);
                throw;
            }
        }

        //Retrieve the file of the picture from the specified path.
        public async Task<IFormFile?> GetPictureAsync(string pathToPicture) 
        {
            try {
                var picName = Path.GetFileName(pathToPicture);

                if (!File.Exists(pathToPicture))
                    return null;

                // Resolve content type
                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(picName, out var contentType))
                    contentType = "application/octet-stream";

                var fileBytes = await File.ReadAllBytesAsync(pathToPicture);

                // Use a MemoryStream so the returned IFormFile is backed by a stream that stays alive
                var memoryStream = new MemoryStream(fileBytes);

                var formFile = new FormFile(memoryStream, 0, memoryStream.Length, "file", picName)
                {
                    Headers = new HeaderDictionary(),
                    ContentType = contentType
                };

                return formFile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FileName}", pathToPicture);
                throw;
            }
        }

        public bool IsValidFileExtension(string fileName)
        {
            var allowedExtensions = _configuration.GetSection("FileStorage:AllowedExtenstions").Get<List<string>>();
            if (allowedExtensions is null || allowedExtensions.Count == 0)
                return true;

            var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            return allowedExtensions.Any(ext => ext.Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsValidPhotoExtension(string PhotoName)
        {
            var allowedExtensions = _configuration.GetSection("FileStorage:PhotoExtensions").Get<List<string>>();
            if (allowedExtensions is null || allowedExtensions.Count == 0)
                return true;

            var PhotoExt = Path.GetExtension(PhotoName).ToLowerInvariant();
            return allowedExtensions.Any(ext => ext.Equals(PhotoExt, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsValidFileSize(long fileSizeBytes)
        {
            var maxSizeMB = _configuration.GetValue<int>("FileStorage:MaxFileSizeMB");
            var maxSizeBytes = maxSizeMB * 1024 * 1024;
            return fileSizeBytes <= maxSizeBytes;
        }

        private string GetAllowedExtensions()
        {
            var extensions = _configuration.GetSection("FileStorage:AllowedExtenstions").Get<List<string>>();
            return extensions is null ? "None configured" : string.Join(", ", extensions);
        }

        private string GetAllowedPictureExtensions()
        {
            var ext = _configuration.GetSection("FileStorage:PhotoExtensions").Get<List<string>>();
            return ext is null ? "None Configured" : string.Join(", ", ext);
        }

        private int GetMaxFileSize()
        {
            return _configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 100);
        }

        private string ResolveStoragePath(string configuredPath)
        {
            if (string.IsNullOrWhiteSpace(configuredPath))
                throw new InvalidOperationException("File storage path not configured.");

            // Trim and normalize
            var trimmed = configuredPath.Trim();

            // If configured path is rooted (absolute), return full absolute path
            if (Path.IsPathRooted(trimmed))
                return Path.GetFullPath(trimmed);

            // If leading ~ or leading slashes, trim them and combine with content root
            trimmed = trimmed.TrimStart('~', '/', '\\');
            var combined = Path.Combine(_env.WebRootPath, trimmed);
            return Path.GetFullPath(combined);
        }
    }
}
