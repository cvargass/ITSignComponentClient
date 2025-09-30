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

downloadDocSignedFile = function () {
    var fileDoc = sessionStorage.getItem("FileSigned");
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

showConfirmSwall = function (title, text, icon) {
    return Swal.fire({
        title: title,
        text: text,
        icon: icon,
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Sí',
        cancelButtonText: 'No'
    });
}

showSuccessSwall = function (title, text) {
    return Swal.fire({
        title: title,
        text: text,
        icon: 'success',
        timer: 3000,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Aceptar'
    });
}

showErrorSwall = function (title, text) {
    return Swal.fire({
        title: title,
        text: text,
        icon: 'error',
        timer: 3000,
        confirmButtonColor: '#3085d6',
        confirmButtonText: 'Aceptar'
    });
}

onFinalizeClientSigning = async function (urlGetSignedFile) {
    try {

        var guidPendingFile = getCookie("GuidPendingFile");
        //console.log(guidPendingFile);
        if (window.location.toString().includes("signer-file?") && guidPendingFile) {
            const response = await fetch(urlGetSignedFile + guidPendingFile);
            const responseJson = await response.json();
            sessionStorage.setItem("FileSigned", responseJson.fileBase64);
            showSuccessSwall("Success", "¡ Archivo Firmado Correctamente !");
            setVisibleBtnDownloadDoc();
        } else {
             showSuccessSwall("Success", "¡ Archivos Firmados Correctamente !");

             setTimeout(()=> {
                 location.reload();
             }, 2000)
        }
    } catch (err) {
        console.error("Error:", err);
        showErrorSwall("Error.", "Ha ocurrido un inconveniente firmando el documento.");
        setTimeout(() => {
            setTimeout(() => {
                location.reload();
            }, 3000)
        });
    }
}

setGuidPendingSigning = function (guid) {
    //console.log("Guardando GUID en cookie:", guid);
    setCookie("GuidPendingFile", guid);
}

clearGuidPendingSigning = function () {
    document.cookie = "GuidPendingFile=; Max-Age=-99999999; path=/";
}

clearFileSigned = function () {
    sessionStorage.removeItem("FileSigned");
}

function setCookie(name, value, minutes) {
    let expires = "";
    if (minutes) {
        const date = new Date();
        date.setTime(date.getTime() + (minutes * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }

    var cookie = name + "=" + (value || "") + expires + "; path=/"
    document.cookie = cookie;
}

function getCookie(name) {
    const nameEQ = name + "=";

    const ca = document.cookie.split(';');
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}