using System.Collections.Generic;

namespace Tests.Plugins.Hydra.nodes
{
  public class EventHandler
  {
    public Event Get(int id)
    {
      return new Event { Id = id, FirstName = "Bilbo Baggins"};
    }

    public List<Event> Get()
    {
      return new List<Event>
      {
        new Event {Id = 1, Customer = new Customer {Name = "Boromear"}},
        new Event
        {Id = 2, Customers =
        {
          new Customer {Name = "An elf"},
          new Customer {Name = "Another elf"},
        }}
      };
    }

    public Event PostCreate(CreateAction action)
    {
      return new Event{Id = 1};
    }
  }
}