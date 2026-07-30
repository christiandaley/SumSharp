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

    class AsyncDisposable(Action onDispose) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            onDispose();

            return ValueTask.CompletedTask;
        }
    }


    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(Disposable))]
    partial class StringOrDisposable
    {

    }

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", "T")]
    partial class GenericStringOrDisposable<T>
    {

    }

    [UnionCase("Case0", typeof(string))]
    [UnionCase("Case1", typeof(AsyncDisposable))]
    partial class StringOrAsyncDisposable
    {

    }

    [UnionCase("Case0", typeof(Disposable))]
    [UnionCase("Case1", typeof(AsyncDisposable))]
    partial class DisposableOrAsyncDisposable
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

    [Fact]
    public void GenericDispose()
    {
        bool disposed = false;

        {
            using GenericStringOrDisposable<Disposable> value = "string";
        }

        Assert.False(disposed);

        {
            using GenericStringOrDisposable<Disposable> value = new Disposable(() => disposed = true);

            Assert.False(disposed);
        }

        Assert.True(disposed);
    }

    [Fact]
    public async Task NonGenericAsyncDispose()
    {
        bool disposed = false;

        {
            await using StringOrAsyncDisposable value = "string";
        }

        Assert.False(disposed);

        {
            await using StringOrAsyncDisposable value = new AsyncDisposable(() => disposed = true);

            Assert.False(disposed);
        }

        Assert.True(disposed);
    }

    [Fact]
    public async Task GenericAsyncDispose()
    {
        bool disposed = false;

        {
            await using GenericStringOrDisposable<AsyncDisposable> value = "string";
        }

        Assert.False(disposed);

        {
            await using GenericStringOrDisposable<AsyncDisposable> value = new AsyncDisposable(() => disposed = true);

            Assert.False(disposed);
        }

        Assert.True(disposed);
    }

    [Fact]
    public async Task DisposableAndAsyncDisposable()
    {
        bool disposed = false;

        {
            using DisposableOrAsyncDisposable value = new Disposable(() => disposed = true);
        }

        Assert.True(disposed);

        disposed = false;

        {
            await using DisposableOrAsyncDisposable value = new Disposable(() => disposed = true);
        }

        Assert.True(disposed);

        disposed = false;

        {
            using DisposableOrAsyncDisposable value = new AsyncDisposable(() => disposed = true);
        }

        Assert.False(disposed);

        {
            await using DisposableOrAsyncDisposable value = new AsyncDisposable(() => disposed = true);
        }

        Assert.True(disposed);
    }
}