using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenRasta.Collections;
using OpenRasta.Testing;

namespace OpenRasta.Tests.Unit.Collections
{
    public class when_iterating_over_a_collection
    {
        [Test]
        public void the_selected_items_trigger_notification()
        {
            var selected = new List<int>();
            var discarded = new List<int>();
            var source = new[] { 1, 2, 3 };
            var result = source.AsObservable(x => x.Where(i=>i != 2), selected.Add, discarded.Add).ToList();

            selected.LegacyShouldHaveCountOf(2)
                .LegacyShouldContain(1)
                .LegacyShouldContain(3);

            discarded.LegacyShouldHaveCountOf(1)
                .LegacyShouldContain(2);
        }
        [Test]
        public void all_discarded_items_are_notified()
        {
            var selected = new List<int>();
            var discarded = new List<int>();
            var source = new[] { 1, 2, 3 };
            var result = source.AsObservable(x => x.Where(i => false), selected.Add, discarded.Add).ToList();

            selected.LegacyShouldHaveCountOf(0);

            discarded.LegacyShouldHaveCountOf(3)
                .LegacyShouldContain(1)
                .LegacyShouldContain(2)
                .LegacyShouldContain(3);
            
        }
    }
}
