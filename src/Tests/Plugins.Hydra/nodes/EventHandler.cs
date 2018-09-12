using System.Collections.Generic;
using Tests.Plugins.Hydra.Examples;

namespace Tests.Plugins.Hydra.nodes
{
  public class EventHandler
  {
    public Event Get(int id)
    {
      return new Event { Id = id };
    }

    public List<Event> Get()
    {
      return new List<Event>
      {
        new Event() {Id = 1},
        new Event() {Id = 2}
      };
    }

    public Event PostCreate(CreateAction action)
    {
      return new Event{Id = 1};
    }
  }
}