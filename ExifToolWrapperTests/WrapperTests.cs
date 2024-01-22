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
public class WrapperTests {
	public TestContext TestContext { get; set; } = default!;

	private  Wrapper GetWrapper(LogLevel logLevel = LogLevel.Trace) {
		var Wrapper = new Wrapper(Settings.Exiftool);
		Wrapper.Logger = GetLogger(logLevel);
		return Wrapper;
	}

	private TestContextLogger GetLogger(LogLevel logLevel) {
		return new(TestContext, typeof(Wrapper).FullName!, logLevel);
	}

	public static IEnumerable<object[]> TestFiles() {
		foreach (var item in Settings.ImageFolder.EnumerateFiles("*.*", SearchOption.AllDirectories)) {
			var extension = Path.GetExtension(item.FullName).ToLowerInvariant();
			if (!Wrapper.AuthorizeExtensions.Contains(extension)) continue;
			yield return new object[] {item.FullName};
		}
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetInfo(string fullFileName) {
		var wrapper = GetWrapper(LogLevel.Information);
		var output  = wrapper.GetInfo(fullFileName);
		TestContext.WriteLine(output);
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetTags(string fullFileName) {
		var wrapper = GetWrapper(LogLevel.Information);
		var tags    = wrapper.GetTags(fullFileName);
		foreach (var item in tags) {
			var key = $"[{item.Container}] {item.Name}";
			TestContext.WriteLine($"{key,-40} : {item.Value}");
		}
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetFileType(string fullFileName) {
		var wrapper  = GetWrapper(LogLevel.Information);
		var tags     = wrapper.GetTags(fullFileName).ToList();
		var fileType = Wrapper.GetFileType(tags);
		TestContext.WriteLine($"File type: {fileType}");
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetKeywords(string fullFileName) {
		var wrapper = GetWrapper(LogLevel.Information);
		var tags    = wrapper.GetKeywords(fullFileName);
		foreach (var item in tags.OrderBy(static i => i)) TestContext.WriteLine(item);
	}
}
