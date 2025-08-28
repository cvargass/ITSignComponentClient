// Función para ejecutar la aplicación con protocolo personalizado
function launchAppWithProtocol(protocol, parameter) {
    //console.log(`Ejecutando aplicación con protocolo: ${protocol}:${parameter}`);

    const url = `${protocol}:${parameter}`;

    // Método 1: Intentar con window.location (más común)
    try {
        window.location.href = url;
        return "{ success: true, method: 'window.location' }";
    } catch (error) {
        console.log('Método window.location falló:', error);
    }

    // Método 2: Crear iframe temporal
    try {
        const iframe = document.createElement('iframe');
        iframe.style.display = 'none';
        iframe.src = url;
        document.body.appendChild(iframe);

        setTimeout(() => {
            if (iframe.parentNode) {
                document.body.removeChild(iframe);
            }
        }, 1000);

        return "{ success: true, method: 'iframe' }";
    } catch (error) {
        console.log('Método iframe falló:', error);
    }

    // Método 3: Usar window.open
    try {
        window.open(url, '_self');
        return "{ success: true, method: 'window.open' }";
    } catch (error) {
        console.log('Método window.open falló:', error);
    }

    // Método 4: Usar anchor tag
    try {
        const link = document.createElement('a');
        link.href = url;
        link.style.display = 'none';
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        return "{ success: true, method: 'anchor' }";
    } catch (error) {
        console.log('Método anchor falló:', error);
    }

    return { success: false, error: 'Todos los métodos fallaron' };
}

// Hacer la función disponible globalmente
window.launchAppWithProtocol = launchAppWithProtocol;