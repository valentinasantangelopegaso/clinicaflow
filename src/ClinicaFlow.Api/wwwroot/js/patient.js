// Script per l’area Paziente di ClinicaFlow.
// La visualizzazione usa i campi reali restituiti dagli AppointmentReadDto e MedicalReportReadDto.

document.addEventListener('DOMContentLoaded', () => {
  const loginView = document.getElementById('patient-login-view');
  const dashboardView = document.getElementById('patient-dashboard-view');
  const loginForm = document.getElementById('patient-login-form');
  const loginAlert = document.getElementById('patient-login-alert');
  const patientNameLabel = document.getElementById('patient-name');
  const logoutBtn = document.getElementById('patient-logout-btn');
  const appointmentsTableBody = document.querySelector('#patient-appointments-table tbody');
  const reportDetails = document.getElementById('patient-report-details');

  // Mostra la dashboard paziente.
  function showDashboard() {
    loginView.classList.add('d-none');
    dashboardView.classList.remove('d-none');
  }

  // Converte lo stato numerico dell'appuntamento in testo leggibile.
  function statusText(status) {
    const numericStatus = Number(status);
    switch (numericStatus) {
      case 0:
        return 'Pianificato';
      case 1:
        return 'Completato';
      case 2:
        return 'Annullato';
      default:
        return 'Sconosciuto';
    }
  }

  // Restituisce testo sicuro per valori vuoti.
  function valueOrDash(value) {
    return value === null || value === undefined || value === '' ? '-' : value;
  }

  const existingAuth = getAuthData();
  if (existingAuth && existingAuth.role === 'Patient') {
    showDashboard();
    loadPatientData(existingAuth);
  }

  // Login paziente.
  loginForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    loginAlert.innerHTML = '';

    const username = e.target.username.value.trim();
    const password = e.target.password.value.trim();

    try {
      const auth = await login(username, password);

      if (auth.role !== 'Patient') {
        clearAuthData();
        showAlert(loginAlert, 'Ruolo non autorizzato per l\'area Paziente.', 'danger');
        return;
      }

      showDashboard();
      await loadPatientData(auth);
    } catch (err) {
      showAlert(loginAlert, err.message || 'Errore di autenticazione.', 'danger');
    }
  });

  // Logout.
  logoutBtn?.addEventListener('click', () => {
    logout();
  });

  // Carica dati paziente e appuntamenti.
  async function loadPatientData(auth) {
    await loadPatientDetails(auth.patientId);
    await loadAppointments();
  }

  // Carica il profilo del paziente autenticato.
  async function loadPatientDetails(patientId) {
    try {
      const response = await apiFetch(`/Patients/${patientId}`);
      const patient = await response.json();
      patientNameLabel.textContent = `${patient.firstName} ${patient.lastName}`;
    } catch (err) {
      console.error('Errore nel recupero dati paziente:', err);
    }
  }

  // Carica appuntamenti filtrati dal backend in base al paziente autenticato.
  async function loadAppointments() {
    try {
      const response = await apiFetch('/Appointments');
      const appointments = await response.json();
      appointmentsTableBody.innerHTML = '';

      appointments.forEach((appointment) => {
        const row = document.createElement('tr');
        row.setAttribute('data-id', appointment.id);
        row.innerHTML = `
          <td>${formatDateTime(appointment.startTime)}</td>
          <td>${appointment.doctorFullName}</td>
          <td>${statusText(appointment.status)}</td>
        `;
        appointmentsTableBody.appendChild(row);
      });
    } catch (err) {
      console.error('Errore nel caricamento appuntamenti paziente:', err);
    }
  }

  // Carica il referto associato alla riga selezionata.
  appointmentsTableBody?.addEventListener('click', async (e) => {
    const row = e.target.closest('tr');
    if (!row) return;

    const appointmentId = row.getAttribute('data-id');
    await loadReportDetails(appointmentId);
  });

  // Mostra diagnosi, terapia e note del referto.
  async function loadReportDetails(appointmentId) {
    try {
      const response = await apiFetch(`/MedicalReports/by-appointment/${appointmentId}`);
      if (response.ok) {
        const report = await response.json();
        reportDetails.innerHTML = `
          <strong>Diagnosi:</strong> ${valueOrDash(report.diagnosis)}<br />
          <strong>Terapia:</strong> ${valueOrDash(report.therapy)}<br />
          <strong>Note:</strong> ${valueOrDash(report.notes)}<br />
          <small class="text-muted">Creato il ${formatDateTime(report.createdAt)}</small>
        `;
        return;
      }

      reportDetails.textContent = 'Referto non disponibile per questo appuntamento.';
    } catch {
      reportDetails.textContent = 'Referto non disponibile per questo appuntamento.';
    }
  }
});
