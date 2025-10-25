const apiUrl = "http://localhost:5224/api/appointments";
const patientsApi = "http://localhost:5224/api/patients";
const doctorsApi = "http://localhost:5224/api/doctors";

let editId = null;

document.addEventListener("DOMContentLoaded", () => {
    fetchPatients();
    fetchDoctors();
    fetchAppointments();

    const form = document.getElementById("appointmentForm");
    const cancelBtn = document.getElementById("cancelEditBtn");

    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const appointment = {
            patientId: parseInt(document.getElementById("patientId").value),
            doctorId: parseInt(document.getElementById("doctorId").value),
            appointmentDate: document.getElementById("appointmentDate").value
        };

        if (editId) {
            const response = await fetch(`${apiUrl}/${editId}`, {
                method: "PUT",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ ...appointment, appointmentId: editId })
            });
            if (response.ok) resetForm();
        } else {
            const response = await fetch(apiUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(appointment)
            });
            if (response.ok) resetForm();
        }

        fetchAppointments();
    });

    cancelBtn.addEventListener("click", resetForm);
});

async function fetchPatients() {
    const select = document.getElementById("patientId");
    const response = await fetch(patientsApi);
    const patients = await response.json();
    select.innerHTML = `<option value="">Select Patient</option>`;
    patients.forEach(p => {
        select.innerHTML += `<option value="${p.patientId}">${p.name}</option>`;
    });
}

async function fetchDoctors() {
    const select = document.getElementById("doctorId");
    const response = await fetch(doctorsApi);
    const doctors = await response.json();
    select.innerHTML = `<option value="">Select Doctor</option>`;
    doctors.forEach(d => {
        select.innerHTML += `<option value="${d.doctorId}">${d.name}</option>`;
    });
}

async function fetchAppointments() {
    const tbody = document.querySelector("#appointmentsTable tbody");
    tbody.innerHTML = "";

    const response = await fetch(apiUrl);
    const appointments = await response.json();

    appointments.forEach(a => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${a.appointmentId}</td>
            <td>${a.patientName || a.patientId}</td>
            <td>${a.doctorName || a.doctorId}</td>
            <td>${new Date(a.appointmentDate).toLocaleString()}</td>
            <td>
                <button onclick="editAppointment(${a.appointmentId})">Edit</button>
                <button onclick="deleteAppointment(${a.appointmentId})">Delete</button>
            </td>
        `;
        tbody.appendChild(row);
    });
}

async function editAppointment(id) {
    const response = await fetch(`${apiUrl}/${id}`);
    const a = await response.json();

    document.getElementById("patientId").value = a.patientId;
    document.getElementById("doctorId").value = a.doctorId;
    document.getElementById("appointmentDate").value = a.appointmentDate.slice(0, 16);

    editId = id;
    document.getElementById("submitBtn").textContent = "Update Appointment";
    document.getElementById("cancelEditBtn").style.display = "inline-block";
}

async function deleteAppointment(id) {
    if (!confirm("Delete this appointment?")) return;
    await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
    fetchAppointments();
}

function resetForm() {
    editId = null;
    document.getElementById("appointmentForm").reset();
    document.getElementById("submitBtn").textContent = "Add Appointment";
    document.getElementById("cancelEditBtn").style.display = "none";
}
