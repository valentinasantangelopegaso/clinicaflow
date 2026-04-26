/*
 * common.js
 *
 * Funzioni condivise del frontend ClinicaFlow.
 * Questo file gestisce login JWT, sessionStorage, chiamate API e formattazioni.
 */

const apiBaseUrl = "/api";

const jwtClaimTypes = {
  role: "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
  name: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name",
  nameIdentifier: "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
};

/**
 * Restituisce i dati di autenticazione salvati in sessione.
 * @returns {Object|null} Dati utente autenticato o null.
 */
function getAuthData() {
  const rawValue = sessionStorage.getItem("authData");

  if (!rawValue) {
    return null;
  }

  try {
    return JSON.parse(rawValue);
  } catch {
    clearAuthData();
    return null;
  }
}

/**
 * Salva i dati di autenticazione in sessionStorage.
 * @param {Object} data Dati utente autenticato.
 */
function setAuthData(data) {
  sessionStorage.setItem("authData", JSON.stringify(data));
}

/**
 * Cancella i dati di autenticazione dalla sessione.
 */
function clearAuthData() {
  sessionStorage.removeItem("authData");
}

/**
 * Decodifica il payload di un token JWT.
 * La funzione non valida la firma: serve solo a leggere i claim lato client.
 * @param {string} token Token JWT.
 * @returns {Object|null} Payload decodificato.
 */
function parseJwt(token) {
  if (!token || typeof token !== "string") {
    return null;
  }

  const parts = token.split(".");
  if (parts.length < 2) {
    return null;
  }

  try {
    const base64Url = parts[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((char) => "%" + ("00" + char.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );

    return JSON.parse(jsonPayload);
  } catch (error) {
    console.error("Token JWT non leggibile.", error);
    return null;
  }
}

/**
 * Restituisce il primo valore valorizzato tra una lista di possibili proprietà.
 * @param {Object} source Oggetto sorgente.
 * @param {string[]} keys Chiavi candidate.
 * @returns {*|null} Valore trovato o null.
 */
function getFirstValue(source, keys) {
  if (!source) {
    return null;
  }

  for (const key of keys) {
    if (source[key] !== undefined && source[key] !== null && source[key] !== "") {
      return source[key];
    }
  }

  return null;
}

/**
 * Converte un valore numerico nullable.
 * @param {*} value Valore da convertire.
 * @returns {number|null} Numero o null.
 */
function toNullableNumber(value) {
  if (value === undefined || value === null || value === "") {
    return null;
  }

  const parsed = Number(value);
  return Number.isNaN(parsed) ? null : parsed;
}

/**
 * Esegue il login applicativo tramite AuthController.
 * Usa prima i campi restituiti dal backend e solo come fallback i claim del JWT.
 * @param {string} username Username.
 * @param {string} password Password.
 * @returns {Promise<Object>} Dati di autenticazione normalizzati.
 */
async function login(username, password) {
  const response = await fetch(`${apiBaseUrl}/Auth/login`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      username: username,
      password: password
    })
  });

  const responseText = await response.text();

  if (!response.ok) {
    throw new Error(responseText || "Username o password errati.");
  }

  let responseData;
  try {
    responseData = responseText ? JSON.parse(responseText) : {};
  } catch {
    throw new Error("Risposta di login non valida.");
  }

  const token = getFirstValue(responseData, ["token", "Token"]);
  if (!token) {
    throw new Error("Token JWT non restituito dal backend.");
  }

  const payload = parseJwt(token) || {};

  const role = getFirstValue(responseData, ["role", "Role"])
    || getFirstValue(payload, ["role", "Role", jwtClaimTypes.role]);

  const doctorId = toNullableNumber(
    getFirstValue(responseData, ["doctorId", "DoctorId"])
    || getFirstValue(payload, ["doctorId", "DoctorId"])
  );

  const patientId = toNullableNumber(
    getFirstValue(responseData, ["patientId", "PatientId"])
    || getFirstValue(payload, ["patientId", "PatientId"])
  );

  if (!role) {
    throw new Error("Ruolo utente non restituito dal backend.");
  }

  const authData = {
    token: token,
    username: username,
    role: role,
    doctorId: doctorId,
    patientId: patientId
  };

  setAuthData(authData);
  return authData;
}

/**
 * Esegue una chiamata HTTP autenticata verso le API.
 * @param {string} path Percorso API relativo, es. "/Patients".
 * @param {Object} options Opzioni fetch.
 * @returns {Promise<Response>} Risposta HTTP.
 */
async function apiFetch(path, options = {}) {
  const authData = getAuthData();

  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {})
  };

  if (authData && authData.token) {
    headers.Authorization = `Bearer ${authData.token}`;
  }

  const url = path.startsWith("http") || path.startsWith("/api")
    ? path
    : `${apiBaseUrl}${path}`;

  const response = await fetch(url, {
    ...options,
    headers: headers
  });

  if (response.status === 401) {
    clearAuthData();
    throw new Error("Sessione scaduta o utente non autenticato.");
  }

  if (response.status === 403) {
    throw new Error("Operazione non autorizzata per il ruolo corrente.");
  }

  return response;
}

/**
 * Esegue una chiamata API e restituisce il JSON, se presente.
 * @param {string} path Percorso API.
 * @param {Object} options Opzioni fetch.
 * @returns {Promise<Object|null>} JSON della risposta o null.
 */
async function apiJson(path, options = {}) {
  const response = await apiFetch(path, options);

  if (response.status === 204) {
    return null;
  }

  const text = await response.text();

  if (!response.ok) {
    throw new Error(text || `Errore HTTP ${response.status}.`);
  }

  return text ? JSON.parse(text) : null;
}

/**
 * Mostra un alert Bootstrap.
 * @param {HTMLElement} container Contenitore.
 * @param {string} message Messaggio.
 * @param {string} type Tipo alert Bootstrap.
 */
function showAlert(container, message, type = "danger") {
  if (!container) {
    alert(message);
    return;
  }

  container.innerHTML = `
    <div class="alert alert-${type} alert-dismissible fade show" role="alert">
      ${escapeHtml(message)}
      <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Chiudi"></button>
    </div>`;
}

/**
 * Pulisce un contenitore di alert.
 * @param {HTMLElement} container Contenitore.
 */
function clearAlert(container) {
  if (container) {
    container.innerHTML = "";
  }
}

/**
 * Esegue logout e torna alla home corretta, anche da pagine dentro /pages.
 */
function logout() {
  clearAuthData();

  if (window.location.pathname.includes("/pages/")) {
    window.location.href = "../index.html";
  } else {
    window.location.href = "index.html";
  }
}

/**
 * Verifica che l'utente autenticato abbia il ruolo richiesto.
 * @param {string} expectedRole Ruolo richiesto.
 * @returns {Object|null} Dati autenticazione.
 */
function requireRole(expectedRole) {
  const authData = getAuthData();

  if (!authData || authData.role !== expectedRole) {
    clearAuthData();
    return null;
  }

  return authData;
}

/**
 * Formatta una data ISO in formato italiano.
 * @param {string} value Data ISO.
 * @returns {string} Data formattata.
 */
function formatDate(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleDateString("it-IT");
}

/**
 * Formatta una data/ora ISO in formato italiano.
 * @param {string} value Data/ora ISO.
 * @returns {string} Data/ora formattata.
 */
function formatDateTime(value) {
  if (!value) {
    return "";
  }

  return new Date(value).toLocaleString("it-IT", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit"
  });
}

/**
 * Restituisce la label italiana dello stato appuntamento.
 * @param {number|string} status Stato proveniente dall'API.
 * @returns {string} Descrizione.
 */
function appointmentStatusLabel(status) {
  if (status === 0 || status === "0" || status === "Scheduled") {
    return "Pianificato";
  }

  if (status === 1 || status === "1" || status === "Completed") {
    return "Completato";
  }

  if (status === 2 || status === "2" || status === "Cancelled") {
    return "Annullato";
  }

  return "";
}

/**
 * Effettua escape di testo da inserire in HTML.
 * @param {*} value Valore.
 * @returns {string} Testo sicuro.
 */
function escapeHtml(value) {
  return String(value ?? "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

/**
 * Resetta un form e svuota eventuale campo id nascosto.
 * @param {HTMLFormElement} form Form da resettare.
 */
function resetForm(form) {
  if (!form) {
    return;
  }

  form.reset();

  const idInput = form.querySelector('input[name="id"]');
  if (idInput) {
    idInput.value = "";
  }
}
