using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.IO;

public partial class SQLServerFileSystemTools
{
    /// <summary>
    /// replacement for undocumented stored procedure
    /// returns recordset with file information.
    /// </summary>
    /// <param name="filePath">file name with path.</param>
    [SqlProcedure]
    // ReSharper disable InconsistentNaming
    public static void xp_getfiledetails(string filePath)
    // ReSharper restore InconsistentNaming
    {
        //pipe to sql server
        SqlPipe pipe = SqlContext.Pipe;

        if (File.Exists(filePath))
        {

            //try and open the requested file
            FileInfo file;
            try
            {
                file = new FileInfo(filePath);
            }
            catch (Exception e)
            {
                try { pipe.ExecuteAndSend(new SqlCommand("raiserror ('xp_getfiledetails() returned error 2, ''The system cannot find the file specified.''',16,1)")); }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                { }
                //if I don't re-throw here I get errors below
                throw (e);
            }

            //Build retrun record
            SqlMetaData alternateName = new SqlMetaData("Alternate Name", SqlDbType.NVarChar, 4000);
            SqlMetaData size = new SqlMetaData("Size", SqlDbType.BigInt);
            SqlMetaData creationDate = new SqlMetaData("Creation Date", SqlDbType.NChar, 8);
            SqlMetaData creationTime = new SqlMetaData("Creation Time", SqlDbType.NChar, 6);
            SqlMetaData lastWrittenDate = new SqlMetaData("Last Written Date", SqlDbType.NChar, 8);
            SqlMetaData lastWrittenTime = new SqlMetaData("Last Written Time", SqlDbType.NChar, 6);
            SqlMetaData lastAccessedDate = new SqlMetaData("Last Accessed Date", SqlDbType.NChar, 8);
            SqlMetaData lastAccessedTime = new SqlMetaData("Last Accessed Time", SqlDbType.NChar, 6);
            SqlMetaData attributes = new SqlMetaData("Attributes", SqlDbType.Int);

            SqlDataRecord record = new SqlDataRecord(new[] {
                alternateName,
                size,
                creationDate,
                creationTime,
                lastWrittenDate,
                lastWrittenTime,
                lastAccessedDate,
                lastAccessedTime,
                attributes});

            //try to add data to the retrun record
            try
            {
                record.SetString(0, file.Name);
                record.SetInt64(1, file.Length);
                record.SetString(2, file.CreationTime.ToString("yyyyMMdd"));
                record.SetString(3, file.CreationTime.ToString("HHmmss"));
                record.SetString(4, file.LastWriteTime.ToString("yyyyMMdd"));
                record.SetString(5, file.LastWriteTime.ToString("HHmmss"));
                record.SetString(6, file.LastAccessTime.ToString("yyyyMMdd"));
                record.SetString(7, file.LastAccessTime.ToString("HHmmss"));
                record.SetInt32(8, (int)file.Attributes);
            }
            catch (Exception)
            {
                try { pipe.ExecuteAndSend(new SqlCommand("raiserror ('xp_getfiledetails() returned error 2, ''The system cannot find the file specified.''',16,1)")); }
                // ReSharper disable EmptyGeneralCatchClause
                catch { }
                // ReSharper restore EmptyGeneralCatchClause
            }

            //send record back to sql server
            try
            {
                pipe.Send(record);
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        else
        {
            try { pipe.ExecuteAndSend(new SqlCommand("raiserror ('xp_getfiledetails() returned error 2, ''The system cannot find the file specified.''',16,1)")); }
            // ReSharper disable EmptyGeneralCatchClause
            catch { }
            // ReSharper restore EmptyGeneralCatchClause
        }
    }
}
