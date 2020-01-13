using Microsoft.Extensions.DependencyInjection;
using System;
using System.Transactions;

namespace Kiukie.Tests.Integration
{
    public sealed class IsolationScope : IDisposable
    {
        private readonly IServiceScope ServiceScope;
        private readonly TransactionScope TransactionScope;

        public IsolationScope(IServiceProvider serviceProvider)
        {
            ServiceScope = serviceProvider.CreateScope();
            TransactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        }

        public IServiceProvider Provider { get => ServiceScope.ServiceProvider; }

        public void Dispose()
        {
            TransactionScope.Dispose();
            ServiceScope.Dispose();
        }
    }
}
