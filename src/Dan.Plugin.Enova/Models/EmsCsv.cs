using System;
using CsvHelper.Configuration.Attributes;

namespace Dan.Plugin.Enova.Models;

[Serializable]
public class EmsCsv
{
    [Name("Knr")]
    public int? Kommunenummer { get; set; }

    [Name("Gnr")]
    public string Gaardsnummer { get; set; }

    [Name("Bnr")]
    public string Bruksnummer { get; set; }

    [Name("Snr")]
    public string Seksjonsnummer { get; set; }

    [Name("Fnr")]
    public string Festenummer { get; set; }

    [Name("Andelsnummer")]
    public string Andelsnummer { get; set; }

    [Name("Bygningsnummer")]
    public string Bygningsnummer { get; set; }

    [Name("GateAdresse")]
    public string GateAdresse { get; set; }

    [Name("Postnummer")]
    public int? Postnummer { get; set; }

    [Name("Poststed")]
    public string Poststed { get; set; }

    [Name("BruksEnhetsNummer")]
    public string BruksEnhetsNummer { get; set; }

    [Name("Organisasjonsnummer")]
    public string Organisasjonsnummer { get; set; }

    [Name("Bygningskategori")]
    public string Bygningskategori { get; set; }

    [Name("Byggear")]
    public int? Byggear { get; set; }

    [Name("Energikarakter")]
    public string Energikarakter { get; set; }

    [Name("Oppvarmingskarakter")]
    public string Oppvarmingskarakter { get; set; }

    [Name("Utstedelsesdato")]
    public DateTime? Utstedelsesdato { get; set; }

    [Name("TypeRegistrering")]
    public string TypeRegistrering { get; set; }

    [Name("Attestnummer")]
    public string Attestnummer { get; set; }

    [Name("BeregnetLevertEnergiTotaltkWhm2")]
    public double BeregnetLevertEnergiTotaltkWhm2 { get; set; }

    [Name("BeregnetFossilandel")]
    public string BeregnetFossilandel { get; set; }

    [Name("Materialvalg")]
    public string Materialvalg { get; set; }

    [Name("HarEnergiVurdering")]
    public bool HarEnergiVurdering { get; set; }

    [Name("EnergiVurderingDato")]
    public DateTime? EnergiVurderingDato { get; set; }
}
