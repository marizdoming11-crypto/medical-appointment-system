const API = "http://localhost:5200/api/auth";

// REGISTER
async function registerUser() {
    const fullName = document.getElementById("fullname").value;
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    const res = await fetch(API + "/register", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ fullName, email, password })
    });

    const msg = await res.text();

    if (res.ok) {
        alert("Registered!");
        window.location.href = "login.html";
    } else {
        alert(msg);
    }
}

// LOGIN
async function loginUser() {
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    const response = await fetch("http://localhost:5200/api/auth/login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ email, password })
    });

    const result = await response.json();
    console.log(result); // 🔍 DEBUG

    if (response.ok) {
        localStorage.setItem("token", result.token);
        localStorage.setItem("user", JSON.stringify(result.user));

        window.location.href = "dashboard.html";
    } else {
        alert("Invalid login");
    }
}

// LOGOUT
function logoutUser() {
    localStorage.removeItem("token");
    window.location.href = "login.html";
}
