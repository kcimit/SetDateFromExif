using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace SetDateFromExif
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0) return;
            //args = new string[1];
            //args[0] = @"c:\temp\t.txt";
            if (!File.Exists(args[0])) return;
            string line;
            var errors = false;
            if (args[0].EndsWith(".jpg") || args[0].EndsWith(".jpeg"))
            {
                DateTime? dateTime = null;
                var failure = GetDateTime(args[0], ref dateTime);
                if (failure)
                {
                    Console.WriteLine($"Wrong file of no exif data.");
                    errors = true;
                }
                else
                {
                    try
                    {
                        File.SetCreationTime(args[0], (DateTime)dateTime);
                        File.SetLastWriteTime(args[0], (DateTime)dateTime);
                        Console.WriteLine($"Success setting date for {args[0]} to {dateTime}");
                    }
                    catch
                    {
                        Console.WriteLine($"Failed setting date for {args[0]} to {dateTime}");
                        errors = true;
                    }
                }
            }
            else
            {
                /*try
                {
                    File.Copy(args[0], @"c:\temp\t.txt");
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                }*/
                using var file = new System.IO.StreamReader(args[0], Encoding.UTF7);
                while ((line = file.ReadLine()) != null)
                {
                    var skip = false;
                    DateTime? dateTime = null;

                    if (!File.Exists(line)) skip = true;
                    if (!(line.Contains(".jpg") || line.Contains(".jpeg"))) skip = true;
                    if (!skip)
                        skip = GetDateTime(line, ref dateTime);
                    if (skip)
                    {
                        Console.WriteLine($"Skipping {line} - wrong file of no exif data.");
                        errors = true;
                        continue;
                    }
                    try
                    {
                        File.SetCreationTime(line, (DateTime)dateTime);
                        File.SetLastWriteTime(line, (DateTime)dateTime);
                        Console.WriteLine($"Success setting date for {line} to {dateTime}");
                    }
                    catch
                    {
                        Console.WriteLine($"Failed setting date for {line} to {dateTime}");
                        errors = true;
                    }
                }
            }
            if (errors)
                Console.ReadLine();
        }

        private static bool GetDateTime(string line, ref DateTime? dateTime)
        {
            bool failure = false;

            var exifdata = ImageMetadataReader.ReadMetadata(line);
            if (exifdata == null || exifdata.Count == 0) failure = true;
            if (!failure)
            {
                // access the date time
                var subIfdDirectory = exifdata.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                if (subIfdDirectory == null) failure = true;
                if (!failure)
                {
                    try
                    {
                        dateTime = subIfdDirectory?.GetDateTime(ExifDirectoryBase.TagDateTimeOriginal);
                    }
                    catch
                    {
                        failure = true;
                    }
                    if (dateTime == null) failure = true;
                }
            }

            return failure;
        }
    }
}
