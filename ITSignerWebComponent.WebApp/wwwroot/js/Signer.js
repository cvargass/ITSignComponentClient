////BEGIN SECTION UTILS FUNCTIONS
window.convertArrayBufferToBase64 = function convertArrayBufferToBase64(buffer) {
    var binary = '';
    var bytes = new Uint8Array(buffer);
    var len = bytes.byteLength;
    for (var i = 0; i < len; i++) {
        binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
}

window.convertBase64ToArrayBuffer = function (base64) {
    var binary_string = base64.replace(/\\n/g, '');
    binary_string = window.atob(base64);
    var len = binary_string.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) {
        bytes[i] = binary_string.charCodeAt(i);
    }
    return bytes.buffer;
}

window.generateGuid = function () {
    var result, i, j;
    result = '';
    for (j = 0; j < 32; j++) {
        if (j == 8 || j == 12 || j == 16 || j == 20)
            result = result + '-';
        i = Math.floor(Math.random() * 16).toString(16).toUpperCase();
        result = result + i;
    }
    return result;
}

////END SECTION UTILS FUNCTIONS




//Initialize Fortify Component
window.InitializeFortify = async function () {
    try {
        var fortifyCertificates = document.getElementById('fortify-component');

        //Adding events
        fortifyCertificates.addEventListener('selectionCancel', cancelHandler);
        fortifyCertificates.addEventListener('selectionSuccess', continueHandler);

        self.ws = new WebcryptoSocket.SocketProvider({
            storage: await WebcryptoSocket.BrowserStorage.create(),
        });

        ws.connect("127.0.0.1:31337")
            .on("error", function (e) {
                console.error(e);
            })
            .on("listening", async (e) => {
                // Check if end-to-end session is approved
                if (! await ws.isLoggedIn()) {
                    const pin = await ws.challenge();
                    // show PIN
                    setTimeout(() => {
                        alert("2key session PIN:" + pin);
                    }, 100)
                    // ask to approve session
                    await ws.login();
                }
            });
    } catch (e) {
        console.error(e);
    }
}          

//Method Handler to Cancel Fortify Functionality
window.cancelHandler = function () {
    window.localStorage.removeItem("PEMSelected");
    window.localStorage.removeItem("ProviderId");
    window.localStorage.removeItem("CertificateId");
    window.localStorage.removeItem("PrivateKeyId");
    setInvisibleCertificateSelectedIcon();
};

//Method Handler to Continue Fortify Functionality
window.continueHandler = async function (event) {
    var provider = await event.detail.socketProvider.getCrypto(event.detail.providerId);
    var cert = await provider.certStorage.getItem(event.detail.certificateId);
    var certRawData = await provider.certStorage.exportCert('raw', cert);

    var pemBase64 = window.convertArrayBufferToBase64(certRawData);

    window.localStorage.setItem("PEMSelected", pemBase64);
    window.localStorage.setItem("ProviderId", event.detail.providerId);
    window.localStorage.setItem("CertificateId", event.detail.certificateId);
    window.localStorage.setItem("PrivateKeyId", event.detail.privateKeyId);

    setVisibleCertificateSelectedIcon();
};

window.isValidAuthorityCertificate = async function (validCertificateAuthority) {
    let isValid = false;
    var providerId = window.localStorage.getItem("ProviderId");
    var certificateId = window.localStorage.getItem("CertificateId");

    const provider = await ws.getCrypto(providerId);
    const cert = await provider.certStorage.getItem(certificateId);


    if (cert._issuerName.length !== null) {
        let authorityToValidate = cert._issuerName.substring(cert._issuerName.indexOf("CN="), cert._issuerName.length);

        let certificateAuthority = authorityToValidate.substring(authorityToValidate.indexOf("CN=") + 3, authorityToValidate.includes(",") ? authorityToValidate.indexOf(",") : authorityToValidate.length)


        if (validCertificateAuthority.trim().toLowerCase() === certificateAuthority.trim().toLowerCase()) {
            isValid = true;
        }
    }
    
   
    return isValid;
}


window.getPemSelected = async function () {
    return window.localStorage.getItem("PEMSelected");
};

window.generateCMSToEmbed = async function (dataToSign) {
    try {
        var demoData = window.convertBase64ToArrayBuffer(dataToSign);

        var providerId = window.localStorage.getItem("ProviderId");
        const provider = await ws.getCrypto(providerId);

        provider.sign = provider.subtle.sign.bind(provider.subtle);

        let nameEngine = 'new3ngin3' + window.generateGuid() + Date();

        pkijs.setEngine(
            nameEngine,
            provider,
            new pkijs.CryptoEngine({
                name: "",
                crypto: provider,
                subtle: provider.subtle,
            })
        );

        var certificateId = window.localStorage.getItem("CertificateId");
        var privateKeyId = window.localStorage.getItem("PrivateKeyId");

        var cert = await provider.certStorage.getItem(certificateId);
        var privateKey = await provider.keyStorage.getItem(privateKeyId);
        var certRawData = await provider.certStorage.exportCert('raw', cert);

        var pkiCert = new pkijs.Certificate({
            schema: asn1js.fromBER(certRawData).result,
        });

        var signedData = new pkijs.SignedData({
            version: 1,
            encapContentInfo: new pkijs.EncapsulatedContentInfo({
                eContentType: "1.2.840.113549.1.7.1", // "data" content type
            }),
            signerInfos: [
                new pkijs.SignerInfo({
                    version: 1,
                    sid: new pkijs.IssuerAndSerialNumber({
                        issuer: pkiCert.issuer,
                        serialNumber: pkiCert.serialNumber,
                    }),
                }),
            ],
            certificates: [pkiCert],
        });

        var contentInfo = new pkijs.EncapsulatedContentInfo({
            eContent: new asn1js.OctetString({
                valueHex: demoData,
            }),
        });

        signedData.encapContentInfo.eContent = contentInfo.eContent;

        await signedData.sign(privateKey, 0, "sha-256");

        var cms = new pkijs.ContentInfo({
            contentType: "1.2.840.113549.1.7.2",
            content: signedData.toSchema(true),
        });

        var result = cms.toSchema().toBER(false);

        var resultBase64 = await window.convertArrayBufferToBase64(result);

        return resultBase64;
    } catch (error) {
        alert('Failed Signing CMS - Errors: \n' + error);
        //console.error(error);
        window.location.reload();
    }
}