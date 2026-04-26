// Script per l’area Medico di ClinicaFlow
document.addEventListener('DOMContentLoaded', () => {
  const loginView = document.getElementById('doctor-login-view');
  const dashboardView = document.getElementById('doctor-dashboard-view');
  const loginForm = document.getElementById('doctor-login-form');
  const loginAlert = document.getElementById('doctor-login-alert');
  const doctorNameLabel = document.getElementById('doctor-name');
  const logoutBtn = document.getElementById('doctor-logout-btn');
  const appointmentsTableBody = document.querySelector('#doctor-appointments-table tbody');
  const reportsTableBody = document.querySelector('#doctor-reports-table tbody');
  const manageReportModalEl = document.getElementById('manageReportModal');
  const manageReportForm = document.getElementById('manage-report-form');
  const manageReportAlert = document.getElementById('manage-report-alert');
  let currentReportId = null;

  // Inizializza modale con Bootstrap per poterlo controllare via JS
  let manageReportModal;
  if (typeof bootstrap !== 'undefined') {
    manageReportModal = new bootstrap.Modal(manageReportModalEl);
  }

  // Mostra la dashboard nascondendo il login
  function showDashboard() {
    loginView.classList.add('d-none');
    dashboardView.classList.remove('d-none');
  }

  // Controlla se esiste già un token per un medico loggato
  const existingAuth = getAuthData();
  if (existingAuth && existingAuth.role === 'Doctor') {
    showDashboard();
    loadDoctorData(existingAuth);
  }

  // Gestione login
  loginForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    loginAlert.innerHTML = '';
    const username = e.target.username.value.trim();
    const password = e.target.password.value.trim();
    try {
      const auth = await login(username, password);
      if (auth.role !== 'Doctor') {
        clearAuthData();
        showAlert(loginAlert, 'Ruolo non autorizzato per l\'area Medico', 'danger');
        return;
      }
      showDashboard();
      loadDoctorData(auth);
    } catch (err) {
      showAlert(loginAlert, err.message || 'Errore di autenticazione', 'danger');
    }
  });

  // Gestione logout
  logoutBtn?.addEventListener('click', () => {
    logout();
  });

  /**
   * Carica i dati del medico (info, appuntamenti e referti)
   * @param {Object} auth Oggetto di autenticazione contenente doctorId
   */
  async function loadDoctorData(auth) {
    try {
      await loadDoctorDetails(auth.doctorId);
      await loadAppointments();
      await loadReports();
    } catch (err) {
      console.error(err);
    }
  }

  /**
   * Carica le informazioni del medico e aggiorna l'intestazione.
   * @param {number} doctorId Identificativo del medico
   */
  async function loadDoctorDetails(doctorId) {
    try {
      const resp = await apiFetch(`/doctors/${doctorId}`);
      const doctor = await resp.json();
      doctorNameLabel.textContent = `${doctor.firstName} ${doctor.lastName}`;
    } catch (err) {
      console.error('Errore nel recupero delle informazioni del medico:', err);
    }
  }

  /**
   * Carica la lista degli appuntamenti del medico e popola la tabella con azioni.
   */
  async function loadAppointments() {
    try {
      const resp = await apiFetch('/appointments');
      const appointments = await resp.json();
      appointmentsTableBody.innerHTML = '';
      appointments.forEach((a) => {
        const tr = document.createElement('tr');
        // pulsanti azioni
        let actionBtns = '';
        if (a.status === 'Scheduled' || a.status === 'Prenotato') {
          actionBtns += `<button type="button" class="btn btn-sm btn-success me-1" data-action="complete" data-id="${a.id}">Completa</button>`;
          actionBtns += `<button type="button" class="btn btn-sm btn-danger me-1" data-action="cancel" data-id="${a.id}">Annulla</button>`;
        }
        actionBtns += `<button type="button" class="btn btn-sm btn-secondary" data-action="report" data-id="${a.id}">Referto</button>`;
        tr.innerHTML = `
          <td>${formatDateTime(a.dateTime)}</td>
          <td>${a.patient?.firstName || ''} ${a.patient?.lastName || ''}</td>
          <td>${a.status}</td>
          <td>${actionBtns}</td>
        `;
        appointmentsTableBody.appendChild(tr);
      });
    } catch (err) {
      console.error('Errore nel caricamento appuntamenti medico:', err);
    }
  }

  /**
   * Carica i referti del medico ricavandoli dagli appuntamenti.
   */
  async function loadReports() {
    try {
      const resp = await apiFetch('/appointments');
      const appointments = await resp.json();
      reportsTableBody.innerHTML = '';
      for (const a of appointments) {
        try {
          const repResp = await apiFetch(`/medicalreports/by-appointment/${a.id}`);
          if (repResp.ok) {
            const report = await repResp.json();
            const tr = document.createElement('tr');
            tr.innerHTML = `
              <td>${formatDateTime(a.dateTime)}</td>
              <td>${a.patient?.firstName || ''} ${a.patient?.lastName || ''}</td>
              <td>${report.diagnosis || ''}</td>
              <td><button type="button" class="btn btn-sm btn-secondary" data-action="report" data-id="${a.id}">Visualizza</button></td>
            `;
            reportsTableBody.appendChild(tr);
          }
        } catch (err) {
          // Nessun referto per questo appuntamento
        }
      }
    } catch (err) {
      console.error('Errore nel caricamento referti medico:', err);
    }
  }

  // Gestione clic sulle azioni nella tabella appuntamenti
  appointmentsTableBody?.addEventListener('click', async (e) => {
    const btn = e.target.closest('button');
    if (!btn) return;
    const id = btn.getAttribute('data-id');
    const action = btn.getAttribute('data-action');
    if (action === 'complete' || action === 'cancel') {
      await updateAppointmentStatus(id, action);
    } else if (action === 'report') {
      openReportModal(id);
    }
  });

  // Gestione clic sulle azioni nella tabella referti (per visualizzare/gestire referto)
  reportsTableBody?.addEventListener('click', (e) => {
    const btn = e.target.closest('button');
    if (!btn) return;
    const id = btn.getAttribute('data-id');
    openReportModal(id);
  });

  /**
   * Aggiorna lo stato di un appuntamento. Supporta completamento e cancellazione.
   * @param {number} appointmentId
   * @param {string} action 'complete' oppure 'cancel'
   */
  async function updateAppointmentStatus(appointmentId, action) {
    try {
      let url;
      if (action === 'complete') {
        url = `/appointments/${appointmentId}/complete`;
      } else if (action === 'cancel') {
        url = `/appointments/${appointmentId}/cancel`;
      } else {
        return;
      }
      const resp = await apiFetch(url, { method: 'PUT' });
      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg || 'Errore nell\'aggiornamento dello stato');
      }
      await loadAppointments();
      await loadReports();
    } catch (err) {
      showAlert(document.getElementById('doctor-appointments-alert'), err.message || 'Errore', 'danger');
    }
  }

  /**
   * Apre il modale per gestire il referto relativo all'appuntamento dato.
   * @param {number} appointmentId
   */
  async function openReportModal(appointmentId) {
    manageReportAlert.innerHTML = '';
    // Imposta l'id nell'input nascosto
    manageReportForm.appointmentId.value = appointmentId;
    // Resetta campi
    manageReportForm.diagnosis.value = '';
    manageReportForm.description.value = '';
    currentReportId = null;
    try {
      const resp = await apiFetch(`/medicalreports/by-appointment/${appointmentId}`);
      if (resp.ok) {
        const report = await resp.json();
        currentReportId = report.id;
        manageReportForm.diagnosis.value = report.diagnosis || '';
        manageReportForm.description.value = report.description || '';
      }
    } catch (err) {
      // nessun referto, si lascia la form vuota
    }
    manageReportModal.show();
  }

  // Salvataggio referto da modale
  manageReportForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    manageReportAlert.innerHTML = '';
    const appointmentId = parseInt(manageReportForm.appointmentId.value, 10);
    const data = {
      appointmentId: appointmentId,
      diagnosis: manageReportForm.diagnosis.value.trim(),
      description: manageReportForm.description.value.trim(),
    };
    try {
      let resp;
      if (currentReportId) {
        resp = await apiFetch(`/medicalreports/${currentReportId}`, {
          method: 'PUT',
          body: JSON.stringify(data),
        });
      } else {
        resp = await apiFetch('/medicalreports', {
          method: 'POST',
          body: JSON.stringify(data),
        });
      }
      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg || 'Errore nel salvataggio del referto');
      }
      manageReportModal.hide();
      await loadAppointments();
      await loadReports();
    } catch (err) {
      showAlert(manageReportAlert, err.message || 'Errore', 'danger');
    }
  });
});