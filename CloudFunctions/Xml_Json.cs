
using System;


namespace EncompassWebServices
{
    /// <summary>
    /// Object that Json can be loaded into and easily looped through to pull out all of the values
    /// </summary>
    /// <example>
    /// <code>
    /// while (!myJsonDataSet.EOF()) {
    ///     myJsonDataSet.Item("ProductID");
    ///     myJsonDataSet.MoveNext();
    /// }
    /// // "1"
    /// // "2"
    /// </code>
    /// </example>
    public class BC_JsonDataSet
    {
        private readonly List<BC_Dictionary<string>> DataList;
        private int CurrentRecordIndex = -1;
        private const int myMaxReadCount = 2_000_000;
        private int myReadCounter;
        
    }
}
