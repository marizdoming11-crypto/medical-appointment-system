async function loginUser() {
    const email = document.getElementById("email").value;
    const password = document.getElementById("password").value;

    const result = await postData("auth/login", { email, password });

    if (result.token) {
        localStorage.setItem("token", result.token);
        window.location.href = "dashboard.html";
    } else {
        alert("Login failed");
    }
}