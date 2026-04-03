# ClinicaFlow

ClinicaFlow è un'applicazione web full stack sviluppata come progetto universitario per la gestione delle attività di una clinica privata.

Il sistema consente di gestire pazienti, medici, disponibilità delle visite, prenotazioni e referti medici attraverso un'architettura basata su API REST.

## Funzionalità principali

- gestione anagrafica dei pazienti
- gestione dei medici e delle specializzazioni
- definizione delle disponibilità per le visite
- prenotazione degli appuntamenti
- inserimento dei referti medici

## Tecnologie utilizzate

- ASP.NET Core Web API
- Entity Framework Core (approccio Code First)
- SQL Server 2025
- HTML, CSS e JavaScript
- Bootstrap
- Swagger / OpenAPI per la documentazione delle API

## Architettura del sistema

L'applicazione è organizzata secondo un'architettura client-server:

- **Frontend**: interfaccia web sviluppata con HTML, CSS e JavaScript
- **Backend**: API REST sviluppate con ASP.NET Core
- **Persistence layer**: database relazionale gestito tramite Entity Framework Core

## Dominio applicativo

Le principali entità del sistema sono:

- Patient
- Doctor
- Specialty
- AvailabilitySlot
- Appointment
- MedicalReport

Il flusso principale del sistema è:

**paziente → prenotazione visita → visita → referto medico**

## Obiettivo del progetto

L'obiettivo del progetto è dimostrare la progettazione e lo sviluppo di un'applicazione web completa, comprendente modellazione dei dati, implementazione delle API, interfaccia utente e documentazione tecnica.

## Repository

Il codice sorgente è gestito tramite GitHub con controllo di versione.

---

Progetto sviluppato nell'ambito del **Project Work 16**.
