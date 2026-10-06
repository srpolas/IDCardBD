namespace IDCardBD.Web.Services
{
    public class PhotoService : IPhotoService
    {
        private const int MaxPhotoBytes = 100 * 1024;

        private readonly IWebHostEnvironment _environment;

        public PhotoService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<PhotoSaveResult> SaveProfilePhotoAsync(IFormFile? photo)
        {
            if (photo == null || photo.Length == 0)
                return new PhotoSaveResult();

            if (photo.Length > MaxPhotoBytes)
                return new PhotoSaveResult { Error = "Photo size must be within 100KB." };

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg")
                return new PhotoSaveResult { Error = "Only .jpg files are allowed." };

            using var stream = photo.OpenReadStream();

            // Verify the content really is JPEG (FF D8 magic bytes), not a renamed file.
            var header = new byte[2];
            if (await stream.ReadAsync(header.AsMemory(0, 2)) < 2 || header[0] != 0xFF || header[1] != 0xD8)
                return new PhotoSaveResult { Error = "Only .jpg files are allowed." };

            string uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadDir);

            string fileName = Guid.NewGuid().ToString("N") + ".jpg";
            using (var fileStream = new FileStream(Path.Combine(uploadDir, fileName), FileMode.Create))
            {
                await fileStream.WriteAsync(header.AsMemory(0, 2));
                await stream.CopyToAsync(fileStream);
            }

            return new PhotoSaveResult { RelativePath = "/uploads/profiles/" + fileName };
        }

        public void DeletePhotoFile(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;

            string uploadRoot = Path.Combine(_environment.WebRootPath, "uploads", "profiles");
            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));
            }
            catch (Exception)
            {
                return;
            }

            // Never delete outside the profiles upload directory.
            if (!fullPath.StartsWith(uploadRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                if (File.Exists(fullPath)) File.Delete(fullPath);
            }
            catch (Exception)
            {
                // Best-effort cleanup; a stale file is harmless.
            }
        }
    }
}
