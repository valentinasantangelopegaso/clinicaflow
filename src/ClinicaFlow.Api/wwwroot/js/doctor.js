// Script per l’area Medico di ClinicaFlow.
// Usa i DTO reali restituiti dal backend e decodifica gli stati appuntamento.

document.addEventListener('DOMContentLoaded', () => {
  const loginView = document.getElementById('doctor-login-view');
  const dashboardView = document.getElementById('doctor-dashboard-view');
  const loginForm = document.getElementById('doctor-login-form');
  const loginAlert = document.getElementById('doctor-login-alert');
  const doctorNameLabel = document.getElementById('doctor-name');
  const logoutBtn = document.getElementById('doctor-logout-btn');
  const appointmentsAlert = document.getElementById('doctor-appointments-alert');
  const appointmentsTableBody = document.querySelector('#doctor-appointments-table tbody');
  const reportsTableBody = document.querySelector('#doctor-reports-table tbody');
  const manageReportModalEl = document.getElementById('manageReportModal');
  const manageReportForm = document.getElementById('manage-report-form');
  const manageReportAlert = document.getElementById('manage-report-alert');

  let currentReportId = null;
  let appointmentsCache = [];

  const manageReportModal = manageReportModalEl ? new bootstrap.Modal(manageReportModalEl) : null;

  // Mostra la dashboard medico.
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

  // Legge un messaggio di errore HTTP.
  async function readErrorMessage(response, fallbackMessage) {
    try {
      const text = await response.text();
      if (!text) return fallbackMessage;

      try {
        const json = JSON.parse(text);
        if (json.title) return json.title;
        if (json.errors) return Object.values(json.errors).flat().join(' ');
      } catch {
        return text;
      }

      return text;
    } catch {
      return fallbackMessage;
    }
  }

  // Verifica una sessione medico già presente.
  const existingAuth = getAuthData();
  if (existingAuth && existingAuth.role === 'Doctor') {
    showDashboard();
    loadDoctorData(existingAuth);
  }

  // Gestione login medico.
  loginForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    loginAlert.innerHTML = '';

    const username = e.target.username.value.trim();
    const password = e.target.password.value.trim();

    try {
      const auth = await login(username, password);

      if (auth.role !== 'Doctor') {
        clearAuthData();
        showAlert(loginAlert, 'Ruolo non autorizzato per l\'area Medico.', 'danger');
        return;
      }

      showDashboard();
      await loadDoctorData(auth);
    } catch (err) {
      showAlert(loginAlert, err.message || 'Errore di autenticazione.', 'danger');
    }
  });

  // Gestione logout.
  logoutBtn?.addEventListener('click', () => {
    logout();
  });

  // Carica dati medico, appuntamenti e referti.
  async function loadDoctorData(auth) {
    await loadDoctorDetails(auth.doctorId);
    await loadAppointments();
    await loadReports();
  }

  // Carica informazioni anagrafiche del medico autenticato.
  async function loadDoctorDetails(doctorId) {
    try {
      const response = await apiFetch(`/Doctors/${doctorId}`);
      const doctor = await response.json();
      doctorNameLabel.textContent = `${doctor.firstName} ${doctor.lastName}`;
    } catch (err) {
      console.error('Errore nel recupero delle informazioni del medico:', err);
    }
  }

  // Carica gli appuntamenti filtrati dal backend in base al ruolo Doctor.
  async function loadAppointments() {
    try {
      const response = await apiFetch('/Appointments');
      appointmentsCache = await response.json();
      appointmentsTableBody.innerHTML = '';

      appointmentsCache.forEach((appointment) => {
        const row = document.createElement('tr');
        const status = Number(appointment.status);
        let actionButtons = '';

        if (status === 0) {
          actionButtons += `<button type="button" class="btn btn-sm btn-success me-1" data-action="complete" data-id="${appointment.id}">Completa</button>`;
          actionButtons += `<button type="button" class="btn btn-sm btn-outline-danger" data-action="cancel" data-id="${appointment.id}">Annulla</button>`;
        }

        if (status === 1) {
          actionButtons += `<button type="button" class="btn btn-sm btn-outline-primary" data-action="report" data-id="${appointment.id}">Referto</button>`;
        }

        row.innerHTML = `
          <td>${formatDateTime(appointment.startTime)}</td>
          <td>${appointment.patientFullName}</td>
          <td>${statusText(appointment.status)}</td>
          <td>${valueOrDash(appointment.notes)}</td>
          <td>${actionButtons || '-'}</td>
        `;

        appointmentsTableBody.appendChild(row);
      });
    } catch (err) {
      console.error('Errore nel caricamento degli appuntamenti medico:', err);
      showAlert(appointmentsAlert, err.message || 'Errore nel caricamento degli appuntamenti.', 'danger');
    }
  }

  // Carica i referti relativi agli appuntamenti del medico.
  async function loadReports() {
    reportsTableBody.innerHTML = '';

    for (const appointment of appointmentsCache) {
      try {
        const response = await apiFetch(`/MedicalReports/by-appointment/${appointment.id}`);
        if (!response.ok) continue;

        const report = await response.json();
        const row = document.createElement('tr');
        row.innerHTML = `
          <td>${formatDateTime(appointment.startTime)}</td>
          <td>${appointment.patientFullName}</td>
          <td>${valueOrDash(report.diagnosis)}</td>
          <td>${valueOrDash(report.therapy)}</td>
          <td><button type="button" class="btn btn-sm btn-outline-primary" data-action="report" data-id="${appointment.id}">Visualizza</button></td>
        `;
        reportsTableBody.appendChild(row);
      } catch {
        // Referto assente: comportamento normale per appuntamenti non refertati.
      }
    }
  }

  // Gestione azioni tabella appuntamenti.
  appointmentsTableBody?.addEventListener('click', async (e) => {
    const button = e.target.closest('button');
    if (!button) return;

    const appointmentId = parseInt(button.getAttribute('data-id'), 10);
    const action = button.getAttribute('data-action');

    if (action === 'complete') {
      await updateAppointmentStatus(appointmentId, 1);
    } else if (action === 'cancel') {
      await updateAppointmentStatus(appointmentId, 2);
    } else if (action === 'report') {
      await openReportModal(appointmentId);
    }
  });

  // Gestione azioni tabella referti.
  reportsTableBody?.addEventListener('click', async (e) => {
    const button = e.target.closest('button');
    if (!button) return;

    const appointmentId = parseInt(button.getAttribute('data-id'), 10);
    await openReportModal(appointmentId);
  });

  // Aggiorna lo stato usando l'endpoint reale /Appointments/{id}/status.
  async function updateAppointmentStatus(appointmentId, status) {
    try {
      const response = await apiFetch(`/Appointments/${appointmentId}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status }),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nell\'aggiornamento dello stato.'));
      }

      await loadAppointments();
      await loadReports();
    } catch (err) {
      showAlert(appointmentsAlert, err.message || 'Errore nell\'aggiornamento dello stato.', 'danger');
    }
  }

  // Apre il modale di gestione referto per l'appuntamento indicato.
  async function openReportModal(appointmentId) {
    if (!manageReportModal) return;

    manageReportAlert.innerHTML = '';
    currentReportId = null;

    manageReportForm.appointmentId.value = appointmentId;
    manageReportForm.diagnosis.value = '';
    manageReportForm.therapy.value = '';
    manageReportForm.notes.value = '';

    try {
      const response = await apiFetch(`/MedicalReports/by-appointment/${appointmentId}`);
      if (response.ok) {
        const report = await response.json();
        currentReportId = report.id;
        manageReportForm.diagnosis.value = report.diagnosis || '';
        manageReportForm.therapy.value = report.therapy || '';
        manageReportForm.notes.value = report.notes || '';
      }
    } catch {
      // Referto assente: si apre il modale vuoto per la creazione.
    }

    manageReportModal.show();
  }

  // Salva il referto usando i campi reali del DTO: diagnosis, therapy, notes.
  manageReportForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    manageReportAlert.innerHTML = '';

    const data = {
      appointmentId: parseInt(manageReportForm.appointmentId.value, 10),
      diagnosis: manageReportForm.diagnosis.value.trim(),
      therapy: manageReportForm.therapy.value.trim() || null,
      notes: manageReportForm.notes.value.trim() || null,
    };

    try {
      const response = currentReportId
        ? await apiFetch(`/MedicalReports/${currentReportId}`, { method: 'PUT', body: JSON.stringify(data) })
        : await apiFetch('/MedicalReports', { method: 'POST', body: JSON.stringify(data) });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nel salvataggio del referto.'));
      }

      manageReportModal.hide();
      await loadAppointments();
      await loadReports();
    } catch (err) {
      showAlert(manageReportAlert, err.message || 'Errore nel salvataggio del referto.', 'danger');
    }
  });
});
