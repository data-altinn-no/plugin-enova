using Dan.Plugin.Enova.Models;

namespace Dan.Plugin.Enova.Mappers;

public class EmsResponseModelMapper : IMapper<EmsCsv, EmsResponseModel>
{
    public EmsResponseModel Map(EmsCsv input)
    {
        return new EmsResponseModel
        {
            Kommunenummer = input.Kommunenummer,
            Gaardsnummer = input.Gaardsnummer,
            Bruksnummer = input.Bruksnummer,
            Seksjonsnummer = input.Seksjonsnummer,
            Festenummer = input.Festenummer,
            Andelsnummer = input.Andelsnummer,
            Bygningsnummer = input.Bygningsnummer,
            GateAdresse = input.GateAdresse,
            Postnummer = input.Postnummer,
            Poststed = input.Poststed,
            BruksEnhetsNummer = input.BruksEnhetsNummer,
            Organisasjonsnummer = input.Organisasjonsnummer,
            Bygningskategori = input.Bygningskategori,
            Byggear = input.Byggear,
            Energikarakter = input.Energikarakter,
            Utstedelsesdato = input.Utstedelsesdato,
            TypeRegistrering = input.TypeRegistrering,
            Attestnummer = input.Attestnummer,
            BeregnetLevertEnergiTotaltkWhm2 = input.BeregnetLevertEnergiTotaltkWhm2,
            Materialvalg = input.Materialvalg
        };
    }
}
