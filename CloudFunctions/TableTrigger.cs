using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EncompassWebServices
{
    public static class BC_TableTrigger
    {
        public enum TriggerType
        {
            AfterAdd = 1,
            AfterEdit = 2,
            AfterDelete = 3,
            Merge = 4,
            BeforeAdd = 5,
            BeforeDelete = 6,
            BeforeEdit = 7
        }

        public enum TableTriggerStatus
        {
            New = 0,
            Running = 4,
            Completed = 5,
            Failed = 6,
            Expired = 7
        }

        [Serializable]
        public struct Trigger
        {
            public string TableTriggerID;
            public string APICommand;
            // Task 1069095: Change fields that join to API Command ID to use API Command and look up the correctly versioned API Command ID
            public TriggerType TriggerTypeID;
            public string[] FieldArr;
            public bool RunAsync;
            // Task 601565: Add TableTriggers.Active to control which triggers run based on system settings
            public bool Active;
        }

        public class TriggerParameter
        {
            public TriggerType myTriggerType;
            public List<BC_Dictionary> FieldDicts;
            public string TableName;
            public string KeyField;
            public BC_Fmt.DataTypeEnum KeyFieldDataType;
            public int KeyFieldLength;
            public string[] KeyValueArr;
            public List<string> ChangedFields;
            public string[] TablesToCopy;
            // Task 654719: BC_TableTrigger.GetTriggerParameter 'Error converting value {null} to type 'System.Int32'.'
            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public int CopyFromKeyValue;
            public BC_Dictionary OverrideCopyFields;
            public string MergeToDestID;
            public bool IsCustom;
            public int TriggerExecuteCounter;
            public bool ReturnValidationErrorHTML;
            public Dictionary<string, Trigger> TriggerDict;
            public List<BC_Dictionary> PreValueDicts;
            public bool OverridePermissions;
            // Task 627880: Add the User Object to calls to Lambda, allowing us to know their Encompass ID and other parameters
            public int AuthenticationID;
            public string EncompassID;
            public string FullName;
            public string Email;

            public TriggerParameter()
            {
                FieldDicts = new List<BC_Dictionary>();
                PreValueDicts = new List<BC_Dictionary>();
            }
        }

        public static TriggerParameter Get_TriggerParameter(string TriggerParameterStr)
        {
            TriggerParameter myTriggerParameter;
            if (!BC_Fmt.IsNull(TriggerParameterStr))
            {
                myTriggerParameter = BC_Fmt.JsonDeserialize<TriggerParameter>(TriggerParameterStr);
            }
            else
            {
                myTriggerParameter = null;
            }

            return myTriggerParameter;
        }
    }
}