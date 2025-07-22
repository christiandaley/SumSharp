using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dotsum.Generator;

internal class SymbolHandler
{
    public abstract class TypeInfo
    {
        public abstract string Name { get; }

        public abstract bool IsGeneric { get;}

        public abstract bool IsAlwaysValueType { get; }

        public abstract bool IsAlwaysRefType { get; }

        public abstract bool IsInterface { get; }

        public class NonArray(INamedTypeSymbol symbol) : TypeInfo
        {
            public override string Name => symbol.ToDisplayString();

            public override bool IsGeneric => false;

            public override bool IsAlwaysValueType => symbol.IsValueType;

            public override bool IsAlwaysRefType => symbol.IsReferenceType;

            public override bool IsInterface => symbol.TypeKind == TypeKind.Interface;
        }

        public class Array(string name) : TypeInfo
        {
            public override string Name => name;

            public override bool IsGeneric => false;

            public override bool IsAlwaysValueType => false;

            public override bool IsAlwaysRefType => true;

            public override bool IsInterface => false;
        }

        public class SimpleGenericTypeArgument(ITypeParameterSymbol symbol) : TypeInfo
        {
            public override string Name => symbol.Name;

            public override bool IsGeneric => true;

            public override bool IsAlwaysValueType => symbol.HasValueTypeConstraint;

            public override bool IsAlwaysRefType => symbol.HasReferenceTypeConstraint;

            public override bool IsInterface => false;
        }

        public class GeneralGenericTypeArgument(string name) : TypeInfo
        {
            public override string Name => name;

            public override bool IsGeneric => true;

            public override bool IsAlwaysValueType => false;

            public override bool IsAlwaysRefType => false;

            public override bool IsInterface => false;
        }
    }

    public record CaseData(int Index, string Name, TypeInfo? TypeInfo, bool StoreAsObject)
    {
        public string? FieldType => TypeInfo == null ? null : StoreAsObject ? "object" : TypeInfo.Name;
    }

    public StringBuilder Builder { get; }

    public string? Namespace { get; }

    public bool IsStruct { get; }

    public string Accessibility { get; }

    public bool IsGenericType => TypeArguments.Length > 0;

    public string[] TypeArguments { get; }

    public string NameWithoutTypeArguments { get; }

    public string Name { get; }

    public CaseData[] Cases { get; }

    public CaseData[] UniqueCases { get; }

    public INamedTypeSymbol[] ContainingTypes;

    public Dictionary<string, string> TypeToFieldNameMap { get; } = [];

    public bool BoxesValueTypes { get; } = false;

    public bool EnableStandardJsonSerialization { get; }

    public bool UsingSourceGeneration { get; }

    public SymbolHandler(
        StringBuilder builder,
        INamedTypeSymbol symbol,
        INamedTypeSymbol caseAttrSymbol,
        INamedTypeSymbol enableJsonSerializationAttrSymbol,
        INamedTypeSymbol StorageAttrSymbol)
    {
        Builder = builder;

        Namespace =
            symbol.ContainingNamespace.IsGlobalNamespace
            ? null
            : symbol.ContainingNamespace.ToDisplayString();

        IsStruct = GetIsStruct(symbol);

        Accessibility = GetAccessibility(symbol);

        TypeArguments = [.. symbol.TypeArguments.Select(arg => arg.ToString())];

        NameWithoutTypeArguments = symbol.Name;

        Name = GetFullName(symbol);

        var storageData =
            symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, StorageAttrSymbol))
            .SingleOrDefault();

        var storageStrategy = (int?)storageData?.ConstructorArguments[0].Value ?? 0;

        Cases = symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, caseAttrSymbol))
            .Select((attr, i) =>
            {
                switch (attr.ConstructorArguments)
                {
                    case [var caseName]:
                        return new CaseData(i, caseName.Value!.ToString(), null, true);

                    case [var caseName, var caseType, var caseStorageMode]:

                        TypeInfo? typeInfo = null;

                        if (caseType.Value is INamedTypeSymbol type)
                        {
                            typeInfo = new TypeInfo.NonArray(type);
                        }
                        else if (caseType.Value is IArrayTypeSymbol arrayType)
                        {
                            typeInfo = new TypeInfo.Array(arrayType.ToDisplayString());
                        }
                        else
                        {
                            var genericTypeName = caseType.Value!.ToString();

                            foreach (var genericType in symbol.TypeArguments)
                            {
                                if (genericType.Name == genericTypeName)
                                {
                                    typeInfo = new TypeInfo.SimpleGenericTypeArgument((ITypeParameterSymbol)genericType);
                                }
                            }

                            typeInfo = new TypeInfo.GeneralGenericTypeArgument(genericTypeName);
                        }

                        var storageMode = (int)caseStorageMode.Value!;

                        var storeAsObject = GetStoreAsObject(storageStrategy, storageMode, typeInfo.IsAlwaysValueType);

                        return new CaseData(i, caseName.Value!.ToString(), typeInfo, storeAsObject);

                    default:
                        throw new System.InvalidOperationException();
                };
            })
            .ToArray();

        UniqueCases =
            Cases
            .Where(caseData => caseData.TypeInfo is not null)
            .GroupBy(caseData => caseData.TypeInfo!.Name)
            .Where(group => group.Count() == 1)
            .SelectMany(group => group)
            .ToArray();

        var enableJsonSerializationData =
            symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, enableJsonSerializationAttrSymbol))
            .SingleOrDefault();

        EnableStandardJsonSerialization = enableJsonSerializationData != null;

        if (enableJsonSerializationData != null)
        {
            EnableStandardJsonSerialization = true;

            UsingSourceGeneration = enableJsonSerializationData.ConstructorArguments[0].Value as bool? ?? false;
        }

        UsingSourceGeneration =
            EnableStandardJsonSerialization ?
            (bool)enableJsonSerializationData!.ConstructorArguments[0].Value! :
            false;

        var containingTypes = new List<INamedTypeSymbol>();

        symbol = symbol.ContainingType;

        while (symbol != null)
        {
            containingTypes.Add(symbol);

            symbol = symbol.ContainingType;
        }

        ContainingTypes = [.. containingTypes];

        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                continue;
            }
            if (caseData.StoreAsObject && caseData.TypeInfo.IsAlwaysValueType)
            {
                BoxesValueTypes = true;
            }

            if (TypeToFieldNameMap.ContainsKey(caseData.FieldType!))
            {
                continue;
            }

            TypeToFieldNameMap[caseData.FieldType!] = $"_value{TypeToFieldNameMap.Count}";
        }
    }

    private bool GetStoreAsObject(int storageStrategy, int storageMode, bool isAlwaysValueType)
    {
        if (storageMode == 1) // StorageMode.AsObject
        {
            return true;
        }
        if (storageMode == 2) // StorageMode.AsDeclaredType
        {
            return false;
        }

        if (storageStrategy == 1) // StorageStrategy.OneObject
        {
            return true;
        }

        if (storageStrategy == 2) // StorageStrategy.OneFieldPerType
        {
            return false;
        }

        if (storageStrategy == 3) // StorageStrategy.NoBoxing
        {
            return !isAlwaysValueType;
        }

        return true;
    }

    private bool GetIsStruct(INamedTypeSymbol symbol) => symbol.TypeKind == TypeKind.Struct;

    private string GetAccessibility(INamedTypeSymbol symbol)
    {
        return symbol.DeclaredAccessibility switch
        {
            Microsoft.CodeAnalysis.Accessibility.Public => "public",
            Microsoft.CodeAnalysis.Accessibility.Internal => "internal",
            Microsoft.CodeAnalysis.Accessibility.Protected => "protected",
            Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => "protected internal",
            Microsoft.CodeAnalysis.Accessibility.Private => "private"
        };
    }

    private string GetFullName(INamedTypeSymbol symbol)
    {
        return
            symbol.TypeArguments.Length > 0 ?
            $"{symbol.Name}<{string.Join(", ", symbol.TypeArguments)}>" :
            symbol.Name;
    }

    public string Emit()
    {
        Builder.AppendLine("// <auto-generated />");

        Builder.AppendLine("#pragma warning disable CS8509");

        Builder.AppendLine("using System.Threading.Tasks;");

        if (Namespace != null)
        {
            EmitBeginNamespace();
        }

        EmitContainingTypes();

        if (EnableStandardJsonSerialization && !UsingSourceGeneration)
        {
            EmitJsonConverterAttribute();
        }

        EmitFieldsAndConstructor();

        if (BoxesValueTypes)
        {
            EmitBoxType();
        }

        EmitEquals();

        EmitCaseConstructors();

        EmitSwitch();

        EmitSwitchAsync();

        EmitMatch();

        EmitIs();

        EmitAs();

        EmitIf();

        EmitIfAsync();

        EmitImplicitConversions();

        if (EnableStandardJsonSerialization)
        {
            EmitStandardJsonConverter();
        }

        EmitEndClassDeclaration();

        if (EnableStandardJsonSerialization && IsGenericType)
        {
            EmitStaticClass();

            EmitStandardJsonConverterFactory();

            EmitEndStaticClass();
        }

        EmitEndContainingTypes();

        EmitEndNamespace();

        return Builder.ToString();
    }

    private void EmitBeginNamespace()
    {
        Builder.AppendLine($@"namespace {Namespace}
{{");
    }

    private void EmitContainingTypes()
    {
        foreach (var symbol in ContainingTypes)
        {
            Builder.AppendLine($@"
{GetAccessibility(symbol)} partial {(GetIsStruct(symbol) ? "struct" : "class")} {GetFullName(symbol)}
{{");

        }
    }

    private void EmitJsonConverterAttribute()
    {
        Builder.Append($@"
[System.Text.Json.Serialization.JsonConverter(typeof({NameWithoutTypeArguments}.StandardJsonConverter))]");
    }

    private void EmitFieldsAndConstructor()
    {
        Builder.AppendLine($@"
{Accessibility} partial {(IsStruct ? "struct" : "class")} {Name} : IEquatable<{Name}>
{{
    public int Index {{ get; }}");

        foreach (var typeToField in TypeToFieldNameMap)
        {
            Builder.Append($@"
    private {typeToField.Key} {typeToField.Value} {{ get; init; }} = default;");
        }

        Builder.AppendLine($@" 

    private {NameWithoutTypeArguments}(int index)
    {{
        System.Diagnostics.Debug.Assert(index >= 0 && index < {Cases.Length});

        Index = index;
    }}");
    }

    public void EmitBoxType()
    {
        Builder.AppendLine(@"
    class Box<T> : IEquatable<Box<T>> where T : struct
    {
        public T Value;

        public bool Equals(Box<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals(System.Runtime.CompilerServices.Unsafe.As<Box<T>>(obj));
        }

        public override int GetHashCode() => Value.GetHashCode();
    }");
    }
    public void EmitEquals()
    { 
        Builder.Append($@"
    public bool Equals({Name} other)
    {{
        {(IsStruct ? "" : "if (ReferenceEquals(null, other)) return false;")}
        if (Index != other.Index) return false;

        return Index switch
        {{");

        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                Builder.Append($@"
            {caseData.Index} => true,");
            }
            else
            {
                var fieldName = TypeToFieldNameMap[caseData.FieldType!];

                Builder.Append($@"
            {caseData.Index} => Equals({fieldName}, other.{fieldName}),");
            }
        }

        Builder.Append($@"
        }};
    }}

    public override bool Equals(object obj)
    {{
        if (ReferenceEquals(null, obj)) return false;
        {(IsStruct ? "" : "if (ReferenceEquals(this, obj)) return true;")}
        if (obj.GetType() != GetType()) return false;

        return Equals(({Name})obj);
    }}

    public override int GetHashCode()
    {{
        return Index switch
        {{");

        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                Builder.Append($@"
            {caseData.Index} => Index.GetHashCode(),");
            }
            else
            {
                Builder.Append($@"
            {caseData.Index} => System.HashCode.Combine({caseData.Index}, {TypeToFieldNameMap[caseData.FieldType!]}),");
            }
        }

        Builder.AppendLine($@"
        }};
    }}

    public static bool operator==({Name} left, {Name} right) => left.Equals(right);

    public static bool operator!=({Name} left, {Name} right) => !left.Equals(right);");
    }
    private void EmitCaseConstructors()
    {
        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                Builder.AppendLine($@"
    public static readonly {Name} {caseData.Name} = new({caseData.Index});");
            }
            else
            {
                var valueToStore =
                    caseData.StoreAsObject && caseData.TypeInfo.IsAlwaysValueType ?
                    $"new Box<{caseData.TypeInfo.Name}> {{ Value = value }}" :
                    "value";

                Builder.AppendLine($@"
    public static {Name} {caseData.Name}({caseData.TypeInfo.Name} value) => new({caseData.Index}) {{ {TypeToFieldNameMap[caseData.FieldType!]} = {valueToStore} }};");
            }
        }
    }
    private void EmitSwitch()
    {
        Builder.Append($@"
    public void Switch(");

        Builder.Append(string.Join(", ", Cases.Select(caseData =>
        {
            if (caseData.TypeInfo == null)
            {
                return $"Action f{caseData.Index}";
            }
            else
            {
                return $"Action<{caseData.TypeInfo.Name}> f{caseData.Index}";
            }
        })));

        Builder.Append(")");

        Builder.Append(@"
    {
        switch (Index)
        {");

        foreach (var caseData in Cases)
        {
            var arg = caseData.TypeInfo == null ? "" : $"As{caseData.Name}Unsafe";

            Builder.Append($@"
            case {caseData.Index}: f{caseData.Index}({arg}); break;");
        }

        Builder.Append(@"
        }
    }");
    }

    private void EmitSwitchAsync()
    {
        Builder.Append($@"
    public Task Switch(");

        Builder.Append(string.Join(", ", Cases.Select(caseData =>
        {
            if (caseData.TypeInfo == null)
            {
                return $"Func<Task> f{caseData.Index}";
            }
            else
            {
                return $"Func<{caseData.TypeInfo.Name}, Task> f{caseData.Index}";
            }
        })));

        Builder.Append(")");

        Builder.Append(@"
    {
        return Index switch
        {");

        foreach (var caseData in Cases)
        {
            var arg = caseData.TypeInfo == null ? "" : $"As{caseData.Name}Unsafe";

            Builder.Append($@"
            {caseData.Index} => f{caseData.Index}({arg}),");
        }

        Builder.Append(@"
        };
    }");
    }

    private void EmitMatch()
    {
        Builder.Append($@"
    public TRet_ Match<TRet_>(");

        Builder.Append(string.Join(", ", Cases.Select(caseData =>
        {
            if (caseData.TypeInfo == null)
            {
                return $"Func<TRet_> f{caseData.Index}";
            }
            else
            {
                return $"Func<{caseData.TypeInfo.Name}, TRet_> f{caseData.Index}";
            }
        })));

        Builder.Append(")");

        Builder.Append(@"
    {
        return Index switch
        {");

        foreach (var caseData in Cases)
        {
            var arg = caseData.TypeInfo == null ? "" : $"As{caseData.Name}Unsafe";

            Builder.Append($@"
            {caseData.Index} => f{caseData.Index}({arg}),");
        }

        Builder.Append(@"
        };
    }");
    }

    public void EmitIs()
    {
        foreach (var caseData in Cases)
        {
            Builder.AppendLine($@"
    public bool Is{caseData.Name} => Index == {caseData.Index};");
        }
    }

    public void EmitAs()
    {
        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                continue;
            }

            Builder.AppendLine($@"
    public {caseData.TypeInfo.Name} As{caseData.Name} => Index == {caseData.Index} ? As{caseData.Name}Unsafe : throw new InvalidOperationException($""Attempted to access case index {caseData.Index} but index is {{Index}}"");");

            var fieldName = TypeToFieldNameMap[caseData.FieldType!];

            string? valueExpression = null;

            if (caseData.StoreAsObject)
            {
                if (caseData.TypeInfo.IsAlwaysRefType)
                {
                    valueExpression = $"System.Runtime.CompilerServices.Unsafe.As<{caseData.TypeInfo.Name}>({fieldName})";
                }
                else if (caseData.TypeInfo.IsAlwaysValueType)
                {
                    valueExpression = $"System.Runtime.CompilerServices.Unsafe.As<Box<{caseData.TypeInfo.Name}>>({fieldName}).Value";
                }
                else
                {
                    valueExpression = $"({caseData.TypeInfo.Name}){fieldName}";
                }
            }
            else
            {
                valueExpression = fieldName;
            }

            Builder.AppendLine($@"
    private {caseData.TypeInfo.Name} As{caseData.Name}Unsafe
    {{
        get
        {{
            System.Diagnostics.Debug.Assert(Index == {caseData.Index});

            return {valueExpression};
        }}
    }}");
        }
    }

    private void EmitIf()
    {
        foreach (var caseData in Cases)
        {
            var argType = caseData.TypeInfo == null ? "Action" : $"Action<{caseData.TypeInfo.Name}>";

            var arg = caseData.TypeInfo == null ? "" : $"As{caseData.Name}Unsafe";

            Builder.Append($@"
    public void If{caseData.Name}({argType} f)");
            Builder.AppendLine($@"
    {{
        if (Index == {caseData.Index})
        {{
            f({arg});
        }}
    }}");
        }
    }

    private void EmitIfAsync()
    {
        foreach (var caseData in Cases)
        {
            var argType = caseData.TypeInfo == null ? "Func<Task>" : $"Func<{caseData.TypeInfo.Name}, Task>";

            var arg = caseData.TypeInfo == null ? "" : $"As{caseData.Name}Unsafe";

            Builder.AppendLine($@"
    public ValueTask If{caseData.Name}({argType} f) => Index == {caseData.Index} ? new ValueTask(f({arg})) : ValueTask.CompletedTask;");
        }
    }

    private void EmitImplicitConversions()
    {
        foreach (var caseData in UniqueCases)
        {
            Builder.AppendLine($@"
    public static implicit operator {Name}({caseData.TypeInfo!.Name} value) => {caseData.Name}(value);");
        }
    }

    private void EmitStandardJsonConverter()
    {
        Builder.Append($@"
    public partial class StandardJsonConverter : System.Text.Json.Serialization.JsonConverter<{Name}>
    {{
        public override {Name} Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {{
            if (reader.TokenType == System.Text.Json.JsonTokenType.Null)
            {{
                return default;
            }}

            if (reader.TokenType != System.Text.Json.JsonTokenType.StartObject)
            {{
                throw new System.Text.Json.JsonException($""Expected StartObject token but found: {{reader.TokenType}}"");
            }}

            reader.Read();

            if (reader.TokenType != System.Text.Json.JsonTokenType.PropertyName)
            {{
                throw new System.Text.Json.JsonException($""Expected PropertyName token but found: {{reader.TokenType}}"");
            }}

            var index = int.Parse(reader.GetString());

            reader.Read();

            var ret = index switch
            {{");

        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                Builder.Append($@"
                {caseData.Index} => {Name}.{caseData.Name},");
            }
            else
            {
                Builder.Append($@"
                {caseData.Index} => {Name}.{caseData.Name}(System.Text.Json.JsonSerializer.Deserialize<{caseData.TypeInfo.Name}>(ref reader, options)),");
            }
        }

        Builder.Append($@"
            }};

            reader.Read();

            if (reader.TokenType != System.Text.Json.JsonTokenType.EndObject)
            {{
                throw new System.Text.Json.JsonException($""Expected EndObject token but found: {{reader.TokenType}}"");
            }}

            return ret;
        }}

        public override void Write(System.Text.Json.Utf8JsonWriter writer, {Name} value, System.Text.Json.JsonSerializerOptions options)
        {{
            writer.WriteStartObject();

            switch (value.Index)
            {{");

        foreach (var caseData in Cases)
        {
            Builder.Append($@"
                case {caseData.Index}:");

            if (caseData.TypeInfo == null)
            {
                Builder.AppendLine($@"
                    writer.WriteNull(""{caseData.Index}"");");
            }
            else
            {
                Builder.Append($@"
                    writer.WritePropertyName(""{caseData.Index}"");");

                Builder.AppendLine($@"
                    System.Text.Json.JsonSerializer.Serialize(writer, value.AsCase{caseData.Index}Unsafe, options);");
            }

            Builder.Append($@"
                    break;");
        }

        Builder.Append(@"
            }

            writer.WriteEndObject();
        }
    }");
    }
    private void EmitEndClassDeclaration()
    {
        Builder.AppendLine("}");
    }

    private void EmitStaticClass()
    {
        Builder.Append($@"
{Accessibility} static partial class {NameWithoutTypeArguments}
{{");
    }

    private void EmitStandardJsonConverterFactory()
    {
        var genericTypeDefinition = $"{NameWithoutTypeArguments}<{new string(',', TypeArguments.Length - 1)}>";

        Builder.Append($@"
    public partial class StandardJsonConverter : System.Text.Json.Serialization.JsonConverterFactory
    {{
        public override bool CanConvert(Type typeToConvert)
        {{
            return typeToConvert.IsGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == typeof({genericTypeDefinition});
        }}

        public override System.Text.Json.Serialization.JsonConverter CreateConverter(Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {{
            var converterType = typeof({genericTypeDefinition}.StandardJsonConverter).MakeGenericType(typeToConvert.GetGenericArguments());

            return (System.Text.Json.Serialization.JsonConverter)System.Activator.CreateInstance(converterType);
        }}
    }}");
    }

    private void EmitEndStaticClass()
    {
        Builder.Append(@"
}");
    }

    private void EmitEndContainingTypes()
    {
        foreach (var symbol in ContainingTypes)
        {
            Builder.AppendLine("}");
        }
    }

    private void EmitEndNamespace()
    {
        if (!string.IsNullOrWhiteSpace(Namespace))
        {
            Builder.AppendLine("}");
        }
    }
}
