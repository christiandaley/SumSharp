namespace Tests;

using SumSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class Dispose
{
    class Disposable(Action onDispose) : IDisposable
    {
        public void Dispose() => onDispose();
    }


    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(Disposable))]
    partial class StringOrDisposable
    {

    }

    


    [Fact]
    public void NonGenericDispose()
    {
        bool disposed = false;

        {
            using StringOrDisposable value = "string";
        }

        Assert.False(disposed);

        {
            using StringOrDisposable value = new Disposable(() => disposed = true);

            Assert.False(disposed);
        }

        Assert.True(disposed);
    }

    
}