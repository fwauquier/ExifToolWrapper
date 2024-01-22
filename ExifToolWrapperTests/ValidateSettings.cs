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

[TestClass]
public class ValidateSettings {
	public TestContext TestContext { get; set; } = default!;

	[TestMethod]
	public void Validate() {
		TestContext.WriteLine($"ImageFolder: {Settings.ImageFolder}");
		TestContext.WriteLine($"Exiftool   : {Settings.Exiftool}");
		Assert.IsTrue(Settings.ImageFolder.Exists);
		Assert.IsTrue(Settings.Exiftool.Exists);
	}
	[TestMethod]
	public void TestContext_Dump() {
	TestContext.Dump();
	}

	[TestMethod]
	public void ValidateLogger() {
		var logger = TestContext.GetLogger();
		Assert.IsNotNull(logger);
		logger.LogTrace("LogTrace");
		logger.LogDebug("LogDebug");
		logger.LogInformation("LogInformation");
		logger.LogWarning("LogWarning");
		logger.LogError("LogError");
		logger.LogCritical("LogCritical");
	}

	[TestMethod]
	public void TestContextDirectory() {
		TestContext.WriteLine($"CurrentDirectory           {Environment.CurrentDirectory}");
		TestContext.WriteLine($"SystemDirectory            {Environment.SystemDirectory}");

		TestContext.WriteLine($"DeploymentDirectory        {TestContext.DeploymentDirectory}");
		TestContext.WriteLine($"TestDir                    {TestContext.TestDir}");
		TestContext.WriteLine($"TestLogsDir                {TestContext.TestLogsDir}");
		TestContext.WriteLine($"TestDeploymentDir          {TestContext.TestDeploymentDir}");
		TestContext.WriteLine($"ResultsDirectory           {TestContext.ResultsDirectory}");
		TestContext.WriteLine($"TestResultsDirectory       {TestContext.TestResultsDirectory}");
		TestContext.WriteLine($"TestRunDirectory           {TestContext.TestRunDirectory}");
		TestContext.WriteLine($"TestRunResultsDirectory    {TestContext.TestRunResultsDirectory}");

		foreach(var item in TestContext.Properties) {
			item.Dump();
		}

	}
}
