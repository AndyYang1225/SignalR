"use strict";

// 從 Dom tree 當中取得送出按鈕、訊息輸入元件
let btnSend = document.getElementById("sendButton");
let messageInput = document.getElementById("messageInput");

// 從 Dom tree 當中取得登入按鈕、使用者名稱輸入、密碼輸入
let userInput = document.getElementById("userInput");
let passInput = document.getElementById("passInput");
let btnLogin = document.getElementById("loginButton");

// 使送出按鈕無法點選，直到登入後 SignalR 連線建立
btnSend.disabled = true;

let access_token = "";
// 以 token 起始一個 SignalR 連線，連線到 /chatHub 端點
let connection = new signalR.HubConnectionBuilder().withUrl("chathub", {
    accessTokenFactory: () => access_token
}).build();

// 點擊登入鈕事件
btnLogin.addEventListener("click", async function (event) {
    try {
        // 取得 token
        let data = {
            userId: userInput.value,
            password: passInput.value
        };
        let res = await axios.post("./api/Token/signin", data);
        access_token = res.data.token

        // 起始連線，將送出按鈕啟用
        await connection.start()
        btnSend.disabled = false;
    } catch (err) {
        alert("嘗試取得Token建立連線時發生錯誤，詳細資料：" + err.toString() + ";");
    }
});

// 註冊連線接收到 ReceiveMessage 時的行為
// 這個行為會呼叫帶有參數 user, message 的回呼函數
connection.on("ReceiveMessage", function (user, message) {
    // 將&、<、>取代為相對應的 html code
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    // 設定顯示文字、新增一個顯示對話的 li dom 插入至 messagesList
    var encodedMsg = "[" + user + "] " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

// 設定按下送出訊息的行為
btnSend.addEventListener("click", function (event) {
    // 以參數 userInput、messageInput 的值作為參數呼叫 server 端的 SendMessage
    connection
        .invoke("SendMessage", userInput.value, messageInput.value)
        .catch(function (err) {
            return console.error(err.toString());
        });
    // 取消 html 按鈕執行預設行為
    event.preventDefault();
});