const apiUrl = "http://localhost:5224/api/rooms"; 
const patientsApi = "http://localhost:5224/api/patients";

let patients = [];

document.addEventListener("DOMContentLoaded", () => {
    fetchPatients();
    fetchRooms();

    const addForm = document.getElementById("addRoomForm");
    addForm.addEventListener("submit", async (e) => {
        e.preventDefault();
        const roomNumber = document.getElementById("roomNumber").value.trim();
        const bedNumber = document.getElementById("bedNumber").value.trim();

        if (!roomNumber) {
            alert("Room Number is required");
            return;
        }

        try {
            const res = await fetch(apiUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ roomNumber, bedNumber })
            });
            if (!res.ok) throw new Error("Failed to add room");
            addForm.reset();
            fetchRooms();
        } catch (err) {
            alert(err.message);
            console.error(err);
        }
    });
});

async function fetchPatients() {
    try {
        const res = await fetch(patientsApi);
        patients = await res.json();
    } catch (err) {
        console.error("Failed to fetch patients", err);
    }
}

async function fetchRooms() {
    const tableBody = document.querySelector("#roomsTable tbody");
    tableBody.innerHTML = "";

    try {
        const res = await fetch(apiUrl);
        if (!res.ok) throw new Error("Failed to fetch rooms");
        const rooms = await res.json();

        rooms.forEach(room => {
            const tr = document.createElement("tr");
            const patientLabel = room.patientName ? `${room.patientName} (ID: ${room.patientId})` : "";

            let optionsHtml = `<option value="">-- Select patient --</option>`;
            optionsHtml += `<option value="__unassign__">-- Unassign --</option>`;
            patients.forEach(p => {
                const selected = room.patientId === p.patientId ? "selected" : "";
                optionsHtml += `<option value="${p.patientId}" ${selected}>${p.name} (ID: ${p.patientId})</option>`;
            });

            tr.innerHTML = `
                <td>${room.roomId}</td>
                <td>${room.roomNumber}</td>
                <td>${room.bedNumber ?? ""}</td>
                <td>${room.isOccupied ? "Yes" : "No"}</td>
                <td>${patientLabel}</td>
                <td>
                    <select class="patient-select" data-roomid="${room.roomId}">
                        ${optionsHtml}
                    </select>
                    <button class="assign-btn" data-roomid="${room.roomId}">Save</button>
                </td>
                <td>
                    <button class="delete-btn" data-roomid="${room.roomId}">Delete</button>
                </td>
            `;

            tableBody.appendChild(tr);
        });

        document.querySelectorAll("button.assign-btn").forEach(btn => {
            btn.addEventListener("click", async () => {
                const roomId = btn.getAttribute("data-roomid");
                const select = document.querySelector(`select.patient-select[data-roomid='${roomId}']`);
                const val = select.value;

                try {
                    if (val === "") { alert("Select a patient or Unassign option"); return; }

                    if (val === "__unassign__") {
                        const r = await fetch(`${apiUrl}/${roomId}/unassign`, { method: "POST" });
                        if (!r.ok) throw new Error("Failed to unassign");
                        fetchRooms();
                        return;
                    }

                    const r = await fetch(`${apiUrl}/${roomId}/assign/${val}`, { method: "POST" });
                    if (!r.ok) throw new Error("Failed to assign room");
                    fetchRooms();
                } catch (err) {
                    alert(err.message);
                    console.error(err);
                }
            });
        });

        document.querySelectorAll("button.delete-btn").forEach(btn => {
            btn.addEventListener("click", async () => {
                const roomId = btn.getAttribute("data-roomid");
                if (!confirm("Delete this room?")) return;

                try {
                    const r = await fetch(`${apiUrl}/${roomId}`, { method: "DELETE" });
                    if (!r.ok) throw new Error("Failed to delete room");
                    fetchRooms();
                } catch (err) {
                    alert(err.message);
                    console.error(err);
                }
            });
        });

    } catch (err) {
        console.error("Failed to fetch rooms", err);
    }
}
