const apiUrl = "http://localhost:5224/api/billings";
const appointmentsApiUrl = "http://localhost:5224/api/appointments";

let editBillingId = null;

document.addEventListener("DOMContentLoaded", () => {
    populateAppointmentsDropdown();
    fetchBillings();

    const consultationInput = document.getElementById("consultationFee");
    const medicineInput = document.getElementById("medicineCharges");
    const totalInput = document.getElementById("totalAmount");

    function updateTotal() {
        const consultation = parseFloat(consultationInput.value) || 0;
        const medicine = parseFloat(medicineInput.value) || 0;
        totalInput.value = consultation + medicine;
    }

    consultationInput.addEventListener("input", updateTotal);
    medicineInput.addEventListener("input", updateTotal);

    const form = document.getElementById("billingForm");
    form.addEventListener("submit", async (e) => {
        e.preventDefault();

        const billing = {
            appointmentId: parseInt(document.getElementById("appointmentId").value),
            consultationFee: parseFloat(consultationInput.value),
            medicineCharges: parseFloat(medicineInput.value),
            totalAmount: parseFloat(totalInput.value),
            paidAmount: parseFloat(document.getElementById("paidAmount").value),
            status: document.getElementById("status").value
        };

        const method = editBillingId ? "PUT" : "POST";
        const url = editBillingId ? `${apiUrl}/${editBillingId}` : apiUrl;

        try {
            const response = await fetch(url, {
                method: method,
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(editBillingId ? { ...billing, billingId: editBillingId } : billing)
            });

            if (!response.ok) throw new Error(`Server Error: ${response.status}`);

            form.reset();
            totalInput.value = "";
            editBillingId = null;
            fetchBillings();
        } catch (error) {
            console.error("Error saving billing:", error);
            alert("Failed to save billing. Check console for details.");
        }
    });
});

async function populateAppointmentsDropdown() {
    const dropdown = document.getElementById("appointmentId");
    dropdown.innerHTML = '<option value="">Select Appointment</option>';

    try {
        const response = await fetch(appointmentsApiUrl);
        const appointments = await response.json();
        appointments.forEach(a => {
            const option = document.createElement("option");
            option.value = a.appointmentId;
            option.text = `ID:${a.appointmentId} | Patient: ${a.patientName} | Doctor: ${a.doctorName} | Date: ${new Date(a.appointmentDate).toLocaleDateString()}`;
            dropdown.appendChild(option);
        });
    } catch (error) {
        console.error("Error fetching appointments:", error);
    }
}

async function fetchBillings() {
    const tableBody = document.querySelector("#billingsTable tbody");
    tableBody.innerHTML = "";

    try {
        const response = await fetch(apiUrl);
        const billings = await response.json();

        billings.forEach(b => {
            const row = document.createElement("tr");
            row.innerHTML = `
                <td>${b.billingId}</td>
                <td>ID:${b.appointmentId} | Patient: ${b.patientName} | Doctor: ${b.doctorName}</td>
                <td>${b.consultationFee}</td>
                <td>${b.medicineCharges}</td>
                <td>${b.totalAmount}</td>
                <td>${b.paidAmount}</td>
                <td>${b.status}</td>
                <td>
                    <button class="edit-btn" onclick="startEdit(${b.billingId}, ${b.appointmentId}, ${b.consultationFee}, ${b.medicineCharges}, ${b.paidAmount}, '${b.status}')">Edit</button>
                    <button class="delete-btn" onclick="deleteBilling(${b.billingId})">Delete</button>
                </td>
            `;
            tableBody.appendChild(row);
        });
    } catch (error) {
        console.error("Error fetching billings:", error);
    }
}

function startEdit(id, appointmentId, consultationFee, medicineCharges, paidAmount, status) {
    editBillingId = id;
    document.getElementById("appointmentId").value = appointmentId;
    document.getElementById("consultationFee").value = consultationFee;
    document.getElementById("medicineCharges").value = medicineCharges;
    document.getElementById("totalAmount").value = consultationFee + medicineCharges;
    document.getElementById("paidAmount").value = paidAmount;
    document.getElementById("status").value = status;
}

async function deleteBilling(id) {
    if (confirm("Are you sure you want to delete this billing?")) {
        await fetch(`${apiUrl}/${id}`, { method: "DELETE" });
        fetchBillings();
    }
}
