using System;
using System.Collections.Generic;
using System.Text;

namespace Tests;

using SumSharp;

public partial class RecordStructs
{
    [UnionCase("A", typeof(CustomType1))]
    [UnionCase("B", typeof(CustomType2))]
    [UnionCase("Int", typeof(int))]
    [UnionCase("Float", typeof(float))]
    readonly partial struct TestUnion;

    partial record struct CustomType1(string Message);

    partial record struct CustomType2(string Message);
}
