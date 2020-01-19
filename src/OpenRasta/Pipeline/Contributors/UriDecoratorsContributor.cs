using System;
using System.Collections.Generic;
using System.Linq;
using OpenRasta.DI;
using OpenRasta.Web;
using OpenRasta.Web.UriDecorators;

namespace OpenRasta.Pipeline.Contributors
{
  public class UriDecoratorsContributor : IPipelineContributor
  {
    readonly Func<IEnumerable<IUriDecorator>> _decorators;

    public UriDecoratorsContributor(IDependencyResolver resolver)
      : this(resolver.ResolveAll<IUriDecorator>)
    {
    }

    public UriDecoratorsContributor(Func<IEnumerable<IUriDecorator>> decorators)
    {
      _decorators = decorators;
    }

    PipelineContinuation ProcessDecorators(ICommunicationContext context)
    {
      var currentUri = context.Request.Uri;
      var decorators = CreateDecorators();

      /* Whenever we execute the decorators, each decorator gets a say in matching a url.
       * Whenever a decorator fails, it is ignored.
       * Whenever a decorator succeeds, it is marked as such so that its Apply() method gets called
       * Whenever a decorator that succeeded has changed the url, we reprocess all the decorators that failed before with the new url.
       * */
      for (int i = 0; i < decorators.Count; i++)
      {
        var decorator = decorators[i];
        if (decorator.Successful ||
            !decorator.Decorator.Parse(currentUri, out var processedUri))
          continue;
        decorator.Successful = true;
        if (currentUri == processedUri || processedUri == null) continue;
        currentUri = processedUri;
        i = -1;
      }

      foreach (var decorator in decorators)
      {
        if (decorator.Successful)
          decorator.Decorator.Apply();
      }

      context.Request.Uri = currentUri;
      return PipelineContinuation.Continue;
    }

    public void Initialize(IPipeline pipelineRunner)
    {
      pipelineRunner.Notify(ProcessDecorators).Before<KnownStages.IUriMatching>();
    }

    IList<DecoratorPointer> CreateDecorators()
    {
      return _decorators()
        .Select(decorator => new DecoratorPointer {Decorator = decorator})
        .ToList();
    }

    class DecoratorPointer
    {
      public IUriDecorator Decorator { get; set; }
      public bool Successful { get; set; }
    }
  }
}