using System.Text;
using System.Threading.Tasks;

namespace EncompassWebServices
{
    /// <summary>
    /// Helper class to make editing the tables/fields in Encompass easier
    /// </summary>
    /// <example>
    /// <code>
    /// TableEdit myTableEdit = new TableEdit("Products", "12345abcdef", "Pioneer");
    /// myTableEdit.EditRecord("5", "Custom_TableEdit_API");
    /// myTableEdit.UpdateRecord("Sequence", "400");
    /// string results = myTableEdit.SaveRecord();
    /// </code>
    /// </example>
    public class TableEdit
    {
        /// <summary>
        /// Actions that TableEdit can take on a table
        /// </summary>
        public enum TableEditAction
        {
            Add = 1,
            Edit = 2,
            Delete = 3
        }

        public readonly string TableName;
        private readonly string APIToken;
        private readonly string EncompassSessionID;
        private readonly string EncompassID;
        public readonly BC_Dictionary<string> FieldDict;
        private TableEditAction myAction;
        private string APICommand;
        private string myEditMemo;
        private string myKeyValue;

        /// <summary>
        /// Sets the Edit memo to be stored on the Field Audit in Encompass
        /// </summary>
        public string EditMemo
        {
            get => myEditMemo;
            set => myEditMemo = BC_Fmt.SQLMemoSafe(value);
        }

        /// <summary>
        /// Returns the selected action to be taken on the table
        /// </summary>
        public TableEditAction Action
        {
            get => myAction;
        }

        /// <summary>
        /// Returns the key value of the record to be edited
        /// </summary>
        public string KeyValue
        {
            get => myKeyValue;
        }

        /// <summary>
        /// Creates a new TableEdit object that is ready to edit the table passed in. Requires an API Token associated with an API Publisher.
        /// </summary>
        public TableEdit(string TableName, string APIToken, string EncompassID, string EncompassSessionID = null)
        {
            this.TableName = TableName;
            this.APIToken = APIToken;
            this.EncompassSessionID = EncompassSessionID;
            this.EncompassID = EncompassID;
            FieldDict = new BC_Dictionary<string>();
            myEditMemo = "";
        }

        /// <summary>
        /// Tells the TableEdit object that we are going to be adding a new record. Requires APICommand that inherits TableEdit_Add_Record.
        /// </summary>
        public void AddRecord(string APICommand)
        {
            ResetTableEdit(APICommand, TableEditAction.Add, "");
        }

        /// <summary>
        /// Tells the TableEdit object that we are going to be editing an existing record. Requires APICommand that inherits TableEdit_Edit_Record.
        /// </summary>
        public void EditRecord(string APICommand, string KeyValue)
        {
            ResetTableEdit(APICommand, TableEditAction.Edit, KeyValue);
        }

        /// <summary>
        /// Tells the TableEdit object that we are going to be removing an existing record. If a record has associated sub-table records the record will not be deleted. Requires APICommand that inherits TableEdit_Delete_Record.
        /// </summary>
        public void DeleteRecord(string APICommand, string KeyValue)
        {
            ResetTableEdit(APICommand, TableEditAction.Delete, KeyValue);
        }

        /// <summary>
        /// Tells the TableEdit object to set a field to a value. Used when adding or editing a record in the table.
        /// </summary>
        public void UpdateRecord(string FieldName, string NewValue)
        {
            if (!BC_Fmt.IsNull(FieldName))
            {
                if (FieldDict.Exists(FieldName))
                {
                    FieldDict[FieldName] = NewValue;
                }
                else
                {
                    FieldDict.Add(FieldName, NewValue);
                }
            }
        }

        /// <summary>
        /// Saves the changes to the server.
        /// </summary>
        public string SaveRecord()
        {
            return SaveRecord_Async().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Saves the changes to the server, faster in an async format
        /// </summary>
        public Task<string> SaveRecord_Async()
        {
            if (BC_Fmt.IsNull(TableName))
            {
                throw new System.Exception("Invalid Table Edit: Table Name must be set to a valid string.");
            }
            if (BC_Fmt.IsNull(myAction))
            {
                throw new System.Exception("Invalid Action: Must call 'AddRecord()', 'EditRecord()', or 'DeleteRecord()' before using SaveRecord() on the TableEdit object");
            }
            if (myAction != TableEditAction.Delete && FieldDict.Count == 0)
            {
                throw new System.Exception("Invalid Table Edit: Must call 'UpdateRecord()' on at least one field before calling SaveRecord()");
            }
            if (myAction != TableEditAction.Add && BC_Fmt.IsNull(myKeyValue))
            {
                throw new System.Exception("Invalid Table Edit: Key Value must be set to a valid string.");
            }

            string[] Keys = FieldDict.Keys;
            StringBuilder jsonB = new();

            jsonB.Append("{\"EditMemo\":\"").Append(BC_Fmt.JsonSafe(myEditMemo)).Append('"');
            jsonB.Append(",\"TableName\":\"").Append(BC_Fmt.JsonSafe(TableName)).Append('"');
            if (myAction != TableEditAction.Add)
            {
                jsonB.Append(",\"KeyValue\":\"").Append(BC_Fmt.JsonSafe(myKeyValue)).Append('"');
            }
            jsonB.Append(",\"FieldDict\":{");
            for (int i = 0; i < Keys.Length; i++)
            {
                if (i != 0)
                {
                    jsonB.Append(',');
                }
                jsonB.Append('"').Append(BC_Fmt.JsonSafe(Keys[i])).Append("\":\"").Append(BC_Fmt.JsonSafe(FieldDict[Keys[i]])).Append('"');
            }
            jsonB.Append("}}");

            BC_HttpRequest myHttpRequest = new BC_HttpRequest(APIToken, APICommand, EncompassID, EncompassSessionID);
            myHttpRequest.AddRequestVariable("TableEditParameter", jsonB.ToString(), BC_HttpRequest.HTTPVerb.Post);
            return myHttpRequest.Submit_Async();
        }

        private void ResetTableEdit(string APICommand, TableEditAction Action, string KeyValue)
        {
            if (FieldDict.Count != 0)
            {
                FieldDict.Clear();
            }
            this.APICommand = APICommand;
            myKeyValue = KeyValue;
            myAction = Action;
        }
    }
}