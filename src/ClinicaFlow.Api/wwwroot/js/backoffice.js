// Gestione del Back Office: login, dashboard e CRUD basilari.
// Il file usa esclusivamente gli endpoint reali esposti dal backend ASP.NET Core.

document.addEventListener('DOMContentLoaded', () => {
  const loginView = document.getElementById('login-view');
  const dashboardView = document.getElementById('dashboard-view');
  const loginForm = document.getElementById('bo-login-form');
  const loginAlert = document.getElementById('bo-login-alert');
  const logoutBtn = document.getElementById('bo-logout-btn');
  const adminUsernameLabel = document.getElementById('admin-username');

  const newPatientBtn = document.getElementById('new-patient-btn');
  const newDoctorBtn = document.getElementById('new-doctor-btn');

  const createPatientForm = document.getElementById('create-patient-form');
  const createDoctorForm = document.getElementById('create-doctor-form');
  const createSpecialtyForm = document.getElementById('create-specialty-form');
  const createSlotForm = document.getElementById('create-slot-form');
  const createAppointmentForm = document.getElementById('create-appointment-form');

  const patientModalTitle = document.getElementById('patient-modal-title');
  const patientSubmitBtn = document.getElementById('patient-submit-btn');
  const patientPasswordHelp = document.getElementById('patient-password-help');

  const doctorModalTitle = document.getElementById('doctor-modal-title');
  const doctorSubmitBtn = document.getElementById('doctor-submit-btn');
  const doctorPasswordHelp = document.getElementById('doctor-password-help');

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

  // Converte una data ISO in formato compatibile con input[type=date].
  function toDateInputValue(value) {
    if (!value) return '';
    return String(value).substring(0, 10);
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

  // Apre un modale Bootstrap in modo sicuro.
  function showModal(modalId) {
    const modalEl = document.getElementById(modalId);
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.show();
  }

  // Chiude un modale Bootstrap in modo sicuro.
  function hideModal(modalId) {
    const modalEl = document.getElementById(modalId);
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    modal.hide();
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

  // Prepara il modale paziente in modalità creazione.
  newPatientBtn?.addEventListener('click', () => {
    resetPatientForm();
  });

  // Prepara il modale medico in modalità creazione.
  newDoctorBtn?.addEventListener('click', () => {
    resetDoctorForm();
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
        <td>${valueOrDash(patient.username)}</td>
        <td>
          <button type="button" class="btn btn-sm btn-outline-primary" data-action="edit-patient" data-id="${patient.id}">Modifica</button>
        </td>
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
        <td>${valueOrDash(doctor.username)}</td>
        <td>
          <button type="button" class="btn btn-sm btn-outline-primary" data-action="edit-doctor" data-id="${doctor.id}">Modifica</button>
        </td>
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

  // Intercetta le azioni sulle righe paziente.
  patientsTableBody?.addEventListener('click', (e) => {
    const button = e.target.closest('button');
    if (!button) return;

    const action = button.getAttribute('data-action');
    const id = parseInt(button.getAttribute('data-id'), 10);

    if (action === 'edit-patient') {
      const patient = patientsCache.find((item) => item.id === id);
      if (patient) {
        preparePatientEdit(patient);
      }
    }
  });

  // Intercetta le azioni sulle righe medico.
  doctorsTableBody?.addEventListener('click', (e) => {
    const button = e.target.closest('button');
    if (!button) return;

    const action = button.getAttribute('data-action');
    const id = parseInt(button.getAttribute('data-id'), 10);

    if (action === 'edit-doctor') {
      const doctor = doctorsCache.find((item) => item.id === id);
      if (doctor) {
        prepareDoctorEdit(doctor);
      }
    }
  });

  // Resetta la form paziente in modalità creazione.
  function resetPatientForm() {
    const form = createPatientForm;
    document.getElementById('create-patient-alert').innerHTML = '';
    form.reset();
    form.elements.id.value = '';
    patientModalTitle.textContent = 'Nuovo Paziente';
    patientSubmitBtn.textContent = 'Crea';
    form.username.required = true;
    form.password.required = true;
    patientPasswordHelp.textContent = 'Obbligatoria in fase di creazione.';
  }

  // Prepara la form paziente in modalità modifica.
  function preparePatientEdit(patient) {
    const form = createPatientForm;
    document.getElementById('create-patient-alert').innerHTML = '';
    form.reset();
    form.elements.id.value = patient.id;
    form.firstName.value = patient.firstName || '';
    form.lastName.value = patient.lastName || '';
    form.taxCode.value = patient.taxCode || '';
    form.birthDate.value = toDateInputValue(patient.birthDate);
    form.phone.value = patient.phone || '';
    form.email.value = patient.email || '';
    form.username.value = patient.username || '';
    form.password.value = '';
    form.username.required = true;
    form.password.required = !patient.username;
    patientModalTitle.textContent = 'Modifica Paziente';
    patientSubmitBtn.textContent = 'Salva';
    patientPasswordHelp.textContent = patient.username ? 'Lasciare vuoto per mantenere la password attuale.' : 'Obbligatoria perché il paziente non ha ancora un account.';
    showModal('createPatientModal');
  }

  // Resetta la form medico in modalità creazione.
  function resetDoctorForm() {
    const form = createDoctorForm;
    document.getElementById('create-doctor-alert').innerHTML = '';
    form.reset();
    form.elements.id.value = '';
    doctorModalTitle.textContent = 'Nuovo Medico';
    doctorSubmitBtn.textContent = 'Crea';
    form.username.required = true;
    form.password.required = true;
    doctorPasswordHelp.textContent = 'Obbligatoria in fase di creazione.';
  }

  // Prepara la form medico in modalità modifica.
  function prepareDoctorEdit(doctor) {
    const form = createDoctorForm;
    document.getElementById('create-doctor-alert').innerHTML = '';
    form.reset();
    form.elements.id.value = doctor.id;
    form.firstName.value = doctor.firstName || '';
    form.lastName.value = doctor.lastName || '';
    form.taxCode.value = doctor.taxCode || '';
    form.specialtyId.value = doctor.specialtyId;
    form.username.value = doctor.username || '';
    form.password.value = '';
    form.username.required = true;
    form.password.required = !doctor.username;
    doctorModalTitle.textContent = 'Modifica Medico';
    doctorSubmitBtn.textContent = 'Salva';
    doctorPasswordHelp.textContent = doctor.username ? 'Lasciare vuoto per mantenere la password attuale.' : 'Obbligatoria perché il medico non ha ancora un account.';
    showModal('createDoctorModal');
  }

  // Creazione o modifica paziente, incluse le credenziali di accesso.
  createPatientForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-patient-alert');
    alertContainer.innerHTML = '';

    const id = e.target.elements.id.value;
    const isEdit = Boolean(id);
    const username = e.target.username.value.trim();
    const password = e.target.password.value;

    if (!isEdit && (!username || !password)) {
      showAlert(alertContainer, 'Username e password sono obbligatori per creare l\'accesso del paziente.', 'danger');
      return;
    }

    const data = {
      firstName: e.target.firstName.value.trim(),
      lastName: e.target.lastName.value.trim(),
      taxCode: e.target.taxCode.value.trim(),
      birthDate: e.target.birthDate.value,
      phone: e.target.phone.value.trim() || null,
      email: e.target.email.value.trim() || null,
      username: username || null,
      password: password || null,
    };

    try {
      const response = await apiFetch(isEdit ? `/Patients/${id}` : '/Patients', {
        method: isEdit ? 'PUT' : 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, isEdit ? 'Errore nella modifica del paziente.' : 'Errore nella creazione del paziente.'));
      }

      hideModal('createPatientModal');
      e.target.reset();
      await loadPatients();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nel salvataggio del paziente.', 'danger');
    }
  });

  // Creazione o modifica medico, incluse le credenziali di accesso.
  createDoctorForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-doctor-alert');
    alertContainer.innerHTML = '';

    const id = e.target.elements.id.value;
    const isEdit = Boolean(id);
    const username = e.target.username.value.trim();
    const password = e.target.password.value;

    if (!isEdit && (!username || !password)) {
      showAlert(alertContainer, 'Username e password sono obbligatori per creare l\'accesso del medico.', 'danger');
      return;
    }

    const data = {
      firstName: e.target.firstName.value.trim(),
      lastName: e.target.lastName.value.trim(),
      taxCode: e.target.taxCode.value.trim(),
      specialtyId: parseInt(e.target.specialtyId.value, 10),
      username: username || null,
      password: password || null,
    };

    try {
      const response = await apiFetch(isEdit ? `/Doctors/${id}` : '/Doctors', {
        method: isEdit ? 'PUT' : 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, isEdit ? 'Errore nella modifica del medico.' : 'Errore nella creazione del medico.'));
      }

      hideModal('createDoctorModal');
      e.target.reset();
      await loadDoctors();
      await loadSlots();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nel salvataggio del medico.', 'danger');
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

      hideModal('createSpecialtyModal');
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

      hideModal('createSlotModal');
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
      notes: e.target.notes.value.trim() || null,
    };

    try {
      const response = await apiFetch('/Appointments', {
        method: 'POST',
        body: JSON.stringify(data),
      });

      if (!response.ok) {
        throw new Error(await readErrorMessage(response, 'Errore nella creazione dell\'appuntamento.'));
      }

      hideModal('createAppointmentModal');
      e.target.reset();
      await loadSlots();
      await loadAppointments();
      await loadReports();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore nella creazione dell\'appuntamento.', 'danger');
    }
  });
});
