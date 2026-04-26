// Gestione del Back Office: login, dashboard e CRUD basilari.
// Il file usa esclusivamente gli endpoint reali esposti dal backend ASP.NET Core.

document.addEventListener('DOMContentLoaded', () => {
  const loginView = document.getElementById('login-view');
  const dashboardView = document.getElementById('dashboard-view');
  const loginForm = document.getElementById('bo-login-form');
  const loginAlert = document.getElementById('bo-login-alert');
  const logoutBtn = document.getElementById('bo-logout-btn');
  const adminUsernameLabel = document.getElementById('admin-username');

  const createPatientForm = document.getElementById('create-patient-form');
  const createDoctorForm = document.getElementById('create-doctor-form');
  const createSpecialtyForm = document.getElementById('create-specialty-form');
  const createSlotForm = document.getElementById('create-slot-form');
  const createAppointmentForm = document.getElementById('create-appointment-form');

  const doctorSpecialtySelect = document.getElementById('doctor-specialty-select');
  const slotDoctorSelect = document.getElementById('slot-doctor-select');
  const appointmentPatientSelect = document.getElementById('appointment-patient-select');
  const appointmentSlotSelect = document.getElementById('appointment-slot-select');

  const patientsTableBody = document.querySelector('#patients-table tbody');
  const doctorsTableBody = document.querySelector('#doctors-table tbody');
  const specialtiesTableBody = document.querySelector('#specialties-table tbody');
  const slotsTableBody = document.querySelector('#slots-table tbody');
  const appointmentsTableBody = document.querySelector('#appointments-table tbody');
  const reportsTableBody = document.querySelector('#reports-table tbody');

  let patientsCache = [];
  let doctorsCache = [];
  let specialtiesCache = [];
  let slotsCache = [];
  let appointmentsCache = [];
  let reportsCache = [];

  // Mostra la dashboard amministrativa dopo il login riuscito.
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

  // Legge il corpo di errore restituito dalle API e lo converte in messaggio.
  async function readErrorMessage(response, fallbackMessage) {
    try {
      const text = await response.text();
      if (!text) return fallbackMessage;

      try {
        const json = JSON.parse(text);
        if (json.title) return json.title;
        if (json.errors) {
          return Object.values(json.errors).flat().join(' ');
        }
      } catch {
        return text;
      }

      return text;
    } catch {
      return fallbackMessage;
    }
  }

  // Popola una select con una prima opzione neutra.
  function resetSelect(select, placeholder) {
    if (!select) return;
    select.innerHTML = '';
    const option = document.createElement('option');
    option.value = '';
    option.textContent = placeholder;
    select.appendChild(option);
  }

  // Verifica eventuale sessione admin già presente.
  const existingAuth = getAuthData();
  if (existingAuth && existingAuth.role === 'Admin') {
    adminUsernameLabel.textContent = existingAuth.username;
    showDashboard();
    loadAllData();
  }

  // Gestione del login Back Office.
  loginForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    loginAlert.innerHTML = '';

    const username = loginForm.username.value.trim();
    const password = loginForm.password.value.trim();

    try {
      const authData = await login(username, password);

      if (authData.role !== 'Admin') {
        clearAuthData();
        showAlert(loginAlert, 'Accesso consentito solo agli amministratori.', 'danger');
        return;
      }

      adminUsernameLabel.textContent = authData.username;
      showDashboard();
      await loadAllData();
    } catch (err) {
      showAlert(loginAlert, err.message || 'Errore di autenticazione.', 'danger');
    }
  });

  // Gestione logout.
  logoutBtn?.addEventListener('click', () => {
    logout();
  });

  // Carica i dati in sequenza per rispettare le dipendenze tra le tabelle.
  async function loadAllData() {
    await loadPatients();
    await loadSpecialties();
    await loadDoctors();
    await loadSlots();
    await loadAppointments();
    await loadReports();
  }

  // Carica pazienti e select per la creazione appuntamento.
  async function loadPatients() {
    const response = await apiFetch('/Patients');
    patientsCache = await response.json();

    patientsTableBody.innerHTML = '';
    resetSelect(appointmentPatientSelect, 'Seleziona un paziente');

    patientsCache.forEach((patient) => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${patient.firstName}</td>
        <td>${patient.lastName}</td>
        <td>${patient.taxCode}</td>
        <td>${valueOrDash(patient.phone)}</td>
        <td>${valueOrDash(patient.email)}</td>
      `;
      patientsTableBody.appendChild(row);

      const option = document.createElement('option');
      option.value = patient.id;
      option.textContent = `${patient.firstName} ${patient.lastName} - ${patient.taxCode}`;
      appointmentPatientSelect.appendChild(option);
    });
  }

  // Carica specializzazioni e select usata dai medici.
  async function loadSpecialties() {
    const response = await apiFetch('/Specialties');
    specialtiesCache = await response.json();

    specialtiesTableBody.innerHTML = '';
    resetSelect(doctorSpecialtySelect, 'Seleziona una specialità');

    specialtiesCache.forEach((specialty) => {
      const row = document.createElement('tr');
      row.innerHTML = `<td>${specialty.name}</td>`;
      specialtiesTableBody.appendChild(row);

      const option = document.createElement('option');
      option.value = specialty.id;
      option.textContent = specialty.name;
      doctorSpecialtySelect.appendChild(option);
    });
  }

  // Carica medici e select usata dagli slot.
  async function loadDoctors() {
    const response = await apiFetch('/Doctors');
    doctorsCache = await response.json();

    doctorsTableBody.innerHTML = '';
    resetSelect(slotDoctorSelect, 'Seleziona un medico');

    doctorsCache.forEach((doctor) => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${doctor.firstName}</td>
        <td>${doctor.lastName}</td>
        <td>${doctor.taxCode}</td>
        <td>${doctor.specialtyName}</td>
      `;
      doctorsTableBody.appendChild(row);

      const option = document.createElement('option');
      option.value = doctor.id;
      option.textContent = `${doctor.firstName} ${doctor.lastName} - ${doctor.specialtyName}`;
      slotDoctorSelect.appendChild(option);
    });
  }

  // Carica slot dal controller AvailabilitySlots e popola select appuntamento.
  async function loadSlots() {
    const response = await apiFetch('/AvailabilitySlots');
    slotsCache = await response.json();

    slotsTableBody.innerHTML = '';
    resetSelect(appointmentSlotSelect, 'Seleziona uno slot disponibile');

    slotsCache.forEach((slot) => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${slot.doctorFullName}</td>
        <td>${formatDateTime(slot.startTime)}</td>
        <td>${formatDateTime(slot.endTime)}</td>
        <td><span class="badge ${slot.isAvailable ? 'text-bg-success' : 'text-bg-secondary'}">${slot.isAvailable ? 'Sì' : 'No'}</span></td>
      `;
      slotsTableBody.appendChild(row);

      if (slot.isAvailable) {
        const option = document.createElement('option');
        option.value = slot.id;
        option.textContent = `${formatDateTime(slot.startTime)} - ${slot.doctorFullName}`;
        appointmentSlotSelect.appendChild(option);
      }
    });
  }

  // Carica appuntamenti e mostra i campi realmente restituiti dal DTO.
  async function loadAppointments() {
    const response = await apiFetch('/Appointments');
    appointmentsCache = await response.json();

    appointmentsTableBody.innerHTML = '';

    appointmentsCache.forEach((appointment) => {
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${formatDateTime(appointment.startTime)}</td>
        <td>${appointment.patientFullName}</td>
        <td>${appointment.doctorFullName}</td>
        <td>${statusText(appointment.status)}</td>
        <td>${valueOrDash(appointment.notes)}</td>
      `;
      appointmentsTableBody.appendChild(row);
    });
  }

  // Carica referti e arricchisce la vista con i dati appuntamento già caricati.
  async function loadReports() {
    const response = await apiFetch('/MedicalReports');
    reportsCache = await response.json();

    const appointmentById = new Map(appointmentsCache.map((appointment) => [appointment.id, appointment]));
    reportsTableBody.innerHTML = '';

    reportsCache.forEach((report) => {
      const appointment = appointmentById.get(report.appointmentId);
      const row = document.createElement('tr');
      row.innerHTML = `
        <td>${appointment ? formatDateTime(appointment.startTime) : `#${report.appointmentId}`}</td>
        <td>${appointment ? appointment.patientFullName : '-'}</td>
        <td>${appointment ? appointment.doctorFullName : '-'}</td>
        <td>${valueOrDash(report.diagnosis)}</td>
        <td>${valueOrDash(report.therapy)}</td>
        <td>${formatDateTime(report.createdAt)}</td>
      `;
      reportsTableBody.appendChild(row);
    });
  }

  // Creazione paziente. Il backend richiede birthDate; se manca, segnala errore chiaro.
  createPatientForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-patient-alert');
    alertContainer.innerHTML = '';

    const birthDateField = e.target.birthDate;
    if (!birthDateField) {
      showAlert(alertContainer, 'La form paziente deve contenere il campo data di nascita.', 'danger');
      return;
    }

    const data = {
      firstName: e.target.firstName.value.trim(),
      lastName: e.target.lastName.value.trim(),
      taxCode: e.target.taxCode.value.trim(),
      birthDate: birthDateField.value,
      phone: e.target.phone?.value.trim() || null,
      email: e.target.email?.value.trim() || null,
    };

    try {
      const response = await apiFetch('/Patients', {
        method: 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nella creazione del paziente.'));
      }

      bootstrap.Modal.getInstance(document.getElementById('createPatientModal')).hide();
      e.target.reset();
      await loadPatients();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nella creazione del paziente.', 'danger');
    }
  });

  // Creazione medico. Il backend supporta nome, cognome, codice fiscale e specialità.
  createDoctorForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-doctor-alert');
    alertContainer.innerHTML = '';

    const data = {
      firstName: e.target.firstName.value.trim(),
      lastName: e.target.lastName.value.trim(),
      taxCode: e.target.taxCode.value.trim(),
      specialtyId: parseInt(e.target.specialtyId.value, 10),
    };

    try {
      const response = await apiFetch('/Doctors', {
        method: 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nella creazione del medico.'));
      }

      bootstrap.Modal.getInstance(document.getElementById('createDoctorModal')).hide();
      e.target.reset();
      await loadDoctors();
      await loadSlots();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nella creazione del medico.', 'danger');
    }
  });

  // Creazione specialità. Il backend non prevede il campo descrizione.
  createSpecialtyForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-specialty-alert');
    alertContainer.innerHTML = '';

    const data = {
      name: e.target.name.value.trim(),
    };

    try {
      const response = await apiFetch('/Specialties', {
        method: 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nella creazione della specialità.'));
      }

      bootstrap.Modal.getInstance(document.getElementById('createSpecialtyModal')).hide();
      e.target.reset();
      await loadSpecialties();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nella creazione della specialità.', 'danger');
    }
  });

  // Creazione slot tramite AvailabilitySlotsController.
  createSlotForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-slot-alert');
    alertContainer.innerHTML = '';

    const data = {
      doctorId: parseInt(e.target.doctorId.value, 10),
      startTime: e.target.startTime.value,
      endTime: e.target.endTime.value,
    };

    try {
      const response = await apiFetch('/AvailabilitySlots', {
        method: 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nella creazione dello slot.'));
      }

      bootstrap.Modal.getInstance(document.getElementById('createSlotModal')).hide();
      e.target.reset();
      await loadSlots();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nella creazione dello slot.', 'danger');
    }
  });

  // Creazione appuntamento: il backend richiede availabilitySlotId, non slotId.
  createAppointmentForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-appointment-alert');
    alertContainer.innerHTML = '';

    const data = {
      patientId: parseInt(e.target.patientId.value, 10),
      availabilitySlotId: parseInt(e.target.slotId.value, 10),
      notes: e.target.notes?.value.trim() || null,
    };

    try {
      const response = await apiFetch('/Appointments', {
        method: 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nella creazione dell\'appuntamento.'));
      }

      bootstrap.Modal.getInstance(document.getElementById('createAppointmentModal')).hide();
      e.target.reset();
      await loadSlots();
      await loadAppointments();
      await loadReports();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nella creazione dell\'appuntamento.', 'danger');
    }
  });
});
