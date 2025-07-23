using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using static Dotsum.Generator.SymbolHandler;

namespace Dotsum.Generator;

internal class SymbolHandler
{
    public abstract class TypeInfo
    {
        public abstract string Name { get; }

        public abstract int PrimitiveTypeSize { get; }

        public bool IsPrimitiveType => PrimitiveTypeSize > 0;

        public abstract bool IsAlwaysValueType { get; }

        public abstract bool IsAlwaysRefType { get; }

        public abstract bool IsInterface { get; }

        public class NonArray(INamedTypeSymbol symbol) : TypeInfo
        {
            public override string Name => symbol.ToDisplayString();

            public override int PrimitiveTypeSize
            {
                get
                {
                    if (!symbol.IsValueType)
                    {
                        return -1;
                    }

                    return symbol.SpecialType switch
                    {
                        SpecialType.System_Boolean => sizeof(bool),
                        SpecialType.System_Byte => sizeof(byte),
                        SpecialType.System_SByte => sizeof(sbyte),
                        SpecialType.System_Int16 => sizeof(Int16),
                        SpecialType.System_UInt16 => sizeof(UInt16),
                        SpecialType.System_Int32 => sizeof(Int32),
                        SpecialType.System_UInt32 => sizeof(UInt32),
                        SpecialType.System_Int64 => sizeof(Int64),
                        SpecialType.System_UInt64 => sizeof(UInt64),
                        //SpecialType.System_IntPtr => sizeof(IntPtr),
                        //SpecialType.System_UIntPtr => sizeof(UIntPtr),
                        SpecialType.System_Char => sizeof(char),
                        SpecialType.System_Single => sizeof(Single),
                        SpecialType.System_Double => sizeof(double),
                        _ => -1
                    };
                }
            }

            public override bool IsAlwaysValueType => symbol.IsValueType;

            public override bool IsAlwaysRefType => symbol.IsReferenceType;

            public override bool IsInterface => symbol.TypeKind == TypeKind.Interface;
        }

        public class Array(string name) : TypeInfo
        {
            public override string Name => name;

            public override int PrimitiveTypeSize => -1;

            public override bool IsAlwaysValueType => false;

            public override bool IsAlwaysRefType => true;

            public override bool IsInterface => false;
        }

        public class SimpleGenericTypeArgument(ITypeParameterSymbol symbol) : TypeInfo
        {
            public override string Name => symbol.Name;

            public override int PrimitiveTypeSize => -1;

            public override bool IsAlwaysValueType => symbol.HasValueTypeConstraint;

            public override bool IsAlwaysRefType => symbol.HasReferenceTypeConstraint;

            public override bool IsInterface => false;
        }

        public class GeneralGenericTypeArgument(string name) : TypeInfo
        {
            public override string Name => name;

            public override int PrimitiveTypeSize => -1;

            public override bool IsAlwaysValueType => false;

            public override bool IsAlwaysRefType => false;

            public override bool IsInterface => false;
        }
    }

    public record CaseData(int Index, string Name, TypeInfo? TypeInfo, bool StoreAsObject)
    {
        public bool UsePrimitiveStorage => !StoreAsObject && (TypeInfo != null && TypeInfo.IsPrimitiveType);

        public string? FieldName =>
            TypeInfo == null ? null :
            UsePrimitiveStorage ? PrimitiveStorageFieldName :
            StoreAsObject ? "_object" :
            $"_{TypeInfo.Name.Replace('.', '_')}";

        public string? FieldType =>
            TypeInfo == null ? null :
            UsePrimitiveStorage ? PrimitiveStorageTypeName :
            StoreAsObject ? "object" :
            TypeInfo.Name;

        public string? Access =>
            TypeInfo == null ? null :
            UsePrimitiveStorage ? $"{PrimitiveStorageFieldName}._{TypeInfo.Name}" :
            FieldName;
    }

    public const string PrimitiveStorageTypeName = "global::Dotsum.Internal.PrimitiveStorage";

    public const string PrimitiveStorageFieldName = "_primitiveStorage";

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

    public bool BoxesValueTypes { get; } = false;

    public bool EnableStandardJsonSerialization { get; }

    public bool EnableNewtonsoftJsonSerialization { get; }

    public bool AddJsonConverterAttribute { get; } = false;

    public bool DisableValueEquality { get; } = false;

    public bool IsRecord { get; }

    public bool EnableOneOfConversions { get; }

    public SymbolHandler(
        StringBuilder builder,
        INamedTypeSymbol symbol,
        INamedTypeSymbol caseAttrSymbol,
        INamedTypeSymbol enableJsonSerializationAttrSymbol,
        INamedTypeSymbol storageAttrSymbol,
        INamedTypeSymbol disableValueEqualitySymbol,
        INamedTypeSymbol enableOneOfConversionsSymbol)
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

        var containingTypes = new List<INamedTypeSymbol>();

        var tempSymbol = symbol.ContainingType;

        while (tempSymbol != null)
        {
            containingTypes.Insert(0, tempSymbol);

            tempSymbol = tempSymbol.ContainingType;
        }

        ContainingTypes = [.. containingTypes];

        ITypeSymbol[] allGenericTypeArguments =
            [.. symbol.TypeArguments, ..ContainingTypes.SelectMany(t => t.TypeArguments)];

        var storageData =
            symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, storageAttrSymbol))
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

                            foreach (var genericType in allGenericTypeArguments)
                            {
                                if (genericType.Name == genericTypeName)
                                {
                                    typeInfo = new TypeInfo.SimpleGenericTypeArgument((ITypeParameterSymbol)genericType);

                                    break;
                                }
                            }
                            if (typeInfo == null)
                            {
                                typeInfo = new TypeInfo.GeneralGenericTypeArgument(genericTypeName);
                            }
                        }

                        var storageMode = (int)caseStorageMode.Value!;

                        var storeAsObject = GetStoreAsObject(storageStrategy, storageMode, typeInfo.IsAlwaysValueType);

                        return new CaseData(i, caseName.Value!.ToString(), typeInfo, storeAsObject);

                    default:
                        throw new System.InvalidOperationException();
                };
            })
            .ToArray();

        var distinctTypes =
            Cases
            .Where(caseData => caseData.TypeInfo != null)
            .Select(caseData => caseData.TypeInfo!.Name)
            .Distinct()
            .ToArray();

        if (distinctTypes.Length == 1)
        {
            Cases = [.. Cases.Select(caseData => caseData with { StoreAsObject = false })];
        }

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

        if (enableJsonSerializationData != null)
        {
            var support = enableJsonSerializationData.ConstructorArguments[0].Value as int? ?? 0;

            EnableStandardJsonSerialization = (support & 1) != 0;

            EnableNewtonsoftJsonSerialization = (support & 2) != 0;

            AddJsonConverterAttribute = enableJsonSerializationData.ConstructorArguments[1].Value as bool? ?? false;

            if (allGenericTypeArguments.Length > symbol.TypeArguments.Length)
            {
                AddJsonConverterAttribute = false;
            }
        }

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
        }

        IsRecord = symbol.IsRecord;

        DisableValueEquality =
            IsRecord ||
            symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, disableValueEqualitySymbol))
            .Any();

        if (distinctTypes.Length == Cases.Length)
        {
            EnableOneOfConversions =
                symbol!
                .GetAttributes()
                .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, enableOneOfConversionsSymbol))
                .Any();
        }
    }

    private bool GetStoreAsObject(int storageStrategy, int storageMode, bool isAlwaysValueType)
    {
        if (storageMode == 1) // StorageMode.AsObject
        {
            return true;
        }
        if (storageMode == 2) // StorageMode.Inline
        {
            return false;
        }

        if (storageStrategy == 1) // StorageStrategy.OneObject
        {
            return true;
        }

        if (storageStrategy == 2) // StorageStrategy.InlineValueTypes
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

        if (AddJsonConverterAttribute)
        {
            if (EnableStandardJsonSerialization)
            {
                EmitStandardJsonConverterAttribute();
            }
            if (EnableNewtonsoftJsonSerialization)
            {
                EmitNewtonsoftJsonConverterAttribute();
            }
        }

        EmitFieldsAndConstructor();

        if (BoxesValueTypes)
        {
            EmitBoxType();
        }

        if (!DisableValueEquality)
        {
            EmitEquals();
        }

        EmitCaseConstructors();

        EmitAs();

        EmitIs();

        EmitSwitch();

        EmitSwitchAsync();

        EmitMatch();

        EmitIf();

        EmitIfAsync();

        EmitImplicitConversions();

        if (EnableOneOfConversions)
        {
            EmitOneOfConversions();
        }

        EmitToString();

        if (EnableStandardJsonSerialization)
        {
            EmitStandardJsonConverter();
        }
        if (EnableNewtonsoftJsonSerialization)
        {
            EmitNewtonsoftJsonConverter();
        }

        EmitEndClassDeclaration();

        if ((EnableStandardJsonSerialization || EnableNewtonsoftJsonSerialization) && IsGenericType)
        {
            EmitStaticClass();

            if (EnableStandardJsonSerialization)
            {
                EmitStandardJsonConverterFactory();
            }
            if (EnableNewtonsoftJsonSerialization)
            {
                EmitGenericNewtonsoftJsonConverter();
            }

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

    private void EmitStandardJsonConverterAttribute()
    {
        Builder.Append($@"
[System.Text.Json.Serialization.JsonConverter(typeof({NameWithoutTypeArguments}.StandardJsonConverter))]");
    }

    private void EmitNewtonsoftJsonConverterAttribute()
    {
        Builder.Append($@"
[Newtonsoft.Json.JsonConverter(typeof({NameWithoutTypeArguments}.NewtonsoftJsonConverter))]");
    }

    private void EmitFieldsAndConstructor()
    {
        Dictionary<string, string> fieldNameTypeMap = [];

        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                continue;
            }

            fieldNameTypeMap[caseData.FieldType!] = caseData.FieldName!;
        }

        var declarationKind = (IsStruct, IsRecord) switch
        {
            (true, true) => "record struct",
            (true, false) => "struct",
            (false, true) => "record",
            (false, false) => "class"
        };

        Builder.AppendLine($@"
{Accessibility} partial {declarationKind} {Name}{(DisableValueEquality ? "" : $" : IEquatable<{Name}>")}
{{
    public int Index {{ get; }}");

        foreach (var field in fieldNameTypeMap)
        {
            Builder.Append($@"
    private {field.Key} {field.Value} = default;");
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
    class Box<TBoxed_> : IEquatable<Box<TBoxed_>> where TBoxed_ : struct
    {
        public TBoxed_ Value;

        public bool Equals(Box<TBoxed_> other)
        {
            if (ReferenceEquals(null, other)) return false;
            
            return Equals(Value, other.Value);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals(System.Runtime.CompilerServices.Unsafe.As<Box<TBoxed_>>(obj));
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
                Builder.Append($@"
            {caseData.Index} => Equals(As{caseData.Name}Unsafe, other.As{caseData.Name}Unsafe),");
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

        return Equals({(IsStruct ? $"({Name})obj" : $"System.Runtime.CompilerServices.Unsafe.As<{Name}>(obj)")});
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
            {caseData.Index} => Index,");
            }
            else
            {
                Builder.Append($@"
            {caseData.Index} => System.HashCode.Combine({caseData.Index}, As{caseData.Name}Unsafe),");
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
            else if (caseData.StoreAsObject && caseData.TypeInfo.IsAlwaysValueType)
            {
                Builder.AppendLine($@"
    public static {Name} {caseData.Name}({caseData.TypeInfo.Name} value)
    {{
        var ret = new {Name}({caseData.Index});
        
        ret.{caseData.Access} = new Box<{caseData.TypeInfo.Name}>() {{ Value = value }};

        return ret;
    }}");
            }
            else
            {
                Builder.AppendLine($@"
    public static {Name} {caseData.Name}({caseData.TypeInfo.Name} value)
    {{
        var ret = new {Name}({caseData.Index});
        
        ret.{caseData.Access} = value;

        return ret;
    }}");
            }
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

            Builder.AppendLine($@"
    private {caseData.TypeInfo.Name} As{caseData.Name}Unsafe
    {{
        get
        {{
            System.Diagnostics.Debug.Assert(Index == {caseData.Index});");

            if (caseData.StoreAsObject)
            {
                if (caseData.TypeInfo.IsAlwaysValueType)
                {
                    Builder.AppendLine($@"
            return System.Runtime.CompilerServices.Unsafe.As<Box<{caseData.TypeInfo.Name}>>({caseData.Access}).Value;");
                }
                else if (caseData.TypeInfo.IsAlwaysRefType)
                {
                    Builder.AppendLine($@"
            return System.Runtime.CompilerServices.Unsafe.As<{caseData.TypeInfo.Name}>({caseData.Access});");
                }
                else
                {
                    Builder.AppendLine($@"
            return ({caseData.TypeInfo.Name}){caseData.Access};");
                }
            }
            else
            {
                Builder.AppendLine($@"
            return {caseData.Access};");

            }
            Builder.AppendLine(@"
        }
    }");
        }
    }
    public void EmitIs()
    {
        foreach (var caseData in Cases)
        {
            Builder.AppendLine($@"
    public bool Is{caseData.Name} => Index == {caseData.Index};");
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
            if (caseData.TypeInfo!.IsInterface)
            {
                continue;
            }

            Builder.AppendLine($@"
    public static implicit operator {Name}({caseData.TypeInfo!.Name} value) => {caseData.Name}(value);");
        }
    }

    private void EmitOneOfConversions()
    {
        var oneOfName = $"global::OneOf.OneOf<{string.Join(", ", Cases.Select(caseData => caseData.TypeInfo!.Name))}>";

        Builder.AppendLine($@"
    public static implicit operator {Name}({oneOfName} value)
    {{
        return value.Match({string.Join(", ", Cases.Select(caseData => caseData.Name))});
    }}");

        var conversionFuncs = string.Join(", ", Enumerable.Repeat($"static _ => ({oneOfName})_", Cases.Length));

        Builder.Append($@"
    public static implicit operator {oneOfName}({Name} value)
    {{
        return value.Match({conversionFuncs});
    }}");
    }

    private void EmitToString()
    {
        Builder.Append($@"
    public override string ToString()
    {{
        var valueString = Index switch
        {{");

        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                Builder.Append($@"
            {caseData.Index} => ""(empty)"",");
            }
            else
            {
                Builder.Append($@"
            {caseData.Index} => As{caseData.Name}Unsafe.ToString(),");
            }
        }

        Builder.AppendLine(@"
        };

        return $""{{ Index = {Index}, Value = {valueString} }}"";
    }
");
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

    private void EmitNewtonsoftJsonConverter()
    {
        Builder.Append($@"
    public partial class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter<{Name}>
    {{
        public override {Name} ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, {Name} existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {{
            if (reader.TokenType == Newtonsoft.Json.JsonToken.Null)
            {{
                return default;
            }}

            if (reader.TokenType != Newtonsoft.Json.JsonToken.StartObject)
            {{
                throw new Newtonsoft.Json.JsonException($""Expected StartObject token but found: {{reader.TokenType}}"");
            }}

            reader.Read();

            if (reader.TokenType != Newtonsoft.Json.JsonToken.PropertyName)
            {{
                throw new Newtonsoft.Json.JsonException($""Expected PropertyName token but found: {{reader.TokenType}}"");
            }}

            var index = int.Parse(reader.Value.ToString());

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
                {caseData.Index} => {Name}.{caseData.Name}(serializer.Deserialize<{caseData.TypeInfo.Name}>(reader)),");
            }
        }

        Builder.Append($@"
            }};

            reader.Read();

            if (reader.TokenType != Newtonsoft.Json.JsonToken.EndObject)
            {{
                throw new Newtonsoft.Json.JsonException($""Expected EndObject token but found: {{reader.TokenType}}"");
            }}

            return ret;
        }}
    
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, {Name} value, Newtonsoft.Json.JsonSerializer serializer)
        {{
            writer.WriteStartObject();
            
            switch (value.Index)
            {{");

        foreach (var caseData in Cases)
        {
            Builder.Append($@"
                case {caseData.Index}:");

            Builder.Append($@"
                    writer.WritePropertyName(""{caseData.Index}"");");

            if (caseData.TypeInfo == null)
            {
                Builder.AppendLine($@"
                    writer.WriteNull();");
            }
            else
            {
                Builder.AppendLine($@"
                    serializer.Serialize(writer, value.AsCase{caseData.Index}Unsafe);");
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

    private void EmitGenericNewtonsoftJsonConverter()
    {
        var genericTypeDefinition = $"{NameWithoutTypeArguments}<{new string(',', TypeArguments.Length - 1)}>";

        Builder.AppendLine($@"
public class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
{{
    static readonly System.Collections.Concurrent.ConcurrentDictionary<System.Type, Newtonsoft.Json.JsonConverter> _converters = new();

    private static Newtonsoft.Json.JsonConverter GetConverter(Type objectType)
    {{
        var converterType = typeof({genericTypeDefinition}.NewtonsoftJsonConverter).MakeGenericType(objectType.GetGenericArguments());

        return _converters.GetOrAdd(converterType, static converterType => (Newtonsoft.Json.JsonConverter)System.Activator.CreateInstance(converterType));
    }}

    public override bool CanConvert(Type objectType)
    {{
        return objectType.IsGenericType &&
               objectType.GetGenericTypeDefinition() == typeof({genericTypeDefinition});
    }}

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
    {{
        GetConverter(value.GetType()).WriteJson(writer, value, serializer);
    }}

    public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {{
        return GetConverter(objectType).ReadJson(reader, objectType, existingValue, serializer);
    }}
}}
");
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
