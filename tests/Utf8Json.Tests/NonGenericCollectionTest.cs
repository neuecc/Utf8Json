using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Utf8Json.Tests
{
    public class NonGenericCollectionTest
    {
        [Fact]
        public void List()
        {
            var xs = new System.Collections.ArrayList { 1, 100, "hoge", 999.888 };
            {
                var bin = JsonSerializer.Serialize<IList>(xs);
                var v = JsonSerializer.Deserialize<IList>(bin);

                ((double)v[0]).Is((double)1);
                ((double)v[1]).Is((double)100);
                ((string)v[2]).Is("hoge");
                ((double)v[3]).Is(999.888);
            }
            {
                var bin = JsonSerializer.Serialize(xs);
                var v = JsonSerializer.Deserialize<ArrayList>(bin);

                ((double)v[0]).Is((double)1);
                ((double)v[1]).Is((double)100);
                ((string)v[2]).Is("hoge");
                ((double)v[3]).Is(999.888);
            }
        }

        [Fact]
        public void Dictionary()
        {
            {
                var xs = new System.Collections.Hashtable { { "a", 1 }, { 100, "hoge" }, { "foo", 999.888 } };
                var bin = JsonSerializer.Serialize<IDictionary>(xs);
                var v = JsonSerializer.Deserialize<IDictionary>(bin);

                v["a"].Is((object)(double)1);
                v["100"].Is((object)(string)"hoge");
                v["foo"].Is((object)(double)999.888);
            }
            {
                var xs = new System.Collections.Hashtable { { "a", 1 }, { 100, "hoge" }, { "foo", 999.888 } };
                var bin = JsonSerializer.Serialize<Hashtable>(xs);
                var v = JsonSerializer.Deserialize<Hashtable>(bin);

                v["a"].Is((object)(double)1);
                v["100"].Is((object)(string)"hoge");
                v["foo"].Is((object)(double)999.888);
            }
        }

        public void IEnumerableTest()
        {
            var xs = new System.Collections.ArrayList { 1, 100, "hoge", 999.888 };
            {
                var bin = JsonSerializer.Serialize<IEnumerable>(xs);
                var v = JsonSerializer.Deserialize<IEnumerable>(bin).GetEnumerator();

                v.MoveNext();
                ((double)v.Current).Is((double)1);

                v.MoveNext();
                ((double)v.Current).Is((double)100);

                v.MoveNext();
                ((string)v.Current).Is("hoge");

                v.MoveNext();
                ((double)v.Current).Is(999.888);
            }
        }

        public void ICollectionTest()
        {
            var xs = new System.Collections.ArrayList { 1, 100, "hoge", 999.888 };
            {
                var bin = JsonSerializer.Serialize<ICollection>(xs);
                var v2 = JsonSerializer.Deserialize<ICollection>(bin);
                v2.Count.Is(4);

                var v = v2.GetEnumerator();

                v.MoveNext();
                ((double)v.Current).Is((double)1);

                v.MoveNext();
                ((double)v.Current).Is((double)100);

                v.MoveNext();
                ((string)v.Current).Is("hoge");

                v.MoveNext();
                ((double)v.Current).Is(999.888);
            }
        }
    }
}
