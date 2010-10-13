using System;
using System.Linq;
using NUnit.Framework;
using NoCap.Library.Util;

namespace NoCap.Library.Tests.Util {
    [TestFixture]
    public class LinqHelpersTests {
        private class TrackingEqualsWrapper<T> {
            private readonly T wrappedObject;

            public T WrappedObject {
                get {
                    return this.wrappedObject;
                }
            }

            public int EqualsCalls {
                get;
                set;
            }

            public TrackingEqualsWrapper(T wrappedObject) {
                if (wrappedObject == null) {
                    throw new ArgumentNullException("wrappedObject");
                }

                this.wrappedObject = wrappedObject;
            }

            public override bool Equals(object obj) {
                EqualsCalls += 1;

                var objAsTEW = obj as TrackingEqualsWrapper<T>;

                if (objAsTEW == null) {
                    return false;
                }

                return WrappedObject.Equals(objAsTEW.WrappedObject);
            }

            public override int GetHashCode() {
                if (WrappedObject == null) {
                    return 1337;
                }

                return WrappedObject.GetHashCode();
            }
        }

        [Test]
        public void UniqueThrowsOnNullThis() {
            Assert.Throws<ArgumentNullException>(() => LinqHelpers.Unique<object>(null));
        }

        [Test]
        public void UniqueCallsEquals() {
            var items = new[] { 1, 1, 1 }.Select((b) => new TrackingEqualsWrapper<int>(b)).ToList();

            var uniqueItems = items.Unique();

            // At least two comparisons must be made per object
            foreach (var item in items) {
                Assert.GreaterOrEqual(3, item.EqualsCalls);
            }

            // Make sure the return value of equals was used
            Assert.AreEqual(1, uniqueItems.Count());
            Assert.AreEqual(1, uniqueItems.First().WrappedObject);
        }
    }
}
