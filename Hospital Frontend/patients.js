const apiUrl = "http://localhost:5224/api/Patients";

document.addEventListener("DOMContentLoaded", () => {
    fetchPatients();

    const addForm = document.getElementById("addPatientForm");
    addForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const patient = {
            name: document.getElementById("name").value,
            age: parseInt(document.getElementById("age").value),
            gender: document.getElementById("gender").value,
            phone: document.getElementById("phone").value,
            address: document.getElementById("address").value,
            disease: document.getElementById("disease").value
        };

        try {
            const response = await fetch(apiUrl, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(patient)
            });

            if (!response.ok) throw new Error("Failed to add patient");

            addForm.reset();
            fetchPatients();
        } catch (err) {
            alert(err.message);
            console.error(err);
        }
    });
});

async function fetchPatients() {
    const tableBody = document.querySelector("#patientsTable tbody");
    tableBody.innerHTML = "";

    try {
        const response = await fetch(apiUrl);
        const patients = await response.json();

        patients.forEach(p => {
            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${p.patientId}</td>
                <td><input type="text" value="${p.name}" class="edit-name"></td>
                <td><input type="number" value="${p.age}" class="edit-age"></td>
                <td>
                    <select class="edit-gender">
                        <option value="Male" ${p.gender === 'Male' ? 'selected' : ''}>Male</option>
                        <option value="Female" ${p.gender === 'Female' ? 'selected' : ''}>Female</option>
                        <option value="Transgender" ${p.gender === 'Transgender' ? 'selected' : ''}>Transgender</option>
                    </select>
                </td>
                <td><input type="text" value="${p.phone}" class="edit-phone"></td>
                <td><input type="text" value="${p.address}" class="edit-address"></td>
                <td><input type="text" value="${p.disease}" class="edit-disease"></td>
                <td>
                    <button class="edit-btn" onclick="updatePatient(${p.patientId}, this)">Save</button>
                    <button class="delete-btn" onclick="deletePatient(${p.patientId})">Delete</button>
                </td>
            `;
            tableBody.appendChild(row);
        });
    } catch (err) {
        console.error("Failed to fetch patients", err);
    }
}

async function updatePatient(id, btn) {
    const row = btn.closest("tr");
    const updatedPatient = {
        patientId: id,
        name: row.querySelector(".edit-name").value,
        age: parseInt(row.querySelector(".edit-age").value),
        gender: row.querySelector(".edit-gender").value,
        phone: row.querySelector(".edit-phone").value,
        address: row.querySelector(".edit-address").value,
        disease: row.querySelector(".edit-disease").value
    };

    try {
        const response = await fetch(`${apiUrl}/${id}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(updatedPatient)
        });

        if (!response.ok) throw new Error("Failed to update patient");
        fetchPatients();
    } catch (err) {
        alert(err.message);
        console.error(err);
    }
}

async function deletePatient(id) {
    if (confirm("Are you sure you want to delete this patient?")) {
        try {
            const response = await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
            if (!response.ok) throw new Error("Failed to delete patient");
            fetchPatients();
        } catch (err) {
            alert(err.message);
            console.error(err);
        }
    }
}
