using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MrAdvice.Aspects.Tests.Snapshot
{
    [TestClass]
    public class SnapshotTest
    {   
        [TestMethod]
        public void TestWrite()
        {
            SnapshotAspect.Profile.Value = new SnapshotProfile
            {
                FolderPath = $"{nameof(SnapshotTest)}",
                Mode = SnapshotMode.Write,
            };

            var data = new Data
            {
                Foo = "foo",
                Bar = 123,
                Date = new DateTime(2017, 04, 20),
            };
            data.References.Add(data);

            var output = Procedure(data);

            Assert.AreSame(data, output);
        }

        [TestMethod]
        public void TestRead()
        {
            SnapshotAspect.Profile.Value = new SnapshotProfile
            {
                FolderPath = $"{nameof(SnapshotTest)}",
                Mode = SnapshotMode.Read,
            };

            var data = new Data
            {
                Foo = "foo",
                Bar = 123,
                Date = new DateTime(2017, 04, 20),
            };
            data.References.Add(data);

            var output = Procedure(data);

            Assert.AreNotSame(data, output);
            Assert.AreEqual(data.Foo, output.Foo);
            Assert.AreEqual(data.Bar, output.Bar);
            Assert.AreEqual(data.Date, output.Date);
            Assert.AreEqual(1, output.References.Count);
            Assert.AreSame(output, output.References[0]);
        }

        [Snapshot]
        public Data Procedure(Data input)
        {
            return input;
        }

        public class Data
        {
            public string Foo { get; set; }
            public int Bar { get; set; }
            public DateTime Date { get; set; }
            public List<Data> References { get; set; } = new List<Data>();
        }
    }
}
