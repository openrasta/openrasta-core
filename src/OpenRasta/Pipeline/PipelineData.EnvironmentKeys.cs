namespace OpenRasta.Pipeline
{

  public partial class PipelineData
  {
    static class EnvironmentKeys
    {
      const string OR_PIPELINE = "__OR_PIPELINE_";

      public const string PIPELINE_STATE = OR_PIPELINE + "PipelineStage";
      public const string HANDLER_TYPE = OR_PIPELINE + "HandlerType";
      public const string RESOURCE_KEY = OR_PIPELINE + "ResourceKey";
      public const string RESPONSE_CODEC = OR_PIPELINE + "ResponseCodec";
      public const string SELECTED_HANDLERS = OR_PIPELINE + "SelectedHandlers";
      public const string SELECTED_RESOURCE = OR_PIPELINE + "SelectedResource";
      public const string OPERATIONS = OR_PIPELINE + "Operations";
      public const string OPERATIONS_ASYNC = "openrasta.operations";
      public const string LEGACY_KEYS = "openrasta.LegacyKeys";

      public const string OWIN_REQUEST_PROTOCOL = "owin.RequestProtocol";
      public const string OWIN_SSL_CLIENT_CERTIFICATE = "ssl.ClientCertificate";
      public const string OWIN_SSL_CLIENT_CERTIFICATE_LOAD = "ssl.LoadClientCertAsync";
    }
  }
}