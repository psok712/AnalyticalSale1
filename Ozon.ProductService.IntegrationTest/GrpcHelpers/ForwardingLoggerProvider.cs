#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using Microsoft.Extensions.Logging;

namespace Ozon.ProductService.IntegrationTest.GrpcHelpers;

internal class ForwardingLoggerProvider(LogMessage logAction) : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new ForwardingLogger(categoryName, logAction);
    }

    public void Dispose()
    {
    }

    internal class ForwardingLogger(string categoryName, LogMessage logAction) : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return null!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            logAction(logLevel, categoryName, eventId, formatter(state, exception), exception);
        }
    }
}