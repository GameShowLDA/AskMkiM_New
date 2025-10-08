async function login() {
    const username = document.getElementById("login").value.trim();
    const password = document.getElementById("password").value.trim();
    const errorBox = document.getElementById("errorBox");

    errorBox.style.display = "none";

    const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password })
    });

    if (res.ok) {
        window.location.href = "/home";
    } else {
        errorBox.textContent = "❌ Неверный логин или пароль";
        errorBox.style.display = "block";
    }
}
