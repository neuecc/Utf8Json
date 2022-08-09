using System.Collections.Generic;
using Utf8Json.Resolvers;
using Xunit;

namespace Utf8Json.Tests
{
	public class PolymorphicTest
	{
		[Fact]
		public void Derived_class_is_serialized_and_deserialized_as_instance_of_base_type()
		{
			var obj = new Derived
			{
				BaseProp = "base",
				DerivedProp = "derived"
			};
			JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivate);
			var json = JsonSerializer.Serialize<Base>(obj);
			var deserialized = JsonSerializer.Deserialize<Base>(json);
			
			Assert.IsType(typeof(Derived), deserialized);
			Assert.Equal(obj.BaseProp, deserialized.BaseProp);
			Assert.Equal(obj.DerivedProp, ((Derived)deserialized).DerivedProp);
		}

		[Fact]
		public void Null_is_serialized_and_deserialized_correctly()
		{
			JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivate);
			var json = JsonSerializer.Serialize<Base>(null);
			var deserialized = JsonSerializer.Deserialize<Base>(json);
			
			Assert.Null(deserialized);
		}

		[Fact]
		public void List_of_polymorphic_type_is_serialized_and_deserialized_correctly()
		{
			var list = new List<Base>
			{
				new Derived { BaseProp = "base", DerivedProp = "derived"},
				new AnotherDerived { BaseProp = "base", AnotherProp = "another"},
				null
			};
			JsonSerializer.SetDefaultResolver(StandardResolver.AllowPrivate);
			var json = JsonSerializer.Serialize(list);
			var deserialized = JsonSerializer.Deserialize<List<Base>>(json);
			
			Assert.Collection(deserialized,
				first =>
				{
					var derived = first as Derived;
					Assert.NotNull(derived);
					Assert.Equal(list[0].BaseProp, derived.BaseProp);
					Assert.Equal(((Derived)list[0]).DerivedProp, derived.DerivedProp);
				},
				second =>
				{
					var another = second as AnotherDerived;
					Assert.NotNull(another);
					Assert.Equal(list[1].BaseProp, another.BaseProp);
					Assert.Equal(((AnotherDerived)list[1]).AnotherProp, another.AnotherProp);
				},
				Assert.Null);
		}
	}

	[PolymorphicFormatter]
	abstract class Base
	{
		public string BaseProp { get; set; }
	}

	class Derived : Base
	{
		public string DerivedProp { get; set; }
	}
	
	class AnotherDerived : Base
	{
		public string AnotherProp { get; set; }
	}
}