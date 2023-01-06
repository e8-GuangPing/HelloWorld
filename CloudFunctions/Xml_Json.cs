
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace EncompassWebServices
{
    /// <summary>
    /// Object that Json can be loaded into and easily looped through to pull out all of the values
    /// </summary>
    /// <example>
    /// <code>
    /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
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

        /// <summary>
        /// Constructor that creates a new JsonDataSet, must then be populated manually
        /// </summary>
        public BC_JsonDataSet()
        {
            DataList = new List<BC_Dictionary<string>>();
        }

        /// <summary>
        /// Constructor that creates a new JsonDataSet, and populates it from the Json passed in
        /// </summary>
        public BC_JsonDataSet(string Json, string RootName = "", string DataXPath = "")
        {
            DataList = new List<BC_Dictionary<string>>();
            System.Xml.XmlDocument myXML = BC_Xml_Json.JsonToXml(Json, RootName);
            SetupDataSet(myXML, RootName, DataXPath);
        }

        /// <summary>
        /// Constructor that creates a new JsonDataSet, and populates it from the Xml passed in
        /// </summary>
        public BC_JsonDataSet(System.Xml.XmlDocument XmlDoc, string RootName = "", string DataXPath = "")
        {
            DataList = new List<BC_Dictionary<string>>();
            SetupDataSet(XmlDoc, RootName, DataXPath);
        }

        private void SetupDataSet(System.Xml.XmlDocument XmlDoc, string RootName, string DataXPath)
        {
            // Task 648988: Cloud Function: GetJsonDataSet() does not return data correctly
            if (DataXPath.Length == 0)
            {
                if (RootName.Length == 0)
                {
                    DataXPath = "/RootName";
                }
                else
                {
                    DataXPath = "/";
                }
            }

            if (XmlDoc != null)
            {
                foreach (System.Xml.XmlNode row in XmlDoc.SelectNodes(DataXPath))
                {
                    if (row.ChildNodes.Count > 0)
                    {
                        AddRow();
                        foreach (System.Xml.XmlNode field in row.ChildNodes)
                        {
                            AddColumn(field.Name, field.InnerText);
                        }
                    }
                }
                MoveFirst();
            }
        }

        /// <summary>
        /// Adds a new row, empty to the data set, and sets the index to this row
        /// Used for manual populating
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.AddRow();
        /// </code>
        /// </example>
        public void AddRow()
        {
            DataList.Add(new BC_Dictionary<string>());
            CurrentRecordIndex = DataList.Count - 1;
        }

        /// <summary>
        /// Adds a new column and value to the current selected row in the data set
        /// Used for manual populating
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.AddRow();
        /// myJsonDataSet.AddColumn("ProductID", "1");
        /// </code>
        /// </example>
        public void AddColumn(string Name, object Value)
        {
            DataList[CurrentRecordIndex].Add(Name, BC_Fmt.CStr(Value));
        }

        /// <summary>
        /// Returns an array of the column/key names for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.Keys();
        /// // { "ProductID", "ProductName" }
        /// </code>
        /// </example>
        public string[] Keys()
        {
            return DataList[CurrentRecordIndex].Keys;
        }

        /// <summary>
        /// Determines if a column name exists for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.ColumnExists("ProductID");
        /// // true
        /// </code>
        /// </example>
        public bool ColumnExists(string Name)
        {
            return DataList[CurrentRecordIndex].Exists(Name);
        }

        /// <summary>
        /// Returns the number of columns in the data set
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.ColumnCount();
        /// // 2
        /// </code>
        /// </example>
        public int ColumnCount()
        {
            return DataList[CurrentRecordIndex].Count;
        }

        /// <summary>
        /// Returns the number of rows in the data set
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.RecordCount();
        /// // 10
        /// </code>
        /// </example>
        public int RecordCount()
        {
            return DataList.Count;
        }

        /// <summary>
        /// Returns the number of columns in the data set
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.GetName(0);
        /// // "ProductID"
        /// </code>
        /// </example>
        public string GetName(int Index)
        {
            if (Index >= 0 && Index < DataList[CurrentRecordIndex].Count)
            {
                return DataList[CurrentRecordIndex].Keys[Index];
            }
            else
            {
                throw new Exception("JsonDataSet.GetName(): Column " + Index + " wasn't found in the data view");
            }
        }

        /// <summary>
        /// Gets the current row index
        /// </summary>
        /// <example>
        /// <code>
        /// myJsonDataSet.Position;
        /// // 1
        /// </code>
        /// </example>
        public int Position
        {
            get
            {
                return CurrentRecordIndex;
            }
        }

        /// <summary>
        /// Moves the row selection to the next row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.MoveNext(); // HERE
        /// }
        /// </code>
        /// </example>
        public void MoveNext()
        {
            CurrentRecordIndex++;
            myReadCounter = 0;
        }

        /// <summary>
        /// Moves the row selection to the previous row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// myJsonDataSet.MoveNext();
        /// myJsonDataSet.MovePrevious(); // back at the first row
        /// </code>
        /// </example>
        public void MovePrevious()
        {
            CurrentRecordIndex--;
            myReadCounter = 0;
        }

        /// <summary>
        /// Moves the row selection to the first row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.MoveNext();
        /// }
        /// myJsonDataSet.MoveFirst(); // HERE
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.MoveNext();
        /// }
        /// </code>
        /// </example>
        public void MoveFirst()
        {
            CurrentRecordIndex = 0;
            myReadCounter = 0;
        }

        /// <summary>
        /// Determines if you are at the end of the rows
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) { // HERE
        ///     myJsonDataSet.MoveNext();
        /// }
        /// </code>
        /// </example>
        public bool EOF()
        {
            return DataList.Count == 0 || CurrentRecordIndex >= DataList.Count;
        }

        /// <summary>
        /// Determines if you are at the first row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.MoveNext();
        /// }
        /// while (!myJsonDataSet.BOF()) { // HERE
        ///     myJsonDataSet.MovePrevious();
        /// }
        /// </code>
        /// </example>
        public bool BOF()
        {
            return CurrentRecordIndex <= 0;
        }

        private object GetValue(string Name)
        {
            if (DataList[CurrentRecordIndex].Exists(Name))
            {
                return DataList[CurrentRecordIndex][Name];
            }
            else
            {
                return null;
            }
        }

        private object GetValue(int Index)
        {
            if (myReadCounter >= myMaxReadCount)
            {
                throw new Exception("Infinite loop detected: Read Counter has Exceeded the Max Allowed Value of " + myMaxReadCount + " reads in a single row.");
            }
            myReadCounter++;

            if (DataList[CurrentRecordIndex].Exists(Index))
            {
                return DataList[CurrentRecordIndex][Index];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieves the element name at the specified index
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// myJsonDataSet.Name(1);
        /// // "ProductName"
        /// </code>
        /// </example>
        public string Name(int Index)
        {
            string myName = "";
            if (DataList[0].Exists(Index))
            {
                myName = DataList[0].Keys[Index];
            }

            return myName;
        }

        /// <summary>
        /// Retrieves the element value as a string for the element name passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.Item("ProductID");
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // "1"
        /// // "2"
        /// </code>
        /// </example>
        public string Item(string Name)
        {
            return BC_Fmt.CStr(GetValue(Name));
        }

        /// <summary>
        /// Retrieves the element value as a string for the element index passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.Item(2);
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // "Widget1"
        /// // "Widget2"
        /// </code>
        /// </example>
        public string Item(int Index)
        {
            return BC_Fmt.CStr(GetValue(Index));
        }

        /// <summary>
        /// Retrieves the element value as a decimal for the element name passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemDec("ProductID");
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // 1
        /// // 2
        /// </code>
        /// </example>
        public decimal ItemDec(string Name)
        {
            return BC_Fmt.Null2Zero(GetValue(Name));
        }

        /// <summary>
        /// Retrieves the element value as a decimal for the element index passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemDec(1);
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // 1
        /// // 2
        /// </code>
        /// </example>
        public decimal ItemDec(int Index)
        {
            return BC_Fmt.Null2Zero(GetValue(Index));
        }

        /// <summary>
        /// Retrieves the element value as an integer for the element name passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemInt("ProductID");
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // 1
        /// // 2
        /// </code>
        /// </example>
        public int ItemInt(string Name)
        {
            return BC_Fmt.Null2ZeroInt(GetValue(Name));
        }

        /// <summary>
        /// Retrieves the element value as an integer for the element index passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemInt(1);
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // 1
        /// // 2
        /// </code>
        /// </example>
        public int ItemInt(int Index)
        {
            return BC_Fmt.Null2ZeroInt(GetValue(Index));
        }

        /// <summary>
        /// Retrieves the element value as a long for the element name passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemLong("ProductID");
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // 1
        /// // 2
        /// </code>
        /// </example>
        public long ItemLong(string Name)
        {
            return System.Convert.ToInt64(GetValue(Name));
        }

        /// <summary>
        /// Retrieves the element value as a long for the element index passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"ProductName\":\"Widget1\"},{\"ProductID\":2,\"ProductName\":\"Widget2\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemLong(1);
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // 1
        /// // 2
        /// </code>
        /// </example>
        public long ItemLong(int Index)
        {
            return System.Convert.ToInt64(GetValue(Index));
        }

        /// <summary>
        /// Retrieves the element value as a bool for the element name passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"Active\":true},{\"ProductID\":2,\"Active\":false}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemBool("Active");
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // true
        /// // false
        /// </code>
        /// </example>
        public bool ItemBool(string Name)
        {
            return BC_Fmt.CBool(GetValue(Name));
        }

        /// <summary>
        /// Retrieves the element value as a bool for the element index passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"Active\":true},{\"ProductID\":2,\"Active\":false}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemBool(2);
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // true
        /// // false
        /// </code>
        /// </example>
        public bool ItemBool(int Index)
        {
            return BC_Fmt.CBool(GetValue(Index));
        }

        /// <summary>
        /// Retrieves the element value as a DateTime for the element name passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"StartDate\":\"2000-01-01\"},{\"ProductID\":2,\"StartDate\":\"2000-01-02\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemDate("StartDate");
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // DateTime object = 2000-01-01
        /// // DateTime object = 2000-01-02
        /// </code>
        /// </example>
        public DateTime ItemDate(string Name)
        {
            return BC_Fmt.CDate(GetValue(Name));
        }

        /// <summary>
        /// Retrieves the element value as a DateTime for the element index passed in, for the current row
        /// </summary>
        /// <example>
        /// <code>
        /// BC_JsonDataSet myJsonDataSet = new myJsonDataSet("[{\"ProductID\":1,\"StartDate\":\"2000-01-01\"},{\"ProductID\":2,\"StartDate\":\"2000-01-02\"}]");
        /// while (!myJsonDataSet.EOF()) {
        ///     myJsonDataSet.ItemDate(2);
        ///     myJsonDataSet.MoveNext();
        /// }
        /// // DateTime object = 2000-01-01
        /// // DateTime object = 2000-01-02
        /// </code>
        /// </example>
        public DateTime ItemDate(int Index)
        {
            return BC_Fmt.CDate(GetValue(Index));
        }
    }

    /// <summary>
    /// Library of functions to make dealing with XML/Json files easier in C#
    /// </summary>
    public static class BC_Xml_Json
    {
        /// <summary>
        /// Gets the text value out of an XML Node or node path
        /// Safely handles if the Node is null, or the path is invalid by returning an empty string
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.XmlValue(myXmlNode, "Product/Price/FullPrice");
        /// // 25.50
        /// </code>
        /// </example>
        public static string XmlValue(System.Xml.XmlNode Data, string Node)
        {
            if (Data == null || Node == null || Data.SelectNodes(Node) == null || Data.SelectNodes(Node)[0] == null)
            {
                return "";
            }
            else
            {
                return Data.SelectNodes(Node)[0].InnerText;
            }
        }

        /// <summary>
        /// Gets the date value out of an XML Node or node path
        /// Safely handles if the Node is null, or the path is invalid, or if the value is not a valid date/time by returning the BC_Fmt.DefaultDate instead
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.XmlDateValue(myXmlNode, "Product/Price/StartDate");
        /// // DateTime(2019, 1, 1)
        /// </code>
        /// </example>
        public static DateTime XmlDateValue(ref System.Xml.XmlNode Data, string Node)
        {
            DateTime ValueDate = BC_Fmt.DefaultDate;
            if (Data.SelectNodes(Node)[0] != null)
            {
                string DateStr = Data.SelectNodes(Node)[0].InnerText;
                if (DateStr.Length == 8)
                {
                    DateStr = BC_Fmt.Left(DateStr, 4) + "-" + BC_Fmt.Right(BC_Fmt.Left(DateStr, 6), 2) + "-" + BC_Fmt.Right(DateStr, 2);
                }

                if (!BC_Fmt.IsDate(DateStr))
                {
                    DateStr = Data[Node].InnerText;
                }

                if (DateStr.Length == 8)
                {
                    DateStr = BC_Fmt.Left(DateStr, 4) + "-" + BC_Fmt.Right(BC_Fmt.Left(DateStr, 6), 2) + "-" + BC_Fmt.Right(DateStr, 2);
                }

                ValueDate = BC_Fmt.DateIfNull(DateStr, BC_Fmt.DefaultDate);
            }

            return ValueDate;
        }

        /// <summary>
        /// Gets the attribute value from an XML Node
        /// Safely handles the situation where the attribute does not exist by returning an empty string instead
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.XmlDateValue(myXmlNode, "testAttribute");
        /// // "sample"
        /// </code>
        /// </example>
        public static string AttrValue(System.Xml.XmlNode Node, string Name)
        {
            if (Node.Attributes[Name] != null)
            {
                return Node.Attributes[Name].Value;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Creates a new XML Node with a string of Value.ToString() as it's contents, and defaults the attributes (option)
        /// Safely handles a null Value object by using an empty string instead
        /// </summary>
        /// <example>
        /// <code>
        /// XmlAttribute newAttr = BC_Xml_Json.CreateAttribute("type", "beverage", ref myXmlDoc);
        /// BC_Xml_Json.CreateElement("ProductID", "123", ref myXmlDoc, newAttr);
        /// </code>
        /// </example>
        public static System.Xml.XmlNode CreateElement(string Name, object Value, ref System.Xml.XmlDocument XmlData, params System.Xml.XmlNode[] Attributes)
        {
            System.Xml.XmlNode Node = XmlData.CreateNode(System.Xml.XmlNodeType.Element, Name, null);

            if (Value != null && Value.ToString().Length != 0)
            {
                Node.InnerText = Value.ToString();
            }

            if (Attributes != null && Attributes.Length != 0)
            {
                for (int i = 0; i < Attributes.Length; i++)
                {
                    Node.Attributes.SetNamedItem(Attributes[i]);
                }
            }

            return Node;
        }

        /// <summary>
        /// Creates a new XML Attribute
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.CreateAttribute("ProductID", "123", ref myXmlDoc);
        /// </code>
        /// </example>
        public static System.Xml.XmlNode CreateAttribute(string Name, object Value, ref System.Xml.XmlDocument XmlData)
        {
            System.Xml.XmlNode Attribute = XmlData.CreateNode(System.Xml.XmlNodeType.Attribute, Name, null);
            Attribute.Value = Value.ToString();

            return Attribute;
        }

        /// <summary>
        /// Appends a new child node to a parent node
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.AppendChild(ref myParentNode, myChildNode);
        /// </code>
        /// </example>
        public static System.Xml.XmlNode AppendChild(ref System.Xml.XmlNode Parent, System.Xml.XmlNode Child)
        {
            return Parent.AppendChild(Child);
        }

        /// <summary>
        /// Appends a set of XML nodes to a parent node
        /// </summary>
        /// <example>
        /// <code>
        /// System.Xml.XmlNodeList myChildNodes = ...;
        /// BC_Xml_Json.AppendChild(ref myParentNode, myChildNodes);
        /// </code>
        /// </example>
        public static void AppendData(ref System.Xml.XmlNode Destination, System.Xml.XmlNodeList Source)
        {
            foreach (System.Xml.XmlNode Node in Source)
            {
                Destination.AppendChild(Destination.OwnerDocument.ImportNode(Node, true));
            }
        }

        public static void ReplaceData(ref System.Xml.XmlNode Destination, System.Xml.XmlNodeList Source)
        {
            System.Xml.XmlDocument Data = Destination.OwnerDocument;
            System.Xml.XmlNode Parent = Destination.ParentNode;
            System.Xml.XmlNode LastNode = null;

            foreach (System.Xml.XmlNode Node in Source)
            {
                System.Xml.XmlNode WorkingNode = Data.ImportNode(Node, true);

                if (LastNode != null)
                {
                    Parent.InsertAfter(WorkingNode, LastNode);
                }
                else
                {
                    Parent.ReplaceChild(WorkingNode, Destination);
                }

                LastNode = WorkingNode;
            }
        }

        /// <summary>
        /// Replaces one node in the XML document with another
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.ReplaceNode(ref myNode1, ref myNode2);
        /// </code>
        /// </example>
        public static void ReplaceNode(ref System.Xml.XmlNode Destination, ref System.Xml.XmlDocument Source)
        {
            if (Source != null && Source.DocumentElement != null && Source.DocumentElement.ChildNodes.Count > 0)
            {
                ReplaceData(ref Destination, Source.DocumentElement.ChildNodes);
            }
            else
            {
                Destination.ParentNode.RemoveChild(Destination);
            }

            Source = null;
        }

        /// <summary>
        /// Converts a BC_DataReader object into an XmlDocument
        /// </summary>
        /// <example>
        /// <code>
        /// BC_DataReader adoRs = ...;
        /// BC_Xml_Json.DataReaderToXml(myXmlDoc, myXmlNode, adoRs, "ProductID", "ProductID");
        /// // "&lt;root>&lt;ProductID>1&lt;/ProductID>&lt;/root>"
        /// </code>
        /// </example>
        public static System.Xml.XmlDocument DataReaderToXml(ref System.Xml.XmlDocument XmlData, ref System.Xml.XmlNode XmlNode, ref BC_DataReader adoRs, string StartStr, string EndStr)
        {
            int StartInt = 0;
            int EndInt = 0;
            int loopTo = adoRs.FieldCount;
            for (int i = 0; i < loopTo; i++)
            {
                if (StartInt != 0 && EndInt != 0)
                {
                    break;
                }

                if (adoRs.GetName(i) == StartStr)
                {
                    StartInt = i;
                }

                if (adoRs.GetName(i) == EndStr)
                {
                    EndInt = i;
                }
            }

            for (int i = StartInt; i <= EndInt; i++)
            {
                string FieldName = adoRs.GetName(i);
                if (!BC_Fmt.IsNull(adoRs.ItemObj(FieldName).ToString()))
                {
                    if (FieldName.IndexOf('_') != -1)
                    {
                        AppendChild(ref XmlNode, CreateElement(BC_Fmt.Right(FieldName, FieldName.Length - (FieldName.IndexOf('_') + 1)), adoRs.ItemObj(FieldName), ref XmlData));
                    }
                    else
                    {
                        AppendChild(ref XmlNode, CreateElement(FieldName, adoRs.ItemObj(FieldName), ref XmlData));
                    }
                }
            }

            return XmlData;
        }

        /// <summary>
        /// Converts a BC_DataReader to a Json string
        /// </summary>
        /// <example>
        /// <code>
        /// BC_DataReader adoRs = ...;
        /// BC_Xml_Json.DataReaderToJson(myXmlDoc);
        /// // "{\"root\":{\"ProductID\":\"1\"}}"
        /// </code>
        /// </example>
        public static string DataReaderToJson(ref BC_DataReader adoRs, string Root, string AppendJson = "")
        {
            StringBuilder jsonB = new();

            jsonB.Append("{\"").Append(Root).Append("\":[");

            int RecordCount = 0;

            while (!adoRs.EOF)
            {
                if (RecordCount != 0)
                {
                    jsonB.Append(",{");
                }
                else
                {
                    jsonB.Append('{');
                }

                for (int i = 0, loopTo = adoRs.FieldCount; i < loopTo; i++)
                {
                    if (i != 0)
                    {
                        jsonB.Append(',');
                    }

                    jsonB.Append('"').Append(BC_Fmt.JsonSafe(adoRs.GetName(i))).Append("\":\"").Append(BC_Fmt.JsonSafe(adoRs.ItemObj(adoRs.GetName(i)).ToString())).Append('"');
                }

                jsonB.Append('}');

                RecordCount++;

                adoRs.MoveNext();
            }

            jsonB.Append("],\"recordCount\":").Append(RecordCount);

            if (AppendJson.Length != 0)
            {
                jsonB.Append(',').Append(AppendJson);
            }

            jsonB.Append('}');

            return jsonB.ToString();
        }

        /// <summary>
        /// Converts a BC_DataReader to a Json string
        /// </summary>
        /// <example>
        /// <code>
        /// BC_DataReader adoRs = ...;
        /// BC_Xml_Json.DataReaderToJsonWithoutRoot(myXmlDoc);
        /// // "[{\"ProductID\":\"1\"}]"
        /// </code>
        /// </example>
        public static string DataReaderToJsonWithoutRoot(ref BC_DataReader adoRs)
        {
            StringBuilder jsonB = new();

            jsonB.Append('[');

            int RecordCount = 0;

            while (!adoRs.EOF)
            {
                if (RecordCount != 0)
                {
                    jsonB.Append(",{");
                }
                else
                {
                    jsonB.Append('{');
                }

                for (int i = 0, loopTo = adoRs.FieldCount; i < loopTo; i++)
                {
                    if (i != 0)
                    {
                        jsonB.Append(',');
                    }

                    jsonB.Append('"').Append(BC_Fmt.JsonSafe(adoRs.GetName(i))).Append("\":\"").Append(BC_Fmt.JsonSafe(adoRs.ItemObj(adoRs.GetName(i)).ToString())).Append('"');
                }

                jsonB.Append('}');

                RecordCount++;

                adoRs.MoveNext();
            }

            jsonB.Append(']');

            return jsonB.ToString();
        }

        /// <summary>
        /// Converts a Json string to a BC_JsonDataSet object
        /// </summary>
        /// <example>
        /// <code>
        /// string json = "{\"root\":{\"ProductID\":\"1\"}}";
        /// BC_JsonDataSet myJsonDataset = BC_Xml_Json.JsonToDataSet(json, "root", "");
        /// </code>
        /// </example>
        public static BC_JsonDataSet JsonToDataSet(string Json, string RootName, string DataXPath)
        {
            return new BC_JsonDataSet(Json, RootName, DataXPath);
        }

        /// <summary>
        /// Converts an entire Xml Document into 1 string that contains all of the XML
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.XmlDocumentToXmlString(myXmlDoc);
        /// // "&lt;root>&lt;node1>&lt;/node1>...."
        /// </code>
        /// </example>
        public static string XmlDocumentToXmlString(ref System.Xml.XmlDocument XmlData, System.Xml.Formatting Format = System.Xml.Formatting.None)
        {
            StringBuilder xmlB = new();

            using (System.IO.StringWriter Writer = new System.IO.StringWriter(xmlB))
            {
                using (System.Xml.XmlTextWriter XmlWriter = new System.Xml.XmlTextWriter(Writer))
                {
                    // Task 1091649: File sent on Star 2 System is not populating for Pepsi
                    XmlWriter.Formatting = Format;
                    XmlData.WriteTo(XmlWriter);
                }
            }

            return xmlB.ToString();
        }

        /// <summary>
        /// Converts a string of XML to an JSON string of the same format
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.XmlStringToJson("&lt;root>&lt;node1>Value&lt;/node1>&lt;/root>");
        /// // "{\"root\":{\"node1\":\"Value\"}}"
        /// </code>
        /// </example>
        public static string XmlStringToJson(string Xml)
        {
            System.Xml.XmlDocument XmlData = new System.Xml.XmlDocument();
            XmlData.LoadXml(Xml);

            return XmlDocumentToJson(XmlData);
        }

        /// <summary>
        /// Converts an XmlDocument to an JSON string of the same format
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.XmlDocumentToJson(myXmlDoc);
        /// // "{\"root\":{\"node1\":\"Value\"}}"
        /// </code>
        /// </example>
        public static string XmlDocumentToJson(System.Xml.XmlDocument XmlData)
        {
            return Newtonsoft.Json.JsonConvert.SerializeXmlNode(XmlData);
        }

        /// <summary>
        /// Converts a Json string to an Xml string
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.JsonToXmlString("{\"root\":{\"node1\":\"Value\"}}");
        /// // "&lt;root>&lt;node1>Value&lt;/node1>&lt;/root>"
        /// </code>
        /// </example>
        public static string JsonToXmlString(string Json)
        {
            System.Xml.XmlDocument XmlData = (System.Xml.XmlDocument)Newtonsoft.Json.JsonConvert.DeserializeObject(Json);

            return XmlDocumentToXmlString(ref XmlData);
        }

        /// <summary>
        /// Converts a Json string to an Xml document
        /// </summary>
        /// <example>
        /// <code>
        /// System.Xml.XmlDocument myXmlDoc = BC_Xml_Json.JsonToXml("{\"root\":{\"node1\":\"Value\"}}");
        /// </code>
        /// </example>
        public static System.Xml.XmlDocument JsonToXml(string Json, string RootName)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeXmlNode(Json, RootName);
        }

        /// <summary>
        /// Converts a c# enum to a json string
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Xml_Json.EnumToJson(BC_Fmt.RequestFormat);
        /// // "{\"Json\":\"1\",\"XML\":\"2\",\"CSV\":\"3\",\"HTML\":\"4\"}"
        /// </code>
        /// </example>
        public static string EnumToJson(Type EnumType)
        {
            StringBuilder jsonB = new();
            jsonB.Append('{');

            string[] EnumNames = Enum.GetNames(EnumType);
            for (int i = 0; i < EnumNames.Length; i++)
            {
                if (i != 0)
                {
                    jsonB.Append(',');
                }

                jsonB.Append('"').Append(EnumNames[i]).Append("\":\"").Append((int)Enum.Parse(EnumType, EnumNames[i])).Append('"');
            }
            jsonB.Append('}');

            return jsonB.ToString();
        }

        /// <summary>
        /// Converts a BC_Dictionary object to a json string
        /// </summary>
        /// <example>
        /// <code>
        /// BC_Dictionary myDict = new BC_Dictionary&lt;string>();
        /// myDict.AddIfNotExists("name1", "value1");
        /// myDict.AddIfNotExists("name2", "value2");
        /// BC_Xml_Json.DictionaryToJson(myDict);
        /// // "{\"name1\":\"value1\",\"name2\":\"value2\"}"
        /// </code>
        /// </example>
        public static string DictionaryToJson(ref BC_Dictionary dict)
        {
            StringBuilder jsonB = new();

            jsonB.Append('{');

            string[] Keys = dict.Keys;
            for (int i = 0; i < Keys.Length; i++)
            {
                if (Keys[i] == null)
                {
                    return "";
                }

                if (i != 0)
                {
                    jsonB.Append(',');
                }

                jsonB.Append('"').Append(BC_Fmt.JsonSafe(Keys[i])).Append("\":\"").Append(BC_Fmt.JsonSafe(BC_Fmt.CStr(dict[Keys[i]]))).Append('"');
            }

            jsonB.Append('}');

            return jsonB.ToString();
        }

        /// <summary>
        /// Takes a complex XML document and will 'flatten' the XML so that it all nodes have a depth of 1
        /// </summary>
        /// <example>
        /// <code>
        /// XmlDocument doc = new XmlDocument();
        /// doc.LoadXml("&lt;book>&lt;title>Pride And Prejudice&lt;/title>&lt;otherInfo>&lt;author>Jane Austen&lt;/author>&lt;type>Romantic&lt;/type>&lt;/otherInfo>&lt;/book>");
        /// BC_Xml_Json.FlattenXMLFileByElement(myDict);
        /// BC_Xml_Json.XmlDocumentToXmlString(doc);
        /// // "&lt;book>&lt;title>Pride And Prejudice&lt;/title>&lt;otherInfo_author>Jane Austen&lt;/otherInfo_author>&lt;otherInfo_type>Romantic&lt;/otherInfo_type>&lt;/book>"
        /// </code>
        /// </example>
        public static void FlattenXMLFileByElement(ref System.Xml.XmlDocument myXmlDocument, string NodePath)
        {
            // Task 1117939: PreUpgrade to 22.08, Dashboard = 180571: Customized Package Adjustments Failing to Load after call to Update_Metrc_Packages_Table
            // Note:
            //    I read the source code of the .NET System.Xml library.
            //    XmlDocument.GetElementsByTagName method returns a XmlElementList object.
            //    An XmlElementListListener is generated inside XmlElementList and bound to the NodeInserted and NodeRemoved events of XmlDocument.
            //    For large XML documents, adding new nodes creates a lot of event handler objects, which triggers GC frequently.
            //    This makes this method very slow.
            //    The only solution I found was to manually dispose XmlElementList object. (When disposed, the event listeners are removed)
            //    So I modified the code below to use the `using` statement.
            List<System.Xml.XmlNode> NodesToFlatten = new List<System.Xml.XmlNode>();

            if (!BC_Fmt.IsNull(NodePath))
            {
                using (System.Xml.XmlNodeList Nodes = myXmlDocument.DocumentElement.SelectNodes(NodePath))
                {
                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        NodesToFlatten.Add(Nodes[i]);
                    }
                }
            }

            if (NodesToFlatten.Count == 0)
            {
                using (System.Xml.XmlNodeList Nodes = myXmlDocument.GetElementsByTagName(NodePath))
                {
                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        NodesToFlatten.Add(Nodes[i]);
                    }
                }

                if (NodesToFlatten.Count == 0)
                {
                    NodePath = FindMostRepeatedNode(ref myXmlDocument).Split('_')[0];

                    using (System.Xml.XmlNodeList Nodes = myXmlDocument.GetElementsByTagName(NodePath))
                    {
                        for (int i = 0; i < Nodes.Count; i++)
                        {
                            NodesToFlatten.Add(Nodes[i]);
                        }
                    }

                    if (NodesToFlatten.Count == 0)
                    {
                        throw new Exception("Could not find any repeating XML Nodes.");
                    }
                }
            }

            string NodeName = NodesToFlatten[0].Name;

            // Copy Parent information to each child, then move the child up a level.
            foreach (System.Xml.XmlNode Node in NodesToFlatten)
            {
                FlattenChildrenNodes(ref myXmlDocument, Node);
                FlattenNode(ref myXmlDocument, Node);
            }

            // Final Clean-up Steps - remove remaining unnecessary elements, and bring the level 1 elements into the repeated nodes
            System.Xml.XmlNodeList DocumentChildNodes = myXmlDocument.DocumentElement.ChildNodes;
            for (int i = 0; i < DocumentChildNodes.Count; i++)
            {
                System.Xml.XmlNode ChildNode = DocumentChildNodes[i];

                if (ChildNode.Name != NodeName &&
                    !ChildNode.Name.EndsWith("_" + NodeName) &&
                    !ChildNode.Name.StartsWith(NodeName + "_") &&
                    !ChildNode.Name.Contains("_" + NodeName + "_"))
                {
                    FlattenNode(ref myXmlDocument, ChildNode);

                    for (int j = 0; j < NodesToFlatten.Count; j++)
                    {
                        System.Xml.XmlNode Node = NodesToFlatten[j];
                        if (IsTextElement(ChildNode))
                        {
                            System.Xml.XmlNode InnerNode = myXmlDocument.CreateElement("base-" + ChildNode.Name);
                            InnerNode.InnerXml = ChildNode.InnerXml;
                            Node.AppendChild(InnerNode);
                        }
                        else
                        {
                            for (int k = 0; k < ChildNode.ChildNodes.Count; k++)
                            {
                                System.Xml.XmlNode InnerNode = myXmlDocument.CreateElement(ChildNode.Name + "-" + ChildNode.ChildNodes[k].Name);
                                InnerNode.InnerXml = ChildNode.ChildNodes[k].InnerXml;
                                Node.AppendChild(InnerNode);
                            }
                        }
                    }

                    myXmlDocument.DocumentElement.RemoveChild(ChildNode);
                }
            }
        }

        private static void FlattenNode(ref System.Xml.XmlDocument myXmlDocument, System.Xml.XmlNode Node)
        {
            System.Xml.XmlNode ParentNode = Node.ParentNode;

            if (!ParentNode.Equals(myXmlDocument.DocumentElement))
            {
                // If Parent is the root element, then we are done moving the child node up.
                // Pull single sibling elements into the parent element
                MergeOrDeleteSiblingNodes(ref myXmlDocument, ref ParentNode, new string[] { ParentNode.Name + "_" + Node.Name });

                // Flatten sibling nodes that have different names
                FlattenChildrenNodes(ref myXmlDocument, ParentNode, Node.Name.Split('_'));

                // Find all text elements, copy them to the child, then move the child up.
                List<System.Xml.XmlNode> NodeListToMoveUp = new List<System.Xml.XmlNode>();

                for (int i = 0; i < ParentNode.ChildNodes.Count; i++)
                {
                    NodeListToMoveUp.Add(ParentNode.ChildNodes[i]);
                }

                foreach (System.Xml.XmlNode ChildNode in NodeListToMoveUp)
                {
                    // Task 626367: SQL Failed, Error: Column count doesn't match value count at row 1
                    // Keep single node, like: <PONum />
                    if (IsTextElement(ChildNode) || ChildNode.ChildNodes.Count == 0)
                    {
                        System.Xml.XmlNode NewNode = myXmlDocument.ImportNode(ChildNode, true);
                        AppendChild(ref Node, NewNode);
                        ChildNode.ParentNode.RemoveChild(ChildNode);
                    }
                }

                // Move the child node up a level and rename
                System.Xml.XmlNode MoveUpNode = myXmlDocument.CreateElement(ParentNode.Name + "_" + Node.Name);
                MoveUpNode.InnerXml = Node.InnerXml;
                ParentNode.RemoveChild(Node);
                ParentNode.ParentNode.InsertBefore(MoveUpNode, ParentNode);

                if (!IsTextElement(ParentNode) && !ParentNode.HasChildNodes)
                {
                    ParentNode.ParentNode.RemoveChild(ParentNode);
                }

                // Flatten sibling nodes that have different names

                // Recursion time
                FlattenNode(ref myXmlDocument, MoveUpNode);
            }
        }

        // Flatten Child node records of the current node
        private static void FlattenChildrenNodes(ref System.Xml.XmlDocument myXmlDocument, System.Xml.XmlNode Node, string[] NodeNamesToExclude = null)
        {
            List<System.Xml.XmlNode> ChildrenNodes = new List<System.Xml.XmlNode>();
            List<string> ExistingElements = new List<string>();

            if (NodeNamesToExclude == null)
            {
                NodeNamesToExclude = Array.Empty<string>();
            }

            foreach (System.Xml.XmlNode ChildNode in Node.ChildNodes)
            {
                if (!IsTextElement(ChildNode) && !ExistingElements.Contains(ChildNode.Name) && BC_Fmt.Array_Intersect(ChildNode.Name.Split('_'), NodeNamesToExclude).Length == 0)
                {
                    ChildrenNodes.Add(ChildNode);
                    ExistingElements.Add(ChildNode.Name);
                }
            }

            foreach (System.Xml.XmlNode NodeToFlatten in ChildrenNodes)
            {
                FlattenChildrenNodes(ref myXmlDocument, NodeToFlatten);

                // Move nodes up and rename
                foreach (System.Xml.XmlNode ChildNode in NodeToFlatten.ChildNodes)
                {
                    System.Xml.XmlNode NewNode = myXmlDocument.CreateElement(NodeToFlatten.Name + "-" + ChildNode.Name);
                    NewNode.InnerXml = ChildNode.InnerXml;
                    NodeToFlatten.ParentNode.InsertBefore(NewNode, NodeToFlatten);
                }
            }
        }

        public static string FindMostRepeatedNode(ref System.Xml.XmlDocument myXmlDocument)
        {
            BC_Dictionary<int> myNodeDic = new BC_Dictionary<int>();

            FindMostRepeatedNode_Recursion(ref myXmlDocument, myXmlDocument.DocumentElement, ref myNodeDic, 1);

            string MostFrequentNode = "";
            int HighestCount = 0;
            string[] Keys = myNodeDic.Keys;

            for (int i = 0; i < Keys.Length; i++)
            {
                string Node = Keys[i];
                if (myNodeDic[Node] > HighestCount || (myNodeDic[Node] == HighestCount && BC_Fmt.CInt(Node.Split('_')[^1]) > BC_Fmt.CInt(MostFrequentNode.Split('_')[^1])))
                {
                    MostFrequentNode = Node;
                    HighestCount = myNodeDic[Node];
                }
            }

            return MostFrequentNode;
        }

        private static void FindMostRepeatedNode_Recursion(ref System.Xml.XmlDocument myXMLDocument, System.Xml.XmlElement CurrentNode, ref BC_Dictionary<int> NodeDic, int CurrentNodeDepth)
        {
            using System.Xml.XmlNodeList ChildElements = CurrentNode.GetElementsByTagName("*");
            if (ChildElements.Count > 0)
            {
                NodeDic.AddIfNotExists(CurrentNode.Name + "_" + CurrentNodeDepth, 0);
                NodeDic[CurrentNode.Name + "_" + CurrentNodeDepth]++;

                foreach (System.Xml.XmlElement ChildNode in ChildElements)
                {
                    FindMostRepeatedNode_Recursion(ref myXMLDocument, ChildNode, ref NodeDic, CurrentNodeDepth + 1);
                }
            }
        }

        private static bool IsTextElement(System.Xml.XmlNode Node)
        {
            return Node.ChildNodes.Count == 1 && Node.FirstChild.NodeType == System.Xml.XmlNodeType.Text;
        }

        private static void MergeOrDeleteSiblingNodes(ref System.Xml.XmlDocument myXMLDocument, ref System.Xml.XmlNode Node, string[] NodeIgnoreList)
        {
            System.Xml.XmlNode PreviousSibling = Node.PreviousSibling;
            BC_Dictionary<System.Xml.XmlNode> NodesToMerge = new BC_Dictionary<System.Xml.XmlNode>();

            while (PreviousSibling != null)
            {
                if (PreviousSibling.Name != Node.Name
                    && !BC_Fmt.Contains(NodeIgnoreList, PreviousSibling.Name)
                    && IsTextElement(PreviousSibling)
                    && !BC_Fmt.Contains(NodesToMerge.Keys, PreviousSibling.Name))
                {
                    NodesToMerge.Add(PreviousSibling.Name, PreviousSibling);
                }

                PreviousSibling = PreviousSibling.PreviousSibling;
            }

            System.Xml.XmlNode NextSibling = Node.NextSibling;

            while (NextSibling != null)
            {
                if (NextSibling.Name != Node.Name && !BC_Fmt.Contains(NodesToMerge.Keys, NextSibling.Name) && !BC_Fmt.Contains(NodeIgnoreList, NextSibling.Name) && IsTextElement(NextSibling))
                {
                    NodesToMerge.Add(NextSibling.Name, NextSibling);
                }

                NextSibling = NextSibling.NextSibling;
            }

            for (int i = 0; i < Node.ParentNode.ChildNodes.Count; i++)
            {
                System.Xml.XmlNode ChildNode = Node.ParentNode.ChildNodes[i];
                if (Node.Name == ChildNode.Name)
                {
                    foreach (System.Xml.XmlNode MergeNode in NodesToMerge.Values)
                    {
                        System.Xml.XmlNode NewNode = myXMLDocument.ImportNode(MergeNode, true);
                        AppendChild(ref ChildNode, NewNode);
                    }
                }
            }

            // Remove merged nodes
            foreach (System.Xml.XmlNode DeleteNode in NodesToMerge.Values)
            {
                Node.ParentNode.RemoveChild(DeleteNode);
            }
        }
    }

    public static class JsonElementExtension
    {
        // Task 1031358: Want a more convenient way to get property value from JsonElement
        /// <summary>
        /// Returning property value if the property was found; otherwise, empty string.
        /// </summary>
        public static string GetPropertyNull2Empty(this JsonElement jsonElement, string propertyName)
        {
            if (jsonElement.TryGetProperty(propertyName, out JsonElement result))
            {
                return result.ToString();
            }
            else
            {
                return "";
            }
        }
    }
}
