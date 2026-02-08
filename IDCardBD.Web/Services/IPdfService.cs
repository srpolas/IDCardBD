using IDCardBD.Web.Models;

namespace IDCardBD.Web.Services
{
    public interface IPdfService
    {
        byte[] GenerateIdCard(IdentityBase person, CardTemplate template);
    }
}
