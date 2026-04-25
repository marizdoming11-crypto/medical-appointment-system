async function loadDoctors() {
    const doctors = await getData("doctor");

    const list = document.getElementById("doctorList");

    doctors.forEach(d => {
        const li = document.createElement("li");

        li.innerHTML = `
            ${d.fullName} - ${d.specialty}
            <button onclick="viewSlots(${d.doctorId})">View Slots</button>
        `;

        list.appendChild(li);
    });
}

function viewSlots(id) {
    localStorage.setItem("doctorId", id);
    window.location.href = "slots.html";
}

loadDoctors();