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

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.Logging;
#pragma warning disable CA1848

namespace ExifrootWrapper;

/* https://www.w3.org/TR/png-3/
 Predefined keywords
 ===================
	Keyword value		Description
	------------		-----------
	Title				Short (one line) title or caption for image
	Author				Name of image's creator
	Description			Description of image (possibly long)
	Copyright			Copyright notice
	Creation Time		Time of original image creation
	Software			Software used to create the image
	Disclaimer			Legal disclaimer
	Warning				Warning of nature of content
	Source				Device used to create the image
	Comment				Miscellaneous comment
	XML:com.adobe.xmp	Extensible Metadata Platform (XMP) information, formatted as required by the XMP specification [XMP]. The use of iTXt, with Compression Flag set to 0, and both Language Tag and Translated Keyword set to the null string, are recommended for XMP compliance.
*/

public sealed class  ImageMetadata {
	private static readonly Regex rTag = new("\\[(?<Container>.*)\\]\\s+(?<Name>.*)\\s+:\\s+(?<Value>.*)",
	                                         RegexOptions.Multiline
	                                         | RegexOptions.Compiled
	                                         | RegexOptions.CultureInvariant);

	private Lazy<string> lzy_Outpout;
	private Lazy<List<ExifTag>> lzyTags;

	private string? m_Caption;
	private string? m_Comments;

	private string? m_Copyright;
	private string[]? m_Keywords;
	private string? m_Label;
	private int? m_Note;
	private string? m_Title;

	public ImageMetadata(string fullFileName, ILogger? logger = null) {
		FullFileName = fullFileName;
		Logger = logger;
		InitializeLazy();
	}

	public string ExifToolOutput => lzy_Outpout.Value;
	public IReadOnlyCollection<ExifTag> AllTags => lzyTags.Value;

	public string FullFileName { get; }

	/// <summary>
	///     <b>READ :</b><br />
	///     IPTC:ObjectName (digiKam)<br />
	///     XMP:Title (exiftool)<br />
	///     EXIF:XPTitle (digiKam)<br />
	///     <b>WRITE :</b><br />
	/// </summary>
	public string Title {
		get {
			return m_Title ??= GetTitle() ?? string.Empty;

			string? GetTitle() {
				if (TryGetTag("IPTC", "ObjectName", out var value)) return value;
				if (TryGetTag("XMP", "Title", out value)) return value;
				if (TryGetTag("EXIF", "XPTitle", out value)) return value;
				return null;
			}
		}
	}

	/// <summary>
	///     <b>READ :</b><br />
	///     IPTC:Caption-Abstract<br />
	///     XMP:Caption<br />
	///     <b>WRITE :</b><br />
	/// </summary>
	public string Caption {
		get {
			return m_Caption ??= GetTitle();

			string GetTitle() {
				if (TryGetTag("IPTC", "Caption-Abstract", out var value)) return value;
				if (TryGetTag("XMP", "Caption", out value)) return value;

				return string.Empty;
			}
		}
	}

	/// <summary>
	///     <b>READ :</b><br />
	///     IPTC:Copyright Notice<br />
	///     <b>WRITE :</b><br />
	/// </summary>
	public string Copyright {
		get {
			return m_Copyright ??= GetCopyright();

			string GetCopyright() {
				if (TryGetTag("IPTC", "Copyright Notice", out var value)) return value;

				return string.Empty;
			}
		}
	}

	/// <summary>
	///     <b>READ :</b><br />
	///     Xmp.xmp.rating (digiKam)<br />
	///     Xmp.acdsee.rating (digiKam)<br />
	///     Xmp.MicrosoftPhoto.Rating (digiKam)<br />
	///     Exif.Image.Rating (digiKam)<br />
	///     Exif.Image.RatingPercent (digiKam)<br />
	///     Iptc.Application2.Urgency (digiKam)<br />
	///     <b>WRITE :</b><br />
	/// </summary>
	public int? Note {
		get {
			return m_Note ??= GetNote();

			int? GetNote() {
				if (TryGetTag("EXIF", "Rating", out var value) && int.TryParse(value, out var i)) return i;
				if (TryGetTag("XMP", "Rating", out value) && int.TryParse(value, out i)) return i;
				if (TryGetTag("IPTC", "Urgency", out value) && int.TryParse(value, out i)) return i;

				//if (TryGetTag("EXIF", "RatingPercent", out value)) return value;
				return null;
			}
		}
	}

	/// <summary>
	///     <b>READ :</b><br />
	///     xmp.digiKam.ColorLabel (digiKam)<br />
	///     xmp.xmp.label (digiKam)<br />
	///     xmp.photoshop.Urgency (digiKam)<br />
	///     <b>WRITE :</b><br />
	/// </summary>

	public string Label {
		get {
			return m_Label ??= Get() ?? string.Empty;

			string? Get() {
				if (TryGetTag("XMP", "Label", out var value)) return value;
				if (TryGetTag("XMP", "ColorLabel", out value)) return value;
				if (TryGetTag("XMP", "Urgency", out value)) return value;
				return null;
			}
		}
	}

	/// <summary>
	///     EXIF:Image Description<br />
	///     XMP:Description
	/// </summary>
	public string Description {
		get {
			return m_Comments ??= Get() ?? string.Empty;

			string? Get() {
				if (TryGetTag("EXIF", "Image Description", out var value)) return value;
				if (TryGetTag("XMP", "Description", out value)) return value;

				// if (TryGetTag("XMP", "User Comment", out value)) return value;
				// if (TryGetTag("XMP", "Image Description", out value)) return value;
				// if (TryGetTag("XMP", "Notes", out value)) return value;
				// if (TryGetTag("File", "Comment", out value)) return value;
				// if (TryGetTag("EXIF", "User Comment", out value)) return value;
				// if (TryGetTag("EXIF", "XPComment", out value)) return value;
				// if (TryGetTag("IPTC", "Caption", out value)) return value;
				return null;
			}
		}
	}

	/// <summary>
	///     <b>READ :</b><br />
	///     Xmp.digiKam.TagsList (digiKam)<br />
	///     Xmp.MicrosoftPhoto.LastKeywordXMP (digiKam)<br />
	///     Xmp.lr.hierarchicalSubject (digiKam)<br />
	///     Xmp.mediapro.CatalogSets (digiKam)<br />
	///     Xmp.acdssee.Categories (digiKam)<br />
	///     Xmp.dc.subject (digiKam)<br />
	///     Iptc.Application2.Keywords (digiKam)<br />
	///     Exif.Image.XPKeywords (digiKam)<br />
	///     <b>WRITE :</b><br />
	/// </summary>

	public IReadOnlyCollection<string> Keywords {
		get {
			return m_Keywords ??= Get(true);

			string[] Get(bool combineAll) {
				if (combineAll) {
					var values = new List<string>();
					if (TryGetKeywords("XMP", "Subject", out var value)) Combine(value);
					if (TryGetKeywords("IPTC", "Keywords", out value)) Combine(value);
					if (TryGetKeywords("EXIF", "XP Keywords", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Category", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Weighted Flat Subject", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Hierarchical Subject", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Tags List", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Catalog Sets", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Last Keyword XMP", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Last Keyword IPTC", out value)) Combine(value);
					if (TryGetKeywords("XMP", "TagList", out value)) Combine(value);
					if (TryGetKeywords("XMP", "Categories", out value)) Combine(value);
					return [..values];

					void Combine(string[] items) {
						foreach (var item in items) {
							var trimmed = item.Trim();
							if (!string.IsNullOrWhiteSpace(trimmed) && !values.Contains(trimmed)) values.Add(trimmed);
						}
					}
				} else {
					if (TryGetKeywords("XMP", "Subject", out var value)) return value;
					if (TryGetKeywords("IPTC", "Keywords", out value)) return value;
					if (TryGetKeywords("EXIF", "XP Keywords", out value)) return value;
					if (TryGetKeywords("XMP", "Category", out value)) return value;
					if (TryGetKeywords("XMP", "Weighted Flat Subject", out value)) return value;
					if (TryGetKeywords("XMP", "Hierarchical Subject", out value)) return value;
					if (TryGetKeywords("XMP", "Tags List", out value)) return value;
					if (TryGetKeywords("XMP", "Catalog Sets", out value)) return value;
					if (TryGetKeywords("XMP", "Last Keyword XMP", out value)) return value;
					if (TryGetKeywords("XMP", "Last Keyword IPTC", out value)) return value;
					if (TryGetKeywords("XMP", "TagList", out value)) return value;
					if (TryGetKeywords("XMP", "Categories", out value)) return value;
				}
				return Array.Empty<string>();

				bool TryGetKeywords(string container, string name, [NotNullWhen(true)] out string[]? value) {
					if (!TryGetTag(container, name, out var value1)) {
						value = null;
						return false;
					}
					var result = new List<string>();
					if (value1.Contains("</")) {
						var xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(value1);
						var categoryNodes = xmlDoc.SelectNodes("/Categories/Category");

						if (categoryNodes != null) {
							foreach (XmlNode categoryNode in categoryNodes) {
								var categoryName = categoryNode.InnerText;
								Add(categoryName);
							}
						}
					} else
						Add(value1);
					if (result.Count > 0) {
						result.Sort();
						value = result.ToArray();
						return true;
					}
					value = null;
					return false;

					void Add(string value) {
						var values = value.Split(';', ',', '|', '/', '\\');
						foreach (var item in values) {
							var trimmed = item.Trim();
							if (!string.IsNullOrWhiteSpace(trimmed) && !result.Contains(trimmed)) result.Add(trimmed);
						}
					}
				}
			}
		}
	}

	/// <summary>
	///     Logger
	/// </summary>
	public ILogger? Logger { get; set; }

	public static FileInfo? ExifTool { get; set; }

	[MemberNotNull(nameof(lzy_Outpout), nameof(lzyTags))]
	private void InitializeLazy() {
		m_Title = null;
		m_Note = null;
		m_Keywords = null;
		m_Comments = null;
		m_Label = null;
		lzy_Outpout = new(() => Execute($"-f -L -G -u -a \"{FullFileName}\""));
		lzyTags = new(GetTags);

		List<ExifTag> GetTags() {
			var output = lzy_Outpout.Value;

			var result = new List<ExifTag>();
			foreach (var line in output.Split('\n', '\r')) {
				if (string.IsNullOrWhiteSpace(line)) continue;
				var match = rTag.Match(line);
				if (!match.Success) continue;
				var container = match.Groups["Container"].Value.Trim();
				var trim      = match.Groups["Name"].Value.Trim();
				var value     = match.Groups["Value"].Value.Trim();
				if (container == "ExifTool") continue;
				if (container == "Composite") continue;
				if (container == "RIFF") continue;

				//if(container=="File" && trim!="MIME Type") continue;

				result.Add(new(container, trim, value));
			}
			return result;
		}
	}

	private string Execute(string parameters) {
		if (ExifTool is null || !ExifTool.Exists) throw new("ExifTool not found");

		var command = $"{ExifTool.FullName} {parameters}";
		Logger?.LogInformation("[Execute] {Command}", command);

		// Set up process start info
		var startInfo = new ProcessStartInfo
		                {
			                FileName = "cmd.exe",        // Use cmd.exe on Windows
			                Arguments = $"/C {command}", // /C carries out the command specified by string and then terminates
			                RedirectStandardOutput = true,
			                UseShellExecute = false,
			                CreateNoWindow = true
		                };

		// Start the process
		using var process = new Process();
		process.StartInfo = startInfo;
		process.Start();
		var output = process.StandardOutput.ReadToEnd();
		process.WaitForExit();
		if (process.ExitCode != 0) {
			Logger?.LogWarning("[Execute]Exit Code: {ExitCode}", process.ExitCode);
			Logger?.LogWarning("[Execute]{Output}", output);
			throw new(output);
		}
		Logger?.LogDebug("[Execute]{Output}", output);

		return output;
	}

	public bool TryGetTag(string container, string name, [NotNullWhen(true)] out string? value) {
		var tags = lzyTags.Value;
		foreach (var tag in tags) {
			if (string.Equals(tag.Container, container, StringComparison.OrdinalIgnoreCase) && string.Equals(tag.Name, name, StringComparison.OrdinalIgnoreCase)) {
				value = tag.Value;
				return true;
			}
		}
		value = null;
		return false;
	}

	public void SetDateTime(DateTime referenceDate) {
		var fullFileName = FullFileName;
		File.SetCreationTime(fullFileName, referenceDate);
		File.SetLastWriteTime(fullFileName, referenceDate);
		File.SetLastAccessTime(fullFileName, referenceDate);
	}

	public void Update(string? title = null,
		string? caption = null,
		string? copyright = null,
		string? description = null,
		int? note = null,
		string? label = null,
		List<string>? keywords = null,
		bool deleteOtherTags = false,
		bool forceUpdateFields = false) {
		var extension = Path.GetExtension(FullFileName).ToLowerInvariant();

		var args = new List<string>();
		if (keywords is not null) AddKeywordsArgs();

		if (forceUpdateFields || !string.Equals(title, Title, StringComparison.Ordinal)) args.Add($"-title=\"{title}\" -iptc:objectname=\"{title}\" -exif:xptitle=\"{title}\"");
		if (forceUpdateFields || !string.Equals(caption, Caption, StringComparison.Ordinal)) args.Add($"-caption=\"{caption}\" -iptc:caption-abstract=\"{caption}\"");
		if (forceUpdateFields || !string.Equals(copyright, Copyright, StringComparison.Ordinal)) args.Add($"-copyright=\"{copyright}\" -iptc:copyrightnotice=\"{copyright}\"");
		if (forceUpdateFields || !string.Equals(description, Description, StringComparison.Ordinal)) args.Add($"-exif:imagedescription=\"{description}\" -description=\"{description}\"");
		if (forceUpdateFields || note != Note) args.Add($"-rating={note}");
		if (forceUpdateFields || !string.Equals(label, Label, StringComparison.Ordinal)) args.Add($"-label=\"{label}\"");

		if (args.Count == 0) return;
		if (deleteOtherTags) args.Insert(0, "-all=");
		Execute($"{string.Join(" ", args)} \"{FullFileName}\"");
		var fileInfo = new FileInfo(FullFileName + "_original");
		if (fileInfo.Exists) fileInfo.Delete();
		InitializeLazy();

		void AddKeywordsArgs() {
			var updated = string.Join(", ", keywords.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).Distinct().OrderBy(i => i));
			if (!forceUpdateFields) {
				var existing = string.Join(", ", Keywords.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).Distinct().OrderBy(i => i));
				if (string.Equals(existing, updated, StringComparison.Ordinal)) return;
			}
			args.Add($"-subject=\"{updated}\"");
			switch (extension) {
				case ".jpg":
				case ".jpeg":
					args.Add($"-iptc:keywords=\"{updated}\"");
					break;
				case ".mp4":
					args.Add($"-category=\"{updated}\"");

					//args.Add($"-subject=\"{updated}\"");
					break;
				default: return;
			}
		}
	}

	public record ExifTag(string Container, string Name, string Value);
}
