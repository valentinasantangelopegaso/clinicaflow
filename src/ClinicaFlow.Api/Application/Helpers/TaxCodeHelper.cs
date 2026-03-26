using System.Linq;

namespace ClinicaFlow.Api.Application.Helpers;

/// <summary>
/// Helper per la normalizzazione del codice fiscale.
/// </summary>
public static class TaxCodeHelper
{
    /// <summary>
    /// Normalizza il codice fiscale rimuovendo gli spazi e convertendolo in maiuscolo.
    /// </summary>
    /// <param name="taxCode">Codice fiscale da normalizzare.</param>
    /// <returns>Codice fiscale normalizzato.</returns>
    public static string Normalize(string? taxCode)
    {
        if (string.IsNullOrWhiteSpace(taxCode))
        {
            return string.Empty;
        }

        return new string(taxCode
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray())
            .ToUpperInvariant();
    }
}