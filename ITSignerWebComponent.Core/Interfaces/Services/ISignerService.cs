using ITSignerWebComponent.Core.DTOs.Signer.EmbedSignature;
using ITSignerWebComponent.Core.DTOs.Signer.PreSigned;

namespace ITSignerWebComponent.Core.Interfaces.Services
{
    public interface ISignerService
    {
        (byte[] pdfPrepared, byte[] dataToSign) BeginPreSigningProcess(PreSignedDto signedDto);
        byte[] SigningProcessEmbedSignature(EmbedSignatureDto signatureDto);
    }
}