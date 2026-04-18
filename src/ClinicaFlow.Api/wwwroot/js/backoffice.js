/*
 * backoffice.js
 *
 * Script dedicato alla pagina di Back Office. Gestisce il caricamento
 * delle tabelle e l'invio dei form per le principali entità del
 * sistema: pazienti, medici, specializzazioni, slot di disponibilità,
 * appuntamenti e referti. Sfrutta le funzioni di utilità definite in
 * common.js per le chiamate API e la formattazione.
 */

// Chiave di sessione e codice demo per l'accesso simulato back office.
const backofficeSessionKey = 'clinicaflow_backoffice_auth';
const backofficeDemoCode = 'BACKOFFICE2026';
let backofficeInitialized = false;

// Esecuzione al caricamento della pagina
document.addEventListener('DOMContentLoaded', () => {
  const loginSection = document.getElementById('backofficeLoginSection');
  const dashboardSection = document.getElementById('backofficeDashboard');
  const loginButton = document.getElementById('backofficeLoginButton');
  const logoutButton = document.getElementById('backofficeLogoutButton');
  const accessCodeInput = document.getElementById('backofficeAccessCode');
  const loginAlert = document.getElementById('backofficeLoginAlert');

  function showDashboard() {
    loginSection.classList.add('d-none');
    dashboardSection.classList.remove('d-none');
    initializeBackofficeDashboard();
  }

  function showLogin() {
    dashboardSection.classList.add('d-none');
    loginSection.classList.remove('d-none');
    accessCodeInput.value = '';
  }

  function handleLogin() {
    const code = accessCodeInput.value.trim().toUpperCase();

    if (!code) {
      showAlert(loginAlert, 'Inserisci il codice di accesso del back office.');
      return;
    }

    if (code !== backofficeDemoCode) {
      showAlert(loginAlert, 'Codice di accesso non valido.');
      return;
    }

    sessionStorage.setItem(backofficeSessionKey, 'true');
    loginAlert.innerHTML = '';
    showDashboard();
  }

  function handleLogout() {
    sessionStorage.removeItem(backofficeSessionKey);
    showLogin();
  }

  loginButton.addEventListener('click', handleLogin);
  logoutButton.addEventListener('click', handleLogout);
  accessCodeInput.addEventListener('keydown', (event) => {
    if (event.key === 'Enter') {
      event.preventDefault();
      handleLogin();
    }
  });

  if (sessionStorage.getItem(backofficeSessionKey) === 'true') {
    showDashboard();
  } else {
    showLogin();
  }
});

function initializeBackofficeDashboard() {
  if (backofficeInitialized) {
    return;
  }

  backofficeInitialized = true;

  // Carica dati iniziali
  loadSpecialties().then(() => {
    loadDoctors();
    loadSlots();
  });
  loadPatients();
  loadAppointments();
  loadReports();

  // Gestione form paziente
  const patientForm = document.getElementById('patientForm');
  patientForm.addEventListener('submit', handlePatientSubmit);
  document.getElementById('patientCancel').addEventListener('click', () => resetForm(patientForm));

  // Gestione form medico
  const doctorForm = document.getElementById('doctorForm');
  doctorForm.addEventListener('submit', handleDoctorSubmit);
  document.getElementById('doctorCancel').addEventListener('click', () => resetForm(doctorForm));

  // Gestione form specializzazione
  const specialtyForm = document.getElementById('specialtyForm');
  specialtyForm.addEventListener('submit', handleSpecialtySubmit);
  document.getElementById('specialtyCancel').addEventListener('click', () => resetForm(specialtyForm));

  // Gestione form slot
  const slotForm = document.getElementById('slotForm');
  slotForm.addEventListener('submit', handleSlotSubmit);
  document.getElementById('slotCancel').addEventListener('click', () => resetForm(slotForm));

  // Gestione form appuntamento
  const appointmentForm = document.getElementById('appointmentForm');
  appointmentForm.addEventListener('submit', handleAppointmentSubmit);
  document.getElementById('appointmentCancel').addEventListener('click', () => resetForm(appointmentForm));

  // Gestione form referto
  const reportForm = document.getElementById('reportForm');
  reportForm.addEventListener('submit', handleReportSubmit);
  document.getElementById('reportCancel').addEventListener('click', () => resetForm(reportForm));
}

/* ===== Pazienti ===== */

async function loadPatients() {
  try {
    const patients = await apiFetch('/patients');
    const tbody = document.querySelector('#patientsTable tbody');
    tbody.innerHTML = '';
    patients.forEach((p) => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${p.id}</td>
        <td>${p.firstName}</td>
        <td>${p.lastName}</td>
        <td>${p.taxCode}</td>
        <td>${formatDate(p.birthDate)}</td>
        <td>${p.phone || ''}</td>
        <td>${p.email || ''}</td>
        <td>
          <button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${p.id}">Modifica</button>
          <button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${p.id}">Elimina</button>
        </td>`;
      tbody.appendChild(tr);
    });
    // Aggiunge listener sui pulsanti
    tbody.querySelectorAll('button').forEach((btn) => {
      const id = btn.getAttribute('data-id');
      const action = btn.getAttribute('data-action');
      if (action === 'edit') {
        btn.addEventListener('click', () => editPatient(id));
      } else if (action === 'delete') {
        btn.addEventListener('click', () => deletePatient(id));
      }
    });
  } catch (err) {
    showAlert(document.getElementById('patientsAlert'), `Errore nel caricamento pazienti: ${err.message}`);
  }
}

async function handlePatientSubmit(event) {
  event.preventDefault();
  const form = event.target;
  const id = form.querySelector('input[name="id"]').value;
  const data = {
    firstName: form.firstName.value.trim(),
    lastName: form.lastName.value.trim(),
    taxCode: form.taxCode.value.trim(),
    birthDate: form.birthDate.value,
    phone: form.phone.value.trim() || null,
    email: form.email.value.trim() || null,
  };
  try {
    if (id) {
      // update
      await apiFetch(`/patients/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('patientsAlert'), 'Paziente aggiornato correttamente.', 'success');
    } else {
      // create
      await apiFetch('/patients', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('patientsAlert'), 'Paziente creato correttamente.', 'success');
    }
    resetForm(form);
    loadPatients();
  } catch (err) {
    showAlert(document.getElementById('patientsAlert'), err.message);
  }
}

async function editPatient(id) {
  try {
    const p = await apiFetch(`/patients/${id}`);
    const form = document.getElementById('patientForm');
    form.querySelector('input[name="id"]').value = p.id;
    form.firstName.value = p.firstName;
    form.lastName.value = p.lastName;
    form.taxCode.value = p.taxCode;
    form.birthDate.value = p.birthDate.substring(0, 10);
    form.phone.value = p.phone || '';
    form.email.value = p.email || '';
    document.getElementById('patientSubmit').textContent = 'Aggiorna';
  } catch (err) {
    showAlert(document.getElementById('patientsAlert'), `Impossibile caricare paziente: ${err.message}`);
  }
}

async function deletePatient(id) {
  if (!confirm('Sei sicuro di voler eliminare questo paziente?')) return;
  try {
    await apiFetch(`/patients/${id}`, { method: 'DELETE' });
    showAlert(document.getElementById('patientsAlert'), 'Paziente eliminato con successo.', 'success');
    loadPatients();
  } catch (err) {
    showAlert(document.getElementById('patientsAlert'), err.message);
  }
}

/* ===== Medici ===== */

async function loadDoctors() {
  try {
    const doctors = await apiFetch('/doctors');
    const tbody = document.querySelector('#doctorsTable tbody');
    tbody.innerHTML = '';
    doctors.forEach((d) => {
      const tr = document.createElement('tr');
      tr.innerHTML = `
        <td>${d.id}</td>
        <td>${d.firstName}</td>
        <td>${d.lastName}</td>
        <td>${d.taxCode}</td>
        <td>${d.specialtyName}</td>
        <td>
          <button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${d.id}">Modifica</button>
          <button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${d.id}">Elimina</button>
        </td>`;
      tbody.appendChild(tr);
    });
    tbody.querySelectorAll('button').forEach((btn) => {
      const id = btn.getAttribute('data-id');
      const action = btn.getAttribute('data-action');
      if (action === 'edit') {
        btn.addEventListener('click', () => editDoctor(id));
      } else if (action === 'delete') {
        btn.addEventListener('click', () => deleteDoctor(id));
      }
    });
    // Aggiorna la select dei dottori negli slot
    populateDoctorSelects(doctors);
  } catch (err) {
    showAlert(document.getElementById('doctorsAlert'), `Errore nel caricamento medici: ${err.message}`);
  }
}

async function handleDoctorSubmit(event) {
  event.preventDefault();
  const form = event.target;
  const id = form.querySelector('input[name="id"]').value;
  const data = {
    firstName: form.firstName.value.trim(),
    lastName: form.lastName.value.trim(),
    taxCode: form.taxCode.value.trim(),
    specialtyId: parseInt(form.specialtyId.value),
  };
  try {
    if (id) {
      await apiFetch(`/doctors/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('doctorsAlert'), 'Medico aggiornato correttamente.', 'success');
    } else {
      await apiFetch('/doctors', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('doctorsAlert'), 'Medico creato correttamente.', 'success');
    }
    resetForm(form);
    document.getElementById('doctorSubmit').textContent = 'Salva';
    // Ricarica specialità in caso di nuove.
    await loadSpecialties();
    await loadDoctors();
    await loadSlots();
  } catch (err) {
    showAlert(document.getElementById('doctorsAlert'), err.message);
  }
}

async function editDoctor(id) {
  try {
    const d = await apiFetch(`/doctors/${id}`);
    const form = document.getElementById('doctorForm');
    form.querySelector('input[name="id"]').value = d.id;
    form.firstName.value = d.firstName;
    form.lastName.value = d.lastName;
    form.taxCode.value = d.taxCode;
    form.specialtyId.value = d.specialtyId;
    document.getElementById('doctorSubmit').textContent = 'Aggiorna';
  } catch (err) {
    showAlert(document.getElementById('doctorsAlert'), `Impossibile caricare medico: ${err.message}`);
  }
}

async function deleteDoctor(id) {
  if (!confirm('Sei sicuro di voler eliminare questo medico?')) return;
  try {
    await apiFetch(`/doctors/${id}`, { method: 'DELETE' });
    showAlert(document.getElementById('doctorsAlert'), 'Medico eliminato con successo.', 'success');
    await loadDoctors();
    await loadSlots();
  } catch (err) {
    showAlert(document.getElementById('doctorsAlert'), err.message);
  }
}

/* ===== Specializzazioni ===== */

async function loadSpecialties() {
  try {
    const specialties = await apiFetch('/specialties');
    const tbody = document.querySelector('#specialtiesTable tbody');
    if (tbody) {
      tbody.innerHTML = '';
      specialties.forEach((s) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${s.id}</td>
          <td>${s.name}</td>
          <td>
            <button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${s.id}">Modifica</button>
            <button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${s.id}">Elimina</button>
          </td>`;
        tbody.appendChild(tr);
      });
      tbody.querySelectorAll('button').forEach((btn) => {
        const id = btn.getAttribute('data-id');
        const action = btn.getAttribute('data-action');
        if (action === 'edit') {
          btn.addEventListener('click', () => editSpecialty(id));
        } else if (action === 'delete') {
          btn.addEventListener('click', () => deleteSpecialty(id));
        }
      });
    }
    // Aggiorna la select delle specialità nel form medico
    const select = document.getElementById('doctorSpecialty');
    if (select) {
      // Mantieni valore corrente se presente
      const current = select.value;
      select.innerHTML = '<option value="">Seleziona…</option>';
      specialties.forEach((s) => {
        const option = document.createElement('option');
        option.value = s.id;
        option.textContent = s.name;
        select.appendChild(option);
      });
      if (current) select.value = current;
    }
    return specialties;
  } catch (err) {
    showAlert(document.getElementById('specialtiesAlert'), `Errore nel caricamento specializzazioni: ${err.message}`);
    return [];
  }
}

async function handleSpecialtySubmit(event) {
  event.preventDefault();
  const form = event.target;
  const id = form.querySelector('input[name="id"]').value;
  const data = { name: form.name.value.trim() };
  try {
    if (id) {
      await apiFetch(`/specialties/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('specialtiesAlert'), 'Specializzazione aggiornata correttamente.', 'success');
    } else {
      await apiFetch('/specialties', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('specialtiesAlert'), 'Specializzazione creata correttamente.', 'success');
    }
    resetForm(form);
    document.getElementById('specialtySubmit').textContent = 'Salva';
    await loadSpecialties();
    await loadDoctors();
  } catch (err) {
    showAlert(document.getElementById('specialtiesAlert'), err.message);
  }
}

async function editSpecialty(id) {
  try {
    const s = await apiFetch(`/specialties/${id}`);
    const form = document.getElementById('specialtyForm');
    form.querySelector('input[name="id"]').value = s.id;
    form.name.value = s.name;
    document.getElementById('specialtySubmit').textContent = 'Aggiorna';
  } catch (err) {
    showAlert(document.getElementById('specialtiesAlert'), `Impossibile caricare specializzazione: ${err.message}`);
  }
}

async function deleteSpecialty(id) {
  if (!confirm('Sei sicuro di voler eliminare questa specializzazione?')) return;
  try {
    await apiFetch(`/specialties/${id}`, { method: 'DELETE' });
    showAlert(document.getElementById('specialtiesAlert'), 'Specializzazione eliminata con successo.', 'success');
    await loadSpecialties();
    await loadDoctors();
  } catch (err) {
    showAlert(document.getElementById('specialtiesAlert'), err.message);
  }
}

/* ===== Slot disponibilità ===== */

async function loadSlots() {
  try {
    const slots = await apiFetch('/availabilityslots');
    const tbody = document.querySelector('#slotsTable tbody');
    if (tbody) {
      tbody.innerHTML = '';
      slots.forEach((s) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${s.id}</td>
          <td>${s.doctorFullName}</td>
          <td>${formatDateTime(s.startTime)}</td>
          <td>${formatDateTime(s.endTime)}</td>
          <td>${s.isAvailable ? 'Sì' : 'No'}</td>
          <td>
            <button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${s.id}" ${!s.isAvailable ? 'disabled' : ''}>Modifica</button>
            <button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${s.id}" ${!s.isAvailable ? 'disabled' : ''}>Elimina</button>
          </td>`;
        tbody.appendChild(tr);
      });
      tbody.querySelectorAll('button').forEach((btn) => {
        const id = btn.getAttribute('data-id');
        const action = btn.getAttribute('data-action');
        if (action === 'edit') {
          btn.addEventListener('click', () => editSlot(id));
        } else if (action === 'delete') {
          btn.addEventListener('click', () => deleteSlot(id));
        }
      });
    }
    // Aggiorna select slot disponibili nella form appuntamenti
    populateAvailableSlotSelect(slots);
  } catch (err) {
    showAlert(document.getElementById('slotsAlert'), `Errore nel caricamento slot: ${err.message}`);
  }
}

async function handleSlotSubmit(event) {
  event.preventDefault();
  const form = event.target;
  const id = form.querySelector('input[name="id"]').value;
  const data = {
    doctorId: parseInt(form.doctorId.value),
    startTime: form.startTime.value,
    endTime: form.endTime.value,
  };
  try {
    if (new Date(data.endTime) <= new Date(data.startTime)) {
      throw new Error("L'orario di fine deve essere successivo all'orario di inizio.");
    }
    if (id) {
      await apiFetch(`/availabilityslots/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('slotsAlert'), 'Slot aggiornato correttamente.', 'success');
    } else {
      await apiFetch('/availabilityslots', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('slotsAlert'), 'Slot creato correttamente.', 'success');
    }
    resetForm(form);
    document.getElementById('slotSubmit').textContent = 'Salva';
    await loadSlots();
  } catch (err) {
    showAlert(document.getElementById('slotsAlert'), err.message);
  }
}

async function editSlot(id) {
  try {
    const s = await apiFetch(`/availabilityslots/${id}`);
    const form = document.getElementById('slotForm');
    form.querySelector('input[name="id"]').value = s.id;
    form.doctorId.value = s.doctorId;
    // Converte in formato yyyy-MM-ddTHH:mm
    form.startTime.value = s.startTime.substring(0, 16);
    form.endTime.value = s.endTime.substring(0, 16);
    document.getElementById('slotSubmit').textContent = 'Aggiorna';
  } catch (err) {
    showAlert(document.getElementById('slotsAlert'), `Impossibile caricare slot: ${err.message}`);
  }
}

async function deleteSlot(id) {
  if (!confirm('Sei sicuro di voler eliminare questo slot disponibile?')) return;
  try {
    await apiFetch(`/availabilityslots/${id}`, { method: 'DELETE' });
    showAlert(document.getElementById('slotsAlert'), 'Slot eliminato con successo.', 'success');
    await loadSlots();
  } catch (err) {
    showAlert(document.getElementById('slotsAlert'), err.message);
  }
}

/* ===== Appuntamenti ===== */

async function loadAppointments() {
  try {
    const appointments = await apiFetch('/appointments');
    const tbody = document.querySelector('#appointmentsTable tbody');
    if (tbody) {
      tbody.innerHTML = '';
      appointments.forEach((a) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${a.id}</td>
          <td>${a.patientFullName}</td>
          <td>${a.doctorFullName}</td>
          <td>${formatDateTime(a.startTime)}</td>
          <td>${formatDateTime(a.endTime)}</td>
          <td>${statusText(a.status)}</td>
          <td>${a.notes || ''}</td>
          <td>
            <button class="btn btn-sm btn-outline-success me-1" data-action="complete" data-id="${a.id}" ${a.status !== 0 ? 'disabled' : ''}>Completa</button>
            <button class="btn btn-sm btn-outline-danger" data-action="cancel" data-id="${a.id}" ${a.status !== 0 ? 'disabled' : ''}>Annulla</button>
          </td>`;
        tbody.appendChild(tr);
      });
      tbody.querySelectorAll('button').forEach((btn) => {
        const id = btn.getAttribute('data-id');
        const action = btn.getAttribute('data-action');
        if (action === 'complete') {
          btn.addEventListener('click', () => updateAppointmentStatus(id, 1));
        } else if (action === 'cancel') {
          btn.addEventListener('click', () => updateAppointmentStatus(id, 2));
        }
      });
    }
    // Aggiorna select dei pazienti e slot disponibili nella form
    await populatePatientSelect();
    const slots = await apiFetch('/availabilityslots?onlyAvailable=true');
    populateAvailableSlotSelect(slots);
    // Aggiorna select appuntamento nei referti con completati
    populateCompletedAppointmentSelect(appointments);
  } catch (err) {
    showAlert(document.getElementById('appointmentsAlert'), `Errore nel caricamento appuntamenti: ${err.message}`);
  }
}

async function handleAppointmentSubmit(event) {
  event.preventDefault();
  const form = event.target;
  const id = form.querySelector('input[name="id"]').value;
  const data = {
    patientId: parseInt(form.patientId.value),
    availabilitySlotId: parseInt(form.slotId.value),
    notes: form.notes.value.trim() || null,
  };
  try {
    if (id) {
      // Aggiornamento dello stato non gestito via questo form
      showAlert(document.getElementById('appointmentsAlert'), 'La modifica dell\'appuntamento non è supportata da questo form.', 'danger');
    } else {
      await apiFetch('/appointments', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('appointmentsAlert'), 'Appuntamento creato correttamente.', 'success');
      resetForm(form);
      await loadAppointments();
    }
  } catch (err) {
    showAlert(document.getElementById('appointmentsAlert'), err.message);
  }
}

async function updateAppointmentStatus(id, status) {
  if (status === 2 && !confirm('Sei sicuro di voler annullare questo appuntamento?')) return;
  try {
    await apiFetch(`/appointments/${id}/status`, {
      method: 'PUT',
      body: JSON.stringify({ status }),
    });
    const msg = status === 1 ? 'Appuntamento completato.' : 'Appuntamento annullato.';
    showAlert(document.getElementById('appointmentsAlert'), msg, 'success');
    await loadAppointments();
    await loadReports();
  } catch (err) {
    showAlert(document.getElementById('appointmentsAlert'), err.message);
  }
}

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

/* ===== Referti ===== */

async function loadReports() {
  try {
    const reports = await apiFetch('/medicalreports');
    const tbody = document.querySelector('#reportsTable tbody');
    if (tbody) {
      tbody.innerHTML = '';
      reports.forEach((r) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${r.id}</td>
          <td>${r.appointmentId}</td>
          <td>${r.diagnosis}</td>
          <td>${r.therapy}</td>
          <td>${r.notes || ''}</td>
          <td>${formatDateTime(r.createdAt)}</td>
          <td>
            <button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${r.id}">Modifica</button>
            <button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${r.id}">Elimina</button>
          </td>`;
        tbody.appendChild(tr);
      });
      tbody.querySelectorAll('button').forEach((btn) => {
        const id = btn.getAttribute('data-id');
        const action = btn.getAttribute('data-action');
        if (action === 'edit') {
          btn.addEventListener('click', () => editReport(id));
        } else if (action === 'delete') {
          btn.addEventListener('click', () => deleteReport(id));
        }
      });
    }
    // Aggiorna select appuntamento nel form report
    const appointments = await apiFetch('/appointments');
    populateCompletedAppointmentSelect(appointments);
  } catch (err) {
    showAlert(document.getElementById('reportsAlert'), `Errore nel caricamento referti: ${err.message}`);
  }
}

async function handleReportSubmit(event) {
  event.preventDefault();
  const form = event.target;
  const id = form.querySelector('input[name="id"]').value;
  const data = {
    appointmentId: parseInt(form.appointmentId.value),
    diagnosis: form.diagnosis.value.trim(),
    therapy: form.therapy.value.trim(),
    notes: form.notes.value.trim() || null,
  };
  try {
    if (id) {
      // update
      await apiFetch(`/medicalreports/${id}`, {
        method: 'PUT',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('reportsAlert'), 'Referto aggiornato correttamente.', 'success');
    } else {
      await apiFetch('/medicalreports', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      showAlert(document.getElementById('reportsAlert'), 'Referto creato correttamente.', 'success');
    }
    resetForm(form);
    document.getElementById('reportSubmit').textContent = 'Salva';
    await loadReports();
  } catch (err) {
    showAlert(document.getElementById('reportsAlert'), err.message);
  }
}

async function editReport(id) {
  try {
    const r = await apiFetch(`/medicalreports/${id}`);
    const form = document.getElementById('reportForm');
    form.querySelector('input[name="id"]').value = r.id;
    form.appointmentId.value = r.appointmentId;
    form.diagnosis.value = r.diagnosis;
    form.therapy.value = r.therapy;
    form.notes.value = r.notes || '';
    document.getElementById('reportSubmit').textContent = 'Aggiorna';
  } catch (err) {
    showAlert(document.getElementById('reportsAlert'), `Impossibile caricare referto: ${err.message}`);
  }
}

async function deleteReport(id) {
  if (!confirm('Sei sicuro di voler eliminare questo referto?')) return;
  try {
    await apiFetch(`/medicalreports/${id}`, { method: 'DELETE' });
    showAlert(document.getElementById('reportsAlert'), 'Referto eliminato con successo.', 'success');
    await loadReports();
  } catch (err) {
    showAlert(document.getElementById('reportsAlert'), err.message);
  }
}

/* ===== Helpers ===== */

/**
 * Popola il menu a discesa dei medici presente nei form slot.
 *
 * @param {Array<Object>} doctors Lista di medici.
 */
function populateDoctorSelects(doctors) {
  const slotDoctorSelect = document.getElementById('slotDoctor');
  if (slotDoctorSelect) {
    const current = slotDoctorSelect.value;
    slotDoctorSelect.innerHTML = '<option value="">Seleziona…</option>';
    doctors.forEach((d) => {
      const option = document.createElement('option');
      option.value = d.id;
      option.textContent = `${d.firstName} ${d.lastName}`;
      slotDoctorSelect.appendChild(option);
    });
    if (current) slotDoctorSelect.value = current;
  }
}

/**
 * Popola la select dei pazienti nel form appuntamenti.
 */
async function populatePatientSelect() {
  const select = document.getElementById('appointmentPatient');
  if (!select) return;
  const current = select.value;
  select.innerHTML = '<option value="">Seleziona…</option>';
  try {
    const patients = await apiFetch('/patients');
    patients.forEach((p) => {
      const option = document.createElement('option');
      option.value = p.id;
      option.textContent = `${p.firstName} ${p.lastName}`;
      select.appendChild(option);
    });
    if (current) select.value = current;
  } catch (err) {
    showAlert(document.getElementById('appointmentsAlert'), `Errore nel caricamento pazienti per appuntamento: ${err.message}`);
  }
}

/**
 * Popola la select degli slot disponibili nella form appuntamenti.
 *
 * @param {Array<Object>} slots Lista degli slot.
 */
function populateAvailableSlotSelect(slots) {
  const select = document.getElementById('appointmentSlot');
  if (!select) return;
  const current = select.value;
  select.innerHTML = '<option value="">Seleziona…</option>';
  slots.forEach((s) => {
    if (s.isAvailable) {
      const option = document.createElement('option');
      option.value = s.id;
      option.textContent = `${s.doctorFullName} - ${formatDateTime(s.startTime)}`;
      select.appendChild(option);
    }
  });
  if (current) select.value = current;
}

/**
 * Popola la select degli appuntamenti completati nella form referto.
 *
 * @param {Array<Object>} appointments Elenco di appuntamenti.
 */
function populateCompletedAppointmentSelect(appointments) {
  const select = document.getElementById('reportAppointment');
  if (!select) return;
  const current = select.value;
  select.innerHTML = '<option value="">Seleziona…</option>';
  appointments.forEach((a) => {
    if (a.status === 1) {
      const option = document.createElement('option');
      option.value = a.id;
      option.textContent = `${a.patientFullName} / ${a.doctorFullName} - ${formatDateTime(a.startTime)}`;
      select.appendChild(option);
    }
  });
  if (current) select.value = current;
}