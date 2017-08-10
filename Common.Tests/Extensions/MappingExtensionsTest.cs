using System.Collections.Generic;
using Common.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Common.Tests.Extensions
{
    [TestClass]
    public class MappingExtensionsTest
    {
        public class Entity
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        public class Model
        {
            public int Id { get; set; }
            public string Text { get; set; }
        }

        [TestMethod]
        public void TestCollectionMapping()
        {
            var entity1 = new Entity { Id = 1, Text = "first" };
            var entity2 = new Entity { Id = 2, Text = "second" };
            var entities = new List<Entity> { entity1, entity2 };

            var model1 = new Model { Id = 1, Text = "first changed" };
            var model3 = new Model { Id = 0, Text = "third added" };
            var models = new List<Model> { model1, model3 };

            var added = new List<Entity>();
            var updated = new List<Entity>();
            var removed = new List<Entity>();

            entities.MapFrom(models)
                .WithKeys(e => e.Id, m => m.Id)
                .OnAdd(added.Add)
                .OnUpdate(updated.Add)
                .OnRemove(removed.Add)
                .OnAdd(e => e)
                .OnUpdate(e => e)
                .OnRemove(e => e)
                .MapElements((e, m) =>
                {
                    e.Text = m.Text;
                });

            Assert.AreEqual(2, entities.Count);

            Assert.AreSame(entity1, entities[0]);
            Assert.AreEqual(1, entities[0].Id);
            Assert.AreEqual("first changed", entities[0].Text);

            Assert.AreNotSame(entity2, entities[1]);
            Assert.AreEqual(0, entities[1].Id);
            Assert.AreEqual("third added", entities[1].Text);

            Assert.AreEqual(1, added.Count);
            Assert.AreEqual(0, added[0].Id);
            Assert.AreEqual("third added", added[0].Text);

            Assert.AreEqual(1, updated.Count);
            Assert.AreEqual(1, updated[0].Id);
            Assert.AreEqual("first changed", updated[0].Text);

            Assert.AreEqual(1, removed.Count);
            Assert.AreEqual(2, removed[0].Id);
            Assert.AreEqual("second", removed[0].Text);
        }
    }
}
