namespace OpenRasta.Plugins.Hydra
{
  public static class Vocabularies
  {
    public static readonly Vocabulary Hydra = new Vocabulary("http://www.w3.org/ns/hydra/core#", "hydra") ;
    public static readonly Vocabulary SchemaDotOrg  = new Vocabulary("http://schema.org/", "schema");
    public static readonly Vocabulary Rdf = new Vocabulary("http://www.w3.org/1999/02/22-rdf-syntax-ns#", "rdf");
    public static readonly Vocabulary
      XmlSchema  = new Vocabulary("http://www.w3.org/2001/XMLSchema#", "xsd");
  }
}