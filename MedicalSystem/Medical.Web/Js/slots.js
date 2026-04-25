async function loadSlots() {
    const doctorId = localStorage.getItem("doctorId");

    const slots = await getData(`appointment/available/${doctorId}`);

    const list = document.getElementById("slotList");

    slots.forEach(s => {
        const li = document.createElement("li");

        li.innerHTML = `
            ${new Date(s.startTime).toLocaleString()}
            <button onclick="book(${s.timeSlotId})">Book</button>
        `;

        list.appendChild(li);
    });
}

async function book(slotId) {
    const data = {
        userId: 1,
        doctorId: localStorage.getItem("doctorId"),
        timeSlotId: slotId
    };

    const res = await postData("appointment/book", data);
    alert(res);
}

loadSlots();