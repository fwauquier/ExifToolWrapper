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
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.Logging;
#pragma warning disable CA1848

namespace ExifrootWrapper;


/// <summary>
///     Wrapper for ExifTool executable
/// </summary>
public sealed class Wrapper {
	public enum FileType {
		Unknown,
		Jpeg,
		Pdf,
		Png,
		Gif,
		Bmp,
		Webp,
		Webm,
		Mp4,
		Avi,
		Tif,
		Tiff,
		m2ts
	}

	public static readonly string[] AuthorizeExtensions = [".jpg", ".pdf", ".mts", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".webm", ".mp4", ".avi", ".tif", ".tiff"];

	private static readonly Regex rTag = new("\\[(?<Container>.*)\\]\\s+(?<Name>.*)\\s+:\\s+(?<Value>.*)",
	                                         RegexOptions.Multiline
	                                         | RegexOptions.Compiled
	                                         | RegexOptions.CultureInvariant);

	private readonly FileInfo ExifTool;

	public Wrapper(FileInfo exifTool) {
		ExifTool = exifTool;
	}

	/// <summary>
	///     Logger
	/// </summary>
	public ILogger? Logger { get; set; }

	public List<ExifTag> GetTags(string fullFileName) {
		var output = GetInfo(fullFileName);

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

	public List<string> GetKeywords(string fullFileName) {
		var result = new List<string>();
		var tags   = GetTags(fullFileName).ToList();
		foreach (var tag in tags) {
			switch (tag.Container) {
				// case "File": {
				// 	if (tag.Name == "Comment") Add(tag.Value);
				// 	break;
				// }
				case "Exif": {
					if (tag.Name == "XP Keywords") Add(tag.Value);
					break;
				}
				case "IPTC": {
					if (tag.Name == "Keywords") Add(tag.Value);
					break;
				}
				case "XMP": {
					switch (tag.Name) {
						case "Keywords":
						case "Weighted Flat Subject":
						case "Hierarchical Subject":
						case "Tags List":
						case "Catalog Sets":
						case "Subject":
						case "Last Keyword XMP":
						case "Last Keyword IPTC":
						case "Category":
						case "TagList":
							Add(tag.Value);
							break;
						case "Categories": {
							var xmlDoc = new XmlDocument();
							xmlDoc.LoadXml(tag.Value);

							// Assuming the Categories element is the root element
							var categoryNodes = xmlDoc.SelectNodes("/Categories/Category");

							if (categoryNodes == null) continue;
							foreach (XmlNode categoryNode in categoryNodes) {
								var categoryName = categoryNode.InnerText;
								Add(categoryName);
							}
							break;
						}
					}
					break;
				}
			}
		}
		return result;

		void Add(string value) {
			var values = value.Split(';', ',', '|', '/', '\\');
			foreach (var item in values) {
				var trimmed = item.Trim();
				if (!string.IsNullOrWhiteSpace(trimmed) && !result.Contains(trimmed)) result.Add(trimmed);
			}
		}
	}

	/// <summary>
	/// </summary>
	/// <param name="fullFileName"></param>
	/// <param name="tags"></param>
	/// <param name="additionalParameters">Remove all other tags : ' -all= '</param>
	public void SetTags(string fullFileName, List<string> tags, string additionalParameters = "") {
		// Command to execute (replace with your actual command)
		var    join = string.Join(", ", tags.Where(i => !string.IsNullOrWhiteSpace(i)).Select(i => i.Trim()).Distinct().OrderBy(i => i));
		string command;
		switch (Path.GetExtension(fullFileName).ToLowerInvariant()) {
			case ".jpg":
			case ".jpeg":
				command = $" -iptc:keywords=\"{join}\" -xmp:subject=\"{join}\""; // OK
				break;
			case ".png":
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

				command = $"-xmp:subject=\"{join}\"";
				break;
			case ".gif":
				command = $"-xmp:subject=\"{join}\"";
				break;
			case ".webp":
				command = $"-xmp:subject=\"{join}\""; // OK
				break;
			case ".mp4":
				/*
[QuickTime]     Handler Type                    : Metadata
[QuickTime]     Handler Vendor ID               : Apple
[QuickTime]     Track Number                    : 99
[QuickTime]     Disk Number                     : 49
[QuickTime]     Genre                           : Genre
[QuickTime]     Genre                           : Genre (VLC)
[QuickTime]     Content Create Date             : 2024
[QuickTime]     Title                           : Titre
[QuickTime]     Title                           : Titre (VLC)
[QuickTime]     Title                           : Title (Windows Explorer);
[QuickTime]     Composer                        : Compositeur
[QuickTime]     Composer                        : Compositeur
[QuickTime]     Comment                         : Commentaire
[QuickTime]     Comment                         : Commentaires (VLC)
[QuickTime]     Artist                          : Interpr├¿te
[QuickTime]     Artist                          : Artiste (VLC)
[QuickTime]     Album Artist                    : Artiste de l'album
[QuickTime]     Album                           : Album
[QuickTime]     Album                           : Album (VLC)
[QuickTime]     Subtitle                        : Sous-titre (windows explorer)
[QuickTime]     Subtitle                        : Subtitke (Windows Explorer);
[XMP]           XMP Toolkit                     : Image::ExifTool 12.72
[XMP]           Description                     : dotNet test 2, dotNet test, description
[XMP]           Subject                         : dotNet test 2, dotNet test, Subject
[XMP]           Keywords                        : dotNet test 2, dotNet test, keywords
[XMP]           Category                        : dotNet test 2, dotNet test, xmp_Category
				 */
				command = $"-subject=\"{join}\" -xmp:subject=\"{join}\"";
				break;
			case ".bmp":
			case ".db":
			default:
				return;
		}
		command = $"{additionalParameters} {command} \"{fullFileName}\"";
		Execute(command);
		var fileInfo = new FileInfo(fullFileName + "_original");
		if (fileInfo.Exists) fileInfo.Delete();
	}

	/// <summary>
	///     Get fileType from '[File]MIME Type' tag
	/// </summary>
	/// <param name="tags"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	public static FileType GetFileType(IReadOnlyCollection<ExifTag> tags) {
		var mimeTag = tags.FirstOrDefault(t => t is {Container: "File", Name: "MIME Type"}) ?? throw new("Cannot find MIME Type");
		switch (mimeTag.Value) {
			case "image/jpeg":      return FileType.Jpeg;
			case "image/png":       return FileType.Png;
			case "image/gif":       return FileType.Gif;
			case "image/bmp":       return FileType.Bmp;
			case "image/webp":      return FileType.Webp;
			case "image/tiff":      return FileType.Tif;
			case "video/webm":      return FileType.Webm;
			case "video/mp4":       return FileType.Mp4;
			case "video/avi":       return FileType.Avi;
			case "application/pdf": return FileType.Pdf;
			case "video/m2ts":      return FileType.m2ts;
			default:                throw new("MIME Type not authorized: " + mimeTag.Value);
		}
	}

	public string GetInfo(string fullFileName, string options = "-f -G -u -a") {
		var extension = Path.GetExtension(fullFileName).ToLowerInvariant();
		if (!AuthorizeExtensions.Contains(extension)) throw new ArgumentException($"File with extension '{extension}' is not supported");
		var parameters = $"{options} \"{fullFileName}\"";
		return Execute(parameters);
	}

	private string Execute(string parameters) {
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

	public sealed record ExifTag(string Container, string Name, string Value);
}
