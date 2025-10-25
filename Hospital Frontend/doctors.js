const apiUrl = "http://localhost:5224/api/doctors"; // Make sure your backend is running

document.addEventListener("DOMContentLoaded", () => {
    fetchDoctors();

    const addForm = document.getElementById("addDoctorForm");
    addForm.addEventListener("submit", async (e) => {
        e.preventDefault();

        const doctor = {
            name: document.getElementById("name").value,
            specialization: document.getElementById("specialization").value,
            phone: document.getElementById("phone").value,
            experience: parseInt(document.getElementById("experience").value)
        };

        await fetch(apiUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(doctor)
        });

        addForm.reset();
        fetchDoctors();
    });
});

async function fetchDoctors() {
    const tableBody = document.querySelector("#doctorsTable tbody");
    tableBody.innerHTML = "";

    try {
        console.log("Fetching doctors from:", apiUrl);
        const response = await fetch(apiUrl);
        console.log("Response Status:", response.status);

        if (!response.ok) {
            console.error("Error fetching doctors:", response.statusText);
            return;
        }

        const doctors = await response.json();
        console.log("Doctors Data:", doctors); // 🔍 Check if data is coming

        doctors.forEach(d => {
            const row = document.createElement("tr");

            row.innerHTML = `
                <td>${d.doctorId}</td>
                <td>${d.name}</td>
                <td>${d.specialization}</td>
                <td>${d.phone}</td>
                <td>${d.experience}</td>
                <td>
                    <button class="edit-btn" onclick="editDoctor(${d.doctorId})">Edit</button>
                    <button class="delete-btn" onclick="deleteDoctor(${d.doctorId})">Delete</button>
                </td>
            `;

            tableBody.appendChild(row);
        });
    } catch (error) {
        console.error("Network or server error:", error);
    }
}

// Delete functionality
async function deleteDoctor(id) {
    if (confirm("Are you sure you want to delete this doctor?")) {
        await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
        fetchDoctors();
    }
}

// Edit functionality using prompt
async function editDoctor(id) {
    const name = prompt("Enter new name:");
    const specialization = prompt("Enter new specialization:");
    const phone = prompt("Enter new phone:");
    const experience = prompt("Enter new experience (years):");

    const updatedDoctor = { name, specialization, phone, experience: parseInt(experience), doctorId: id };

    await fetch(`${apiUrl}/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(updatedDoctor)
    });

    fetchDoctors();
}
