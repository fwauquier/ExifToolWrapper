// MIT License
// Copyright (c) 2023 Frédéric Wauquier
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or
// sell copies of the Software, and to permit persons to whom the Software
// is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Microsoft.Extensions.Logging;

namespace ExifrootWrapper;

/// <summary>
///     A logger that writes messages in the debug output window only when a debugger is attached.
/// </summary>
internal sealed class TestContextLogger : ILogger {
	private readonly string m_Name;
	private readonly TestContext m_TestContext;

	/// <summary>
	///     Initializes a new instance of the <see cref="TestContextLogger" /> class.
	/// </summary>
	/// <param name="testContext"></param>
	/// <param name="name">The name of the logger.</param>
	/// <param name="logLevel"></param>
	public TestContextLogger(TestContext testContext, string name, LogLevel logLevel = LogLevel.Information) {
		LogLevel = logLevel;
		m_TestContext = testContext;
		m_Name = name;
	}

	public LogLevel LogLevel { get; }

	/// <inheritdoc />
	public IDisposable BeginScope<TState>(TState state)
		where TState : notnull {
		return NullScope.Singleton;
	}

	/// <inheritdoc />
	public bool IsEnabled(LogLevel logLevel) {
		// Everything is enabled unless the debugger is not attached
		return logLevel >= LogLevel && logLevel != LogLevel.None;
	}

	/// <inheritdoc />
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
		if (!IsEnabled(logLevel)) return;

		if (formatter is null) throw new NullReferenceException(nameof(formatter));

		var message = formatter(state, exception);

		if (string.IsNullOrEmpty(message)) return;

		message = $"{logLevel}: {message}";

		if (exception != null) message += Environment.NewLine + Environment.NewLine + exception;

		//m_TestContext.WriteLine("[" + _name + "]" + message);
		m_TestContext.WriteLine(message);
	}

	internal sealed class NullScope : IDisposable {
		private NullScope() { }

		public static NullScope Singleton { get; } = new();

		/// <inheritdoc />
		public void Dispose() { }
	}
}
