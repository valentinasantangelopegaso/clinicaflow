/*
 * patient.js
 *
 * Script dedicato alla pagina dell'area paziente. Gestisce l'accesso
 * tramite codice fiscale, il caricamento degli appuntamenti del
 * paziente e la visualizzazione dei referti associati. Non consente
 * alcuna modifica lato paziente: l'utente può soltanto consultare
 * l'elenco dei propri appuntamenti e visualizzare, se presente, il
 * referto medico correlato.
 */

document.addEventListener('DOMContentLoaded', () => {
  const loginSection = document.getElementById('patientLoginSection');
  const dashboard = document.getElementById('patientDashboard');
  const loginBtn = document.getElementById('patientLoginBtn');
  const loginAlert = document.getElementById('patientLoginAlert');
  const appointmentsAlert = document.getElementById('patientAppointmentsAlert');
  const appointmentsTableBody = document.querySelector('#patientAppointmentsTable tbody');
  const reportDetails = document.getElementById('patientReportDetails');
  const reportAppointmentInfo = document.getElementById('patientReportAppointmentInfo');
  const reportDiagnosis = document.getElementById('patientReportDiagnosis');
  const reportTherapy = document.getElementById('patientReportTherapy');
  const reportNotes = document.getElementById('patientReportNotes');
  const reportCreatedAt = document.getElementById('patientReportCreatedAt');
  const reportCloseBtn = document.getElementById('patientReportCloseBtn');

  let patientId = null;
  let allAppointments = [];
  let patientAppointments = [];
  let patientReports = [];

  /**
   * Mappa lo stato numerico dell'appuntamento in un testo leggibile in
   * italiano. Questa funzione replica la logica presente nel back
   * office e nell'area medica.
   *
   * @param {number} status Codice di stato.
   * @returns {string} Descrizione dello stato.
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
   * Gestisce il login del paziente richiamando l'API con il codice
   * fiscale. Se il paziente viene trovato, mostra la dashboard e
   * carica i suoi appuntamenti. In caso contrario mostra un errore.
   */
  async function handleLogin() {
    const taxCode = document.getElementById('patientTaxCode').value.trim();
    if (!taxCode) {
      showAlert(loginAlert, 'Inserisci un codice fiscale.');
      return;
    }
    try {
      const patient = await apiFetch(`/patients/by-taxcode/${taxCode}`);
      patientId = patient.id;
      document.getElementById('patientName').textContent = `${patient.firstName} ${patient.lastName}`;
      document.getElementById('patientBirthDate').textContent = formatDate(patient.birthDate);
      document.getElementById('patientEmail').textContent = patient.email || '';
      document.getElementById('patientPhone').textContent = patient.phone || '';
      loginSection.classList.add('d-none');
      dashboard.classList.remove('d-none');
      await loadData();
    } catch (err) {
      showAlert(loginAlert, err.message || 'Paziente non trovato.');
    }
  }

  /**
   * Carica tutti gli appuntamenti e i referti dal backend, quindi
   * seleziona solo quelli relativi al paziente. Gestisce anche
   * l'associazione tra referti e appuntamenti.
   */
  async function loadData() {
    try {
      const [appointments, reports] = await Promise.all([
        apiFetch('/appointments'),
        apiFetch('/medicalreports'),
      ]);
      allAppointments = appointments;
      patientAppointments = appointments.filter((a) => a.patientId === patientId);
      patientReports = reports.filter((r) => {
        const app = appointments.find((a) => a.id === r.appointmentId);
        return app && app.patientId === patientId;
      });
      renderAppointments();
    } catch (err) {
      showAlert(appointmentsAlert, `Errore nel caricamento dei dati: ${err.message}`);
    }
  }

  /**
   * Popola la tabella degli appuntamenti del paziente. Per ogni
   * appuntamento viene visualizzato un pulsante per consultare il
   * referto se esistente.
   */
  function renderAppointments() {
    appointmentsTableBody.innerHTML = '';
    patientAppointments.forEach((a) => {
      const report = patientReports.find((r) => r.appointmentId === a.id);
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${a.id}</td>
        <td>${a.doctorFullName}</td>
        <td>${formatDateTime(a.startTime)}</td>
        <td>${statusText(a.status)}</td>
        <td>${a.notes || ''}</td>
        <td></td>
      `;
      const actionsCell = tr.lastElementChild;
      if (report) {
        const viewBtn = document.createElement('button');
        viewBtn.className = 'btn btn-sm btn-outline-primary';
        viewBtn.textContent = 'Visualizza referto';
        viewBtn.addEventListener('click', () => showReport(report, a));
        actionsCell.appendChild(viewBtn);
      } else {
        actionsCell.textContent = '-';
      }
      appointmentsTableBody.appendChild(tr);
    });
  }

  /**
   * Mostra i dettagli di un referto selezionato. Compila le
   * informazioni nella card dedicata e la rende visibile.
   *
   * @param {Object} report Oggetto referto.
   * @param {Object} appointment Appuntamento associato.
   */
  function showReport(report, appointment) {
    reportAppointmentInfo.textContent = `${appointment.doctorFullName} - ${formatDateTime(appointment.startTime)}`;
    reportDiagnosis.textContent = report.diagnosis;
    reportTherapy.textContent = report.therapy;
    reportNotes.textContent = report.notes || '';
    reportCreatedAt.textContent = formatDateTime(report.createdAt);
    reportDetails.classList.remove('d-none');
    // Scorri verso la sezione per una migliore esperienza
    reportDetails.scrollIntoView({ behavior: 'smooth' });
  }

  /**
   * Nasconde la sezione dei dettagli del referto.
   */
  function hideReport() {
    reportDetails.classList.add('d-none');
  }

  // Associa gli handler agli elementi interattivi
  loginBtn.addEventListener('click', handleLogin);
  reportCloseBtn.addEventListener('click', hideReport);
});