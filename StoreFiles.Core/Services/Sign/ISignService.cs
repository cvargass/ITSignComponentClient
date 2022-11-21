using StoreFiles.Core.DTOs.Sign;

namespace StoreFiles.Core.Services.Sign
{
    public interface ISignService
    {
        byte[] SignFile(SignDto signDto);
    }
}