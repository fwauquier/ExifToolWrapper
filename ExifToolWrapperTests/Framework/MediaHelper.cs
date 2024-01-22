// // MIT License
// // Copyright (c) 2023 Frédéric Wauquier
// //
// // Permission is hereby granted, free of charge, to any person obtaining a copy
// // of this software and associated documentation files (the "Software"), to deal
// // in the Software without restriction, including without limitation the rights
// // to use, copy, modify, merge, publish, distribute, sublicense, and/or
// // sell copies of the Software, and to permit persons to whom the Software
// // is furnished to do so, subject to the following conditions:
// //
// // The above copyright notice and this permission notice shall be included in
// // all copies or substantial portions of the Software.
// //
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.
//
// using System.Diagnostics;
// using System.Xml.Linq;
// using ImageMagick;
// using MetadataExtractor;
// using MetadataExtractor.Formats.Adobe;
// using MetadataExtractor.Formats.Exif;
// using MetadataExtractor.Formats.Exif.Makernotes;
// using MetadataExtractor.Formats.FileSystem;
// using MetadataExtractor.Formats.FileType;
// using MetadataExtractor.Formats.Gif;
// using MetadataExtractor.Formats.Icc;
// using MetadataExtractor.Formats.Iptc;
// using MetadataExtractor.Formats.Jfif;
// using MetadataExtractor.Formats.Jpeg;
// using MetadataExtractor.Formats.Photoshop;
// using MetadataExtractor.Formats.QuickTime;
// using MetadataExtractor.Formats.WebP;
// using MetadataExtractor.Formats.Xmp;
//
// // ReSharper disable UnusedMember.Local
// #pragma warning disable IDE0051
//
// namespace Synology.Image_tools;
//
// public static class MediaHelper {
// 	private static readonly XNamespace acdseeNamespace = "http://ns.acdsee.com/iptc/1.0/";
// 	private static readonly XNamespace MicrosoftPhotoNamespace = "http://ns.microsoft.com/photo/1.0/";
//
// 	//private static readonly XNamespace crsNamespace = "http://ns.adobe.com/camera-raw-settings/1.0/";
// 	private static readonly XNamespace dcNamespace = "http://purl.org/dc/elements/1.1/";
// 	private static readonly XNamespace digiKamNamespace = "http://www.digikam.org/ns/1.0/";
// 	private static readonly XNamespace lrNamespace = "http://ns.adobe.com/lightroom/1.0/";
//
// 	private static readonly XNamespace mediaproNamespace = "http://ns.iview-multimedia.com/mediapro/1.0/";
//
// 	//private static readonly XNamespace stEvtNamespace = "http://ns.adobe.com/xap/1.0/sType/ResourceEvent#";
// 	//private static readonly XNamespace xmpNamespace = "http://ns.adobe.com/xap/1.0/";
// 	//private static readonly XNamespace xmpMMNamespace = "http://ns.adobe.com/xap/1.0/mm/";
// 	private static readonly XNamespace rdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
//
// 	public static void SetDateTime(string fileName, DateTime dateTime) {
// 		var fileInfo = new FileInfo(fileName);
// 		if (fileInfo.CreationTime != dateTime) fileInfo.CreationTime = dateTime;
// 		if (fileInfo.LastWriteTime != dateTime) fileInfo.LastWriteTime = dateTime;
// 		if (fileInfo.LastAccessTime != dateTime) fileInfo.LastAccessTime = dateTime;
// 	}
//
// 	public static bool CanBeUpdated(FileInfo fileInfo) {
// 		switch (fileInfo.Extension.ToLowerInvariant()) {
// 			case ".jpg":
// 			case ".jpeg":
// 			case ".gif":
// 			case ".webp":
// 			case ".mp4":
// 				return true;
// 			case ".db": return false;
// 			default:    return false;
// 		}
// 	}
//
// 	public static List<string> GetTags(FileInfo fileInfo) {
// 		var tags = new List<string>();
// 		if (!CanBeUpdated(fileInfo)) return tags;
//
// 		ReadTagMagickImage(fileInfo, tags);
// 		ReadTagImageMetadataReader(fileInfo, tags);
// 		tags.Sort();
// 		return tags;
//
// 		// static void Adds (List<string> tags,IEnumerable<string?>? tagsToAdd) {
// 		// 	if (tagsToAdd is null) return;
// 		// 	foreach (var tag in tagsToAdd) {
// 		// 		if (string.IsNullOrEmpty(tag)) continue;
// 		// 		Add (tags,tag);
// 		// 	}
// 		// }
// 	}
//
// 	private static void ReadTagImageMetadataReader(FileInfo fileInfo, List<string> tags) {
// 		try {
// 			var readMetadata = ImageMetadataReader.ReadMetadata(fileInfo.FullName);
// 			foreach (var directory in readMetadata) {
// 				switch (directory) {
// 					case IptcDirectory iptcDirectory:
// 						foreach (var tag in iptcDirectory.Tags) {
// 							var value = tag.Description;
// 							if (tag.Name == "Keywords" && !string.IsNullOrWhiteSpace(value)) Add(tags, value);
// 						}
// 						break;
//
// 					case XmpDirectory xmpDirectory:
// 						var prop = xmpDirectory.GetXmpProperties();
// 						foreach (var tag in prop) {
// 							if (tag.Key.StartsWith("lr:weightedFlatSubject", StringComparison.InvariantCultureIgnoreCase)
// 							    || tag.Key.StartsWith("acdsee:keywords", StringComparison.InvariantCultureIgnoreCase)
// 							    || tag.Key.StartsWith("lr:hierarchicalSubject", StringComparison.InvariantCultureIgnoreCase)
// 							    || tag.Key.StartsWith("acdsee:categories", StringComparison.InvariantCultureIgnoreCase)
// 							    || tag.Key.StartsWith("digiKam:TagsList", StringComparison.InvariantCultureIgnoreCase)
// 							    || tag.Key.StartsWith("MicrosoftPhoto:LastKeywordXMP", StringComparison.InvariantCultureIgnoreCase)
// 							    || tag.Key.StartsWith("mediapro:CatalogSets", StringComparison.InvariantCultureIgnoreCase)
// 							    || tag.Key.StartsWith("dc:subject", StringComparison.InvariantCultureIgnoreCase))
// 								Add(tags, tag.Value);
// 						}
// 						break;
// 					case ExifSubIfdDirectory exifSubIfdDirectory:
// 						foreach (var tag in exifSubIfdDirectory.Tags) {
// 							var value = tag.Description;
// 							if (tag.Name == "Keywords" && !string.IsNullOrWhiteSpace(value)) Add(tags, value);
// 						}
// 						break;
// 					case AdobeJpegDirectory:
// 					case CanonMakernoteDirectory:
// 					case ExifIfd0Directory:
// 					case ExifInteropDirectory:
// 					case ExifThumbnailDirectory:
// 					case FileMetadataDirectory:
// 					case FileTypeDirectory:
// 					case GifImageDirectory:
// 					case GifAnimationDirectory:
// 					case GifControlDirectory:
// 					case GifHeaderDirectory:
// 					case GpsDirectory:
// 					case HuffmanTablesDirectory:
// 					case IccDirectory:
// 					case JfifDirectory:
// 					case JpegCommentDirectory:
// 					case JpegDirectory:
// 					case NikonType1MakernoteDirectory:
// 					case NikonType2MakernoteDirectory:
// 					case PanasonicMakernoteDirectory:
// 					case PhotoshopDirectory:
// 					case PrintIMDirectory:
// 					case QuickTimeFileTypeDirectory:
// 					case QuickTimeMovieHeaderDirectory:
// 					case QuickTimeTrackHeaderDirectory:
// 					case WebPDirectory:
// 						break;
//
// 					default: break;
// 				}
//
// 				foreach (var tag in directory.Tags) {
// 					switch (tag.Name) {
// 						case "ExifDirectoryBase.TagKeywords":
// 						case "ExifDirectoryBase.TagSubject":
// 						case "IptcDirectoryBase.TagKeywords":
// 						case "XmpDirectory.TagSubject":
// 						case "XmpDirectory.TagHierarchicalSubject":
// 						case "PhotoshopDirectory.TagKeywords":
// 						case "WicDirectory.TagKeyword":
// 						case "TagKeywords":
// 						case "TagSubject":
// 						case "TagHierarchicalSubject":
// 						case "TagKeyword":
// 						case "Keywords":
// 						case "Categories":
// 							Add(tags, tag.Description);
// 							break;
// 					}
// 				}
// 			}
// 		} catch (Exception ex) {
// 			Console.WriteLine($"[FAILED.ReadTagImageMetadataReader] {fileInfo.FullName} : {ex.Message}");
// 		}
// 	}
//
// 	private static void ReadTagMagickImage(FileInfo fileInfo, List<string> tags) {
// 		try {
// 			using var readMetadata = new MagickImage(fileInfo);
//
// //
// // var exifProfile = readMetadata.GetExifProfile();
// // if (exifProfile is not null) {
// // 	foreach (var tag in exifProfile.Values) {
// // Console.WriteLine($"[EXIF]{tag.Tag}");
// //
// // 	}
// // }
//
// 			var iptc = readMetadata.GetIptcProfile();
// 			if (iptc is not null) {
// 				foreach (var tag in iptc.Values) {
// 					// Console.WriteLine($"[IPTC]{tag.Tag}");
// 					// ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
// 					// ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault
// 					switch (tag.Tag) {
// 						case IptcTag.Category:
// 						case IptcTag.SupplementalCategories:
// 						case IptcTag.Keyword:
// 							Add(tags, tag.Value);
// 							break;
// 					}
// 				}
// 			}
// 			var xmp = readMetadata.GetXmpProfile();
// 			if (xmp is not null) {
// 				try {
// 					var xmlDoc = xmp.ToXDocument();
// 					foreach (var value in xmlDoc.Descendants(rdfNamespace + "Description")) {
// 						var tag = value.Attribute(acdseeNamespace + "categories")?.Value;
// 						Add(tags, tag);
// 					}
//
// 					void Add2(IEnumerable<XElement> elements) {
// 						foreach (var a in elements.Descendants(rdfNamespace + "li")) Add(tags, a.Value);
// 					}
//
// 					Add2(xmlDoc.Descendants(dcNamespace + "subject"));
// 					Add2(xmlDoc.Descendants(lrNamespace + "weightedFlatSubject"));
// 					Add2(xmlDoc.Descendants(lrNamespace + "hierarchicalSubject"));
// 					Add2(xmlDoc.Descendants(digiKamNamespace + "TagsList"));
// 					Add2(xmlDoc.Descendants(MicrosoftPhotoNamespace + "LastKeywordXMP"));
// 					Add2(xmlDoc.Descendants(mediaproNamespace + "CatalogSets"));
// 					Add2(xmlDoc.Descendants(acdseeNamespace + "keywords"));
// 					Add2(xmlDoc.Descendants("keywords"));
// 				} catch {
// 					// NOTHING
// 				}
// 			}
// 		} catch (Exception ex) {
// 			Console.WriteLine($"[FAILED.MagickImage] {fileInfo.FullName} : {ex.Message}");
// 		}
// 	}
//
// 	private static void Add(List<string> tags, string? tag) {
// 		if (string.IsNullOrEmpty(tag)) return;
// 		if (tag.StartsWith("<Categories>", StringComparison.InvariantCultureIgnoreCase)) {
// 			var product = XElement.Parse(tag);
// 			foreach (var item in product.Elements()) Add(tags, item.Value);
// 		} else {
// 			foreach (var t in tag.Split('/',
// 			                            '|',
// 			                            '>',
// 			                            '<',
// 			                            ',',
// 			                            ';')) {
// 				var t2 = t.Trim();
// 				if (!string.IsNullOrWhiteSpace(t2) && !tags.Contains(t2)) tags.Add(t2);
// 			}
// 		}
// 	}
//
// 	public static void SetTags(FileInfo fileInfo, List<string> tags) {
// 		if (!CanBeUpdated(fileInfo)) return;
//
// 		try {
// 			// Command to execute (replace with your actual command)
// 			var    join = string.Join(", ", tags);
// 			string command;
// 			switch (fileInfo.Extension.ToLowerInvariant()) {
// 				case ".jpg":
// 				case ".jpeg":
// 					command = $"-xmp:subject=\"{join}\" -iptc:keywords=\"{join}\""; // OK
// 					break;
// 				case ".gif":
// 					command = $"-subject=\"{join}, Subject\" -keywords=\"{join}, keywords\" -description=\"{join}, description\" -comment=\"{join}, comment\"";
// 					break;
// 				case ".webp":
// 					command = $"-subject=\"{join}\""; // OK
// 					break;
// 				case ".mp4":
// 					/*
// [QuickTime]     Handler Type                    : Metadata
// [QuickTime]     Handler Vendor ID               : Apple
// [QuickTime]     Track Number                    : 99
// [QuickTime]     Disk Number                     : 49
// [QuickTime]     Genre                           : Genre
// [QuickTime]     Genre                           : Genre (VLC)
// [QuickTime]     Content Create Date             : 2024
// [QuickTime]     Title                           : Titre
// [QuickTime]     Title                           : Titre (VLC)
// [QuickTime]     Title                           : Title (Windows Explorer);
// [QuickTime]     Composer                        : Compositeur
// [QuickTime]     Composer                        : Compositeur
// [QuickTime]     Comment                         : Commentaire
// [QuickTime]     Comment                         : Commentaires (VLC)
// [QuickTime]     Artist                          : Interpr├¿te
// [QuickTime]     Artist                          : Artiste (VLC)
// [QuickTime]     Album Artist                    : Artiste de l'album
// [QuickTime]     Album                           : Album
// [QuickTime]     Album                           : Album (VLC)
// [QuickTime]     Subtitle                        : Sous-titre (windows explorer)
// [QuickTime]     Subtitle                        : Subtitke (Windows Explorer);
// [XMP]           XMP Toolkit                     : Image::ExifTool 12.72
// [XMP]           Description                     : dotNet test 2, dotNet test, description
// [XMP]           Subject                         : dotNet test 2, dotNet test, Subject
// [XMP]           Keywords                        : dotNet test 2, dotNet test, keywords
// [XMP]           Category                        : dotNet test 2, dotNet test, xmp_Category
// 					 */
// 					command =
// 						$"-xmp:subject=\"{join}, xmp_Category\" -QuickTime:category=\"{join}, quicktime_Category\" -title=\"{join}, title\" -composer=\"{join}, composer\" -subject=\"{join}, Subject\" -comment=\"{join}, Comment\" -artist=\"{join}, Artist\" -keywords=\"{join}, keywords\" -description=\"{join}, description\" -subtitle=\"{join}, Subtitle\"";
// 					break;
// 				case ".db": return;
// 				default:    return;
// 			}
// 			command = $"exiftool -all= {command} \"{fileInfo.FullName}\"";
// 			Console.WriteLine(command);
//
// 			// Set up process start info
// 			var startInfo = new ProcessStartInfo
// 			                {
// 				                FileName = "cmd.exe",        // Use cmd.exe on Windows
// 				                Arguments = $"/C {command}", // /C carries out the command specified by string and then terminates
// 				                RedirectStandardOutput = true,
// 				                UseShellExecute = false,
// 				                CreateNoWindow = true
// 			                };
//
// 			// Start the process
// 			using (var process = new Process()) {
// 				process.StartInfo = startInfo;
// 				process.Start();
//
// 				// Read the output (if needed)
// 				var output = process.StandardOutput.ReadToEnd();
// 				Console.WriteLine("Output: " + output);
//
// 				process.WaitForExit();
//
// 				// Console.WriteLine("Command executed successfully!");
// 			}
// 		} catch (Exception ex) {
// 			Console.WriteLine($"Error: {ex.Message}");
// 		}
// 	}
// }
