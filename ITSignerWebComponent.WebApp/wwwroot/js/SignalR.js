const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notifyHub")
    .build();

connection.on("ReloadPage", () => {
    //console.log("Recargando página...");
    location.reload();
});

connection.start().catch(err => console.error(err));
