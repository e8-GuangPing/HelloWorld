using System.Text;


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
    }
}
