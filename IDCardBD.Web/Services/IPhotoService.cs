namespace IDCardBD.Web.Services
{
    public sealed class PhotoSaveResult
    {
        /// <summary>Web-relative path of the saved file (e.g. /uploads/profiles/abc.jpg), null when no photo was supplied.</summary>
        public string? RelativePath { get; init; }

        /// <summary>Validation error message, null when the upload is valid.</summary>
        public string? Error { get; init; }

        public bool IsValid => Error == null;
    }

    public interface IPhotoService
    {
        /// <summary>
        /// Validates an uploaded profile photo (max 100KB, JPEG only — checked by extension and content)
        /// and saves it under wwwroot/uploads/profiles with a generated name.
        /// A null photo is valid and produces a null RelativePath.
        /// </summary>
        Task<PhotoSaveResult> SaveProfilePhotoAsync(IFormFile? photo);

        /// <summary>Best-effort deletion of a stored photo; silently ignores missing or invalid paths.</summary>
        void DeletePhotoFile(string? relativePath);
    }
}
