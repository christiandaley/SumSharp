using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SumSharp.Generator;

internal class SymbolHandler
{
    public abstract class TypeInfo
    {
        public abstract string Name { get; }

        public abstract bool IsUnmanaged { get; }

        public abstract bool IsAlwaysValueType { get; }

        public abstract bool IsAlwaysRefType { get; }

        public abstract bool IsInterface { get; }

        public virtual bool IsAccessibleFromCompilationAssembly => true;

        public class NonArray(INamedTypeSymbol symbol, Compilation compilation) : TypeInfo
        {
            private static bool IsUnmanagedType(ITypeSymbol symbol, IAssemblySymbol compilationAssembly)
            {
                if (symbol.SpecialType is 
                        SpecialType.System_SByte or
                        SpecialType.System_Byte or
                        SpecialType.System_Int16 or
                        SpecialType.System_UInt16 or
                        SpecialType.System_Int32 or
                        SpecialType.System_UInt32 or
                        SpecialType.System_Int64 or
                        SpecialType.System_UInt64 or
                        SpecialType.System_IntPtr or
                        SpecialType.System_UIntPtr or
                        SpecialType.System_Char or
                        SpecialType.System_Single or
                        SpecialType.System_Double or
                        SpecialType.System_Boolean)
                {
                        return true;
                }

                if (symbol is IPointerTypeSymbol)
                {
                    return true;
                }

                if (symbol.TypeKind == TypeKind.Enum)
                {
                    return true;
                }

                if (!SymbolEqualityComparer.Default.Equals(symbol.ContainingAssembly, compilationAssembly))
                {
                    return false;
                }

                if (!symbol.IsValueType)
                {
                    return false;
                }

                foreach (var member in symbol.GetMembers())
                {
                    if (member is not IFieldSymbol field || field.IsStatic)
                    {
                        continue;
                    }

                    if (!IsUnmanagedType(field.Type, compilationAssembly))
                    {
                        return false;
                    }
                }

                return true;
            }

            public override string Name => symbol.ToDisplayString();

            public override bool IsUnmanaged { get; } = IsUnmanagedType(symbol, compilation.Assembly);

            public override bool IsAccessibleFromCompilationAssembly => compilation.IsSymbolAccessibleWithin(symbol, compilation.Assembly);

            public override bool IsAlwaysValueType => symbol.IsValueType;

            public override bool IsAlwaysRefType => symbol.IsReferenceType;

            public override bool IsInterface => symbol.TypeKind == TypeKind.Interface;
        }

        public class Array(string name) : TypeInfo
        {
            public override string Name => name;

            public override bool IsUnmanaged => false;

            public override bool IsAlwaysValueType => false;

            public override bool IsAlwaysRefType => true;

            public override bool IsInterface => false;
        }

        public class SimpleGenericTypeArgument(ITypeParameterSymbol symbol) : TypeInfo
        {
            public override string Name => symbol.Name;

            public override bool IsUnmanaged => false;

            public override bool IsAlwaysValueType => symbol.HasValueTypeConstraint;

            public override bool IsAlwaysRefType => symbol.HasReferenceTypeConstraint;

            public override bool IsInterface => false;
        }

        public class GeneralGenericTypeArgument(string name, int genericTypeInfo, bool isInterface) : TypeInfo
        {
            public override string Name => name;

            public override bool IsUnmanaged => false;

            public override bool IsAlwaysValueType => (genericTypeInfo & 1) == 0 && !isInterface;

            public override bool IsAlwaysRefType => (genericTypeInfo & 2) == 0 || isInterface;

            public override bool IsInterface => isInterface;
        }
    }

    public class CaseData
    {
        public CaseData(string unmanagedStorageTypeName, int index, string name, TypeInfo? typeInfo, bool storeAsObject)
        {
            Index = index;
            Name = name;
            TypeInfo = typeInfo;
            StoreAsObject = storeAsObject;

            if (TypeInfo == null)
            {
                return;
            }

            FieldName =
                UseUnmanagedStorage ? UnmanagedStorageFieldName :
                StoreAsObject ? "_object" :
                $"_{_fieldNameRegex.Replace(TypeInfo.Name, "_")}";

            FieldType =
                UseUnmanagedStorage ? $"global::SumSharp.Internal.Generated.{unmanagedStorageTypeName}" :
                StoreAsObject ? "object" :
                TypeInfo.Name;
        }

        public const string UnmanagedStorageFieldName = "_unmanagedStorage";

        public int Index { get; }

        public string Name { get; }

        public TypeInfo? TypeInfo { get; }

        public bool StoreAsObject { get; }

        public bool UseUnmanagedStorage => 
            !StoreAsObject && 
            TypeInfo != null && 
            TypeInfo.IsUnmanaged && 
            TypeInfo.IsAccessibleFromCompilationAssembly;

        public string? FieldName { get; }

        public string? FieldType { get; }
    }

    private static readonly Regex _fieldNameRegex = new("[.<>]");

    public StringBuilder Builder { get; }

    public string? Namespace { get; }

    public bool IsStruct { get; }

    public bool NullableDisabled { get; }

    public string Nullable => NullableDisabled ? "" : "?";

    public string NullableIfRef => NullableDisabled || IsStruct ? "" : "?";

    public string Accessibility { get; }

    public bool IsGenericType => TypeArguments.Length > 0;

    public string[] TypeArguments { get; }

    public string NameWithoutTypeArguments { get; }

    public string Name { get; }

    public CaseData[] Cases { get; }

    public CaseData[] UniqueCases { get; }

    public INamedTypeSymbol[] ContainingTypes;

    public bool HasGenericContainingTypes => ContainingTypes.Any(type => type.TypeArguments.Length > 0);

    public bool EnableStandardJsonSerialization { get; }

    public bool EnableNewtonsoftJsonSerialization { get; }

    public bool AddJsonConverterAttribute { get; } = false;

    public bool DisableValueEquality { get; } = false;

    public bool IsRecord { get; }

    public bool EnableOneOfConversions { get; }

    public string OneOfEmptyCase { get; } = "global::OneOf.Types.None";

    public string UnmanagedStorageTypeName { get; }

    public SymbolHandler(
        StringBuilder builder,
        Compilation compilation,
        INamedTypeSymbol symbol,
        INamedTypeSymbol caseAttrSymbol,
        INamedTypeSymbol enableJsonSerializationAttrSymbol,
        INamedTypeSymbol storageAttrSymbol,
        INamedTypeSymbol disableValueEqualitySymbol,
        INamedTypeSymbol enableOneOfConversionsSymbol,
        INamedTypeSymbol disableNullableSymbol)
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

        UnmanagedStorageTypeName = _fieldNameRegex.Replace(symbol.ToDisplayString(), "_");

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
                var caseName = attr.ConstructorArguments[0].Value!.ToString();

                if (attr.ConstructorArguments.Length == 1)
                {
                    return new CaseData(UnmanagedStorageTypeName, i, caseName, null, true);
                }

                var caseType = attr.ConstructorArguments[1].Value!;

                var caseStorageMode = (int)attr.ConstructorArguments[2].Value!;

                int genericTypeInfo =
                    attr.ConstructorArguments.Length > 3 ?
                    (int)attr.ConstructorArguments[3].Value! :
                    -1;

                bool isGenericInterface =
                    attr.ConstructorArguments.Length > 3 ?
                    (bool)attr.ConstructorArguments[4].Value! :
                    false;

                TypeInfo? typeInfo = null;

                if (caseType is INamedTypeSymbol type)
                {
                    typeInfo = new TypeInfo.NonArray(type, compilation);
                }
                else if (caseType is IArrayTypeSymbol arrayType)
                {
                    typeInfo = new TypeInfo.Array(arrayType.ToDisplayString());
                }
                else
                {
                    var genericTypeName = caseType.ToString();

                    foreach (var genericType in allGenericTypeArguments)
                    {
                        if (genericType.Name == genericTypeName)
                        {
                            typeInfo = new TypeInfo.SimpleGenericTypeArgument((ITypeParameterSymbol)genericType);

                            break;
                        }
                    }

                    typeInfo ??= new TypeInfo.GeneralGenericTypeArgument(genericTypeName, genericTypeInfo, isGenericInterface);
                }

                var storeAsObject = GetStoreAsObject(storageStrategy, caseStorageMode, typeInfo.IsAlwaysValueType);

                return new CaseData(UnmanagedStorageTypeName, i, caseName, typeInfo, storeAsObject);
            })
            .ToArray();

        var distinctTypes =
            Cases
            .Where(caseData => caseData.TypeInfo != null)
            .Select(caseData => caseData.TypeInfo!.Name)
            .Distinct();

        if (distinctTypes.Count() == 1)
        {
            Cases = [.. Cases.Select(caseData => new CaseData(UnmanagedStorageTypeName, caseData.Index, caseData.Name, caseData.TypeInfo, false))];
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

            AddJsonConverterAttribute =
                !HasGenericContainingTypes &&
                (enableJsonSerializationData.ConstructorArguments[1].Value as bool? ?? false);
        }

        IsRecord = symbol.IsRecord;

        DisableValueEquality =
            IsRecord ||
            symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, disableValueEqualitySymbol))
            .Any();


        var enableOneOfConversionsData =
            symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, enableOneOfConversionsSymbol))
            .SingleOrDefault();

        if (enableOneOfConversionsData != null)
        {
            EnableOneOfConversions = true;

            if (enableOneOfConversionsData.ConstructorArguments.Length == 1)
            {
                OneOfEmptyCase = ((INamedTypeSymbol?)enableOneOfConversionsData.ConstructorArguments[0].Value!).ToDisplayString();
            }
        }

        NullableDisabled =
            symbol!
            .GetAttributes()
            .Where(attr => SymbolEqualityComparer.Default.Equals(attr.AttributeClass, disableNullableSymbol))
            .Any();
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

    private string GetDeclarationKind(bool isStruct, bool isRecord)
    {
        return (isStruct, isRecord) switch
        {
            (true, true) => "record struct",
            (true, false) => "struct",
            (false, true) => "record",
            (false, false) => "class"
        };
    }

    private string GetDeclarationKind(INamedTypeSymbol symbol) => GetDeclarationKind(symbol.TypeKind == TypeKind.Struct, symbol.IsRecord);

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

        if (!NullableDisabled)
        {
            Builder.AppendLine("#nullable enable");
        }

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

        EmitUnmanagedStorage();

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
{GetAccessibility(symbol)} partial {GetDeclarationKind(symbol)} {GetFullName(symbol)}
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

        Builder.AppendLine($@"
{Accessibility} partial {GetDeclarationKind(IsStruct, IsRecord)} {Name}{(DisableValueEquality ? "" : $" : IEquatable<{Name}>")}
{{");

        foreach (var field in fieldNameTypeMap)
        {
            Builder.Append($@"
    private {field.Key} {field.Value} = default;");
        }

        Builder.AppendLine($@" 

    ///<summary>The zero-based index of the case held by the discriminated union</summary>
    public int Index {{ get; }}

    private {NameWithoutTypeArguments}(int index)
    {{
        System.Diagnostics.Debug.Assert(index >= 0 && index < {Cases.Length});

        Index = index;
    }}");

        if (EnableStandardJsonSerialization && !AddJsonConverterAttribute)
        {
            Builder.AppendLine($@"
    ///<summary>Default constructor that ensures System.Text.Json generated source code will compile. Will always throw.</summary>
    ///<exception cref=""InvalidOperationException"">Always thrown when the default constructor is invoked</exception>
    public {NameWithoutTypeArguments}() => throw new System.InvalidOperationException(""The default constructor for {Name} exists only to ensure that System.Text.Json generated source code will compile. It is an error to invoke the default constructor. You must use the generated JsonConverter to serialize/deserialize an instance of {Name}."");
");
        }
    }

    public void EmitEquals()
    { 
        Builder.Append($@"
    ///<summary>Compares two {Name} instances for equality. The two instances are equal iff they have the same Index and their underlying values compare equal using Object.Equals</summary>
    public bool Equals({Name}{NullableIfRef} other)
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

    ///<summary>Compares a {Name} instance and another object for equality. The {Name} instance is equal to the other object iff 
    /// the other object is a {Name} and they have the same Index and their underlying values compare equal using Object.Equals</summary>
    public override bool Equals(object{Nullable} obj)
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

    ///<summary>Compares two {Name} instances for equality using IEquatable<{Name}>.Equals</summary>
    public static bool operator==({Name} left, {Name} right) => left.Equals(right);

    ///<summary>Compares two {Name} instances for inequality using IEquatable<{Name}>.Equals</summary>
    public static bool operator!=({Name} left, {Name} right) => !left.Equals(right);");
    }
    private void EmitCaseConstructors()
    {
        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                Builder.AppendLine($@"
    ///<summary>The singleton {Name} that holds a {caseData.Name}</summary>
    public static readonly {Name} {caseData.Name} = new({caseData.Index});");

                continue;
            }

            Builder.AppendLine($@"
    ///<summary>A static factory function that returns a {Name} that holds a {caseData.Name}</summary>
    public static {Name} {caseData.Name}({caseData.TypeInfo.Name} value)
    {{
        var ret = new {Name}({caseData.Index});");

            if (caseData.StoreAsObject && caseData.TypeInfo.IsAlwaysValueType)
            {
                Builder.AppendLine($@"        
        ret.{caseData.FieldName} = new global::SumSharp.Internal.Box<{caseData.TypeInfo.Name}>(value);");
            }
            else if (caseData.UseUnmanagedStorage)
            {
                Builder.AppendLine($@"
        System.Runtime.CompilerServices.Unsafe.As<{caseData.FieldType}, {caseData.TypeInfo.Name}>(ref ret.{caseData.FieldName}) = value;");
            }
            else
            {
                Builder.AppendLine($@"
        ret.{caseData.FieldName} = value;");
            }

            Builder.AppendLine(@"
        return ret;
    }");
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
    private {caseData.TypeInfo.Name} As{caseData.Name}Unsafe
    {{
        get
        {{
            System.Diagnostics.Debug.Assert(Index == {caseData.Index});");

            if (caseData.StoreAsObject)
            {
                if (caseData.TypeInfo.IsAlwaysValueType)
                {
                    Builder.Append($@"
            return System.Runtime.CompilerServices.Unsafe.As<global::SumSharp.Internal.Box<{caseData.TypeInfo.Name}>>({caseData.FieldName}).Value;");
                }
                else if (caseData.TypeInfo.IsAlwaysRefType)
                {
                    Builder.Append($@"
            return System.Runtime.CompilerServices.Unsafe.As<{caseData.TypeInfo.Name}>({caseData.FieldName});");
                }
                else
                {
                    Builder.Append($@"
            return ({caseData.TypeInfo.Name}){caseData.FieldName};");
                }
            }
            else if (caseData.UseUnmanagedStorage)
            {
                Builder.Append($@"
            return System.Runtime.CompilerServices.Unsafe.As<{caseData.FieldType}, {caseData.TypeInfo.Name}>(ref {caseData.FieldName});");

            }
            else
            {
                Builder.Append($@"
            return {caseData.FieldName};");

            }

            Builder.AppendLine(@"
        }
    }");

            Builder.AppendLine($@"
    ///<summary>The {caseData.Name} value, if present. Throws InvalidOperationException if the {Name} does not hold a {caseData.Name}</summary>
    ///<exception cref=""InvalidOperationException"">Thrown if the {Name} does not hold a {caseData.Name}</exception>
    public {caseData.TypeInfo.Name} As{caseData.Name} => Index == {caseData.Index} ? As{caseData.Name}Unsafe : throw new InvalidOperationException($""Attempted to access case index {caseData.Index} but index is {{Index}}"");");

            Builder.AppendLine($@"
    ///<summary>The {caseData.Name} value, if present. Otherwise default({caseData.TypeInfo.Name})</summary>
    public {caseData.TypeInfo.Name}{(NullableDisabled || caseData.TypeInfo.IsAlwaysValueType ? "" : "?")} As{caseData.Name}OrDefault => Index == {caseData.Index} ? As{caseData.Name}Unsafe : default;");

            Builder.AppendLine($@"
    ///<summary>Returns the {caseData.Name} value, if present. Otherwise returns <paramref name=""defaultValue""/></summary>
    ///<param name=""defaultValue"">The default value to return if the {Name} does not hold a {caseData.Name}</param>
    public {caseData.TypeInfo.Name} As{caseData.Name}Or({caseData.TypeInfo.Name} defaultValue) => Index == {caseData.Index} ? As{caseData.Name}Unsafe : defaultValue;");

            Builder.AppendLine($@"
    ///<summary>Returns the {caseData.Name} value, if present. Otherwise returns the result of invoking <paramref name=""defaultValueFactory""/></summary>
    ///<param name=""defaultValueFactory"">Provides the default value to return if the {Name} does not hold a {caseData.Name}</param>
    public {caseData.TypeInfo.Name} As{caseData.Name}Or(Func<{caseData.TypeInfo.Name}> defaultValueFactory) => Index == {caseData.Index} ? As{caseData.Name}Unsafe : defaultValueFactory();");

            Builder.AppendLine($@"
    ///<summary>Returns a ValueTask wrapping the {caseData.Name} value, if present. Otherwise returns the result of invoking <paramref name=""defaultValueFactory""/></summary>
    ///<param name=""defaultValueFactory"">Provides the default value to return if the {Name} does not hold a {caseData.Name}</param>
    public ValueTask<{caseData.TypeInfo.Name}> As{caseData.Name}Or(Func<Task<{caseData.TypeInfo.Name}>> defaultValueFactory) => Index == {caseData.Index} ? ValueTask.FromResult(As{caseData.Name}Unsafe) : new ValueTask<{caseData.TypeInfo.Name}>(defaultValueFactory());");
        }
    }
    public void EmitIs()
    {
        foreach (var caseData in Cases)
        {
            Builder.AppendLine($@"
    ///<summary>True if the {Name} holds a {caseData.Name}, false otherwise</summary>
    public bool Is{caseData.Name} => Index == {caseData.Index};");
        }
    }

    private void EmitSwitch()
    {
        Builder.Append($@"
    ///<summary>Invokes the corresponding function with the underlying value held by the {Name}</summary>
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
    ///<summary>Invokes the corresponding function with the underlying value held by the {Name}</summary>
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
    ///<summary>Invokes the corresponding function with the underlying value held by the {Name} and returns the result</summary>
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
            if (caseData.TypeInfo == null)
            {
                continue;
            }

            var actionArgType = $"Action<{caseData.TypeInfo.Name}>";

            var funcArgType = $"Func<{caseData.TypeInfo.Name}, TRet__>";

            var funcWithCtxArgType = $"Func<TContext__, {caseData.TypeInfo.Name}, TRet__>";

            var handlerName = $"handle{caseData.Name}";

            var arg = $"As{caseData.Name}Unsafe";

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, invokes the <paramref name=""{handlerName}""/> function with the
    ///{caseData.TypeInfo.Name} value, otherwise does nothing.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.TypeInfo.Name} value, if it exists.</param>
    public void If{caseData.Name}({actionArgType} {handlerName})
    {{
        if (Index == {caseData.Index})
        {{
            {handlerName}({arg});
        }}
    }}");

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, invokes the <paramref name=""{handlerName}""/> function with the
    ///{caseData.TypeInfo.Name} value, otherwise invokes <paramref name=""orElse""/>.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.Name} value, if it exists.</param>
    ///<param name=""orElse"">Function to be invoked if the {Name} does not hold a {caseData.Name}</param>
    public void If{caseData.Name}Else({actionArgType} {handlerName}, Action orElse)
    {{
        if (Index == {caseData.Index})
        {{
            {handlerName}({arg});
        }}
        else
        {{
            orElse();
        }}
    }}");

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, returns the result of invoking the <paramref name=""{handlerName}""/>
    ///function with the {caseData.TypeInfo.Name} value, otherwise returns <paramref name=""elseValue""/>.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.Name} value, if it exists.</param>
    ///<param name=""elseValue"">Value to be returned if the {Name} does not hold a {caseData.Name}</param>
    public TRet__ If{caseData.Name}Else<TRet__>({funcArgType} {handlerName}, TRet__ elseValue) => Index == {caseData.Index} ? {handlerName}({arg}) : elseValue;");

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, returns the result of invoking the <paramref name=""{handlerName}""/>
    ///function with the {caseData.TypeInfo.Name} value, otherwise returns the result of invoking <paramref name=""elseFunc""/>.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.Name} value, if it exists.</param>
    ///<param name=""elseFunc"">Produces the value to be returned if the {Name} does not hold a {caseData.Name}</param>
    public TRet__ If{caseData.Name}Else<TRet__>({funcArgType} {handlerName}, Func<TRet__> elseFunc) => Index == {caseData.Index} ? {handlerName}({arg}) : elseFunc();");
        }
    }

    private void EmitIfAsync()
    {
        foreach (var caseData in Cases)
        {
            if (caseData.TypeInfo == null)
            {
                continue;
            }

            var actionArgType = $"Func<{caseData.TypeInfo.Name}, Task>";

            var funcArgType = $"Func<{caseData.TypeInfo.Name}, Task<TRet__>>";

            var handlerName = $"{caseData.Name}Handler";

            var arg = $"As{caseData.Name}Unsafe";

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, invokes the <paramref name=""{handlerName}""/> function with the
    ///{caseData.TypeInfo.Name} value, otherwise does nothing.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.TypeInfo.Name} value, if it exists.</param>
    public ValueTask If{caseData.Name}({actionArgType} f) => Index == {caseData.Index} ? new ValueTask(f({arg})) : ValueTask.CompletedTask;");

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, invokes the <paramref name=""{handlerName}""/> function with the
    ///{caseData.TypeInfo.Name} value, otherwise invokes <paramref name=""orElse"">orElse</paramref>.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.Name} value, if it exists.</param>
    ///<param name=""orElse"">Function to be invoked if the {Name} does not hold a {caseData.Name}</param>
    public Task If{caseData.Name}Else({actionArgType} {handlerName}, Func<Task> elseF) => Index == {caseData.Index} ? {handlerName}({arg}) : elseF();");

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, returns the result of invoking the <paramref name=""{handlerName}""/>
    ///function with the {caseData.TypeInfo.Name} value, otherwise returns <paramref name=""elseValue""/> wrapped in a ValueTask.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.Name} value, if it exists.</param>
    ///<param name=""elseValue"">Value to be returned if the {Name} does not hold a {caseData.Name}</param>
    public ValueTask<TRet__> If{caseData.Name}Else<TRet__>({funcArgType} {handlerName}, TRet__ elseValue) => Index == {caseData.Index} ? new ValueTask<TRet__>({handlerName}({arg})) : ValueTask.FromResult(elseValue);");

            Builder.AppendLine($@"
    ///<summary>If the {Name} holds a {caseData.Name}, returns the result of invoking the <paramref name=""{handlerName}""/>
    ///function with the {caseData.TypeInfo.Name} value, otherwise returns the result of invoking <paramref name=""elseFunc""/>.</summary>
    ///<param name=""{handlerName}"">Function to be invoked with the {caseData.Name} value, if it exists.</param>
    ///<param name=""elseFunc"">Produces the value to be returned if the {Name} does not hold a {caseData.Name}</param>
    public Task<TRet__> If{caseData.Name}Else<TRet__>({funcArgType} {handlerName}, Func<Task<TRet__>> elseFunc) => Index == {caseData.Index} ? {handlerName}({arg}) : elseFunc();");
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
    ///<summary>Converts a {caseData.TypeInfo!.Name} to a {Name} that holds a {caseData.Name}</summary>
    public static implicit operator {Name}({caseData.TypeInfo!.Name} value) => {caseData.Name}(value);");
        }
    }

    private void EmitOneOfConversions()
    {
        var oneOfName = $"global::OneOf.OneOf<{string.Join(", ", Cases.Select(caseData => caseData.TypeInfo?.Name ?? OneOfEmptyCase))}>";

        var oneOfNameShort = $"OneOf<{string.Join(", ", Cases.Select(caseData => caseData.TypeInfo?.Name ?? OneOfEmptyCase))}>";

        var conversionFuncs = Cases.Select(caseData => caseData.TypeInfo == null ? $"static _ => {caseData.Name}" : caseData.Name);

        Builder.AppendLine($@"
    ///<summary>Converts a {oneOfNameShort} to a {Name}</summary>
    public static implicit operator {Name}({oneOfName} value)
    {{
        return value.Match({string.Join(", ", conversionFuncs)});
    }}");

        conversionFuncs = Cases.Select(caseData => caseData.TypeInfo == null ? $"static () => {oneOfName}.FromT{caseData.Index}(new {OneOfEmptyCase}())" : $"static _ => {oneOfName}.FromT{caseData.Index}(_)");

        Builder.Append($@"
    ///<summary>Converts a {Name} to a {oneOfNameShort}</summary>
    public static implicit operator {oneOfName}({Name} value)
    {{
        return value.Match({string.Join(", ", conversionFuncs)});
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
    ///<summary>System.Text.Json converter capable of serializing and deserializing a {Name}</summary>
    public partial class StandardJsonConverter : System.Text.Json.Serialization.JsonConverter<{Name}>
    {{
        public override {Name}{NullableIfRef} Read(ref System.Text.Json.Utf8JsonReader reader, System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {{
            if (reader.TokenType == System.Text.Json.JsonTokenType.Null)
            {{
                {(IsStruct ? $"throw new System.Text.Json.JsonException(\"Expected {NameWithoutTypeArguments} object but found: null\")" : "return null;")};
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

        public override void Write(System.Text.Json.Utf8JsonWriter writer, {Name}{NullableIfRef} value, System.Text.Json.JsonSerializerOptions options)
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
    ///<summary>Newtonsoft converter capable of serializing and deserializing a {Name}</summary>
    public partial class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter<{Name}>
    {{
        public override {Name}{NullableIfRef} ReadJson(Newtonsoft.Json.JsonReader reader, System.Type objectType, {Name}{NullableIfRef} existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
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
    
        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, {Name}{NullableIfRef} value, Newtonsoft.Json.JsonSerializer serializer)
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
    ///<summary>System.Text.Json converter capable of serializing and deserializing any {NameWithoutTypeArguments}</summary>
    public partial class StandardJsonConverter : System.Text.Json.Serialization.JsonConverterFactory
    {{
        public override bool CanConvert(System.Type typeToConvert)
        {{
            return typeToConvert.IsGenericType &&
                   typeToConvert.GetGenericTypeDefinition() == typeof({genericTypeDefinition});
        }}

        public override System.Text.Json.Serialization.JsonConverter CreateConverter(System.Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
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
///<summary>Newtonsoft converter capable of serializing and deserializing any {NameWithoutTypeArguments}</summary>
public class NewtonsoftJsonConverter : Newtonsoft.Json.JsonConverter
{{
    static readonly System.Collections.Concurrent.ConcurrentDictionary<System.Type, Newtonsoft.Json.JsonConverter> _converters = new();

    private static Newtonsoft.Json.JsonConverter GetConverter(System.Type objectType)
    {{
        return _converters.GetOrAdd(objectType, static objectType => 
        {{
            var converterType = typeof({genericTypeDefinition}.NewtonsoftJsonConverter).MakeGenericType(objectType.GetGenericArguments());

            return (Newtonsoft.Json.JsonConverter)System.Activator.CreateInstance(converterType);
        }});
    }}

    public override bool CanConvert(System.Type objectType)
    {{
        return objectType.IsGenericType &&
               objectType.GetGenericTypeDefinition() == typeof({genericTypeDefinition});
    }}

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object{Nullable} value, Newtonsoft.Json.JsonSerializer serializer)
    {{
        GetConverter(value.GetType()).WriteJson(writer, value, serializer);
    }}

    public override object{Nullable} ReadJson(Newtonsoft.Json.JsonReader reader, System.Type objectType, object{Nullable} existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {{
        return GetConverter(objectType).ReadJson(reader, objectType, existingValue, serializer);
    }}
}}
");
    }

    private void EmitEndStaticClass()
    {
        Builder.AppendLine(@"
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

    private void EmitUnmanagedStorage()
    {
        var unmanagedTypes =
            Cases.Where(caseData => caseData.UseUnmanagedStorage)
            .Select(caseData => caseData.TypeInfo!.Name)
            .Distinct()
            .ToArray();

        if (unmanagedTypes.Length == 0)
        {
            return;
        }

        Builder.AppendLine($@"
namespace SumSharp.Internal.Generated
{{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    internal readonly struct {UnmanagedStorageTypeName}
    {{");

        for (int i = 0; i < unmanagedTypes.Length; ++i)
        {
            Builder.Append($@"
        [System.Runtime.InteropServices.FieldOffset(0)] private readonly {unmanagedTypes[i]} _field{i};");
        }

        Builder.Append(@"    
    }
}");
    }
}
