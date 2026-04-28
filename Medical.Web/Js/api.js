const BASE_URL = "https://localhost:7127/api";

async function postData(url, data) {
    const res = await fetch(`${BASE_URL}/${url}`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });

    return await res.json();
}

async function getData(url) {
    const res = await fetch(`${BASE_URL}/${url}`);
    return await res.json();
}