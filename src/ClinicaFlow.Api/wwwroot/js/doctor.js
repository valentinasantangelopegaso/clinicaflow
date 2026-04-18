/*
 * doctor.js
 *
 * Script dedicato alla pagina dell'area medica. Gestisce il login
 * tramite codice fiscale, il caricamento degli appuntamenti e dei
 * referti associati al medico autenticato, nonché la possibilità di
 * completare o annullare appuntamenti e creare o modificare referti.
 * Utilizza le funzioni di utilità definite in common.js per le
 * chiamate alle API e la formattazione.
 */

document.addEventListener('DOMContentLoaded', () => {
  const loginSection = document.getElementById('loginSection');
  const dashboard = document.getElementById('doctorDashboard');
  const loginBtn = document.getElementById('doctorLoginBtn');
  const loginAlert = document.getElementById('doctorLoginAlert');
  const appointmentsAlert = document.getElementById('doctorAppointmentsAlert');
  const reportsAlert = document.getElementById('doctorReportsAlert');
  const appointmentsTableBody = document.querySelector('#doctorAppointmentsTable tbody');
  const reportsTableBody = document.querySelector('#doctorReportsTable tbody');
  const reportEditor = document.getElementById('reportEditor');
  const reportForm = document.getElementById('doctorReportForm');
  const reportCancelBtn = document.getElementById('reportCancelBtn');
  const reportAppointmentInfo = document.getElementById('reportAppointmentInfo');

  let doctorId = null;
  let allAppointments = [];
  let doctorAppointments = [];
  let doctorReports = [];
  let currentReportId = null;
  let currentAppointmentId = null;

  /**
   * Mappa lo stato numerico dell'appuntamento in una descrizione in italiano.
   * @param {number} status Codice di stato.
   * @returns {string} Testo descrittivo.
   */
  function statusText(status) {
    switch (status) {
      case 0:
        return 'Pianificato';
      case 1:
        return 'Completato';
      case 2:
        return 'Annullato';
      default:
        return '';
    }
  }

  /**
   * Gestisce l'evento di login del medico. Valida il codice fiscale,
   * richiama l'API e, in caso di successo, carica i dati e mostra
   * la dashboard. In caso di errore mostra l'allerta.
   */
  async function handleLogin() {
    const taxCode = document.getElementById('doctorTaxCode').value.trim();
    if (!taxCode) {
      showAlert(loginAlert, 'Inserisci un codice fiscale.');
      return;
    }
    try {
      const doctor = await apiFetch(`/doctors/by-taxcode/${taxCode}`);
      doctorId = doctor.id;
      document.getElementById('doctorName').textContent = `${doctor.firstName} ${doctor.lastName}`;
      document.getElementById('doctorSpecialty').textContent = doctor.specialtyName;
      loginSection.classList.add('d-none');
      dashboard.classList.remove('d-none');
      await loadData();
    } catch (err) {
      showAlert(loginAlert, err.message || 'Medico non trovato.');
    }
  }

  /**
   * Carica le liste complete di appuntamenti e referti quindi filtra
   * solamente quelli relativi al medico autenticato. Se si verificano
   * errori, una notifica viene visualizzata.
   */
  async function loadData() {
    try {
      const [appointments, reports] = await Promise.all([
        apiFetch('/appointments'),
        apiFetch('/medicalreports'),
      ]);
      allAppointments = appointments;
      doctorAppointments = appointments.filter((a) => a.doctorId === doctorId);
      // Filtra i referti associati ad appuntamenti del medico
      doctorReports = reports.filter((r) => {
        const app = appointments.find((a) => a.id === r.appointmentId);
        return app && app.doctorId === doctorId;
      });
      renderAppointments();
      renderReports();
    } catch (err) {
      showAlert(appointmentsAlert, `Errore nel caricamento dei dati: ${err.message}`);
    }
  }

  /**
   * Costruisce la tabella degli appuntamenti del medico con le azioni
   * disponibili in base allo stato (completamento, annullamento,
   * gestione referto).
   */
  function renderAppointments() {
    appointmentsTableBody.innerHTML = '';
    doctorAppointments.forEach((a) => {
      const tr = document.createElement('tr');
      const status = statusText(a.status);
      // Verifica se esiste un referto per l'appuntamento
      const report = doctorReports.find((r) => r.appointmentId === a.id);
      tr.innerHTML = `
        <td>${a.id}</td>
        <td>${a.patientFullName}</td>
        <td>${formatDateTime(a.startTime)}</td>
        <td>${status}</td>
        <td>${a.notes || ''}</td>
        <td></td>
      `;
      const actionsCell = tr.lastElementChild;
      // Azioni in base allo stato
      if (a.status === 0) {
        // Appuntamento pianificato: completa o annulla
        const completeBtn = document.createElement('button');
        completeBtn.className = 'btn btn-sm btn-success me-1';
        completeBtn.textContent = 'Completa';
        completeBtn.addEventListener('click', () => updateAppointmentStatus(a.id, 1));
        const cancelBtn = document.createElement('button');
        cancelBtn.className = 'btn btn-sm btn-danger';
        cancelBtn.textContent = 'Annulla';
        cancelBtn.addEventListener('click', () => updateAppointmentStatus(a.id, 2));
        actionsCell.appendChild(completeBtn);
        actionsCell.appendChild(cancelBtn);
      } else if (a.status === 1) {
        // Appuntamento completato: gestisci referto
        const refertoBtn = document.createElement('button');
        refertoBtn.className = 'btn btn-sm btn-outline-primary';
        refertoBtn.textContent = report ? 'Modifica referto' : 'Crea referto';
        refertoBtn.addEventListener('click', () => openReportEditor(a, report));
        actionsCell.appendChild(refertoBtn);
      } else {
        // Annullato: nessuna azione
        actionsCell.textContent = '-';
      }
      appointmentsTableBody.appendChild(tr);
    });
  }

  /**
   * Costruisce la tabella dei referti relativi al medico. Ogni riga
   * contiene informazioni sull'appuntamento e sui contenuti del referto.
   */
  function renderReports() {
    reportsTableBody.innerHTML = '';
    doctorReports.forEach((r) => {
      const app = doctorAppointments.find((a) => a.id === r.appointmentId);
      if (!app) return;
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${r.id}</td>
        <td>${app.patientFullName}</td>
        <td>${formatDateTime(app.startTime)}</td>
        <td>${r.diagnosis}</td>
        <td>${r.therapy}</td>
        <td>${r.notes || ''}</td>
        <td>${formatDateTime(r.createdAt)}</td>
      `;
      reportsTableBody.appendChild(tr);
    });
  }

  /**
   * Aggiorna lo stato di un appuntamento chiamando l'endpoint
   * corrispondente. Dopo l'operazione ricarica i dati per
   * aggiornare l'interfaccia.
   *
   * @param {number} id Identificativo dell'appuntamento.
   * @param {number} status Nuovo stato da impostare (1=Completato, 2=Annullato).
   */
  async function updateAppointmentStatus(id, status) {
    if (status === 2 && !confirm('Sei sicuro di voler annullare questo appuntamento?')) {
      return;
    }
    try {
      await apiFetch(`/appointments/${id}/status`, {
        method: 'PUT',
        body: JSON.stringify({ status }),
      });
      showAlert(appointmentsAlert, 'Stato appuntamento aggiornato correttamente.', 'success');
      await loadData();
    } catch (err) {
      showAlert(appointmentsAlert, err.message || 'Errore durante l\'aggiornamento dello stato.');
    }
  }

  /**
   * Mostra l'editor referto per un dato appuntamento. Se esiste già un
   * referto, popola il form con i dati da modificare; altrimenti
   * azzera i campi per la creazione. L'identificativo dell'appuntamento
   * viene memorizzato per l'invio.
   *
   * @param {Object} appointment Oggetto appuntamento.
   * @param {Object|null} report Referto esistente, se presente.
   */
  function openReportEditor(appointment, report) {
    currentAppointmentId = appointment.id;
    if (report) {
      currentReportId = report.id;
      reportForm.diagnosis.value = report.diagnosis;
      reportForm.therapy.value = report.therapy;
      reportForm.notes.value = report.notes || '';
    } else {
      currentReportId = null;
      reportForm.reset();
    }
    reportAppointmentInfo.textContent = `${appointment.patientFullName} - ${formatDateTime(appointment.startTime)}`;
    reportEditor.classList.remove('d-none');
    // Scorri fino al form per una migliore UX
    window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' });
  }

  /**
   * Invia i dati del referto al backend. Se esiste già un referto
   * (currentReportId non nullo) effettua una PUT, altrimenti una POST.
   * Dopo il salvataggio ricarica i dati e nasconde l'editor.
   */
  async function handleReportSubmit(event) {
    event.preventDefault();
    const dto = {
      appointmentId: currentAppointmentId,
      diagnosis: reportForm.diagnosis.value.trim(),
      therapy: reportForm.therapy.value.trim(),
      notes: reportForm.notes.value.trim() || null,
    };
    try {
      if (currentReportId) {
        await apiFetch(`/medicalreports/${currentReportId}`, {
          method: 'PUT',
          body: JSON.stringify(dto),
        });
        showAlert(reportsAlert, 'Referto aggiornato con successo.', 'success');
      } else {
        await apiFetch('/medicalreports', {
          method: 'POST',
          body: JSON.stringify(dto),
        });
        showAlert(reportsAlert, 'Referto creato con successo.', 'success');
      }
      reportEditor.classList.add('d-none');
      await loadData();
    } catch (err) {
      showAlert(reportsAlert, err.message || 'Errore nella gestione del referto.');
    }
  }

  /**
   * Annulla la modifica del referto nascondendo l'editor e resettando
   * i campi.
   */
  function handleReportCancel() {
    reportForm.reset();
    reportEditor.classList.add('d-none');
    currentReportId = null;
    currentAppointmentId = null;
  }

  // Bind eventi
  loginBtn.addEventListener('click', handleLogin);
  reportForm.addEventListener('submit', handleReportSubmit);
  reportCancelBtn.addEventListener('click', handleReportCancel);
});