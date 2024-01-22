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
public class ImageMetaDataTests {
	public static readonly string[] AuthorizeExtensions = {".jpg", ".pdf", ".mts", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".webm", ".mp4", ".avi", ".tif", ".tiff"};

	public ImageMetaDataTests() {
		ImageMetadata.ExifTool = Settings.Exiftool;
	}

	public TestContext TestContext { get; set; } = default!;

	public static IEnumerable<object[]> TestFiles() {
		foreach (var item in Settings.ImageFolder.EnumerateFiles("*.*", SearchOption.AllDirectories)) {
			var extension = Path.GetExtension(item.FullName).ToLowerInvariant();
			if (!AuthorizeExtensions.Contains(extension)) continue;
			yield return new object[] {item.FullName};
		}
	}

	private TestContextLogger GetLogger(LogLevel logLevel) {
		return new TestContextLogger(TestContext, typeof(Wrapper).FullName!, logLevel);
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetInfo(string fullFileName) {
		var fmd = new ImageMetadata(fullFileName, GetLogger(LogLevel.Information));

		TestContext.WriteLine(fmd.ExifToolOutput);
	}

	[TestMethod]
	public void ListFile() {
		TestContext.WriteLine("Source directory:" + Settings.ImageFolder.FullName);

		foreach (var fullFileName in TestFiles()) TestContext.WriteLine(" - " + fullFileName[0]);
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetTags(string fullFileName) {
		var fmd  = new ImageMetadata(fullFileName, GetLogger(LogLevel.Information));
		var tags = fmd.AllTags;
		foreach (var item in tags) {
			var key = $"[{item.Container}] {item.Name}";
			TestContext.WriteLine($"{key,-40} : {item.Value}");
		}
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetAllDecodedinformations(string fullFileName) {
		var fmd = new ImageMetadata(fullFileName, GetLogger(LogLevel.Information));
		TestContext.WriteLine($"Title     : {fmd.Title}");
		TestContext.WriteLine($"Caption   : {fmd.Caption}");
		TestContext.WriteLine($"Copyright : {fmd.Copyright}");
		TestContext.WriteLine($"Comments  : {fmd.Description}");
		TestContext.WriteLine($"Note      : {fmd.Note}");
		TestContext.WriteLine($"Label     : {fmd.Label}");
		TestContext.WriteLine($"Keywords  : {string.Join(',', fmd.Keywords)}");
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void GetKeywords(string fullFileName) {
		var fmd  = new ImageMetadata(fullFileName, GetLogger(LogLevel.Information));
		var tags = fmd.Keywords;
		foreach (var item in tags.OrderBy(static i => i)) TestContext.WriteLine(item);
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void UpdateKeywords(string fullFileName) {
		var fmd = new ImageMetadata(fullFileName, GetLogger(LogLevel.Information));

		if (fullFileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)) Assert.Inconclusive("BMP files are not supported");

		var tags = fmd.Keywords.ToList();

		tags.Add($"exiftool-{DateTime.Now:HHmmss}");

		var expected = string.Join(", ", tags.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).Distinct().OrderBy(i => i));

		fmd.Update(fmd.Title,
		           fmd.Caption,
		           fmd.Copyright,
		           fmd.Description,
		           fmd.Note,
		           fmd.Label,
		           tags,
		           true,
		           true);


		var actual = string.Join(", ", fmd.Keywords.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).Distinct().OrderBy(i => i));
		TestContext.WriteLine($"Requested: {expected}");
		TestContext.WriteLine($"Actual   : {actual}");

		TestContext.WriteLine($"Title        : {fmd.Title}");
		TestContext.WriteLine($"Caption      : {fmd.Caption}");
		TestContext.WriteLine($"Copyright    : {fmd.Copyright}");
		TestContext.WriteLine($"Description  : {fmd.Description}");
		TestContext.WriteLine($"Note         : {fmd.Note}");
		TestContext.WriteLine($"Label        : {fmd.Label}");
		TestContext.WriteLine($"Keywords     : {string.Join(',', fmd.Keywords)}");
		TestContext.WriteLine(fmd.ExifToolOutput);

		Assert.AreEqual(expected, actual, $"Tags are not the same for {fullFileName}");
	}

	[DataTestMethod]
	[DynamicData(nameof(TestFiles), DynamicDataSourceType.Method)]
	public void ClearAllTags(string fullFileName) {
		if (fullFileName.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)) Assert.Inconclusive("BMP files are not supported");

		var fmd = new ImageMetadata(fullFileName, GetLogger(LogLevel.Information));
		fmd.Update(deleteOtherTags: true);
		TestContext.WriteLine(fmd.ExifToolOutput);
	}
}
