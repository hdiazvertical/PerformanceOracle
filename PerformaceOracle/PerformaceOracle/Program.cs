using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace OraMag {
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    class Program {
        // connection strings for the various tests
        // a base string is created and options are added
        // this uses the hr schema with the default password
        // make sure to adjust this for your environment
        //public static string base_string = "User Id=hr; Password=hr; Data Source=oramag; Enlist=false; ";
        public static string base_string = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=192.168.1.186)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl.localdomain)));User Id=saludsis;Password=SaludS15; Enlist=false; ";
        //public static string base_string = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=tcp)(HOST=181.49.7.246)(PORT=1251))(CONNECT_DATA=(SERVICE_NAME=orcl)));User Id=sisamadmin;Password=KyroN*;Enlist=false; ";
        public static string no_pool = base_string + "Pooling=false";
        public static string with_pool = base_string + "Pooling=true";
        public static string with_cache = with_pool + "; Statement Cache Size=1";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // call the connection pooling test method
            // parameter determines how many connections are created/destroyed
            ConnectionPoolTest(20);

            // call the fetch size test method
            // pass various numbers of rows to retrieve for each fetch
            FetchTest(1);
            FetchTest(10);
            FetchTest(100);
            FetchTest(1000);
            FetchTest(10000);

            // call statement caching test method
            StatementCacheTest(1000);

            // if running in debug mode from the IDE,
            // this prevents the command prompt window from closing
            // before examining the output
            Console.WriteLine("Press ENTER to end.");
            Console.ReadLine();
        }

        public static void ConnectionPoolTest(int iterations)
        {
            // used to track execution duration
            DateTime timeStart;
            DateTime timeEnd;
            double totalSeconds;

            // the connection object to use for the test
            OracleConnection con;

            // display simple prompt text
            Console.WriteLine("Beginning Connection Pool Test with {0} iterations...", iterations.ToString());

            // capture test start time for no pooling test
            timeStart = DateTime.Now;

            // loop creating a connection with no connection pooling
            // number of loops is determined by the iterations parameter
            for (int i = 0; i < iterations; i++) {
                con = new OracleConnection(no_pool);
                con.Open();
                con.Dispose();
            }

            // capture test end time for no pooling test
            timeEnd = DateTime.Now;

            // calculate total seconds for this test
            totalSeconds = timeEnd.Subtract(timeStart).TotalSeconds;

            // display time used for no pooling test
            Console.WriteLine("    No Pooling: {0} total seconds.", totalSeconds.ToString());

            // capture test start time for pooling test
            timeStart = DateTime.Now;

            // loop creating a connection with connection pooling
            // number of loops is determined by the iterations parameter
            for (int i = 0; i < iterations; i++) {
                con = new OracleConnection(with_pool);
                con.Open();
                con.Dispose();
            }

            // capture test end time for pooling test
            timeEnd = DateTime.Now;

            // calculate total seconds for this test
            totalSeconds = timeEnd.Subtract(timeStart).TotalSeconds;

            // display time used for pooling test
            Console.WriteLine("  With Pooling: {0} total seconds.", totalSeconds.ToString());
            Console.WriteLine();
        }

        public static void FetchTest(int numRows)
        {
            // used to count number of rows fetched
            int rowsFetched = 0;

            // used to track execution duration
            DateTime timeStart;
            DateTime timeEnd;
            double totalSeconds;

            // the connection object to use for the test
            OracleConnection con = new OracleConnection(with_pool);
            con.Open();

            // the command object to use for this test
            OracleCommand cmd = con.CreateCommand();
            //cmd.CommandText = "select * from fetch_test";
            cmd.CommandText = "select * from gen_mensajes";

            // the data reader to use for this test
            OracleDataReader dr = cmd.ExecuteReader();

            // set the number of rows to fetch to the value of numRows
            dr.FetchSize = cmd.RowSize * numRows;

            // display simple prompt text
            Console.WriteLine("Beginning Fetch Size Test with Row Size of {0}...", numRows.ToString());

            // capture test start time for this test
            timeStart = DateTime.Now;

            // loop through the data reader fetching numRows at a time
            while (dr.Read()) {
                rowsFetched++;
            }

            // capture test end time for this test
            timeEnd = DateTime.Now;

            // calculate total seconds for this test
            totalSeconds = timeEnd.Subtract(timeStart).TotalSeconds;

            // display time used for this test
            Console.WriteLine("  Fetch Time: {0} total seconds.", totalSeconds.ToString());
            Console.WriteLine();

            // clean up objects
            dr.Dispose();
            cmd.Dispose();
            con.Dispose();
        }

        public static void StatementCacheTest(int iterations)
        {
            // used to track execution duration
            DateTime timeStart;
            DateTime timeEnd;
            double totalSeconds;

            // the connection object to use for the test
            OracleConnection con = new OracleConnection(with_cache);
            con.Open();

            // the command object used for no caching test
            // initial test does not use statement caching
            OracleCommand cmd = new OracleCommand();
            cmd.Connection = con;
            cmd.AddToStatementCache = false;
            cmd.CommandText = "select mensaje from gen_mensajes where id = :1";

            // parameter object for the bind variable
            OracleParameter p_id = new OracleParameter();
            p_id.OracleDbType = OracleDbType.Decimal;
            p_id.Value = 1;

            // add parameter to the collection for the command object
            cmd.Parameters.Add(p_id);

            // the data reader for this test
            OracleDataReader dr;

            // display simple prompt text
            Console.WriteLine("Beginning Statement Cache Test with {0} iterations...", iterations.ToString());

            // capture test start time for no caching test
            timeStart = DateTime.Now;

            // loop creating a connection with no statement caching
            // number of loops is determined by the iterations parameter
            for (int i = 0; i < iterations; i++) {
                dr = cmd.ExecuteReader();
                dr.Read();
                dr.Dispose();
            }

            // capture test end time for no pooling test
            timeEnd = DateTime.Now;

            // calculate total seconds for this test
            totalSeconds = timeEnd.Subtract(timeStart).TotalSeconds;

            // display time used for no caching test
            Console.WriteLine("    No Statement Caching: {0} total seconds.", totalSeconds.ToString());

            // create new command object used for caching test
            cmd.Parameters.Clear();
            cmd.Dispose();
            cmd = new OracleCommand();
            cmd.Connection = con;
            cmd.AddToStatementCache = true;
            //cmd.CommandText = "select data from fetch_test where id = :1";
            cmd.CommandText = "select mensaje from gen_mensajes where id = :1";

            // add parameter to the collection for the command object
            cmd.Parameters.Add(p_id);

            // capture test start time for pooling test
            timeStart = DateTime.Now;

            // loop creating a connection with statement caching
            // number of loops is determined by the iterations parameter
            for (int i = 0; i < iterations; i++) {
                dr = cmd.ExecuteReader();
                dr.Read();
                dr.Dispose();
            }

            // capture test end time for caching test
            timeEnd = DateTime.Now;

            // calculate total seconds for this test
            totalSeconds = timeEnd.Subtract(timeStart).TotalSeconds;

            // display time used for caching test
            Console.WriteLine("  With Statement Caching: {0} total seconds.", totalSeconds.ToString());
            Console.WriteLine();

            // clean up objects
            p_id.Dispose();
            cmd.Dispose();
            con.Dispose();
        }
    }
}
