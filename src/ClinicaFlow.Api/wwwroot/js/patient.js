// Script per l’area Paziente di ClinicaFlow
document.addEventListener('DOMContentLoaded', () => {
  const loginView = document.getElementById('patient-login-view');
  const dashboardView = document.getElementById('patient-dashboard-view');
  const loginForm = document.getElementById('patient-login-form');
  const loginAlert = document.getElementById('patient-login-alert');
  const patientNameLabel = document.getElementById('patient-name');
  const logoutBtn = document.getElementById('patient-logout-btn');
  const appointmentsTableBody = document.querySelector('#patient-appointments-table tbody');
  const reportDetails = document.getElementById('patient-report-details');

  function showDashboard() {
    loginView.classList.add('d-none');
    dashboardView.classList.remove('d-none');
  }

  // Verifica sessione esistente
  const existingAuth = getAuthData();
  if (existingAuth && existingAuth.role === 'Patient') {
    showDashboard();
    loadPatientData(existingAuth);
  }

  // Login submit
  loginForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    loginAlert.innerHTML = '';
    const username = e.target.username.value.trim();
    const password = e.target.password.value.trim();
    try {
      const auth = await login(username, password);
      if (auth.role !== 'Patient') {
        clearAuthData();
        showAlert(loginAlert, 'Ruolo non autorizzato per l\'area Paziente', 'danger');
        return;
      }
      showDashboard();
      loadPatientData(auth);
    } catch (err) {
      showAlert(loginAlert, err.message || 'Errore di autenticazione', 'danger');
    }
  });

  // Logout
  logoutBtn?.addEventListener('click', () => {
    logout();
  });

  /**
   * Carica i dati del paziente: dettagli e appuntamenti.
   * @param {Object} auth
   */
  async function loadPatientData(auth) {
    try {
      await loadPatientDetails(auth.patientId);
      await loadAppointments();
    } catch (err) {
      console.error(err);
    }
  }

  async function loadPatientDetails(patientId) {
    try {
      const resp = await apiFetch(`/patients/${patientId}`);
      const patient = await resp.json();
      patientNameLabel.textContent = `${patient.firstName} ${patient.lastName}`;
    } catch (err) {
      console.error('Errore nel recupero dati paziente:', err);
    }
  }

  async function loadAppointments() {
    try {
      const resp = await apiFetch('/appointments');
      const appointments = await resp.json();
      appointmentsTableBody.innerHTML = '';
      appointments.forEach((a) => {
        const tr = document.createElement('tr');
        tr.setAttribute('data-id', a.id);
        tr.innerHTML = `
          <td>${formatDateTime(a.dateTime)}</td>
          <td>${a.doctor?.firstName || ''} ${a.doctor?.lastName || ''}</td>
          <td>${a.status}</td>
        `;
        appointmentsTableBody.appendChild(tr);
      });
    } catch (err) {
      console.error('Errore nel caricamento appuntamenti paziente:', err);
    }
  }

  // Quando l'utente seleziona un appuntamento, carica il referto
  appointmentsTableBody?.addEventListener('click', async (e) => {
    const row = e.target.closest('tr');
    if (!row) return;
    const appointmentId = row.getAttribute('data-id');
    await loadReportDetails(appointmentId);
  });

  async function loadReportDetails(appointmentId) {
    try {
      const resp = await apiFetch(`/medicalreports/by-appointment/${appointmentId}`);
      if (resp.ok) {
        const report = await resp.json();
        reportDetails.innerHTML = `
          <strong>Diagnosi:</strong> ${report.diagnosis || ''}<br />
          <strong>Descrizione:</strong><br />
          <span>${report.description || ''}</span>
        `;
      } else {
        reportDetails.textContent = 'Referto non disponibile per questo appuntamento.';
      }
    } catch (err) {
      reportDetails.textContent = 'Referto non disponibile per questo appuntamento.';
    }
  }
});