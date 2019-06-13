using System;
using System.IO;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using StackExchange.Redis;
using GraphQL.Client;
using GraphQL.Common.Request;
using Newtonsoft.Json;

class RedisTest
{

    // Testing config values and database connections
    static ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("K1D-REDIS-CLST.ksg.int,password=ZECjTH9cx24ukQA");  // Redis connection handler
    static IDatabase db = redis.GetDatabase();                                                                              // Redis database connection    
    static GraphQLClient graphQL = new GraphQLClient("https://globaldeviceservice.dev.koreone/api/v1");                     // Graphql database connection
    static int numGraphQLQueries = 10;                                                                                      // Config value: Number of graphql queries to load
    static int numSQLQueries = 10;                                                                                          // Config value: Number of SQL queries to load
    static string GraphQLFolder = "TestQueriesGraphQL";                                                                     // Config value: Folder to load graphql queries from
    static string SQLFolder = "TestQueriesSQL";                                                                             // Config value: Folder to load SQL queries from
    static string dataFile = "redis_garbage.txt";                                                                           // Config value: File to load pre-generated k-v pair for redis from
    static int numExecutions = 1000;                                                                                         // Config value: Number of times to execute each query
    static string logFile = "output.txt";                                                                                   // Config value: File to duplicate logging to
    static StreamWriter logWriter = new StreamWriter(logFile);                                                              // The logging stream

    /*
     * Program entry point
     */ 
    static void Main(string[] args)
    {
        logWriter.AutoFlush = true;

        // Get access to sql
        string connectionString = "Data Source=tcp:172.30.100.116;\n;Initial Catalog=GlobalDeviceService;\nUser ID=octopus;Password=octopus";
        SqlConnection cnn = new SqlConnection(connectionString);
        cnn.Open();

        // Load the queries 
        string[] graphqlQueries = get_queries(numGraphQLQueries, GraphQLFolder);
        string[] sqlQueries = get_queries(numSQLQueries, SQLFolder);

        // Process and time the queries for Graphql
        do_logging_writeline("Beginning GraphQL Tests\n");
        for (int i = 0; i < numGraphQLQueries && !String.IsNullOrEmpty(graphqlQueries[i]); i++)
        {

            // Do logging, start timer
            string curQuery = graphqlQueries[i];
            do_logging_writeline("Current Query: ");
            do_logging_write(curQuery + "\n\n");
            MicroLibrary.MicroStopwatch timer = new MicroLibrary.MicroStopwatch();

            // Get the result into the cache
            do_graphql_query_caching(curQuery).GetAwaiter().GetResult();

            // Time the query (cached) and sum the execution times
            long cachedTime = 0;
            for (int j = 0; j < numExecutions; j++)
            {
                // Time the call and aggregate
                timer.Restart();
                do_graphql_query_caching(curQuery).GetAwaiter().GetResult();
                timer.Stop();
                cachedTime += timer.ElapsedMicroseconds;
            }

            // Cleanup
            db.KeyDelete(curQuery);

            // Time the query (uncached) and sum the execution times
            long uncachedTime = 0;
            for (int j = 0; j < numExecutions; j++)
            {

                // Time the call and aggregate
                timer.Restart();
                do_graphql_query_no_caching(curQuery).GetAwaiter().GetResult();
                timer.Stop();
                uncachedTime += timer.ElapsedMicroseconds;

            }

            do_logging_writeline("Uncached execution time (Average of " + numExecutions.ToString() + " runs): " + (uncachedTime / numExecutions).ToString() + "µs");
            do_logging_writeline("Cached execution time (Average of " + numExecutions.ToString() + " runs): " + (cachedTime / numExecutions).ToString() + "µs");
            do_logging_writeline(((double)(uncachedTime / numExecutions) / (double)(cachedTime / numExecutions)).ToString() + " times faster.\n");
            //do_logging_writeline("Size of result set: " + result.Length.ToString() + "\n");
        }

        // Process and time the queries for SQL Server
        do_logging_writeline("\nBeginning SQL Tests\n");
        for (int i = 0; i < numSQLQueries && !String.IsNullOrEmpty(sqlQueries[i]); i++)
        {

            //Do logging, start timer
            string curQuery = sqlQueries[i];
            do_logging_writeline("Current Query: ");
            do_logging_write(curQuery + "\n\n");
            MicroLibrary.MicroStopwatch timer = new MicroLibrary.MicroStopwatch();

            // Get the result into the cache
            do_sql_query_caching(curQuery, cnn).GetAwaiter().GetResult();

            // Time the query (cached) and sum the execution times
            long cachedTime = 0;
            for (int j = 0; j < numExecutions; j++)
            {
                // Time the call and aggregate
                timer.Restart();
                do_sql_query_caching(curQuery, cnn).GetAwaiter().GetResult();
                timer.Stop();
                cachedTime += timer.ElapsedMicroseconds;
            }

            // Cleanup
            db.KeyDelete(curQuery);

            // Time the query (uncached) and sum the execution times
            long uncachedTime = 0;
            for (int j = 0; j < numExecutions; j++)
            {

                // Time the call and aggregate
                timer.Restart();
                do_sql_query_no_caching(curQuery, cnn).GetAwaiter().GetResult();
                timer.Stop();
                uncachedTime += timer.ElapsedMicroseconds;
            }

            do_logging_writeline("Uncached execution time (Average of " + numExecutions.ToString() + " runs): " + (uncachedTime / numExecutions).ToString() + "µs");
            do_logging_writeline("Cached execution time (Average of " + numExecutions.ToString() + " runs): " + (cachedTime / numExecutions).ToString() + "µs");
            do_logging_writeline(((double)(uncachedTime / numExecutions) / (double)(cachedTime / numExecutions)).ToString() + " times faster.\n");
            //do_logging_writeline("Size of result set: " + result.Length.ToString() + "\n");
        }

        // cleanup
        cnn.Close();
    }

    /*
     * Logging with no appeneded newline
     */ 
    static void do_logging_write(string line)
    {
        Console.Write(line);
        logWriter.Write(line);
    }

    /*
     * Logging with an appended newline
     */ 
    static void do_logging_writeline(string line)
    {
        Console.WriteLine(line);
        logWriter.WriteLine(line);
    }

    /*
     * Load queries into a Null terminated string array. Will load as many as specified or as many as possible, whichever is less.
     * The queries should each be in their own file in a folder with only other queries.
     * @Param numGraph
     */
    static string[] get_queries(int numQueries, string queryFolder)
    {
        // Find files
        string[] queries = new string[numQueries + 1];
        string[] filePaths = Directory.GetFiles(queryFolder);

        // Process each file as one query
        int queriesAdded = 0;
        foreach (string filename in filePaths)
        {
            if (queriesAdded == numQueries)
            {
                queries[queriesAdded] = null;
                return queries;
            }

            queries[queriesAdded] = System.IO.File.ReadAllText(filename);
            queriesAdded++;
        }

        queries[queriesAdded] = null;
        return queries;
    }


    /*
     * This function will either execute an arbitrary (NON MUTATION) graphql query, or return the cached result. Cached results
     * are stored until memory limits are exceeded, and then evicted using an LRU policy.
     * @Param: query is the string representation of the graphql query you wish to execute
     */ 
    static async System.Threading.Tasks.Task<dynamic> do_graphql_query_caching(string query)
    {

        // check if the query is in Redis cache
        string key = query;
        string jsonString = db.StringGet(key);
        if (String.IsNullOrEmpty(jsonString))
        {

            // Not in redis cache, query graphql
            var graphQLResponse = await graphQL.PostQueryAsync(query);
            jsonString = graphQLResponse.Data.ToString();

            // Conditionally add to redis cache 
            if (true)//returnVal.Length <= 10000)
            {
                db.StringSetAsync(query, jsonString);
            }

            // done
            //do_logging_write(graphQLResponse.GetType().FullName);
            return graphQLResponse.Data;
        } else
        {
            // Is in redis cache, return result
            return JsonConvert.DeserializeObject<dynamic>(jsonString);
        }
    }

    /*
     * This function will either execute an arbitrary (NON MUTATION) graphql query.
     * @Param: query is the string representation of the graphql query you wish to execute
     */
    static async System.Threading.Tasks.Task<dynamic> do_graphql_query_no_caching(string query)
    {

        // Query graphql
        var request = new GraphQLRequest { Query = query };
        var graphQLResponse = await graphQL.PostAsync(request);

        // done
        return graphQLResponse;
    }

    /*
     * This function will either execute an arbitrary (NON MUTATION) SQL query, or return the cached result. Cached results
     * are stored until memory limits are exceeded, and then evicted using an LRU policy.
     * @Param: query is the string representation of the SQL query you wish to execute
     * @Param: cnn is the SQL server connection the query will be issued against
     */
    static async System.Threading.Tasks.Task<dynamic> do_sql_query_caching(string query, SqlConnection cnn)
    {
        // check if the query is in Redis cache
        string key = query;
        string returnVal = db.StringGet(key);
        if (String.IsNullOrEmpty(returnVal))
        {

            // Not in Redis cache, query SQL
            SqlCommand command = new SqlCommand(query, cnn);
            SqlDataReader reader = command.ExecuteReader();

            // Parse into json string
            returnVal = convert_reader_to_json(reader);
            reader.Close();

            // Conditionally add to redis cache
            if (true)//returnVal.Length <= 10000)
            {
                db.StringSetAsync(query, returnVal);
            }

            // done
            return returnVal;
        } else
        {

            // Is in redis cache, return result
            return returnVal;
        }
    }

    /*
 * This function will execute an arbitrary SELECT query;
 * @Param: query is the string representation of the SQL query you wish to execute
 * @Param: cnn is the SQL server connection the query will be issued against
 */
    static async System.Threading.Tasks.Task<dynamic> do_sql_query_no_caching(string query, SqlConnection cnn)
    {
        // Query SQL
        SqlCommand command = new SqlCommand(query, cnn);
        SqlDataAdapter adapter = new SqlDataAdapter(command);
        DataTable table = new DataTable();
        adapter.Fill(table);
        adapter.Dispose();

        // done
        return table;
    }

    /*
     * This function will convert the provided reader object into a json-style string.
     * Does not support container types. All types will be cast to a string and will need to be cast back by the user.
     * @Param reader: The reader that was generated when a query was executed. Will be transformed into a json string.
     */
    static string convert_reader_to_json(SqlDataReader reader)
    {
        string result = "[";
        
        if (!reader.HasRows)
        {
            return "[]";
        }

        var tableSchema = reader.GetSchemaTable();
        List<string> columnNames = new List<string>();

        // Get the column names
        foreach (DataRow row in tableSchema.Rows)
        {
            //do_logging_writeline(row["ColumnName"].ToString());
            columnNames.Add(row["ColumnName"].ToString());
        }

        List<int> ordinals = new List<int>();
        if (reader.HasRows)
        {
            foreach (string col in columnNames)
            {
                ordinals.Add(reader.GetOrdinal(col));
            }
        }

        while (reader.Read())
        {
            result = $"{result}{{\n";
            for (int i = 0; i < columnNames.Count; i++)
            {
                result = $"{result}\t\"{columnNames.ElementAt(i)}\": \"{reader.GetValue(ordinals.ElementAt(i)).ToString()}\"";
                if (i == columnNames.Count - 1)
                {
                    result = $"{result}\n";
                } else
                {
                    result = $"{result},\n";
                }
            }
            result = $"{result}}}, ";
        }
        result = result.Substring(0, result.Length - 2);
        result = $"{result}]";

        return result;
    }

    /*
     * This function generates a random alphanumeric string of a given length.
     * @Param length: The length of the random string to generate
     */ 
    static string random_string(int length)
    {

        // Setup const string and building string
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] stringChars = new char[length];
        var random = new Random();

        // Generate the string
        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}