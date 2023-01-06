using System.Text;
using System.Threading.Tasks;

namespace EncompassWebServices
{
    /// <summary>
    /// Helper class to make viewing the tables/fields in Encompass easier
    /// </summary>
    public class TableView
    {
        /// <summary>
        /// The valid formats TableView can return.
        /// </summary>
        public enum TableViewFormat
        {
            JSON = 0,
            HTML = 1,
            XML = 2,
            CSV = 3,
            Excel = 4,
            PDF = 5,
            WebQuery = 6,
            Embed = 7,
            SDK = 8
        }

        private struct TableViewFilter
        {
            internal readonly string Name;
            internal readonly string Value;
            internal readonly BC_Fmt.Operators Operator;

            internal TableViewFilter(string Name, string Value, BC_Fmt.Operators Operator)
            {
                this.Name = Name;
                this.Value = Value;
                this.Operator = Operator;
            }
        }

        private readonly string APIToken;
        private readonly string EncompassSessionID;
        private readonly string APICommand;
        private readonly string EncompassID;
        private int myMaxRecords;
        private int mySubMaxRecords;
        public int StartRecordCount;
        public string CurValue;
        public TableViewFormat Format;
        private readonly BC_List<string> Columns;
        private readonly BC_List<string> SelectSort;
        private readonly BC_List<string> SubTables;
        private readonly BC_List<TableViewFilter> Filters;
        private readonly BC_Dictionary<BC_List<string>> SelectDisplayInSubs;

        /// <summary>
        /// The maximum number of records to return from the server. This number Can not be higher than 5000.
        /// </summary>
        public int MaxRecords
        {
            get => myMaxRecords;
            set
            {
                if (value > 5000 || value <= 0)
                {
                    throw new System.Exception("Invalid MaxRecords. The number of records that can be returned from TableView must be between 1 and 5000.");
                }
                else
                {
                    myMaxRecords = value;
                }
            }
        }

        /// <summary>
        /// The maximum number of records to return from the server for each sub-table under the parent records. This number Can not be higher than 1000.
        /// </summary>
        public int SubMaxRecords
        {
            get => mySubMaxRecords;
            set
            {
                if (value > 1000 || value <= 0)
                {
                    throw new System.Exception("Invalid mySubMaxRecords. The number of records that can be returned from TableView must be between 1 and 1000.");
                }
                else
                {
                    mySubMaxRecords = value;
                }
            }
        }

        /// <summary>
        /// Creates a new EC_TableView object. Requires an API Token associated with an API Publisher, and an API Command that inherits TableView.
        /// </summary>
        public TableView(string APIToken, string APICommand, string EncompassID, string EncompassSessionID = null)
        {
            this.APIToken = APIToken;
            this.EncompassSessionID = EncompassSessionID;
            this.APICommand = APICommand;
            this.EncompassID = EncompassID;
            myMaxRecords = 0;
            mySubMaxRecords = 0;
            CurValue = "";
            StartRecordCount = 0;
            Format = TableViewFormat.JSON;
            Columns = new BC_List<string>();
            SubTables = new BC_List<string>();
            SelectDisplayInSubs = new BC_Dictionary<BC_List<string>>();
            SelectSort = new BC_List<string>();
            Filters = new BC_List<TableViewFilter>();
        }

        /// <summary>
        /// Adds a column to the TableView. If this is never called, then the columns returned will be the table's default display columns. If it is called AT LEAST ONCE, then only the columns specified will be returned.
        /// </summary>
        public void AddColumn(string Column)
        {
            if (!Columns.Exists(Column))
            {
                Columns.Add(Column);
            }
            else
            {
                throw new System.Exception("Column '" + Column + "' has already been added to this TableView object, Can not add the same column twice.");
            }
        }

        /// <summary>
        /// Adds this sub-table to the TableView (this must be a valid TableJoinID).
        /// </summary>
        public void AddSubTableJoinID(string TableJoinID)
        {
            if (!SubTables.Exists(TableJoinID))
            {
                SubTables.Add(TableJoinID);
            }
            else
            {
                throw new System.Exception("TableJoinID '" + TableJoinID + "' has already been added to this EC_TableView object, Can not add the same sub-table twice.");
            }
        }

        /// <summary>
        /// Adds a column to the specified sub-table. If this is never called, then the columns returned will be the sub-table's default display columns. If it is called AT LEAST ONCE, then only the columns specified will be returned.
        /// </summary>
        public void AddSubTableColumn(string TableJoinID, string Column)
        {
            if (SubTables.Exists(TableJoinID))
            {
                SelectDisplayInSubs.AddIfNotExists(TableJoinID, new BC_List<string>());
                if (!SelectDisplayInSubs[TableJoinID].Exists(Column))
                {
                    SelectDisplayInSubs[TableJoinID].Add(Column);
                }
                else
                {
                    throw new System.Exception("Column '" + Column + "' has already been added to this TableView object for TableJoinID '" + TableJoinID + "', Can not add the same sub-table column twice.");
                }
            }
            else
            {
                throw new System.Exception("SubTableJoinID: '" + TableJoinID + "' must be added via AddSubTableJoinID() before calling AddSubTableColumn()");
            }
        }

        /// <summary>
        /// Adds a sorting option to the records that are returned. If this is not called, the records will come back in the default sort order.
        /// </summary>
        public void AddSelectSort(string Column, bool Descending)
        {
            if (SelectSort.Exists(Column) || SelectSort.Exists(Column + ".Desc"))
            {
                throw new System.Exception("Column: '" + Column + "' has already been added to this TableView object, Can not sort by the same field more than once.");
            }
            else
            {
                if (Descending)
                {
                    Column += ".Desc";
                }
                SelectSort.AddIfNotExists(Column);
            }
        }

        /// <summary>
        /// Adds a filter on the records to be returned.
        /// </summary>
        public void AddFilter(string Column, string Value, BC_Fmt.Operators Operator)
        {
            for (int i = 0; i < Filters.Count; i++)
            {
                if (Filters[i].Name == Column)
                {
                    throw new System.Exception("Filter for '" + Column + "' has already been added to this EC_TableView object, Can not filter on the same column twice.");
                }
            }
            Filters.Add(new TableViewFilter(Column, Value, Operator));
        }

        /// <summary>
        /// Sends your TableView request to the server and returns the results.
        /// </summary>
        public string GetResults()
        {
            return GetResults_Async().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends your TableView request to the server and returns the results, runs faster in an async format
        /// </summary>
        public Task<string> GetResults_Async()
        {
            BC_HttpRequest myHttpRequest = new BC_HttpRequest(APIToken, APICommand, EncompassID, EncompassSessionID);

            // TableName must be a static parameter on the API Command for security
            myHttpRequest.AddRequestVariable("Format", Format.ToString());

            if (myMaxRecords > 0)
            {
                myHttpRequest.AddRequestVariable("SelectMaxRecords", myMaxRecords.ToString());
            }

            if (mySubMaxRecords > 0)
            {
                myHttpRequest.AddRequestVariable("SubSelectMaxRecords", mySubMaxRecords.ToString());
            }

            if (!BC_Fmt.IsNull(CurValue) && Format == TableViewFormat.HTML)
            {
                myHttpRequest.AddRequestVariable("CurValue", CurValue);
            }

            if (StartRecordCount > 0)
            {
                myHttpRequest.AddRequestVariable("StartRecordCount", StartRecordCount.ToString());
            }

            if (Columns.Count > 0)
            {
                myHttpRequest.AddRequestVariable("SelectDisplayInParent", string.Join(",", Columns.ToArray()));
            }

            if (SubTables.Count > 0)
            {
                myHttpRequest.AddRequestVariable("SubTableJoinID", string.Join(",", SubTables.ToArray()));
            }

            if (SelectDisplayInSubs.Count > 0)
            {
                StringBuilder SelectDisplayInSubsB = new();
                string[] SubTablesJoinIDs = SelectDisplayInSubs.Keys;
                for (int i = 0; i < SubTablesJoinIDs.Length; i++)
                {
                    if (i != 0)
                    {
                        SelectDisplayInSubsB.Append(',');
                    }
                    SelectDisplayInSubsB.Append(SubTablesJoinIDs[i]);
                    for (int j = 0; j < SelectDisplayInSubs[i].Count; j++)
                    {
                        SelectDisplayInSubsB.Append('|');
                        SelectDisplayInSubsB.Append(SelectDisplayInSubs[i][j]);
                    }
                }
                myHttpRequest.AddRequestVariable("SelectDisplayInSubs", SelectDisplayInSubsB.ToString());
            }

            // Task 946842: Cloud Functions SDK: TableView with using AddSelectSort doesn't add the SelectSort Request Variable to the request when calling GetResults.
            if (SelectSort.Count > 0)
            {
                myHttpRequest.AddRequestVariable("SelectSort", string.Join(",", SelectSort.ToArray()));
            }

            if (Filters.Count > 0)
            {
                for (int i = 0; i < Filters.Count; i++)
                {
                    myHttpRequest.AddParameter(Filters[i].Name, Filters[i].Value, Filters[i].Operator);
                }
            }

            return myHttpRequest.Submit_Async();
        }

        /// <summary>
        /// Returns a JSON Data Set object of the table results.
        /// </summary>
        public BC_JsonDataSet GetJsonDataSet()
        {
            return GetJsonDataSet_Async().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns a JSON Data Set object of the table results, runs faster in an async format
        /// </summary>
        async public Task<BC_JsonDataSet> GetJsonDataSet_Async()
        {
            return new BC_JsonDataSet(await GetResults_Async(), "", "/Export/Table/Row");
        }

        /// <summary>
        /// Returns a string containing all of the HTML for the current TableView
        /// </summary>
        public string GetTableViewHTML()
        {
            return GetTableViewHTML_Async().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Returns a string containing all of the HTML for the current TableView, faster in an async format
        /// </summary>
        public Task<string> GetTableViewHTML_Async()
        {
            Format = TableViewFormat.SDK;
            return GetResults_Async();
        }
    }
}