using System;
using Dan.Plugin.Enova.Mappers;
using Dan.Plugin.Enova.Models;

namespace Dan.Plugin.Enova.Test.Mappers;

public class EmsResponseModelMapperTests
{
    [Fact]
    public void Map_EmsResponseModel()
    {
        // Arrange
        var mapper = new EmsResponseModelMapper();
        var emsCsvRecord = new EmsCsv()
        {
            Kommunenummer = 123,
            Gaardsnummer = "Gaardsnummer",
            Bruksnummer = "Bruksnummer",
            Seksjonsnummer = "Seksjonsnummer",
            Festenummer = "Festenummer",
            Andelsnummer = "Andelsnummer",
            Bygningsnummer = "Bygningsnummer",
            GateAdresse = "GateAdresse",
            Postnummer = 1234,
            Poststed = "Poststed",
            BruksEnhetsNummer = "BruksEnhetsNummer",
            Organisasjonsnummer = "Organisasjonsnummer",
            Bygningskategori = "Bygningskategori",
            Byggear = 1995,
            Energikarakter = "Energikarakter",
            Utstedelsesdato = new DateTime(2024, 09, 20),
            TypeRegistrering = "TypeRegistrering",
            Attestnummer = "Attestnummer",
            BeregnetLevertEnergiTotaltkWhm2 = 12.34,
            Materialvalg = "Materialvalg"
        };

        var expected = new EmsResponseModel
        {
            Kommunenummer = 123,
            Gaardsnummer = "Gaardsnummer",
            Bruksnummer = "Bruksnummer",
            Seksjonsnummer = "Seksjonsnummer",
            Festenummer = "Festenummer",
            Andelsnummer = "Andelsnummer",
            Bygningsnummer = "Bygningsnummer",
            GateAdresse = "GateAdresse",
            Postnummer = 1234,
            Poststed = "Poststed",
            BruksEnhetsNummer = "BruksEnhetsNummer",
            Organisasjonsnummer = "Organisasjonsnummer",
            Bygningskategori = "Bygningskategori",
            Byggear = 1995,
            Energikarakter = "Energikarakter",
            Utstedelsesdato = new DateTime(2024, 09, 20),
            TypeRegistrering = "TypeRegistrering",
            Attestnummer = "Attestnummer",
            BeregnetLevertEnergiTotaltkWhm2 = 12.34,
            Materialvalg = "Materialvalg"
        };

        // Act
        var actual = mapper.Map(emsCsvRecord);

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
}
