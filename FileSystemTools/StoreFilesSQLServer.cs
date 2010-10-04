using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using Microsoft.SqlServer.Server;

/// <summary>
/// Tools for working with files and SQL Server.
/// Includes storage compression and encryption.
/// This is a set of stored procedures that allow you to 
/// store files in a database with the option to compress
/// into a zip compatiable stream or encrypt with Rijndael or both.
/// It can output to the file system compressed or decompressed.
/// it can output to a sql record type decompressed
/// </summary>
public partial class SQLServerFileSystemTools
{
    
    // future tasks
    // refactor to get rid of finalizers
    // refactor to get rid of private static vars
    // refactor to get rid of anything in the zip lib not needed like encryption, multiple checksum algorithims
    // add multiple file load into a single zip
    // add read file list from zip
    // fix anything else preventing EXTERNAL_ACCESS
    // move file read to encrypt routine like the compress routine.
    // improve memory consumption
    // improve speed

    /// <summary>
    /// Load file from filesystem into database    
    /// encrypted and uncompressed
    /// </summary>
    /// <param name="fileLocation">The files locaton.</param>
    /// <param name="returnFileId">Return of the inserted FileId</param>
    [SqlProcedure]
    public static void StoreFile(string fileLocation, out long returnFileId)
    {
        returnFileId = new long();
        SqlPipe pipe = SqlContext.Pipe;
        //check to see if the file exists
        if (File.Exists(fileLocation))
        {
            Byte[] bytebin;
            FileInfo binFile = new FileInfo(fileLocation);

            //read the file into holding array
            using (Stream fileStream = binFile.OpenRead())
            {
                bytebin = new Byte[fileStream.Length];

                try
                {
                    fileStream.Read(bytebin, 0, (int) binFile.Length);
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to Read File");
                    pipe.Send(e.Message);
                    throw;
                }
            }
            //get values and insert file into database
            using (SqlConnection cn = new SqlConnection("context connection=true"))
            {
                cn.Open();
                SqlCommand sqlCmd = new SqlCommand("InsertFile", cn) {CommandType = CommandType.StoredProcedure};

                sqlCmd.Parameters.Add("@oFileName", SqlDbType.NVarChar, 255).Value = binFile.Name;
                sqlCmd.Parameters.Add("@oFileSize", SqlDbType.BigInt).Value = binFile.Length;
                sqlCmd.Parameters.Add("@SQLStorageType", SqlDbType.TinyInt).Value = 1;
                sqlCmd.Parameters.Add("@oFilePath", SqlDbType.NVarChar, 255).Value = binFile.DirectoryName;
                sqlCmd.Parameters.Add("@oFileExtention", SqlDbType.NVarChar, 255).Value = binFile.Extension;
                sqlCmd.Parameters.Add("@oFileCreateDate", SqlDbType.NVarChar, 255).Value = binFile.CreationTime;
                sqlCmd.Parameters.Add("@oFileLastWriteDate", SqlDbType.NVarChar, 255).Value = binFile.LastWriteTime;
                sqlCmd.Parameters.Add("@oFileLastAccessDate", SqlDbType.NVarChar, 255).Value = binFile.LastAccessTime;
                SqlParameter outputFileId = new SqlParameter
                                             {
                                                 ParameterName = "@ReturnFileId",
                                                 SqlDbType = SqlDbType.BigInt,
                                                 Direction = ParameterDirection.Output//,Value = returnFileId
                                             };
                sqlCmd.Parameters.Add(outputFileId);

                try
                {
                    sqlCmd.Parameters.Add("@FileData", SqlDbType.VarBinary).Value = bytebin;
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }

                try
                {
                    pipe.ExecuteAndSend(sqlCmd);
                    returnFileId = (long)outputFileId.Value;
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Load file from filesystem into database    
    /// encrypted and uncompressed
    /// </summary>
    /// <param name="fileLocation">The files locaton.</param>
    /// <param name="returnFileId">Return of the inserted FileId</param>
    public static void StoreFileEncrypted(string fileLocation, out long returnFileId)
    {
        returnFileId = new long();
        SqlPipe pipe = SqlContext.Pipe;
        string password = "";
        //check to see if the file exists
        if (File.Exists(fileLocation))
        {
            FileInfo binFile = new FileInfo(fileLocation);
            byte[] buffer = new byte[binFile.Length];
            // Get local path and create stream to it.
            using (FileStream fileStream = new FileStream(fileLocation, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                // Read full stream to in-memory buffer.
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
            }


            using (SqlConnection cn = new SqlConnection("context connection=true"))
            {
                cn.Open();
                SqlCommand sqlCmd = new SqlCommand("RetrievePassPhrase", cn)
                                        {CommandType = CommandType.StoredProcedure};
                try
                {
                    SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    if (sqlReader != null)
                    {
                        sqlReader.Read();
                        password = (string) sqlReader.GetSqlString(0);
                    }
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to Retreve Pass Phrase");
                    pipe.Send("execute StorePassPhrase to add a password");
                    pipe.Send(e.Message);
                    throw;
                }
            }
            //get values and insert file into database
            using (SqlConnection cn = new SqlConnection("context connection=true"))
            {
                cn.Open();
                SqlCommand sqlCmd = new SqlCommand("InsertFile", cn) {CommandType = CommandType.StoredProcedure};
                sqlCmd.Parameters.Add("@oFileName", SqlDbType.NVarChar, 255).Value = binFile.Name;
                sqlCmd.Parameters.Add("@oFileSize", SqlDbType.BigInt).Value = binFile.Length;
                sqlCmd.Parameters.Add("@SQLStorageType", SqlDbType.TinyInt).Value = 2;
                sqlCmd.Parameters.Add("@oFilePath", SqlDbType.NVarChar, 255).Value = binFile.DirectoryName;
                sqlCmd.Parameters.Add("@oFileExtention", SqlDbType.NVarChar, 255).Value = binFile.Extension;
                sqlCmd.Parameters.Add("@oFileCreateDate", SqlDbType.NVarChar, 255).Value = binFile.CreationTime;
                sqlCmd.Parameters.Add("@oFileLastWriteDate", SqlDbType.NVarChar, 255).Value = binFile.LastWriteTime;
                sqlCmd.Parameters.Add("@oFileLastAccessDate", SqlDbType.NVarChar, 255).Value = binFile.LastAccessTime;
                SqlParameter outputFileId = new SqlParameter
                {
                    ParameterName = "@ReturnFileId",
                    SqlDbType = SqlDbType.BigInt,
                    Direction = ParameterDirection.Output//,Value = returnFileId
                };
                sqlCmd.Parameters.Add(outputFileId);


                try
                {
                    sqlCmd.Parameters.Add("@FileData", SqlDbType.VarBinary).Value = EncryptStream(buffer, password);
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }

                try
                {
                    pipe.ExecuteAndSend(sqlCmd);
                    returnFileId = (long)outputFileId.Value;
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Load file from filesystem into database
    /// unencrypted and compressed
    /// </summary>
    /// <param name="fileLocation">The files locaton.</param>
    /// <param name="compressionLevel">The files locaton.</param>
    /// <param name="returnFileId">Return of the inserted FileId</param>
    public static void StoreFileCompressed(string fileLocation, int compressionLevel, out long returnFileId)
    {
        returnFileId = new long();
        SqlPipe pipe = SqlContext.Pipe;
        //check to see if the file exists
        if (File.Exists(fileLocation))
        {
            FileInfo binFile = new FileInfo(fileLocation);

            //read the file into holding array
            using (Stream fileStream = binFile.OpenRead())
            {
              byte[] bytebin = new Byte[fileStream.Length];

                try
                {
                    fileStream.Read(bytebin, 0, (int)binFile.Length);
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to Read File");
                    pipe.Send(e.Message);
                    throw;
                }
            }

            //get values and insert file into database
            using (SqlConnection cn = new SqlConnection("context connection=true"))
            {
                cn.Open();
                SqlCommand sqlCmd = new SqlCommand("InsertFile", cn) { CommandType = CommandType.StoredProcedure };
                sqlCmd.Parameters.Add("@oFileName", SqlDbType.NVarChar, 255).Value = binFile.Name;
                sqlCmd.Parameters.Add("@oFileSize", SqlDbType.BigInt).Value = binFile.Length;
                sqlCmd.Parameters.Add("@SQLStorageType", SqlDbType.TinyInt).Value = 3;
                sqlCmd.Parameters.Add("@oFilePath", SqlDbType.NVarChar, 255).Value = binFile.DirectoryName;
                sqlCmd.Parameters.Add("@oFileExtention", SqlDbType.NVarChar, 255).Value = binFile.Extension;
                sqlCmd.Parameters.Add("@oFileCreateDate", SqlDbType.NVarChar, 255).Value = binFile.CreationTime;
                sqlCmd.Parameters.Add("@oFileLastWriteDate", SqlDbType.NVarChar, 255).Value = binFile.LastWriteTime;
                sqlCmd.Parameters.Add("@oFileLastAccessDate", SqlDbType.NVarChar, 255).Value = binFile.LastAccessTime;
                SqlParameter outputFileId = new SqlParameter
                {
                    ParameterName = "@ReturnFileId",
                    SqlDbType = SqlDbType.BigInt,
                    Direction = ParameterDirection.Output//,Value = returnFileId
                };
                sqlCmd.Parameters.Add(outputFileId);

                try
                {
                    sqlCmd.Parameters.Add("@FileData", SqlDbType.VarBinary).Value = ZipCompress(fileLocation, compressionLevel);
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }

                try
                {
                    SqlContext.Pipe.ExecuteAndSend(sqlCmd);
                    returnFileId = (long)outputFileId.Value;
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Load file from filesystem into database
    /// encrypted and compressed
    /// </summary>
    /// <param name="fileLocation">The files locaton.</param>
    /// <param name="compressionLevel">The files locaton.</param>
    /// <param name="returnFileId"></param>
    public static void StoreFileEncryptedCompressed(string fileLocation, int compressionLevel, out long returnFileId)
    {
        returnFileId = new long();
        SqlPipe pipe = SqlContext.Pipe;
        string password = "";
        //check to see if the file exists
        if (File.Exists(fileLocation))
        {
            FileInfo binFile = new FileInfo(fileLocation);

            using (SqlConnection cn = new SqlConnection("context connection=true"))
            {
                cn.Open();
                SqlCommand sqlCmd = new SqlCommand("RetrievePassPhrase", cn) { CommandType = CommandType.StoredProcedure };
                try
                {
                    SqlDataReader sqlReader = sqlCmd.ExecuteReader();
                    if (sqlReader != null)
                    {
                        sqlReader.Read();
                        password = (string)sqlReader.GetSqlString(0);
                    }
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to Retreve Pass Phrase");
                    pipe.Send("execute StorePassPhrase to add a password");
                    pipe.Send(e.Message);
                    throw;
                }
            }
            //get values and insert file into database
            using (SqlConnection cn = new SqlConnection("context connection=true"))
            {
                cn.Open();
                SqlCommand sqlCmd = new SqlCommand("InsertFile", cn) { CommandType = CommandType.StoredProcedure };
                sqlCmd.Parameters.Add("@oFileName", SqlDbType.NVarChar, 255).Value = binFile.Name;
                sqlCmd.Parameters.Add("@oFileSize", SqlDbType.BigInt).Value = binFile.Length;
                sqlCmd.Parameters.Add("@SQLStorageType", SqlDbType.TinyInt).Value = 4;
                sqlCmd.Parameters.Add("@oFilePath", SqlDbType.NVarChar, 255).Value = binFile.DirectoryName;
                sqlCmd.Parameters.Add("@oFileExtention", SqlDbType.NVarChar, 255).Value = binFile.Extension;
                sqlCmd.Parameters.Add("@oFileCreateDate", SqlDbType.NVarChar, 255).Value = binFile.CreationTime;
                sqlCmd.Parameters.Add("@oFileLastWriteDate", SqlDbType.NVarChar, 255).Value = binFile.LastWriteTime;
                sqlCmd.Parameters.Add("@oFileLastAccessDate", SqlDbType.NVarChar, 255).Value = binFile.LastAccessTime;
                SqlParameter outputFileId = new SqlParameter
                {
                    ParameterName = "@ReturnFileId",
                    SqlDbType = SqlDbType.BigInt,
                    Direction = ParameterDirection.Output//,Value = returnFileId
                };
                sqlCmd.Parameters.Add(outputFileId);

                try
                {
                    sqlCmd.Parameters.Add("@FileData", SqlDbType.VarBinary).Value = EncryptStream(ZipCompress(fileLocation, compressionLevel), password);
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }

                try
                {
                    SqlContext.Pipe.ExecuteAndSend(sqlCmd);
                    returnFileId = (long)outputFileId.Value;
                }
                catch (Exception e)
                {
                    pipe.Send("Failed to insert file");
                    pipe.Send(e.Message);
                    throw;
                }
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Loads files from a path with filtering
    /// * for all flles in directory
    /// txt, doc, etc to filter by file type
    /// calls Loadfile to store the files in the database
    /// unencrypted and uncompressed.
    /// </summary>
    /// <param name="filesLocaton">The files locaton.</param>
    /// <param name="fileSuffix">The file suffix.</param>
    public static void StoreAllFilesInPath(string filesLocaton, string fileSuffix)
    {
        SqlPipe pipe = SqlContext.Pipe;
        //check path to make sure it exists
        if (Directory.Exists(filesLocaton))
        {
            DirectoryInfo dirFiles = new DirectoryInfo(filesLocaton);
            FileInfo[] listOfFiles = dirFiles.GetFiles("*." + fileSuffix);
            //loop through list of files if any were returned
            if (listOfFiles.Length > 0)
            {
                foreach (FileInfo aFile in listOfFiles)
                {
                    //load file
                    try
                    {
                        long returnFileId;
                        StoreFile(aFile.FullName, out returnFileId);
                    }
                    catch (Exception e)
                    {
                        pipe.Send("Failed to insert file");
                        pipe.Send(e.Message);
                        throw;
                    }
                }
            }
            else
            {
                pipe.Send("No files match filter");
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Loads files from a path with filtering
    /// * for all flles in directory
    /// txt, doc, etc to filter by file type
    /// calls Loadfile to store the files in the database
    /// encrypted and uncompressed.
    /// </summary>
    /// <param name="filesLocaton">The files locaton.</param>
    /// <param name="fileSuffix">The file suffix.</param>
    public static void StoreAllFilesInPathEncrypted(string filesLocaton, string fileSuffix)
    {
        SqlPipe pipe = SqlContext.Pipe;
        //check path to make sure it exists
        if (Directory.Exists(filesLocaton))
        {
            DirectoryInfo dirFiles = new DirectoryInfo(filesLocaton);
            FileInfo[] listOfFiles = dirFiles.GetFiles("*." + fileSuffix);

            //loop through list of files if any were returned
            if (listOfFiles.Length > 0)
            {
                foreach (FileInfo aFile in listOfFiles)
                {
                    //load file
                    try
                    {
                        long returnFileId;
                        StoreFileEncrypted(aFile.FullName, out returnFileId);
                    }
                    catch (Exception e)
                    {
                        pipe.Send("Failed to insert file");
                        pipe.Send(e.Message);
                        throw;
                    }
                }
            }
            else
            {
                pipe.Send("No files match filter");
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Loads files from a path with filtering
    /// * for all flles in directory
    /// txt, doc, etc to filter by file type
    /// calls Loadfile to store the files in the database
    /// unencrypted and compressed.
    /// </summary>
    /// <param name="filesLocaton">The files locaton.</param>
    /// <param name="fileSuffix">The file suffix.</param>
    /// <param name="compressionLevel">The storetype.</param>
    public static void StoreAllFilesInPathCompressed(string filesLocaton, string fileSuffix, int compressionLevel)
    {
        SqlPipe pipe = SqlContext.Pipe;
        //check path to make sure it exists
        if (Directory.Exists(filesLocaton))
        {
            DirectoryInfo dirFiles = new DirectoryInfo(filesLocaton);
            FileInfo[] listOfFiles = dirFiles.GetFiles("*." + fileSuffix);

            //loop through list of files if any were returned
            if (listOfFiles.Length > 0)
            {
                foreach (FileInfo aFile in listOfFiles)
                {
                    //load file
                    try
                    {
                        long returnFileId;
                        StoreFileCompressed(aFile.FullName,compressionLevel,out returnFileId);
                    }
                    catch (Exception e)
                    {
                        pipe.Send("Failed to insert file");
                        pipe.Send(e.Message);
                        throw;
                    }
                }
            }
            else
            {
                pipe.Send("No files match filter");
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Loads files from a path with filtering
    /// * for all flles in directory
    /// txt, doc, etc to filter by file type
    /// calls Loadfile to store the files in the database
    /// encrypted and compressed.
    /// </summary>
    /// <param name="filesLocaton">The files locaton.</param>
    /// <param name="fileSuffix">The file suffix.</param>
    /// <param name="compressionLevel">The storetype.</param>
    public static void StoreAllFilesInPathEncryptedCompressed(string filesLocaton, string fileSuffix, int compressionLevel)
    {
        SqlPipe pipe = SqlContext.Pipe;
        //check path to make sure it exists
        if (Directory.Exists(filesLocaton))
        {
            DirectoryInfo dirFiles = new DirectoryInfo(filesLocaton);
            FileInfo[] listOfFiles = dirFiles.GetFiles("*." + fileSuffix);

            //loop through list of files if any were returned
            if (listOfFiles.Length > 0)
            {
                foreach (FileInfo aFile in listOfFiles)
                {
                    //load file
                    try
                    {
                        long returnFileId;
                        StoreFileEncryptedCompressed(aFile.FullName, compressionLevel,out returnFileId);
                    }
                    catch (Exception e)
                    {
                        pipe.Send("Failed to insert file");
                        pipe.Send(e.Message);
                        throw;
                    }
                }
            }
            else
            {
                pipe.Send("No files match filter");
            }
        }
        else
        {
            pipe.Send("Invalid Filename or Path");
        }
    }

    /// <summary>
    /// Retrieves file from database
    /// uncompresses and decrypts it if 
    /// it needs to.
    /// </summary>
    /// <param name="fileId">file id</param>
    /// <param name="location">Location to extract to</param>
    public static void ExtractFileToDisk(long fileId, string location)
    {
        SqlPipe pipe = SqlContext.Pipe;
        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) {CommandType = CommandType.StoredProcedure};
            try
            {
                sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
                sqlCmd.Parameters[0].Value = fileId;
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {
                    sqlReader.Read();
                    string fileName = (string) sqlReader.GetSqlString(0);
 
                    int origionalFileSize = (int) sqlReader.GetSqlInt64(3);
                    int storageType = sqlReader.GetByte(8);

                    MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                    const int length = 4096;
                    byte[] fileBlob = new byte[length];
                    int startPoint = 0;
                    
                    long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                    
                    string password = (string)sqlReader.GetSqlString(9);
                    sqlDataStream.Write(fileBlob, 0, (int) retval);

                    while (retval == length)
                    {
                        startPoint += length;
                        retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                        sqlDataStream.Write(fileBlob, 0, (int) retval);
                    }
                    sqlReader.Close();
                    string fileNameWithPath = GetFileNameWithPath(location, fileName);
                    sqlDataStream.Seek(0, SeekOrigin.End);


                        //if it is encrypted decrypt it.
                    switch (storageType)
                    {
                        case 2:
                            {
                                if (password != "")
                                    using (
                                        BinaryWriter binWriter =
                                            new BinaryWriter(File.Open(fileNameWithPath, FileMode.Create)))
                                    {
                                        binWriter.Write(DecryptStream(sqlDataStream.ToArray(), password));
                                    }
                                else
                                    pipe.Send("Failed to retreve pass phrase file cannot be decrypted");
                            }
                            break;
                        case 3:
                            {
                                using (
                                    BinaryWriter binWriter =
                                        new BinaryWriter(File.Open(fileNameWithPath, FileMode.Create)))
                                {
                                    binWriter.Write(ZipDecompress(sqlDataStream.ToArray(), origionalFileSize));
                                }
                            }
                            break;
                        case 4:
                            {
                                if (password != "")
                                    using (
                                        BinaryWriter binWriter =
                                            new BinaryWriter(File.Open(fileNameWithPath, FileMode.Create)))
                                    {
                                        binWriter.Write(
                                            ZipDecompress(DecryptStream(sqlDataStream.ToArray(), password),
                                                          origionalFileSize));
                                    }


                                else
                                    pipe.Send("Failed to retreve pass phrase file cannot be decrypted");

                            }
                            break;
                        default:
                            using (
                                BinaryWriter binWriter =
                                    new BinaryWriter(File.Open(fileNameWithPath, FileMode.Create)))
                            {
                                binWriter.Write(sqlDataStream.ToArray());
                            }

                            break;
                    }

                }
        }
    }

    /// <summary>
    /// Retrieves file from database raw encrypted form .enc extention
    /// </summary>
    /// <param name="fileId">file id</param>
    /// <param name="location">Location to extract to</param>
    public static void ExtractFileToDiskEncrypted(long fileId, string location)
    {
        SqlPipe pipe = SqlContext.Pipe;
        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) {CommandType = CommandType.StoredProcedure};
            sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
            sqlCmd.Parameters[0].Value = fileId;
            try
            {
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {

                    sqlReader.Read();
                    int storageType = sqlReader.GetByte(8);
                    if (storageType == 2)
                    {
                        string fileName = (string) sqlReader.GetSqlString(0) + ".enc";

                        int origionalFileSize = (int) sqlReader.GetSqlInt64(3);


                        MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                        const int length = 4096;
                        byte[] fileBlob = new byte[length];
                        int startPoint = 0;

                        long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);

                        sqlDataStream.Write(fileBlob, 0, (int) retval);

                        while (retval == length)
                        {
                            startPoint += length;
                            retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                            sqlDataStream.Write(fileBlob, 0, (int) retval);
                        }
                        sqlReader.Close();
                        string fileNameWithPath = GetFileNameWithPath(location, fileName);
                        sqlDataStream.Seek(0, SeekOrigin.Begin);

                        using (BinaryWriter binWriter = new BinaryWriter(File.Open(fileNameWithPath, FileMode.Create)))
                        {
                            binWriter.Write(sqlDataStream.ToArray());
                        }
                    }
                    else
                    {
                        sqlReader.Close();
                        pipe.Send("File is not encrypted");
                    }
                }
        }
    }

    /// <summary>
    /// Retrieves file from database raw compressed form with .zip extention
    /// </summary>
    /// <param name="fileId">file id</param>
    /// <param name="location">Location to extract to</param>
    public static void ExtractFileToDiskCompressed(long fileId, string location)
    {
        SqlPipe pipe = SqlContext.Pipe;
        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) { CommandType = CommandType.StoredProcedure };
            sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
            sqlCmd.Parameters[0].Value = fileId;
            try
            {
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {

                    sqlReader.Read();
                    int storageType = sqlReader.GetByte(8);
                    if (storageType == 3)
                    {
                        string fileName = (string)sqlReader.GetSqlString(0) + ".zip";

                        int origionalFileSize = (int)sqlReader.GetSqlInt64(3);


                        MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                        const int length = 4096;
                        byte[] fileBlob = new byte[length];
                        int startPoint = 0;

                        long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);

                        sqlDataStream.Write(fileBlob, 0, (int)retval);

                        while (retval == length)
                        {
                            startPoint += length;
                            retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                            sqlDataStream.Write(fileBlob, 0, (int)retval);
                        }
                        sqlReader.Close();
                        string fileNameWithPath = GetFileNameWithPath(location, fileName);
                        sqlDataStream.Seek(0, SeekOrigin.Begin);

                        using (BinaryWriter binWriter = new BinaryWriter(File.Open(fileNameWithPath, FileMode.Create)))
                        {
                            binWriter.Write(sqlDataStream.ToArray());
                        }
                    }
                    else
                    {
                        sqlReader.Close();
                        pipe.Send("File is not compressed");
                    }
                }

        }
    }

    /// <summary>
    /// Retrieves file from database raw encrypted and compressed form with .zip.enc extention
    /// </summary>
    /// <param name="fileId">file id</param>
    /// <param name="location">Location to extract to</param>
    public static void ExtractFileToDiskEncryptedCompressed(long fileId, string location)
    {
        SqlPipe pipe = SqlContext.Pipe;
        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) { CommandType = CommandType.StoredProcedure };
            sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
            sqlCmd.Parameters[0].Value = fileId;
            try
            {
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {

                    sqlReader.Read();
                    int storageType = sqlReader.GetByte(8);
                    if (storageType == 4)
                    {
                        string fileName = (string)sqlReader.GetSqlString(0) + ".zip.enc";

                        int origionalFileSize = (int)sqlReader.GetSqlInt64(3);


                        MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                        const int length = 4096;
                        byte[] fileBlob = new byte[length];
                        int startPoint = 0;

                        long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);

                        sqlDataStream.Write(fileBlob, 0, (int)retval);

                        while (retval == length)
                        {
                            startPoint += length;
                            retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                            sqlDataStream.Write(fileBlob, 0, (int)retval);
                        }
                        sqlReader.Close();
                        string fileNameWithPath = GetFileNameWithPath(location, fileName);
                        sqlDataStream.Seek(0, SeekOrigin.Begin);

                        using (BinaryWriter binWriter = new BinaryWriter(File.Open(fileNameWithPath, FileMode.Create)))
                        {
                            binWriter.Write(sqlDataStream.ToArray());
                        }
                    }
                    else
                    {
                        sqlReader.Close();
                        pipe.Send("File is not encrypted and compressed");
                    }
                }
        }
    }

    /// <summary>
    /// Retrieves file from database output to recordset
    /// decrypts and decompresses file if needed.
    /// </summary>
    /// <param name="fileId">file id</param>
    public static void ExtractFileToRecord(long fileId)
    {
        SqlPipe pipe = SqlContext.Pipe;

        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) {CommandType = CommandType.StoredProcedure};
            try
            {
                sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
                sqlCmd.Parameters[0].Value = fileId;
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {
                    sqlReader.Read();
                    string fileName = (string) sqlReader.GetSqlString(0);
 
                    int origionalFileSize = (int) sqlReader.GetSqlInt64(3);
                    int storageType = sqlReader.GetByte(8);

                    MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                    const int length = 4096;
                    byte[] fileBlob = new byte[length];
                    int startPoint = 0;
                    
                    long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                    
                    string password = (string)sqlReader.GetSqlString(9);
                    sqlDataStream.Write(fileBlob, 0, (int) retval);

                    while (retval == length)
                    {
                        startPoint += length;
                        retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                        sqlDataStream.Write(fileBlob, 0, (int) retval);
                    }
                    sqlReader.Close();
                    sqlDataStream.Seek(0, SeekOrigin.End);

                    SqlMetaData fileNameInfo = new SqlMetaData("OrigionalFileName", SqlDbType.NVarChar, 255);
                    SqlMetaData fileBlobInfo = new SqlMetaData("FileData", SqlDbType.Image);

                    // Create a new record with the column metadata.      
                    SqlDataRecord record = new SqlDataRecord(new[]
                                                                 {
                                                                     fileNameInfo,
                                                                     fileBlobInfo,
                                                                 });
                    // Set the record fields.
                    record.SetString(0, fileName);

                    //if it is encrypted decrypt it.
                    switch (storageType)
                    {
                        case 2:
                            {
                                if (password != "")
                                    record.SetBytes(1, 0, DecryptStream(sqlDataStream.ToArray(), password), 0,
                                                    origionalFileSize);
                                else
                                    pipe.Send("Failed to retreve pass phrase file cannot be decrypted");
                            }
                            break;
                        case 3:
                            {
                                record.SetBytes(1, 0, ZipDecompress(sqlDataStream.ToArray(), origionalFileSize), 0,
                                                origionalFileSize);
                            }
                            break;
                        case 4:
                            {
                                if (password != "")
                                    record.SetBytes(1, 0,
                                                    ZipDecompress(DecryptStream(sqlDataStream.ToArray(), password),
                                                                  origionalFileSize), 0, origionalFileSize);
                                else
                                    pipe.Send("Failed to retreve pass phrase file cannot be decrypted");
                            }
                            break;
                        default:
                            record.SetBytes(1, 0, sqlDataStream.GetBuffer(), 0, (int) sqlDataStream.Position);
                            break;
                    }
                    pipe.Send(record);
                }
        }
    }

    /// <summary>
    /// Retrieves file from database as sql recordset in raw encrypted form
    /// </summary>
    /// <param name="fileId">file id</param>
    public static void ExtractFileToRecordEncrypted(long fileId)
    {
        SqlPipe pipe = SqlContext.Pipe;
        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) { CommandType = CommandType.StoredProcedure };
            sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
            sqlCmd.Parameters[0].Value = fileId;
            try
            {
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {
                    sqlReader.Read();
                    string fileName = (string)sqlReader.GetSqlString(0);

                    int origionalFileSize = (int)sqlReader.GetSqlInt64(3);
                    int storageType = sqlReader.GetByte(8);

                    MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                    const int length = 4096;
                    byte[] fileBlob = new byte[length];
                    int startPoint = 0;

                    long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);

                    sqlDataStream.Write(fileBlob, 0, (int)retval);

                    while (retval == length)
                    {
                        startPoint += length;
                        retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                        sqlDataStream.Write(fileBlob, 0, (int)retval);
                    }
                    sqlReader.Close();
                    sqlDataStream.Seek(0, SeekOrigin.End);

                    SqlMetaData fileNameInfo = new SqlMetaData("OrigionalFileName", SqlDbType.NVarChar, 255);
                    SqlMetaData fileBlobInfo = new SqlMetaData("FileData", SqlDbType.Image);

                    // Create a new record with the column metadata.      
                    SqlDataRecord record = new SqlDataRecord(new[]
                                                                 {
                                                                     fileNameInfo,
                                                                     fileBlobInfo,
                                                                 });
                    // Set the record fields.
                    record.SetString(0, fileName);

                    //if it is encrypted decrypt it.
                    if (storageType == 2)
                    {
                        record.SetBytes(1, 0, sqlDataStream.GetBuffer(), 0, (int)sqlDataStream.Position);
                        pipe.Send(record);
                    }
                    else
                    {
                        pipe.Send("File not encrypted");
                    }
                }
        }
    }

    /// <summary>
    /// Retrieves file from database as sql recordset in raw compressed form
    /// </summary>
    /// <param name="fileId">file id</param>
    public static void ExtractFileToRecordCompressed(long fileId)
    {
        SqlPipe pipe = SqlContext.Pipe;
        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) { CommandType = CommandType.StoredProcedure };
            sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
            sqlCmd.Parameters[0].Value = fileId;
            try
            {
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {
                    sqlReader.Read();
                    string fileName = (string)sqlReader.GetSqlString(0);

                    int origionalFileSize = (int)sqlReader.GetSqlInt64(3);
                    int storageType = sqlReader.GetByte(8);

                    MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                    const int length = 4096;
                    byte[] fileBlob = new byte[length];
                    int startPoint = 0;

                    long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);

                    sqlDataStream.Write(fileBlob, 0, (int)retval);

                    while (retval == length)
                    {
                        startPoint += length;
                        retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                        sqlDataStream.Write(fileBlob, 0, (int)retval);
                    }
                    sqlReader.Close();
                    sqlDataStream.Seek(0, SeekOrigin.End);

                    SqlMetaData fileNameInfo = new SqlMetaData("OrigionalFileName", SqlDbType.NVarChar, 255);
                    SqlMetaData fileBlobInfo = new SqlMetaData("FileData", SqlDbType.Image);

                    // Create a new record with the column metadata.      
                    SqlDataRecord record = new SqlDataRecord(new[]
                                                                 {
                                                                     fileNameInfo,
                                                                     fileBlobInfo,
                                                                 });
                    // Set the record fields.
                    record.SetString(0, fileName);

                    //if it is encrypted decrypt it.
                    if (storageType == 3)
                    {
                        record.SetBytes(1, 0, sqlDataStream.GetBuffer(), 0, (int)sqlDataStream.Position);
                        pipe.Send(record);
                    }
                    else
                    {
                        pipe.Send("File not compressed");
                    }
                }
        }
    }

    /// <summary>
    /// Retrieves file from database as sql recordset in raw encrypted and compressed form
    /// </summary>
    /// <param name="fileId">file id</param>
    public static void ExtractFileToRecordEncryptedCompressed(long fileId)
    {
        SqlPipe pipe = SqlContext.Pipe;
        SqlDataReader sqlReader;
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            cn.Open();
            SqlCommand sqlCmd = new SqlCommand("RetrieveFile", cn) { CommandType = CommandType.StoredProcedure };
            sqlCmd.Parameters.Add("@Id", SqlDbType.BigInt);
            sqlCmd.Parameters[0].Value = fileId;
            try
            {
                sqlReader = sqlCmd.ExecuteReader();
            }
            catch (Exception e)
            {
                pipe.Send("Failed to retrieve data");
                pipe.Send(e.Message);
                throw;
            }

            if (sqlReader != null)
                if (sqlReader.HasRows)
                {
                    sqlReader.Read();
                    string fileName = (string)sqlReader.GetSqlString(0);

                    int origionalFileSize = (int)sqlReader.GetSqlInt64(3);
                    int storageType = sqlReader.GetByte(8);

                    MemoryStream sqlDataStream = new MemoryStream(origionalFileSize);
                    const int length = 4096;
                    byte[] fileBlob = new byte[length];
                    int startPoint = 0;

                    long retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);

                    sqlDataStream.Write(fileBlob, 0, (int)retval);

                    while (retval == length)
                    {
                        startPoint += length;
                        retval = sqlReader.GetBytes(10, startPoint, fileBlob, 0, length);
                        sqlDataStream.Write(fileBlob, 0, (int)retval);
                    }
                    sqlReader.Close();
                    sqlDataStream.Seek(0, SeekOrigin.End);

                    SqlMetaData fileNameInfo = new SqlMetaData("OrigionalFileName", SqlDbType.NVarChar, 255);
                    SqlMetaData fileBlobInfo = new SqlMetaData("FileData", SqlDbType.Image);

                    // Create a new record with the column metadata.      
                    SqlDataRecord record = new SqlDataRecord(new[]
                                                                 {
                                                                     fileNameInfo,
                                                                     fileBlobInfo,
                                                                 });
                    // Set the record fields.
                    record.SetString(0, fileName);

                    //if it is encrypted decrypt it.
                    if (storageType == 4)
                    {
                        record.SetBytes(1, 0, sqlDataStream.GetBuffer(), 0, (int)sqlDataStream.Position);
                        pipe.Send(record);
                    }
                    else
                    {
                        pipe.Send("File not encrypted and compressed");
                    }
                }
                else
                {
                    pipe.Send("Invalid FileId");
                }
        }
    }

    /// <summary>
    /// Hashes and stores a passphrase in the database for encryption/decryption
    /// </summary>
    /// <param name="password">Passphrase</param>
    public static void StorePassPhrase(string password)
    {
        SqlPipe pipe = SqlContext.Pipe;
//            pipe.Send(GetMd5Hash(password));
        using (SqlConnection cn = new SqlConnection("context connection=true"))
        {
            try
            {
                cn.Open();
                SqlCommand sqlCmd = new SqlCommand("SavePassPhrase", cn) { CommandType = CommandType.StoredProcedure };
                sqlCmd.Parameters.Add("@Password", SqlDbType.NVarChar, 255).Value = GetMd5Hash(password);
                SqlContext.Pipe.ExecuteAndSend(sqlCmd);

            }
            catch (Exception e)
            {
                pipe.Send("Failed to insert value");
                pipe.Send(e.Message);
                throw;
            }
        }
    }
}
