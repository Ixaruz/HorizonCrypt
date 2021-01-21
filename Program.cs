using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHSE.Core;

namespace PogCrypt.ConsoleApp
{
    public sealed partial class Program
    {
        private static HorizonSave SAV;

        private const string Encrypt = "-c";
        private const string Decrypt = "-d";
        private const string BatchMode = "-b";


        private enum Mode
        {
            Encrypt, Decrypt
        }

        static void Main(string[] args)
        {
            if (!TryExecute(args))
            {
                PrintUsage();
                return;
            }
            else
            {
                Console.WriteLine("Done!");
                return;
            }

            Console.Read();
        }


        private static bool TryExecute(string[] args)
        {
            if (args.Length < 2)
                return false;

            var file = args[args.Length - 1];
            if (!File.Exists(file) && !Directory.Exists(file))
                return false;

            file = Path.GetFullPath(file);
            var directory = Path.GetDirectoryName(file);
            if (directory == null)
                return false;

            var argumentsValid = false;
            var batchMode = false;
            var mode = Mode.Decrypt;
            for (var i = 0; i < args.Length - 1; i++)
            {
                var type = args[i];
                switch (type)
                {
                    case Decrypt:
                        {
                            if (argumentsValid)
                                return false;
                            mode = Mode.Decrypt;
                            argumentsValid = true;
                            break;
                        }

                    case Encrypt:
                        {
                            if (argumentsValid)
                                return false;
                            mode = Mode.Encrypt;
                            argumentsValid = true;
                            break;
                        }

                    case BatchMode:
                        {
                            batchMode = true;
                            break;
                        }

                    default:
                        return false;
                }
            }

            if (!argumentsValid)
                return false;

            // Create out directory to preserve original files.
            var filename = Path.GetFileName(file);
            if (batchMode && string.IsNullOrWhiteSpace(filename))
            {
                file = directory;
                filename = Path.GetFileName(file);
                directory = Path.GetDirectoryName(directory);
            }

            var outDir = Path.Combine(directory, filename + (mode == Mode.Decrypt ? " Decrypted" : " Encrypted"));
            Console.WriteLine("OutDir: " + outDir + "\n");

            // In batch mode the file will be the root directory.
            if (batchMode)
            {
                var rootDir = file;
                if (Directory.Exists(rootDir))
                {
                    ProcessFolder(rootDir, outDir, mode);
                    
                    if(mode == Mode.Encrypt) fixhashes(outDir);
                    return true;
                }

                return false;
            }

            // Process an individual file.
            if (File.Exists(file))
            {
                ProcessFile(file, directory, outDir, mode);
                return true;
            }

            return false;
        }

        private static void ProcessFolder(in string directory, in string outDir, in Mode mode)
        {
            foreach (var file in Directory.GetFiles(directory, "*.dat"))
                ProcessFile(file, directory, outDir, mode);

            foreach (var subDir in Directory.GetDirectories(directory))
                ProcessFolder(subDir, Path.Combine(outDir, Path.GetFileName(subDir)), mode);
        }

        private static void fixhashes(in string directory)
        {
            //lazy hash fix
            string[] files = Directory.GetFiles(directory, "*.dat");
            if (files.Any())
            {
                SAV = new HorizonSave(directory);
                SAV.Save((uint)DateTime.Now.Ticks);
                Console.WriteLine($"fixed hashes in {directory}\n");
            }
            else
            {
                foreach (var subDir in Directory.GetDirectories(directory))
                    fixhashes(subDir);
            }
        }

        private static void ProcessFile(in string file, in string directory, in string outDir, in Mode mode)
        {
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);
            var filename = Path.GetFileNameWithoutExtension(file);
            //eception for landname.dat which is never encrypted
            if (filename.Contains("landname"))
            {
                byte[] landname = File.ReadAllBytes(file);
                File.WriteAllBytes(Path.Combine(outDir, filename + ".dat"), landname);
                Console.WriteLine($"Copied: {file}\n");
            }
            else if (!filename.Contains("Header"))
            {
                switch (mode)
                {
                    case Mode.Decrypt:
                        {
                            var headerPath = Path.Combine(directory, filename + "Header.dat");
                            var headerData = File.ReadAllBytes(headerPath);
                            byte[] encData = File.ReadAllBytes(file);

                            Encryption.Decrypt(headerData, encData);
                            File.WriteAllBytes(Path.Combine(outDir, filename + ".dat"), encData);
                            Console.WriteLine($"Decrypted: {file}\n");
                            break;
                        }

                    case Mode.Encrypt:
                        {
                            byte[] decData = File.ReadAllBytes(file);
                            byte[] Header = decData;

                            // First 256 bytes go unused
                            var importantData = new uint[0x80];
                            Buffer.BlockCopy(Header, 0x100, importantData, 0, 0x200);

                            var seed = (uint)DateTime.Now.Ticks;
                            var encrypt = Encryption.Encrypt(decData, seed, Header);

                            File.WriteAllBytes(Path.Combine(outDir, filename + ".dat"), encrypt.Data);
                            File.WriteAllBytes(Path.Combine(outDir, filename + "Header.dat"), encrypt.Header);
                            Console.WriteLine($"Encrypted: {file}\n");
                            break;
                        }
                }
            }
        }


        private static void PrintUsage()
        {
            Console.WriteLine("HorizonCrypt by Cuyler, Updated by Poyo");
            Console.WriteLine("Combining HorizonCrypt and NHSE, ultimate Piss");
            Console.WriteLine("Usage:");
            Console.WriteLine("\tHorizonCrypt [-b] [-c|-d] <input>");
        }

    }
}
