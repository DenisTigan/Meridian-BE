using MeridianEmployeeHub.Data.Repositories.Interfaces;
using MeridianEmployeeHub.Services.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace MeridianEmployeeHub.Services.Employees
{
    public class ProfilePictureService : IProfilePictureService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        private static readonly Dictionary<string, List<byte[]>> _fileSignatures = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
            { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } }
        };

        private static readonly string[] _allowedContentTypes = { "image/jpeg", "image/png", "image/webp" };

        public ProfilePictureService(
            IEmployeeRepository employeeRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _employeeRepository = employeeRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadProfilePictureAsync(int employeeId, IFormFile file)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee with id {employeeId} not found.");

            ValidateFile(file);

            var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "profile-pictures");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{employeeId}{extension}";
            var absolutePath = Path.Combine(uploadsFolder, fileName);
            var relativePath = $"/uploads/profile-pictures/{fileName}";

            // Delete old picture if it exists and has a different name
            // Even if it has the same name, we can delete it or let FileStream overwrite it.
            // We explicitly delete old file if extension differs (e.g. was .jpg, now .png)
            if (!string.IsNullOrEmpty(employee.ProfilePictureUrl))
            {
                var oldFileName = Path.GetFileName(employee.ProfilePictureUrl);
                if (oldFileName != fileName)
                {
                    var oldAbsolutePath = Path.Combine(uploadsFolder, oldFileName);
                    if (File.Exists(oldAbsolutePath))
                    {
                        File.Exists(oldAbsolutePath); // A trick to ensure we don't have unused var
                        File.Delete(oldAbsolutePath);
                    }
                }
            }

            // Save the new file (this also overwrites if it has the same name)
            using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update DB
            employee.ProfilePictureUrl = relativePath;
            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();

            return relativePath;
        }

        public async Task DeleteProfilePictureAsync(int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId)
                ?? throw new KeyNotFoundException($"Employee with id {employeeId} not found.");

            if (string.IsNullOrEmpty(employee.ProfilePictureUrl))
            {
                // Idempotent: nothing to do if already null
                return;
            }

            var webRootPath = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "uploads", "profile-pictures");
            var oldFileName = Path.GetFileName(employee.ProfilePictureUrl);
            var absolutePath = Path.Combine(uploadsFolder, oldFileName);

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }

            employee.ProfilePictureUrl = null;
            await _employeeRepository.UpdateAsync(employee);
            await _employeeRepository.SaveChangesAsync();
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            if (file.Length > MaxFileSizeInBytes)
                throw new ArgumentException("File size exceeds 5 MB limit.");

            if (!_allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
                throw new ArgumentException("Invalid content type. Only JPEG, PNG and WEBP are allowed.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".webp" && !_fileSignatures.ContainsKey(extension))
                throw new ArgumentException("Invalid file extension.");

            // Read Magic Numbers to verify it's a real image, not just a renamed file
            using var stream = file.OpenReadStream();
            using var reader = new BinaryReader(stream);

            if (extension == ".webp")
            {
                // WEBP header is: RIFF (4 bytes) + file size (4 bytes) + WEBP (4 bytes)
                var webpHeader = reader.ReadBytes(12);
                if (webpHeader.Length < 12 ||
                    webpHeader[0] != 0x52 || webpHeader[1] != 0x49 || webpHeader[2] != 0x46 || webpHeader[3] != 0x46 || // RIFF
                    webpHeader[8] != 0x57 || webpHeader[9] != 0x45 || webpHeader[10] != 0x42 || webpHeader[11] != 0x50)   // WEBP
                {
                    throw new ArgumentException("Invalid file signature (magic numbers mismatch).");
                }
                return;
            }

            var signatures = _fileSignatures[extension];
            var maxSignatureLength = signatures.Max(m => m.Length);
            var headerBytes = reader.ReadBytes(maxSignatureLength);

            bool isMatch = false;
            foreach (var signature in signatures)
            {
                if (headerBytes.Length >= signature.Length)
                {
                    isMatch = true;
                    for (int i = 0; i < signature.Length; i++)
                    {
                        if (headerBytes[i] != signature[i])
                        {
                            isMatch = false;
                            break;
                        }
                    }
                    if (isMatch) break;
                }
            }

            if (!isMatch)
                throw new ArgumentException("Invalid file signature (magic numbers mismatch).");
        }
    }
}
