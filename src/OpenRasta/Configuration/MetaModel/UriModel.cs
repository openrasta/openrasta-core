using System.Globalization;

namespace OpenRasta.Configuration.MetaModel
{
    public class UriModel : ConfigurationModel
    {
        public CultureInfo Language { get; set; }
        public string Name { get; set; }
        public string Uri { get; set; }
    }
}