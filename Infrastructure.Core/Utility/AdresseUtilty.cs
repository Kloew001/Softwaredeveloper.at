namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility;

public static class AdresseUtilty
{
    public static bool IsInEU(this Adresse adresse)
    {
        return adresse.ISO3.IsISO3InEU();
    }
}

public class Adresse
{
    public string City { get; set; }
    public string ZipCode { get; set; }
    public string Street { get; set; }
    public string Country { get; set; }
    public string ISO3 { get; set; }
}

public class Country
{
    public string ISO2 { get; set; }
    public string ISO3 { get; set; }
    public string NameEN { get; set; }
    public string NameDE { get; set; }

    public Country(string iso2, string iso3, string nameEN, string nameDE)
    {
        ISO2 = iso2;
        ISO3 = iso3;
        NameEN = nameEN;
        NameDE = nameDE;
    }
}

public static class CountryUtilty
{
    public static bool IsISO3InEU(this string iso3)
    {
        if (iso3.IsNullOrEmpty())
            return false;

        return AllISO3CodesInEU.Contains(iso3.ToUpper());
    }

    public static string[] AllISO3CodesInEU = ["AUT", "BEL", "BGR", "HRV", "CYP", "CZE", "DNK", "EST", "FIN", "FRA", "DEU", "GRC", "HUN", "IRL", "ITA", "LVA", "LTU", "LUX", "MLT", "NLD", "POL", "PRT", "ROU", "SVK", "SVN", "ESP", "SWE"];

    public static Country GetCountryByISO3(this string iso3)
    {
        if (iso3.IsNullOrEmpty())
            return null;

        return allCountries.FirstOrDefault(x => x.ISO3 == iso3.ToUpper());
    }

    public static List<Country> allCountries =
        [
        new Country("AF", "AFG", "Afghanistan", "Afghanistan"),
        new Country("AL", "ALB", "Albania", "Albanien"),
        new Country("AE", "ARE", "U.A.E.", "Vereinigte Arabische Emirate"),
        new Country("AR", "ARG", "Argentina", "Argentinien"),
        new Country("AM", "ARM", "Armenia", "Armenien"),
        new Country("AU", "AUS", "Australia", "Australien"),
        new Country("AT", "AUT", "Austria", "Österreich"),
        new Country("AZ", "AZE", "Azerbaijan", "Aserbaidschan"),
        new Country("BE", "BEL", "Belgium", "Belgien"),
        new Country("BD", "BGD", "Bangladesh", "Bangladesch"),
        new Country("BG", "BGR", "Bulgaria", "Bulgarien"),
        new Country("BH", "BHR", "Bahrain", "Bahrain"),
        new Country("BA", "BIH", "Bosnia and Herzegovina", "Bosnien und Herzegowina"),
        new Country("BY", "BLR", "Belarus", "Weißrussland"),
        new Country("BZ", "BLZ", "Belize", "Belize"),
        new Country("BO", "BOL", "Bolivia", "Bolivien"),
        new Country("BR", "BRA", "Brazil", "Brasilien"),
        new Country("BN", "BRN", "Brunei Darussalam", "Brunei Darussalam"),
        new Country("CA", "CAN", "Canada", "Kanada"),
        new Country("CH", "CHE", "Switzerland", "Schweiz"),
        new Country("CL", "CHL", "Chile", "Chile"),
        new Country("CN", "CHN", "People's Republic of China", "Volksrepublik China"),
        new Country("CO", "COL", "Colombia", "Kolumbien"),
        new Country("CR", "CRI", "Costa Rica", "Costa Rica"),
        new Country("CZ", "CZE", "Czech Republic", "Tschechische Republik"),
        new Country("DE", "DEU", "Germany", "Deutschland"),
        new Country("DK", "DNK", "Denmark", "Dänemark"),
        new Country("DO", "DOM", "Dominican Republic", "Dominikanische Republik"),
        new Country("DZ", "DZA", "Algeria", "Algerien"),
        new Country("EC", "ECU", "Ecuador", "Ecuador"),
        new Country("EG", "EGY", "Egypt", "Ägypten"),
        new Country("ES", "ESP", "Spain", "Spanien"),
        new Country("EE", "EST", "Estonia", "Estland"),
        new Country("ET", "ETH", "Ethiopia", "Äthiopien"),
        new Country("FI", "FIN", "Finland", "Finnland"),
        new Country("FR", "FRA", "France", "Frankreich"),
        new Country("FO", "FRO", "Faroe Islands", "Färöer"),
        new Country("GB", "GBR", "United Kingdom", "Vereinigtes Königreich"),
        new Country("GE", "GEO", "Georgia", "Georgien"),
        new Country("GR", "GRC", "Greece", "Griechenland"),
        new Country("GL", "GRL", "Greenland", "Grönland"),
        new Country("GT", "GTM", "Guatemala", "Guatemala"),
        new Country("HK", "HKG", "Hong Kong S.A.R.", "Hongkong SAR"),
        new Country("HN", "HND", "Honduras", "Honduras"),
        new Country("HR", "HRV", "Croatia", "Kroatien"),
        new Country("HU", "HUN", "Hungary", "Ungarn"),
        new Country("ID", "IDN", "Indonesia", "Indonesien"),
        new Country("IN", "IND", "India", "Indien"),
        new Country("IE", "IRL", "Ireland", "Irland"),
        new Country("IR", "IRN", "Iran", "Iran"),
        new Country("IQ", "IRQ", "Iraq", "Irak"),
        new Country("IS", "ISL", "Iceland", "Island"),
        new Country("IL", "ISR", "Israel", "Israel"),
        new Country("IT", "ITA", "Italy", "Italien"),
        new Country("JM", "JAM", "Jamaica", "Jamaika"),
        new Country("JO", "JOR", "Jordan", "Jordanien"),
        new Country("JP", "JPN", "Japan", "Japan"),
        new Country("KZ", "KAZ", "Kazakhstan", "Kasachstan"),
        new Country("KE", "KEN", "Kenya", "Kenia"),
        new Country("KG", "KGZ", "Kyrgyzstan", "Kirgisistan"),
        new Country("KH", "KHM", "Cambodia", "Kambodscha"),
        new Country("KR", "KOR", "Korea", "Korea"),
        new Country("KW", "KWT", "Kuwait", "Kuwait"),
        new Country("LA", "LAO", "Lao P.D.R.", "Laos"),
        new Country("LB", "LBN", "Lebanon", "Libanon"),
        new Country("LY", "LBY", "Libya", "Libyen"),
        new Country("LI", "LIE", "Liechtenstein", "Liechtenstein"),
        new Country("LK", "LKA", "Sri Lanka", "Sri Lanka"),
        new Country("LT", "LTU", "Lithuania", "Litauen"),
        new Country("LU", "LUX", "Luxembourg", "Luxemburg"),
        new Country("LV", "LVA", "Latvia", "Lettland"),
        new Country("MO", "MAC", "Macao S.A.R.", "Macao SAR"),
        new Country("MA", "MAR", "Morocco", "Marokko"),
        new Country("MC", "MCO", "Principality of Monaco", "Monaco"),
        new Country("MV", "MDV", "Maldives", "Malediven"),
        new Country("MX", "MEX", "Mexico", "Mexiko"),
        new Country("MK", "MKD", "Macedonia (FYROM)", "Nordmazedonien"),
        new Country("MT", "MLT", "Malta", "Malta"),
        new Country("ME", "MNE", "Montenegro", "Montenegro"),
        new Country("MN", "MNG", "Mongolia", "Mongolei"),
        new Country("MY", "MYS", "Malaysia", "Malaysia"),
        new Country("NG", "NGA", "Nigeria", "Nigeria"),
        new Country("NI", "NIC", "Nicaragua", "Nicaragua"),
        new Country("NL", "NLD", "Netherlands", "Niederlande"),
        new Country("NO", "NOR", "Norway", "Norwegen"),
        new Country("NP", "NPL", "Nepal", "Nepal"),
        new Country("NZ", "NZL", "New Zealand", "Neuseeland"),
        new Country("OM", "OMN", "Oman", "Oman"),
        new Country("PK", "PAK", "Islamic Republic of Pakistan", "Pakistan"),
        new Country("PA", "PAN", "Panama", "Panama"),
        new Country("PE", "PER", "Peru", "Peru"),
        new Country("PH", "PHL", "Republic of the Philippines", "Philippinen"),
        new Country("PL", "POL", "Poland", "Polen"),
        new Country("PR", "PRI", "Puerto Rico", "Puerto Rico"),
        new Country("PT", "PRT", "Portugal", "Portugal"),
        new Country("PY", "PRY", "Paraguay", "Paraguay"),
        new Country("QA", "QAT", "Qatar", "Katar"),
        new Country("RO", "ROU", "Romania", "Rumänien"),
        new Country("RU", "RUS", "Russia", "Russland"),
        new Country("RW", "RWA", "Rwanda", "Ruanda"),
        new Country("SA", "SAU", "Saudi Arabia", "Saudi-Arabien"),
        new Country("CS", "SCG", "Serbia and Montenegro (Former)", "Serbien und Montenegro (ehemalig)"),
        new Country("SN", "SEN", "Senegal", "Senegal"),
        new Country("SG", "SGP", "Singapore", "Singapur"),
        new Country("SV", "SLV", "El Salvador", "El Salvador"),
        new Country("RS", "SRB", "Serbia", "Serbien"),
        new Country("SK", "SVK", "Slovakia", "Slowakei"),
        new Country("SI", "SVN", "Slovenia", "Slowenien"),
        new Country("SE", "SWE", "Sweden", "Schweden"),
        new Country("SY", "SYR", "Syria", "Syrien"),
        new Country("TJ", "TAJ", "Tajikistan", "Tadschikistan"),
        new Country("TH", "THA", "Thailand", "Thailand"),
        new Country("TM", "TKM", "Turkmenistan", "Turkmenistan"),
        new Country("TT", "TTO", "Trinidad and Tobago", "Trinidad und Tobago"),
        new Country("TN", "TUN", "Tunisia", "Tunesien"),
        new Country("TR", "TUR", "Turkey", "Türkei"),
        new Country("TW", "TWN", "Taiwan", "Taiwan"),
        new Country("UA", "UKR", "Ukraine", "Ukraine"),
        new Country("UY", "URY", "Uruguay", "Uruguay"),
        new Country("US", "USA", "United States", "Vereinigte Staaten"),
        new Country("UZ", "UZB", "Uzbekistan", "Usbekistan"),
        new Country("VE", "VEN", "Bolivarian Republic of Venezuela", "Venezuela"),
        new Country("VN", "VNM", "Vietnam", "Vietnam"),
        new Country("YE", "YEM", "Yemen", "Jemen"),
        new Country("ZA", "ZAF", "South Africa", "Südafrika"),
        new Country("ZW", "ZWE", "Zimbabwe", "Simbabwe")
    ];
}