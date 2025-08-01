# SumSharp

A highly configurable C# discriminated union library

[![NuGet](https://img.shields.io/nuget/v/SumSharp.svg)](https://www.nuget.org/packages/SumSharp)
[![Build](https://github.com/christiandaley/SumSharp/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/christiandaley/SumSharp/actions)
[![Publish](https://github.com/christiandaley/SumSharp/actions/workflows/publish.yml/badge.svg)](https://github.com/christiandaley/SumSharp/actions)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

---

1. [Installation](#installation)
2. [Features](#features)
3. [Quick start](#quick-start)
   - [Creating a DU type](#creating-a-du-type)
   - [Empty cases](#empty-cases)
   - [Generic cases](#generic-cases)
4. [Motivation](#motivation)
   - [What about `OneOf`?](#what-about-oneof)
   - [Typical DU implementation approaches](#typical-du-implementation-approaches)
   - [SumSharp's approach](#sumsharps-approach)
5. [Usage Guide](#usage-guide)
   - [Controlling the memory layout](#controlling-the-memory-layout)
   - [Struct union types](#struct-union-types)
   - [Generic interface types](#generic-interface-types)
   - [JSON serialization](#json-serialization)
   - [OneOf interop](#oneof-interop)
   - [Disabling value equality](#disabling-value-equality)
   - [Disabling nullable annotations](#disabling-nullable-annotations)
6. [Contributing](#contributing)
7. [License](#license)

---

## Installation

```bash
dotnet add package SumSharp
```

---

## Features

- Unlimited number of cases
- Support for class, struct, record, and record struct union types
- Support for generic type cases
- **Highly configurable memory layout**, allowing developers to optimize for their app's memory/perfomance requirements
- Built in JSON serialization with both System.Text.Json and Newtonsoft.Json. Compatible with System.Text.Json source generation and AOT compilation
- Implicit conversions to/from `OneOf` types

---

## Quick start

### Creating a DU type

To create a discriminted union type, simply declare a partial class/struct and add `UnionCase` attributes that describe the different cases.

```csharp
using SumSharp;

[UnionCase("String", typeof(string))]
[UnionCase("Double", typeof(double))]
partial class StringOrDouble
{

}
```

That's it! `SumSharp` will generate members for the `StringOrDouble` class that allow it to be used as a discriminated union type. These members include:

- `String` and `Double` static functions that construct instances of `StringOrDouble`
- `AsString` and `AsDouble` properties that return either the underlying string/double value or throw an `InvalidOperationException`
- `IsString` and `IsDouble` boolean properties
- `Switch`, `Match`, `IfString`, and `IfDouble` functions for control flow
- An `Index` int property that reflects the current case
- Implicit conversions from string/double to `StringOrDouble`
- Implementation of the `IEquatable<StringOrDouble>` interface, `Object.Equals` override and `==` and `!=` operators to allow for value equality comparisons
- Various overloads of `As[CaseName]` and `If[CaseName]` to allow for more expressive control flow

```csharp
var x = StringOrDouble.Double(3.14);

// Prints "Value is a double: 3.14"
x.Switch(
  value => Console.WriteLine($"Value is a string: {value}"),
  value => Console.WriteLine($"Value is a double: {value}"));

StringOrDouble y = "abcdefg";

// result is "Value is a string: abcdefg"
var result = y.Match(
  value => $"Value is a string: {value}",
  value => $"Value is a double: {value}");

// Prints "abcdefg"
Console.WriteLine(y.AsString);

// throws InvalidOperationException
Console.WriteLine(y.AsDouble);
```

### Empty cases

`SumSharp` supports empty cases that carry no value. An empty case requires only a name to be supplied.

```csharp
[UnionCase("String", typeof(string))]
[UnionCase("Empty")]
partial class StringOrEmpty
{

}
```

Instead of a static function, empty case constructors are a static, get-only property backed by a singleton.

### Generic cases

Case types can be generic. To define a generic case you must supply the **name** of the generic type rather than the type itself because C\# does not allow for generic types to be used as arguments to attributes.

```csharp
[UnionCase("Some", "T")]
[UnionCase("Empty")]
partial class Optional<T>
{

}
```

---

## Motivation

C\# unfortunately does not offer discriminated unions as a language feature. Although [a proposal](https://github.com/dotnet/csharplang/blob/18a527bcc1f0bdaf542d8b9a189c50068615b439/proposals/TypeUnions.md) has existed for a while, this feature doesn't seem to be coming in the near future.

### What about `OneOf`?

[OneOf](https://github.com/mcintyre321/OneOf) is a popular existing discriminated union library for C\# that I have personally used and found very helpful. There are, however, several pain points in using `OneOf` that I have encountered, such as:

- Limited number of cases (The base library limits you to 8. There is an extended version that allows up to 32)
- No support for case "names"
- The underlying implementation uses a dedicated field for each individual case, resulting in a larger memory footprint than is neccessary
- Limited support for JSON serialization (There is a [separate package](https://github.com/Liversage/OneOf.Serialization.SystemTextJson) that provides System.Text.Json serialization support)
- All `OneOf` instances are structs and all user defined types inheriting from `OneOfBase` must be classes. No ability to pick and choose the type kind you want to use

Overall `OneOf` is an excellent library that has served me and many other developers well, but I felt that with the advent of C\# source generators it would be possible to produce a more powerful discriminated union library.

### Typical DU Implementation approaches

#### Constructing values "in place"

There are several possible ways to represent a discriminated union at runtime. One approach taken by languages such as Rust and C++ (with typical `std::variant` implementations) is to define a discriminated union as a struct that contains an integer "index" that identifies the case along with `N` bytes of storage, where `N` is the size of the largest possible type that the union can hold. When an instance of a union is constructed the underlying value is constructed in place using the dedicated storage.

This approach cannot be implemented in C\# because C\# does not have the concept of a "placement new" operator like C++ and does not allow instances of managed types to share memory addresses.

#### As an abstract class

The most common approach to manually implementing discriminated unions in C\# is to use an abstract class and inheritence to define the cases. This is also how discriminated unions are implemented under the hood in F\#.

```csharp
abstract record StringOrDouble
{
  public record String(string Value) : StringOrDouble;
  public record Double(double Value) : StringOrDouble;
}
```

This approach has the advantage of being simple, but also has several drawbacks. Namely:

- Inheritence requires using classes. Each instance of the union incurs a heap allocation, which is theoretically unnecessary for value type cases
- The union type is not "closed". Nothing prevents any random class from inheriting the base type
- The C\# compiler will not enforce exhaustive switch statements (There is a library called [dunet](https://github.com/domn1995/dunet) that uses code generation to create these abstract class based unions with exhaustive switching)
- Instances of the union must be created by constructing an instance of a dervived type using `new`. In languages with first class support for DUs the case constructors are typically static member functions of the union type.

### `SumSharp`'s approach

`SumSharp` takes a hybrid, customizable approach to representing a discriminated union. Developers can choose between storage "strategies" such as

- Using a single `object` field that is shared between all cases (effectively identical to the abstract class based approach, from a memory usage standpoint)
- Using a single `object` field only for reference types while storing value types directly within the union type itself
- Using "shared" storage for value types that meet the `unmanaged` constraint. This includes any primitive type (int, double, bool, etc.), any enum type, and structs that contain only unmanaged types

Additionally, developers can choose to have their union types be either a class or struct. This means that it's possible to avoid heap allocations entirely if the value being stored is a value type. In the case where all of the possible types meet the `unmanaged` constraint, it is possible for the generated implementation to be identical to that of Rust unions and `std::variant`, where all types share a single chunk of bytes for storage and the overall size of the union is determined by the size of the largest possible type.

---

## Usage guide

### Controlling the memory layout

The memory layout of a union can be controlled on a case-by-case basis using the `StorageMode` argument to the `UnionCase` attribute, and on union-wide basis using the `Strategy` argument to the `Storage` attribute.

#### StorageMode

```csharp
[UnionCase("String", typeof(string), StorageMode: StorageMode.AsObject)]
[UnionCase("Double", typeof(double), StorageMode: StorageMode.Inline)]
partial class StringOrDouble
{

}
```

The `String` case will use an `object` field for its storage. The `Double` case will store its value ["inline"](#what-exactly-is-inline-storage), meaning that the storage will be provided by the union type itself and will not require boxing the double as an object on the heap. So, the `StringOrDouble` class will contain exactly two fields to provide its storage. Note that the `StorageMode.AsObject` argument to the `String` case is redundant because reference types will be stored as an `object` by default.

#### UnionStorageStrategy

```csharp
[UnionCase("String", typeof(string))]
[UnionCase("Double", typeof(double))]
[Storage(Strategy: UnionStorageStrategy.InlineValueTypes)]
partial class StringOrDouble
{

}
```

Using the `InlineValueTypes` storage strategy results in all value type cases being stored "inline". It is equivalent to specifying `StorageMode.Inline` for all value type cases. The other possible strategies are `OneObject` (uses a single object field to store all cases) and `Default` (explained below).

**Note that the `StorageMode` of an individual case takes precedent over the `UnionStorageStrategy` of a union, so if we had specified `StorageMode.AsObject` for the `Double` case then the double value would end up being boxed and use the same `object` field that the `String` case uses.**

#### Rules for how storage is determined

The rules for determining how cases store their values are:

1. If the `StorageMode` for an individual case is:

   - `AsObject`: The value is stored in an `object` field shared with all other cases that are stored as an object.
   - `Inline`: The value is stored inline.
   - `Default`: The overall `UnionStorageStrategy` is used to determine the storage for the case.

2. If the `UnionStorageStrategy` for the union is:
   - `OneObject`: Cases with a `Default` storage mode have their values stored in an `object` field shared with all other cases that are stored as an object.
   - `InlineValueTypes`: Cases with a `Default` storage mode have their values stored inline if `SumSharp` detects that the type is a value type, otherwise in an `object` field shared with all other cases that are stored as an object
   - `Default`: All cases are stored as an object _unless there is exactly one unique type across all cases and none of the cases have an `AsObject` storage mode, in which case inline storage is used for that type._

#### What exactly is inline storage?

What it means for a type to be stored "inline" depends on whether `SumSharp` [recognizes that type as meeting the `unmanaged` constraint](#in-what-circumstances-does-sumsharp-know-if-a-type-is-unmanaged).

- If `SumSharp` detects that a type is unmanaged, it will share storage for that type along with all other types it detects as being unmanaged. The total size of the storage is determined by the size of the largest unmanaged type across all cases. The storage itself is a struct member field in the union type that requires no heap allocation.
- If `SumSharp` cannot determine that a type is unmanaged it will provide a dedicated member field for that type. Any other cases with the same type will share the same field.

#### In what circumstances does SumSharp know if a type is unmanaged?

The circumstances in which `SumSharp` knows that a type meets the `unmanaged` constraint are:

1. The type is a primitive type (double, int, bool, etc.) or an enum type
2. The type is a non-generic struct that is declared in the same assembly as the union type, and recursively contains only primitive/enum types or other structs also defined in the same assmebly as the union type
3. The `ForceUnmanagedStorage` argument is set to `true`

Let's walk through an example:

```csharp

struct UnmanagedStruct
{
  public byte Value;
}

[UnionCase("Case0", typeof(int))]
[UnionCase("Case1", typeof(UnmanagedStruct))]
[UnionCase("Case2", typeof(System.HashCode))]
[UnionCase("Case3", "T")]
[Storage(Strategy: UnionStorageStrategy.InlineValueTypes)]
partial class Example<T> where T : unmanaged
{

}
```

Here we have four possible types: `int`, `UnmanagedStruct`, `System.HashCode`, and the generic `T`. Of these four types, only `int` and `UnmanagedStruct` will be recognized as unmanaged and thus have overlapping storage. `System.HashCode` does meet the `unmanaged` constraint but is defined in a different assembly, so `SumSharp` is not able to confidently determine that it is an unmanaged type and will err on the side of caution. `T` is a generic parameter that has the `unmanaged` constraint but because it does not have a known size at compile time `SumSharp` cannot determine how many bytes to reserve for its storage. `Case2` and `Case3` will end up using separates fields of type `System.HashCode` and `T`.

#### Forcing unmanaged storage

To force `SumSharp` to use unmanaged storage for all four types, we can pass `ForceUnmanagedStorage: true` to `Case2` and `Case3`. Additionally, because `T` has unknown size we must also pass the `UnmanagedStorageSize` argument to the `Storage` attribute to explicitly define how many bytes of storage should be reserved for storing unmanaged types. The `UnmanagedStorageSize` overrides any determination that `SumSharp` makes about the size of unmanaged types.

```csharp
[UnionCase("Case0", typeof(int))]
[UnionCase("Case1", typeof(UnmanagedStruct))]
[UnionCase("Case2", typeof(System.HashCode), ForceUnmanagedStorage: true)]
[UnionCase("Case3", "T", ForceUnmanagedStorage: true)]
[Storage(Strategy: UnionStorageStrategy.InlineValueTypes, UnmanagedStorageSize: 32)]
partial class Example<T> where T : unmanaged
{

}
```

The generated code for `Example<T>` will now have only a single field with a size of 32 bytes. **Note that if we had not forced `T` to use unmanaged storage we could have ommitted the `UnmanagedStorageSize` argument because the size of `System.HashCode` is fixed at compile time. Providing `UnmanagedStorageSize` is only neccessary for generic unmanaged types.**

Whenever unmanaged storage is used `SumSharp` will emit a static constructor for the union that performs a runtime check to ensure that the storage reserved for the unmanaged types is sufficient. If the storage is insufficient a `TypeInitializationException` will be thrown the first time the union is attempted to be used. The exception message will contain information about how much storage is required.

```csharp

var x = Example<(double, double)>.Case0(4); // Okay, 32 bytes is enough to store a tuple with two doubles

var y = Example<(double, double, long, long, ulong)>.Case0(4); // TypeInitializationException, 32 bytes is not enough. Need to increase the storage size to 40

```

**Using `ForceUnmanagedStorage: true` for types that do not meet the `unmanaged` constraint will result in a compilation error.**

#### Generic type constraints

For generic cases where the type is a type argument to the union type (or one of its containing types) `SumSharp` is able to detect `struct` and `class` constraints.

```csharp
[UnionCase("Case0", "T")]
[UnionCase("Case1", "U")]
[Storage(Strategy: UnionStorageStrategy.InlineValueTypes)]
partial class GenericUnion<T, U>
  where T : struct
{

}
```

Because `T` has a `struct` constraint and the storage strategy is `InlineValueTypes`, `T` will be stored inline. `U` will be stored as an `object` because it is not constrained to be a value type.

`SumSharp` is not able to determine constraints for more general generic cases.

```csharp
struct GenericStruct<T>
{
  public T Value;
}


[UnionCase("Case0", "GenericStruct<T>")]
[Storage(Strategy: UnionStorageStrategy.InlineValueTypes)]
partial class GenericUnion<T>
{

}
```

Here, `GenericStruct<T>` is always a value type but `SumSharp` cannot determine that, so even with the `InlineValueTypes` storage strategy it will end up being stored on the heap as an object. To avoid this pass `GenericTypeInfo: GenericTypeInfo.ValueType` to the case constructor.

```csharp
[UnionCase("Case0", "GenericStruct<T>", GenericTypeInfo: GenericTypeInfo.ValueType)]
[Storage(Strategy: UnionStorageStrategy.InlineValueTypes)]
partial class GenericUnion<T>
{

}
```

You can also pass `GenericTypeInfo.ReferenceType` for generic types that you know are reference types. Doing this is never neccessary for the code to work, but it helps `SumSharp` emit more efficient code.

### Struct union types

As mentioned before, `SumSharp` allows for struct and record struct union types. It's important to remember that **any struct union instance that is initialized to `default` is in an invalid state and its behavior is undefined**. The only valid way to create a `SumSharp` union is to use one of its case constructors or conversion operators. C\# allows for any struct instance to be initialized to a `default` value which involves initializing every instance member field to its default value. A `SumSharp` union initialized in such a way is in an invalid, undefined state. Using it may result in exceptions being thrown, or may silently work. **`SumSharp` makes no guarantees about the runtime behavior of default initialized struct unions.**

```csharp
[UnionCase("Case0", typeof(double))]
[UnionCase("Case1", typeof(int))]
partial struct StructUnionType
{

}

// ...

StructUnionType x = default;

// DON'T DO THIS. Undefined behavior. May throw an exception.
x.IfDouble(value =>
{
  Console.WriteLine(value);
});
```

### Generic interface types

As shown in the quickstart guide, `SumSharp` supports generic discriminated unions. If a generic union has a case with a generic interface type, you must add `IsInterface: true` to the case attribute so the generator knows that it is an interface type. This is neccessary because generic types are specified by name rather than a `typeof` expression, so `SumSharp` does not have access to detailed type information like it does for non-generic cases. C\# does not allow conversion operators to work on interface types. To prevent compile errors, `SumSharp` will not emit conversion operators on interface types.

```csharp
[UnionCase("Case0", "IEnumerable<T>", IsInterface: true)]
partial class GenericEnumerable<T>
{

}
```

Failing to add `IsInterface: true` to the above union definition will result in a compile error in the generated code.

### JSON serialization

#### Basics

To enable support for JSON serialization/deserialization just add the `[EnableJsonSerialization]` attribute to your union type. For example:

```csharp
[UnionCase("Case0", typeof(double))]
[UnionCase("Case1", typeof(string))]
[EnableJsonSerialization]
partial class Serializable
{

}
```

A nested, `public partial` class called `StandardJsonConverter` that inherits `System.Text.Json.Serialization.JsonConverter<Serializable>` will be defined in the `Serializable` class. A `System.Text.Json.Serialization.JsonConverterAttribute` will also be added so that this converter will be automatically used at runtime.

If you need support for `Newtonsoft.Json` serialization instead, you can use `[EnableJsonSerialization(JsonSerializationSupport.Newtonsoft)]` and a class called `NewtonsoftJsonConverter` that inherits from `Newtonsoft.Json.JsonConverter<Serializable>` will be generated instead. If you need both standard and Newtonsoft support, you can pass `JsonSerializationSupport.Standard | JsonSerializationSupport.Newtonsoft` to the `EnableJsonSerialization` attribute.

#### Generic types

In addition to a nested `JsonConverter` implementation, `SumSharp` will also emit a non-generic `static partial` class with the same name as your generic union type that contains a `JsonConverter` implementation capable of handling any instance of your generic type. This converter is what is used in the `JsonConverterAttribute` on the class.

```csharp
[UnionCase("Case0", "T")]
[UnionCase("Case1", "U")]
[EnableJsonSerialization]
partial class Serializable<T, U>
{

}
```

The generated `Serializable<T, U>.StandardJsonConverter` is capable of handling a `Serializable<T, U>`, while `Serializable.StandardJsonConverter` is capable of handling any `Serializable<,>`.

This works identically for Newtonsoft serialization.

#### Nested generic types

If your union type is nested within a generic type, the `JsonConverter` attribute will not be automatically added because the generated `StandardJsonConverter` is considered a generic type by the compiler and generic types cannot be used as arguments to attributes.

```csharp
partial class GenericClass<T>
{
  [UnionCase("Case0", typeof(double))]
  [UnionCase("Case1", typeof(string))]
  [EnableJsonSerialization] // No JsonConverterAttribute will be added
  public partial class NestedSerializable
  {

  }
}
```

In this case you must use a `JsonSerializerOptions` and manually create the converter.

#### System.Text.Json source generation

When using `System.Text.Json` source generation there is some boilerplate code that the developer must write. The reason for this is that dotnet code generators cannot see code produced by other generators, meaning that `System.Text.Json` won't be able to see the automatically added `JsonConverter` attribute in the generated code. To get around this you must pass `AddJsonConverterAttribute: false` to the `EnableJsonSerialization` attribute and manually add the `JsonConverter` attribute. You must also redeclare the `StandardJsonConverter` class.

```csharp
[UnionCase("Case0", typeof(double))]
[UnionCase("Case1", typeof(string))]
[EnableJsonSerialization(AddJsonConverterAttribute: false)]
[System.Text.Json.Serialization.JsonConverter(Serializable.StandardJsonConverter)]
partial class Serializable
{
  public partial class StandardJsonConverter : System.Text.Json.Serialization.JsonConverter<Serializable>
  {

  }
}
```

For generic union types the process is similar, except that you must redeclare the converter on the generated static class instead. The converter inherits from `System.Text.Json.Serialization.JsonConverterFactory`.

```csharp
[UnionCase("Case0", "T")]
[UnionCase("Case1", "U")]
[EnableJsonSerialization(AddJsonConverterAttribute: false)]
[System.Text.Json.Serialization.JsonConverter(Serializable.StandardJsonConverter)]
partial class Serializable<T, U>
{

}

static partial class Serializable
{
  public partial class StandardJsonConverter : System.Text.Json.Serialization.JsonConverterFactory
  {

  }
}

```

#### AOT compilation

If you are using AOT compilation, pass `UsingAOTCompilation: true` to the `EnableJsonSerialization` attribute. This will cause `SumSharp` to emit code that prevents trimming of the individual `StandardJsonConverter` classes for each instantiation of the generic union. There's no need to also specify `AddJsonConverterAttribute: false` in this case because using AOT compilation implies that you are using source generation.

### OneOf interop

Interested in using `SumSharp` but already using `OneOf` in your codebase and don't want to completely refactor your code? Not a problem. Add the `[EnableOneOfConversions]` attribute to allow implicit conversions between `OneOf` instances and compatible `SumSharp` union types. You can use `SumSharp` in newer parts of your codebase and seemlessly interop with older code that uses `OneOf`.

```csharp
[UnionCase("String", typeof(string))]
[UnionCase("Double", typeof(double))]
[EnableOneOfConversions]
partial class StringOrDouble
{

}

// ...

// Convert from StringOrDouble to OneOf<string, double>
OneOf<string, double> x = StringOrDouble.Double(2.45);

// Convert from OneOf<string, double> to StringOrDouble
StringOrDouble y = x;
```

Empty `SumSharp` union cases are mapped to `OneOf.Types.None`

```csharp
[UnionCase("String", typeof(string))]
[UnionCase("Empty")]
[EnableOneOfConversions]
partial class StringOrEmpty
{

}

// ...

// Convert from StringOrEmpty to OneOf<string, OneOf.Types.None>
OneOf<string, OneOf.Types.None> x = StringOrEmpty.Empty;

// Convert from OneOf<string, OneOf.Types.None> to StringOrEmpty
StringOrEmpty y = x;
```

If you're using a type other than `OneOf.Types.None` to represent empty cases in your `OneOf` instances, pass that type to the `EnableOneOfConversions` attribute.

```csharp

struct CustomEmptyType
{

}

[UnionCase("String", typeof(string))]
[UnionCase("Empty")]
[EnableOneOfConversions(typeof(CustomEmptyType))]
partial class StringOrEmpty
{

}

// ...

// Convert from StringOrEmpty to OneOf<string, CustomEmptyType>
OneOf<string, CustomEmptyType> x = StringOrEmpty.Empty;

// Convert from OneOf<string, CustomEmptyType> to StringOrEmpty
StringOrEmpty y = x;
```

The custom empty type is required to have a parameterless (default) constructor.

### Disabling value equality

All `SumSharp` union types by default implement the `IEquatable<T>` interface, override the `Object.Equals` member function, and implement `==` and `!=` operators. This allows for value type equality between instances: Two instances of the same union type are equal iff they both hold the same case and their underlying values compare equal using the static `Object.Equals` function.

If you'd rather disable this feature and have reference equality for class type unions add the `[DisableValueEquality]` attribute to your union. _Note that adding this attribute does nothing for record union types because the C# compiler will always add an `IEquatable` implementation for record types._

### Disabling nullable annotations

The generated code emitted by `SumSharp` makes use of `nullable` (?) annotations. If you prefer to not have nullable annotations in the generated code add the `[DisableNullable]` attribute to your union.

---

## Contributing

If you find any bugs or have any feature suggestions, please open an issue. Pull requests are welcome as well.

---

## License

Licensed under the [MIT License](LICENSE).
