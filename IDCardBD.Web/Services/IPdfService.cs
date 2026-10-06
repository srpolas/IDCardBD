using IDCardBD.Web.Models;

namespace IDCardBD.Web.Services
{
    public interface IPdfService
    {
        byte[] GenerateIdCard(IdentityBase person, CardTemplate template);

        /// <summary>PNG bytes of a QR code containing the person's details as a vCard (scan to save contact).</summary>
        byte[] GenerateQrCodeImage(IdentityBase person, CardTemplate template);
    }
}
