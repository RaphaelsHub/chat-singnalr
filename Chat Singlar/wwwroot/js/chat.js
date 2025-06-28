const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub")
    .build();

connection.start().then(() => {
    console.log("SignalR connected");
    connection.invoke("JoinChat", chatId);
});

connection.on("ReceiveMessage", (sender, encryptedText, timestamp) => {
    const msg = document.createElement("div");
    msg.innerText = `[${timestamp}] ${sender}: ${encryptedText}`;
    document.getElementById("messages").appendChild(msg);
});