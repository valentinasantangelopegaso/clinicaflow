// Gestione del Back Office: login, dashboard e CRUD basilari

document.addEventListener('DOMContentLoaded', () => {
  const loginView = document.getElementById('login-view');
  const dashboardView = document.getElementById('dashboard-view');
  const loginForm = document.getElementById('bo-login-form');
  const loginAlert = document.getElementById('bo-login-alert');
  const logoutBtn = document.getElementById('bo-logout-btn');
  const adminUsernameLabel = document.getElementById('admin-username');

  // Modals form references
  const createPatientForm = document.getElementById('create-patient-form');
  const createDoctorForm = document.getElementById('create-doctor-form');
  const createSpecialtyForm = document.getElementById('create-specialty-form');
  const createSlotForm = document.getElementById('create-slot-form');
  const createAppointmentForm = document.getElementById('create-appointment-form');

  // Selects for dynamic lists
  const doctorSpecialtySelect = document.getElementById('doctor-specialty-select');
  const slotDoctorSelect = document.getElementById('slot-doctor-select');
  const appointmentPatientSelect = document.getElementById('appointment-patient-select');
  const appointmentSlotSelect = document.getElementById('appointment-slot-select');

  // Tables
  const patientsTableBody = document.querySelector('#patients-table tbody');
  const doctorsTableBody = document.querySelector('#doctors-table tbody');
  const specialtiesTableBody = document.querySelector('#specialties-table tbody');
  const slotsTableBody = document.querySelector('#slots-table tbody');
  const appointmentsTableBody = document.querySelector('#appointments-table tbody');
  const reportsTableBody = document.querySelector('#reports-table tbody');

  // Mostra la dashboard nascondendo il login
  function showDashboard() {
    loginView.classList.add('d-none');
    dashboardView.classList.remove('d-none');
  }

  // Verifica se esiste un token già salvato e se l'utente è admin
  const existingAuth = getAuthData();
  if (existingAuth && existingAuth.role === 'Admin') {
    adminUsernameLabel.textContent = existingAuth.username;
    showDashboard();
    loadAllData();
  }

  // Gestione del login
  loginForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    // Pulisce eventuali alert
    loginAlert.innerHTML = '';
    const username = loginForm.username.value.trim();
    const password = loginForm.password.value.trim();
    try {
      const authData = await login(username, password);
      if (authData.role !== 'Admin') {
        clearAuthData();
        showAlert(loginAlert, 'Accesso consentito solo agli amministratori', 'danger');
        return;
      }
      adminUsernameLabel.textContent = authData.username;
      showDashboard();
      loadAllData();
    } catch (err) {
      showAlert(loginAlert, err.message || 'Errore di autenticazione', 'danger');
    }
  });

  // Gestione logout
  logoutBtn?.addEventListener('click', () => {
    logout();
  });

  /**
   * Carica tutte le risorse iniziali in parallelo
   */
  async function loadAllData() {
    try {
      await Promise.all([
        loadSpecialties(),
        loadPatients(),
        loadDoctors(),
        loadSlots(),
        loadAppointments(),
        loadReports(),
      ]);
    } catch (err) {
      console.error('Errore nel caricamento dati:', err);
    }
  }

  /**
   * Carica la lista dei pazienti e popola la tabella e il select per appuntamenti.
   */
  async function loadPatients() {
    try {
      const resp = await apiFetch('/patients');
      const patients = await resp.json();
      // Pulisce la tabella e il select
      patientsTableBody.innerHTML = '';
      appointmentPatientSelect.innerHTML = '';
      patients.forEach((p) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${p.firstName}</td>
          <td>${p.lastName}</td>
          <td>${p.taxCode}</td>
          <td>${p.phoneNumber || ''}</td>
          <td></td>
        `;
        patientsTableBody.appendChild(tr);
        // select per appuntamenti
        const opt = document.createElement('option');
        opt.value = p.id;
        opt.textContent = `${p.firstName} ${p.lastName}`;
        appointmentPatientSelect.appendChild(opt);
      });
    } catch (err) {
      console.error(err);
    }
  }

  /**
   * Carica la lista delle specialità e popola la tabella e il select per medici.
   */
  async function loadSpecialties() {
    try {
      const resp = await apiFetch('/specialties');
      const specialties = await resp.json();
      specialtiesTableBody.innerHTML = '';
      doctorSpecialtySelect.innerHTML = '';
      specialties.forEach((s) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${s.name}</td>
          <td>${s.description || ''}</td>
          <td></td>
        `;
        specialtiesTableBody.appendChild(tr);
        // select per medici
        const opt = document.createElement('option');
        opt.value = s.id;
        opt.textContent = s.name;
        doctorSpecialtySelect.appendChild(opt);
      });
    } catch (err) {
      console.error(err);
    }
  }

  /**
   * Carica la lista dei medici e popola la tabella e i select per slot.
   */
  async function loadDoctors() {
    try {
      const resp = await apiFetch('/doctors');
      const doctors = await resp.json();
      doctorsTableBody.innerHTML = '';
      slotDoctorSelect.innerHTML = '';
      doctors.forEach((d) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${d.firstName}</td>
          <td>${d.lastName}</td>
          <td>${d.specialty?.name || ''}</td>
          <td>${d.phoneNumber || ''}</td>
          <td></td>
        `;
        doctorsTableBody.appendChild(tr);
        // select per slot
        const opt = document.createElement('option');
        opt.value = d.id;
        opt.textContent = `${d.firstName} ${d.lastName}`;
        slotDoctorSelect.appendChild(opt);
      });
    } catch (err) {
      console.error(err);
    }
  }

  /**
   * Carica la lista degli slot e popola la tabella e il select per appuntamenti.
   */
  async function loadSlots() {
    try {
      const resp = await apiFetch('/slots');
      const slots = await resp.json();
      slotsTableBody.innerHTML = '';
      appointmentSlotSelect.innerHTML = '';
      slots.forEach((s) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${s.doctor?.firstName || ''} ${s.doctor?.lastName || ''}</td>
          <td>${formatDateTime(s.startTime)}</td>
          <td>${formatDateTime(s.endTime)}</td>
          <td>${s.isAvailable ? 'Sì' : 'No'}</td>
          <td></td>
        `;
        slotsTableBody.appendChild(tr);
        if (s.isAvailable) {
          const opt = document.createElement('option');
          opt.value = s.id;
          opt.textContent = `${formatDateTime(s.startTime)} – ${s.doctor?.firstName || ''} ${s.doctor?.lastName || ''}`;
          appointmentSlotSelect.appendChild(opt);
        }
      });
    } catch (err) {
      console.error(err);
    }
  }

  /**
   * Carica la lista degli appuntamenti e popola la tabella.
   */
  async function loadAppointments() {
    try {
      const resp = await apiFetch('/appointments');
      const appointments = await resp.json();
      appointmentsTableBody.innerHTML = '';
      appointments.forEach((a) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${formatDateTime(a.dateTime)}</td>
          <td>${a.patient?.firstName || ''} ${a.patient?.lastName || ''}</td>
          <td>${a.doctor?.firstName || ''} ${a.doctor?.lastName || ''}</td>
          <td>${a.status}</td>
          <td></td>
        `;
        appointmentsTableBody.appendChild(tr);
      });
    } catch (err) {
      console.error(err);
    }
  }

  /**
   * Carica la lista dei referti (disponibile solo per Admin) e popola la tabella.
   */
  async function loadReports() {
    try {
      const resp = await apiFetch('/medicalreports');
      const reports = await resp.json();
      reportsTableBody.innerHTML = '';
      reports.forEach((r) => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
          <td>${formatDateTime(r.appointment?.dateTime)}</td>
          <td>${r.appointment?.patient?.firstName || ''} ${r.appointment?.patient?.lastName || ''}</td>
          <td>${r.appointment?.doctor?.firstName || ''} ${r.appointment?.doctor?.lastName || ''}</td>
          <td>${r.diagnosis || ''}</td>
          <td></td>
        `;
        reportsTableBody.appendChild(tr);
      });
    } catch (err) {
      console.error(err);
    }
  }

  // Gestione creazione paziente
  createPatientForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-patient-alert');
    alertContainer.innerHTML = '';
    const data = {
      firstName: e.target.firstName.value.trim(),
      lastName: e.target.lastName.value.trim(),
      taxCode: e.target.taxCode.value.trim(),
      phoneNumber: e.target.phoneNumber.value.trim() || null,
    };
    try {
      const resp = await apiFetch('/patients', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg || 'Errore nella creazione del paziente');
      }
      // chiude modale e aggiorna lista
      bootstrap.Modal.getInstance(document.getElementById('createPatientModal')).hide();
      e.target.reset();
      await loadPatients();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore', 'danger');
    }
  });

  // Gestione creazione medico
  createDoctorForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-doctor-alert');
    alertContainer.innerHTML = '';
    const data = {
      firstName: e.target.firstName.value.trim(),
      lastName: e.target.lastName.value.trim(),
      taxCode: e.target.taxCode.value.trim(),
      phoneNumber: e.target.phoneNumber.value.trim() || null,
      specialtyId: parseInt(e.target.specialtyId.value, 10),
    };
    try {
      const resp = await apiFetch('/doctors', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg || 'Errore nella creazione del medico');
      }
      bootstrap.Modal.getInstance(document.getElementById('createDoctorModal')).hide();
      e.target.reset();
      await loadDoctors();
      // Aggiorna anche la lista slot (che dipende dai medici) e specialità
      await loadSlots();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore', 'danger');
    }
  });

  // Gestione creazione specialità
  createSpecialtyForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-specialty-alert');
    alertContainer.innerHTML = '';
    const data = {
      name: e.target.name.value.trim(),
      description: e.target.description.value.trim() || null,
    };
    try {
      const resp = await apiFetch('/specialties', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg || 'Errore nella creazione della specialità');
      }
      bootstrap.Modal.getInstance(document.getElementById('createSpecialtyModal')).hide();
      e.target.reset();
      await loadSpecialties();
      // Aggiorna i dati relativi ai medici
      await loadDoctors();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore', 'danger');
    }
  });

  // Gestione creazione slot
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
      const resp = await apiFetch('/slots', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg || 'Errore nella creazione dello slot');
      }
      bootstrap.Modal.getInstance(document.getElementById('createSlotModal')).hide();
      e.target.reset();
      await loadSlots();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore', 'danger');
    }
  });

  // Gestione creazione appuntamento
  createAppointmentForm?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const alertContainer = document.getElementById('create-appointment-alert');
    alertContainer.innerHTML = '';
    const data = {
      patientId: parseInt(e.target.patientId.value, 10),
      slotId: parseInt(e.target.slotId.value, 10),
    };
    try {
      const resp = await apiFetch('/appointments', {
        method: 'POST',
        body: JSON.stringify(data),
      });
      if (!resp.ok) {
        const msg = await resp.text();
        throw new Error(msg || 'Errore nella creazione dell\'appuntamento');
      }
      bootstrap.Modal.getInstance(document.getElementById('createAppointmentModal')).hide();
      e.target.reset();
      await loadAppointments();
      // Aggiorna anche slot (lo slot non sarà più disponibile)
      await loadSlots();
    } catch (err) {
      showAlert(alertContainer, err.message || 'Errore', 'danger');
    }
  });
});