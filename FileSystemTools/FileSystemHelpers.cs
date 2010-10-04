using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.SqlServer.Server;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
/// <summary>
/// private methods used to handle diffrent operations
/// like encryption and compression
/// </summary>
public partial class SQLServerFileSystemTools
{
    /// <summary>
    /// Stores incomming data as standard zip file into byte array
    /// </summary>
    /// <param name="fileName">Filename</param>
    /// <param name="compressionLevel">Compression level 1-9</param>
    /// <returns>zip file as byte array</returns>
    private static byte[] ZipCompress(string fileName, int compressionLevel)
    {
        //SqlPipe pipe = SqlContext.Pipe;

        using (MemoryStream tempFileStream = new MemoryStream())
        using (ZipOutputStream zipOutput = new ZipOutputStream(tempFileStream))
        {
            zipOutput.SetLevel(compressionLevel);
            Crc32 crc = new Crc32();

            // Get local path and create stream to it.
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Read full stream to in-memory buffer.
                byte[] buffer = new byte[fileStream.Length];
                int offset = 0;
                int remaining = buffer.Length;
                while (remaining > 0)
                {
                    int read = fileStream.Read(buffer, offset, buffer.Length);
                    if (read <= 0)
                        throw new EndOfStreamException
                            (String.Format("End of stream reached with {0} bytes left to read", remaining));
                    remaining -= read;
                    offset += read;
                }

                //get crc to store in header.
                crc.Reset();
                crc.Update(buffer);

                // Create a new entry for the current file.
                ZipEntry entry = new ZipEntry(ZipEntry.CleanName(Path.GetFileName(fileName)))
                                     {
                                         DateTime = DateTime.Now,
                                         Size = fileStream.Length,
                                         Crc = crc.Value
                                     };
                fileStream.Close();

                zipOutput.PutNextEntry(entry);

                zipOutput.Write(buffer, 0, buffer.Length);
                // Finalize the zip output.
                zipOutput.Finish();
                // Flushes the create and close.
                zipOutput.Flush();
            }

            zipOutput.Close();
            //retrun zip compiant data stream
            return tempFileStream.ToArray();
        }
    }

    /// <summary>
    /// uncompresses zip data from byte array
    /// </summary>
    /// <param name="compressedData"></param>
    /// <param name="origionalFileSize"></param>
    /// <returns>uncompressed data in MemoryStream container</returns>
    private static byte[] ZipDecompress(byte[] compressedData, int origionalFileSize)
    {
        SqlPipe pipe = SqlContext.Pipe;

        //setup decompression stream
        MemoryStream compressedStream = new MemoryStream(compressedData);

        byte[] buffer = new byte[origionalFileSize];

        using (ZipInputStream decompressedStream = new ZipInputStream(compressedStream))
        {
            //while there are entries in the zip file
            if ((decompressedStream.GetNextEntry()) != null)
            {
                // Read full stream to in-memory buffer.
                int offset = 0;
                int remaining = buffer.Length;
                while (remaining > 0)
                {
                    int read = decompressedStream.Read(buffer, offset, buffer.Length);
                    if (read <= 0)
                        throw new EndOfStreamException
                            (String.Format("End of stream reached with {0} bytes left to read", remaining));
                    remaining -= read;
                    offset += read;
                }
            }
            else
            {
                pipe.Send("no entries in zip archive");
            }
        }
        return buffer;
    }

    /// <summary>
    /// Encrypt byte array
    /// </summary>
    /// <param name="dataStream">unencrypted data array.</param>
    /// <param name="password">password</param>
    /// <returns>encrypted data array</returns>
    private static byte[] EncryptStream(byte[] dataStream, string password)
    {
        SqlPipe pipe = SqlContext.Pipe;

        if (dataStream.Length > 0)
        {
            MemoryStream stream = new MemoryStream(dataStream.Length);

            //encryption object
            RijndaelManaged cryptic = new RijndaelManaged
                                          {
                                              Key = Encoding.ASCII.GetBytes(password),
                                              IV = Encoding.ASCII.GetBytes("1qazxsw23edcvfr4"),
                                              Padding = PaddingMode.ISO10126,
                                          };
            //setup stream
            CryptoStream crStream = new CryptoStream(stream, cryptic.CreateEncryptor(), CryptoStreamMode.Write);
            //write compressed data to holding stream
            try
            {
                crStream.Write(dataStream, 0, dataStream.Length);
            }
            catch (Exception e)
            {
                pipe.Send("Failed to encrypt stream");
                pipe.Send(e.Message);
                throw;
            }

            crStream.Close();
            //return encrypted data
            return stream.ToArray();
        }
        pipe.Send("no data to encrypt");
        return null;
    }

    /// <summary>
    /// Decrypt byte array
    /// </summary>
    /// <param name="dataStream">encrypted data array</param>
    /// <param name="password">password</param>
    /// <returns>unencrypted data array</returns>
    private static byte[] DecryptStream(byte[] dataStream, string password)
    {
        SqlPipe pipe = SqlContext.Pipe;
        //the decrypter
        RijndaelManaged cryptic = new RijndaelManaged
        {
            Key = Encoding.ASCII.GetBytes(password),
            IV = Encoding.ASCII.GetBytes("1qazxsw23edcvfr4"),
            Padding = PaddingMode.ISO10126,
        };

        //Get a decryptor that uses the same key and IV as the encryptor used.
        ICryptoTransform decryptor = cryptic.CreateDecryptor();

        //Now decrypt encrypted data stream
        MemoryStream msDecrypt = new MemoryStream(dataStream);
        CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

        byte[] fromEncrypt = new byte[dataStream.Length];

        //Read the data out of the crypto stream.
        try
        {
            csDecrypt.Read(fromEncrypt, 0, fromEncrypt.Length);
        }
        catch (Exception e)
        {
            pipe.Send("Failed to decrypt data");
            pipe.Send(e.Message);
            throw;
        }

        return fromEncrypt;
    }

    /// <summary>
    /// Generate 256 bit MD5 hash
    /// </summary>
    /// <param name="input">string to hash.</param>
    /// <returns>hashed string.</returns>
    private static string GetMd5Hash(string input)
    {
        //set salt size minus the password string
        //ideally we like to have 8 bytes available for the salt
        int saltsize = 32 - Encoding.ASCII.GetByteCount(input);
        if (saltsize > 0)
        {
            string rndsalt = RandomASCIIString(saltsize);
            input += rndsalt;
        }

        // Create a new instance of the MD5CryptoServiceProvider object.
        MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

        // Convert the input string to a byte array and compute the hash.
        byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        StringBuilder sBuilder = new StringBuilder();

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for (int i = 0; i < data.Length; i++)
        {
            sBuilder.Append(data[i].ToString("x2"));
        }

        // Return the hexadecimal string.
        return sBuilder.ToString();
    }

    /// <summary>
    /// Generate random ASCII string
    /// </summary>
    /// <param name="length">Size of random string in bytes</param>
    /// <returns>string</returns>
    private static string RandomASCIIString(int length)
    {

        string result = string.Empty;
        Random random = new Random();
        for (int i = 0; i < length; i++)
        {
            char ch = Convert.ToChar(random.Next(33, 126));
            result += ch;
        }
        return result;
    }

    /// <summary>
    /// Creates path if it doesn't exists.
    /// Deletes any files with same name as output file.
    /// </summary>
    /// <param name="location">file path on disk.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns>filename with full path.</returns>
    private static string GetFileNameWithPath(string location, string fileName)
    {
        SqlPipe pipe = SqlContext.Pipe;
        if (!Directory.Exists(location))
        {
            try
            {
                Directory.CreateDirectory(location);
            }
            catch (Exception e)
            {
                pipe.Send("Failed to create output directory");
                pipe.Send(e.Message);
                throw;
            }
        }

        string fileNameWithPath = location + "\\" + fileName;
        if (File.Exists(fileNameWithPath))
        {
            try
            {
                File.Delete(fileNameWithPath);
            }
            catch (Exception e)
            {
                pipe.Send("Failed to delete previous output file");
                pipe.Send(e.Message);
                throw;
            }
        }
        return fileNameWithPath;
    }
}