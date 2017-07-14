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
            var model3 = new Model { Id = 0, Text = "added" };
            var models = new List<Model> { model1, model3 };

            entities.UpdateFrom(models)
                .WithKeys(e => e.Id, m => m.Id)
                .MapValues((e, m) =>
                {
                    e.Text = m.Text;
                });

            Assert.AreEqual(2, entities.Count);

            Assert.AreSame(entity1, entities[0]);
            Assert.AreEqual(1, entities[0].Id);
            Assert.AreEqual("first changed", entities[0].Text);

            Assert.AreNotSame(entity2, entities[1]);
            Assert.AreEqual(0, entities[1].Id);
            Assert.AreEqual("added", entities[1].Text);
        }
    }
}
