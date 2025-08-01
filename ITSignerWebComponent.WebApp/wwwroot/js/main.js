setVisibleCertificateSelectedIcon = function () {
    var certificateSelectedIcon = document.getElementById("wrapper-certificate-selected");
    certificateSelectedIcon.classList.remove("invisible");
    certificateSelectedIcon.classList.add("visible");
}

setInvisibleCertificateSelectedIcon = function () {
    var certificateSelectedIcon = document.getElementById("wrapper-certificate-selected");

    if (certificateSelectedIcon) {
        certificateSelectedIcon.classList.remove("visible");
        certificateSelectedIcon.classList.add("invisible");
    }
}


setLoadingSigningButton = function () {
    var loadingBtnView = document.getElementById("loading-btn-view");
    var signBtnView = document.getElementById("sign-btn-view");

    loadingBtnView.classList.remove("d-none");
    signBtnView.classList.add("d-none");
}

/*
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
*/

downloadDocFile = function (fileDoc) {
    if (!fileDoc || typeof fileDoc !== "string") {
        console.error("fileDoc no es una cadena válida:", fileDoc);
        return;
    }

    try {
        const byteCharacters = atob(fileDoc);
        const byteArray = new Uint8Array(byteCharacters.length);

        for (let i = 0; i < byteCharacters.length; i++) {
            byteArray[i] = byteCharacters.charCodeAt(i);
        }

        const file = new Blob([byteArray], { type: 'application/pdf' });
        const fileURL = URL.createObjectURL(file);
        window.open(fileURL);
    } catch (error) {
        console.error("Error al decodificar el archivo:", error);
    }
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

downloadFileP7z = function (fileBase64, filename) {
    const downloadLink = document.createElement('a');

    downloadLink.href = `data:;base64,${fileBase64}`;
    downloadLink.download = filename + ".p7z";
    downloadLink.click();
}


setCheckboxesValue = function (flag) {
    const checkboxes = document.querySelectorAll('input[type="checkbox"].form-check-input');

    checkboxes.forEach((checkbox) => {
        checkbox.checked = flag;
    });
}

setAllCheckboxesCbValue = function (flag) {
    const checkboxes = document.querySelectorAll('input[type="checkbox"].cbx-mark-all');

    checkboxes.forEach((checkbox) => {
        checkbox.checked = flag;
    });
}