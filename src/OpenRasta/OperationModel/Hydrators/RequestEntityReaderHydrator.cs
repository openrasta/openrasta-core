using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OpenRasta.Binding;
using OpenRasta.Codecs;
using OpenRasta.Collections;
using OpenRasta.DI;
using OpenRasta.Diagnostics;
using OpenRasta.OperationModel.Hydrators.Diagnostics;
using OpenRasta.TypeSystem.ReflectionBased;
using OpenRasta.Web;
using OpenRasta.Pipeline;

namespace OpenRasta.OperationModel.Hydrators
{
  public class RequestEntityReaderHydrator  : IRequestEntityReader
  {
    readonly IRequest _request;
    readonly IDependencyResolver _resolver;

    public RequestEntityReaderHydrator(IDependencyResolver resolver, IRequest request)
    {
      Log = NullLogger<CodecLogSource>.Instance;
      ErrorCollector = NullErrorCollector.Instance;
      _resolver = resolver;
      _request = request;
    }

    public IErrorCollector ErrorCollector { get; set; }
    public ILogger<CodecLogSource> Log { get; set; }

    public async Task<IOperation> Read(IEnumerable<IOperation> operations)
    {
      var selectedOperations = (
        from o in operations
        let codecMatch = o.GetRequestCodec()
        where codecMatch != null
        orderby codecMatch descending, o.Name
        select o
      ).Concat(
        from o in operations
        where o.Inputs.AllReady()
        orderby o.Inputs.CountReady() descending, o.Name
        select o
      );



      var operation = selectedOperations.FirstOrDefault();
      if (operation == null)
      {
        Log.OperationNotFound();
        return null;
      }

      Log.OperationFound(operation);

      if (operation.GetRequestCodec() == null) return operation;

      var codecInstance = CreateMediaTypeReader(operation);

      var codecType = codecInstance.GetType();
      Log.CodecLoaded(codecType);

      if (codecType.Implements(typeof(IKeyedValuesMediaTypeReader<>)) &&
          TryAssignKeyedValues(_request.Entity, codecInstance, codecType, operation))
        return operation;

      if (!codecType.Implements<IMediaTypeReader>()) return operation;
      return await TryReadPayloadAsObject(
        _request.Entity,
        (IMediaTypeReader) codecInstance,
        operation) ? operation : null;
    }

    static ErrorFrom<RequestEntityReaderHydrator> CreateErrorForException(Exception e)
    {
      return new ErrorFrom<RequestEntityReaderHydrator>
      {
        Message = "The codec failed to process the request entity. See the exception below.\r\n" + e,
        Exception = e
      };
    }


    ICodec CreateMediaTypeReader(IOperation operation)
    {
      return
        _resolver.Resolve(operation.GetRequestCodec().CodecRegistration.CodecType, UnregisteredAction.AddAsTransient) as
          ICodec;
    }

    bool TryAssignKeyedValues(IHttpEntity requestEntity, ICodec codec, Type codecType, IOperation operation)
    {
      Log.CodecSupportsKeyedValues();

      return codec.TryAssignKeyValues(requestEntity, operation.Inputs.Select(x => x.Binder), Log.KeyAssigned,
        Log.KeyFailed);
    }

    async Task<bool> TryReadPayloadAsObject(IHttpEntity requestEntity, IMediaTypeReader reader, IOperation operation)
    {
      Log.CodecSupportsFullObjectResolution();
      foreach (var member in from m in operation.Inputs
        where m.Binder.IsEmpty
        select m)
      {
        Log.ProcessingMember(member);
        try
        {
          var readerAsync = reader as IMediaTypeReaderAsync;
          if (readerAsync != null)
          {

          }
          var entityInstance = readerAsync != null
            ? await readerAsync.ReadFrom(requestEntity,
              member.Member.Type,
              member.Member.Name)
            : reader.ReadFrom(requestEntity,
              member.Member.Type,
              member.Member.Name);
          Log.Result(entityInstance);

          if (entityInstance != Missing.Value)
          {
            if (!member.Binder.SetInstance(entityInstance))
            {
              Log.BinderInstanceAssignmentFailed();
              return false;
            }
            Log.BinderInstanceAssignmentSucceeded();
          }
        }
        catch (Exception e)
        {
          ErrorCollector.AddServerError(CreateErrorForException(e));
          return false;
        }
      }
      return true;
    }

  }
}
