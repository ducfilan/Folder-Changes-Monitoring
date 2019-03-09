using System;
using System.IO;
using System.Text;
using Shell32;

namespace TrackFolderChange.Support
{
    public class FilePropertiesExtractor
    {
        public static string GetSpecificFileProperties(string file, params int[] indexes)
        {
            var result = string.Empty;

            try
            {
                var fileName = Path.GetFileName(file);
                var folderName = Path.GetDirectoryName(file);
                var shell = new Shell32.Shell();
                var objFolder = shell.NameSpace(folderName);
                var sb = new StringBuilder();

                foreach (FolderItem2 item in objFolder.Items())
                {
                    if (fileName != item.Name) continue;

                    foreach (var index in indexes)
                    {
                        sb.Append(objFolder.GetDetailsOf(item, index) + ",");
                    }

                    break;
                }

                result = sb.ToString().Trim();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            // Protection for no results causing an exception on the `SubString` method.
            return result.Length == 0 ? string.Empty : result.Substring(0, result.Length - 1);
        }

    }
}
