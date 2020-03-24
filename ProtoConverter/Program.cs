using System;
using System.IO;
using System.Text;

namespace ProtoConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            // path to proto files' folder
            var path = @"C:\sc_protos\s2clientprotocol";

            var fileArray = Directory.GetFiles(path, "*.proto");

            // change proto2 to proto3
            // change the import path
            // remove the word 'optional'
            foreach(string s in fileArray)
            {
                string str = File.ReadAllText(s);
                str = str.Replace("syntax = \"proto2\";", "syntax = \"proto3\";");
                str = str.Replace("import \"s2clientprotocol/", "import \"");
                str = str.Replace("optional ", string.Empty);
                File.WriteAllText(s, str);

                // now go through line by line looking for enums
                // all enums have to have a '0' value enum - new rule for proto3
                var text = new StringBuilder();
                bool isEnum = false;
                foreach(string line in File.ReadAllLines(s))
                {
                    // this will be true if the last line contained the text "enum " (with a space)
                    if(isEnum)
                    {
                        // this is the line after the enum keyword
                        // if this line does not have a field with a value of zero, we have to add it
                        if(line.Contains("= 0;")) // bold assumption that there is going to be a space between '=' and '0;'
                        {
                            // this is OK, this is what we want
                        }
                        else
                        {
                            // create a new member with the value of "0"
                            string tempString = string.Format("    {0} = 0;", RandomString(10));
                            text.AppendLine(tempString);                          
                        }
                        // this is now false
                        isEnum = false;
                    }
                    if(line.Trim().StartsWith("enum ")) // line starts with zero or more blanks and then the word 'enum'
                    {
                        // we mark that this is an enum
                        // but we can't do anything until the next line
                        isEnum = true;
                    }
                    text.AppendLine(line);
                };

                // write out new file
                using (var file = new StreamWriter(File.Create(s)))
                {
                    file.Write(text.ToString());
                }

            }


        }

        // based on this: https://stackoverflow.com/questions/9995839/how-to-make-random-string-of-numbers-and-letters-with-a-length-of-5
        private static string RandomString(int length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz";
            var builder = new StringBuilder();
            Random rand = new Random();

            for (var i = 0; i < length; i++)
            {
                var c = pool[rand.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }
    }
}
