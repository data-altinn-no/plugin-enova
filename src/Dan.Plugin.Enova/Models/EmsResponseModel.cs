using System;
using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Models;

[Serializable]
public class EmsResponseModel
{
    [JsonProperty("kommunenummer")]
    public int? Kommunenummer { get; set; }

    [JsonProperty("gaardsnummer")]
    public string Gaardsnummer { get; set; }

    [JsonProperty("bruksnummer")]
    public string Bruksnummer { get; set; }

    [JsonProperty("seksjonsnummer")]
    public string Seksjonsnummer { get; set; }

    [JsonProperty("festenummer")]
    public string Festenummer { get; set; }

    [JsonProperty("andelsnummer")]
    public string Andelsnummer { get; set; }

    [JsonProperty("bygningsnummer")]
    public string Bygningsnummer { get; set; }

    [JsonProperty("gateAdresse")]
    public string GateAdresse { get; set; }

    [JsonProperty("postnummer")]
    public int? Postnummer { get; set; }

    [JsonProperty("poststed")]
    public string Poststed { get; set; }

    [JsonProperty("bruksEnhetsNummer")]
    public string BruksEnhetsNummer { get; set; }

    [JsonProperty("organisasjonsnummer")]
    public string Organisasjonsnummer { get; set; }

    [JsonProperty("bygningskategori")]
    public string Bygningskategori { get; set; }

    [JsonProperty("byggear")]
    public int? Byggear { get; set; }

    [JsonProperty("energikarakter")]
    public string Energikarakter { get; set; }

    [JsonProperty("oppvarmingskarakter")]
    public string Oppvarmingskarakter { get; set; }

    [JsonProperty("utstedelsesdato")]
    public DateTime? Utstedelsesdato { get; set; }

    [JsonProperty("typeRegistrering")]
    public string TypeRegistrering { get; set; }

    [JsonProperty("attestnummer")]
    public string Attestnummer { get; set; }

    [JsonProperty("beregnetLevertEnergiTotaltkWhm2")]
    public double BeregnetLevertEnergiTotaltkWhm2 { get; set; }

    [JsonProperty("beregnetFossilandel")]
    public string BeregnetFossilandel { get; set; }

    [JsonProperty("materialvalg")]
    public string Materialvalg { get; set; }

    [JsonProperty("harEnergiVurdering")]
    public bool HarEnergiVurdering { get; set; }

    [JsonProperty("energiVurderingDato")]
    public DateTime? EnergiVurderingDato { get; set; }
}
