/*
 * common.js
 *
 * Questo file contiene funzioni di utilità condivise da tutte le pagine
 * dell'interfaccia ClinicaFlow. Serve a centralizzare le chiamate alle
 * API, la gestione degli errori e alcune funzioni di formattazione.
 */

// Base URL delle API. Lasciamo il prefisso vuoto perché il front‑end viene
// servito dalla stessa applicazione ASP.NET e i percorsi sono relativi.
const apiBaseUrl = "/api";

/**
 * Esegue una richiesta HTTP verso l'API REST.
 * Incapsula la gestione degli header JSON e degli errori. Se la
 * risposta non contiene contenuto (HTTP 204) viene restituito null.
 *
 * @param {string} path Percorso relativo all'endpoint (es. '/patients').
 * @param {Object} options Opzioni fetch aggiuntive (metodo, body, ecc.).
 * @returns {Promise<Object|null>} Oggetto JSON restituito dall'API oppure null.
 */
async function apiFetch(path, options = {}) {
  const fetchOptions = Object.assign(
    {
      headers: {
        "Content-Type": "application/json",
      },
    },
    options
  );

  try {
    const response = await fetch(apiBaseUrl + path, fetchOptions);
    // Se la risposta non è ok, tentiamo di leggere il corpo come testo per
    // mostrare eventuali messaggi di errore provenienti dal backend.
    if (!response.ok) {
      const errorText = await response.text();
      throw new Error(errorText || `Errore ${response.status}`);
    }
    if (response.status === 204) {
      return null;
    }
    return await response.json();
  } catch (err) {
    console.error(err);
    throw err;
  }
}

/**
 * Mostra un messaggio di allerta in un contenitore indicato.
 *
 * @param {HTMLElement} container Elemento su cui inserire l'allerta.
 * @param {string} message Testo da visualizzare.
 * @param {string} type Tipo di allerta Bootstrap ('success', 'danger', ecc.).
 */
function showAlert(container, message, type = "danger") {
  if (!container) return;
  container.innerHTML = `<div class="alert alert-${type} alert-dismissible fade show" role="alert">
    ${message}
    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
  </div>`;
}

/**
 * Converte una data ISO in formato locale leggibile.
 *
 * @param {string|Date} isoString Data da convertire.
 * @returns {string} Data formattata secondo la locale italiana.
 */
function formatDate(isoString) {
  const d = new Date(isoString);
  return d.toLocaleDateString("it-IT", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
}

/**
 * Converte un timestamp ISO in data e ora locali leggibili.
 *
 * @param {string|Date} isoString Data e ora da convertire.
 * @returns {string} Data e ora formattate per l'utente.
 */
function formatDateTime(isoString) {
  const d = new Date(isoString);
  return d.toLocaleString("it-IT", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
}

/**
 * Pulisce i valori di un modulo HTML.
 * Tutti gli input e textarea all'interno del form saranno resettati.
 *
 * @param {HTMLFormElement} form Form da resettare.
 */
function resetForm(form) {
  if (!form) return;
  form.reset();
  // Rimuove eventuale campo hidden per l'id utilizzato nell'update
  const hiddenId = form.querySelector("input[name='id']");
  if (hiddenId) hiddenId.value = "";
}

function getToken() {
    return sessionStorage.getItem("authToken");
}

function setAuthData(data) {
    sessionStorage.setItem("authToken", data.token);
    sessionStorage.setItem("authRole", data.role);
    if (data.doctorId) sessionStorage.setItem("doctorId", data.doctorId);
    if (data.patientId) sessionStorage.setItem("patientId", data.patientId);
}

function clearAuthData() {
    sessionStorage.removeItem("authToken");
    sessionStorage.removeItem("authRole");
    sessionStorage.removeItem("doctorId");
    sessionStorage.removeItem("patientId");
}

async function apiFetch(url, options = {}) {
    const token = getToken();

    const headers = {
        "Content-Type": "application/json",
        ...(options.headers || {})
    };

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    return fetch(url, {
        ...options,
        headers
    });
}

// Esporta le funzioni globalmente in modo da poterle utilizzare nei moduli
window.apiFetch = apiFetch;
window.showAlert = showAlert;
window.formatDate = formatDate;
window.formatDateTime = formatDateTime;
window.resetForm = resetForm;