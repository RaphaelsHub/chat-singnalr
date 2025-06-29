let token = localStorage.getItem("jwt");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chathub", {
        accessTokenFactory: () => token
    })
    .build();

connection.start().then(() => {
    console.log("SignalR connected");
    if (window.currentChatId) {
        connection.invoke("JoinChat", window.currentChatId);
    }
});

connection.on("ReceiveMessage", (sender, encryptedText, timestamp) => {
    const msg = document.createElement("div");
    msg.innerText = `[${timestamp}] ${sender}: ${encryptedText}`;
    document.getElementById("messages")?.appendChild(msg);
});

async function loginOrRegister(isRegister) {
    const userName = document.getElementById("userName").value;
    const password = document.getElementById("password").value;
    const url = isRegister ? "/api/auth/register" : "/api/auth/login";

    const res = await fetch(url, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ userName, password })
    });

    if (res.ok) {
        const data = await res.json();
        token = data.token;
        localStorage.setItem("jwt", token);
        document.getElementById("authSection").style.display = "none";
        document.getElementById("chatSection").style.display = "block";
        loadAllUsers();
    } else {
        alert("Authentication failed.");
    }
}

async function sendMessage() {
    const input = document.getElementById("messageInput");
    const text = input.value.trim();
    if (!text) return;

    await fetch(`/api/chat/send?chatId=${window.currentChatId}&encryptedText=${encodeURIComponent(text)}`, {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });

    input.value = "";
}

async function searchUsers() {
    const query = document.getElementById("searchInput").value;
    const res = await fetch(`/api/user/search?name=${encodeURIComponent(query)}`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });
    const users = await res.json();
    renderUserList(users);
}

async function loadAllUsers() {
    const res = await fetch(`/api/user/search?name=`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });
    const users = await res.json();
    renderUserList(users);
}

function renderUserList(users) {
    const container = document.getElementById("searchResults");
    container.innerHTML = "";
    users.forEach(u => {
        const div = document.createElement("div");
        div.innerText = u.userName;
        div.style.cursor = "pointer";
        div.onclick = () => startChat(u.id);
        container.appendChild(div);
    });
}

async function startChat(targetUserId) {
    const res = await fetch(`/api/chat/start?targetUserIdString=${targetUserId}`, {
        method: "POST",
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });
    const chatId = await res.text();
    window.currentChatId = chatId;
    document.getElementById("messages").innerHTML = "";
    connection.invoke("JoinChat", chatId);
    loadMessages(chatId);
}

async function loadMessages(chatId) {
    const res = await fetch(`/api/chat/${chatId}`, {
        headers: {
            "Authorization": `Bearer ${token}`
        }
    });
    const messages = await res.json();
    const container = document.getElementById("messages");
    container.innerHTML = "";
    messages.forEach(m => {
        const div = document.createElement("div");
        div.innerText = `[${m.timestamp}] ${m.senderId}: ${m.encryptedText}`;
        container.appendChild(div);
    });
}
