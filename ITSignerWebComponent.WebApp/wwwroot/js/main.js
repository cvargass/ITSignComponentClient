setVisibleCertificateSelectedIcon = function () {
    var certificateSelectedIcon = document.getElementById("wrapper-certificate-selected");
    certificateSelectedIcon.classList.remove("invisible");
    certificateSelectedIcon.classList.add("visible");
}

setInvisibleCertificateSelectedIcon = function () {
    var certificateSelectedIcon = document.getElementById("wrapper-certificate-selected");
    certificateSelectedIcon.classList.remove("visible");
    certificateSelectedIcon.classList.add("invisible");
}


setLoadingSigningButton = function () {
    var loadingBtnView = document.getElementById("loading-btn-view");
    var signBtnView = document.getElementById("sign-btn-view");

    loadingBtnView.classList.remove("d-none");
    signBtnView.classList.add("d-none");
}


downloadDocFile = function (fileDoc) {
    const byteCharacters = atob(fileDoc);
    const byteNumbers = new Array(byteCharacters.length);
    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    var file = new Blob([byteArray], { type: 'application/pdf' })
    var fileURL = URL.createObjectURL(file);
    window.open(fileURL);
}

setVisibleBtnDownloadDoc = function () {
    document.getElementById("btn-sign").outerHTML = "";
    document.getElementById("btn-downloadDoc").classList.remove("d-none");
}

enableFormSign = function () {
    var formSignedDisabled = document.getElementById("form-signed-disabled");
    formSignedDisabled.classList.add("d-none");
}

log = function (message) {
    console.log(message);
}

cleanLocalStorage = function () {
    window.cancelHandler()
}
