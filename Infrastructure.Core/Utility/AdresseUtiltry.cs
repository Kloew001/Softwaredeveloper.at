using System.Linq;

namespace SoftwaredeveloperDotAt.Infrastructure.Core.Utility
{
    public static class AdresseUtiltry
    {
        public static bool IsInEU(this Adresse adresse)
        {
            return AllISO3CodesInEU().Contains(adresse.ISO3);
        }

        public static string[] AllISO3CodesInEU()
        {
            return ["AUT", "BEL", "BGR", "HRV", "CYP", "CZE", "DNK", "EST", "FIN", "FRA", "DEU", "GRC", "HUN", "IRL", "ITA", "LVA", "LTU", "LUX", "MLT", "NLD", "POL", "PRT", "ROU", "SVK", "SVN", "ESP", "SWE"];
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
}
