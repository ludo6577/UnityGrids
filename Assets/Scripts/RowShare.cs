using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class RowShare : Singleton<RowShare>
{
    
    public RsTable Table = null;
    public string Error = null;

    private static readonly string rowshareUrl = "https://www.rowshare.com";
    private static readonly string rowshareListUrl = "/api/list/load/";
    private static readonly string rowshareColumnUrl = "/api/column/loadforparent/";
    private static readonly string rowshareRowUrl = "/api/row/loadforparent/";

    private static readonly string tableId = "0734e25ecc614f46a9815cda57c560c2";


    public IEnumerator LoadTable()
    {
        //ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;

        /* 
         * Step 1)
         * First try to load from https://www.rowshare.com/
         */
        yield return GetTableById<RsTable>(tableId);
        if (!string.IsNullOrEmpty(Error))
        {
            Debug.Log(string.Format("### Error: loading rowshare table failed\n{0}", Error));
        }
    }

    private IEnumerator GetTableById<T>(string rowShareTableId) where T : RsTable
    {
        //Load table
        Debug.Log("Load Table...");
        WWW tableWww = new WWW(string.Format("{0}{1}{2}", rowshareUrl, rowshareListUrl, rowShareTableId));
        yield return tableWww;
        if (!string.IsNullOrEmpty(tableWww.error))
        {
            Error = tableWww.error;
            Debug.LogError("Error: " + tableWww.error);
            yield return null;
        }
        Table = JsonUtility.FromJson<T>(Encoding.UTF8.GetString(tableWww.bytes));


        //Load columns
        Debug.Log("Load Columns...");
        WWW columnWww = new WWW(string.Format("{0}{1}{2}", rowshareUrl, rowshareColumnUrl, rowShareTableId));
        yield return columnWww;
        if (!string.IsNullOrEmpty(columnWww.error))
        {
            Error = columnWww.error;
            Debug.Log("Error: " + columnWww.error);
            yield return null;
        }
        var jsonColumnsData = "{\"Columns\":" + Encoding.UTF8.GetString(columnWww.bytes) + "}";
        Table.Columns = JsonUtility.FromJson<RsTable>(jsonColumnsData).Columns;


        //Load rows
        Debug.Log("Load Rows...");
        WWW rowsWww = new WWW(string.Format("{0}{1}{2}", rowshareUrl, rowshareRowUrl, rowShareTableId));
        yield return rowsWww;
        if (!string.IsNullOrEmpty(rowsWww.error))
        {
            Error = rowsWww.error;
            Debug.LogError("Error: " + rowsWww.error, null);
            yield return null;
        }
        var jsonRowsData = "{\"Rows\":" + Encoding.UTF8.GetString(rowsWww.bytes) + "}";
        Table.Rows = JsonUtility.FromJson<RsTable>(jsonRowsData).Rows;

        Debug.Log("Load Success!");
    }


    #region Serializable class

    [Serializable]
    public class RsTable
    {
        public RsColumn[] Columns;
        public RsRow[] Rows;

        private Dictionary<string, RsRow> RowByKey;

        public RsTable()
        {
            RowByKey = new Dictionary<string, RsRow>();
        }

        public RsRow GetRowByKey(string key)
        {
            if (RowByKey.ContainsKey(key))
                return RowByKey[key];

            var row = Rows.FirstOrDefault(r => r["Key"].ToString() == key);
            RowByKey.Add(key, row);
            return row;
        }
    }

    [Serializable]
    public class RsColumn
    {
        public string Name;
        public string DisplayName;
        public string SubName;
    }

    [Serializable]
    public class RsRow
    {
        public RsRowValue Values;
        //public Dictionary<string, object> Values;

        private Dictionary<string, string> _dict;

        public Dictionary<string, string> ValuesDic
        {
            get
            {
                if (_dict != null)
                    return _dict;

                _dict = new Dictionary<string, string>();
                var fields = Values.GetType().GetFields();
                foreach (var fieldInfo in fields)
                {
                    _dict.Add(fieldInfo.Name, fieldInfo.GetValue(Values).ToString());
                }
                return _dict;
            }

        }

        //public string[] Fields
        //{
        //    get { return ValuesDic.Keys.ToArray(); }
        //}

        public string this[string key]
        {
            get
            {
                return ValuesDic[key];
            }
            set
            {
                ValuesDic[key] = value;
                Values.GetType().GetField(key).SetValue(Values, value);
            }
        }
    }

    [Serializable]
    public class RsRowValue
    {
        public string TextColumn;
        public bool YesNoColumn;
        public decimal NumberColumn;
        public string DateColumn;
        public string TimeColumn;
        public string ValueList;
        public decimal DecimalColumn;
        public string RichTextColumn;
        public string TextColumn2;
        public string TextColumn3;
    }

    //[Serializable]
    //public class RsRowValue
    //{
    //    public string Key;
    //    public string English;
    //    public string French;
    //    public string Italian;
    //    public string Spanish;
    //    public string German;
    //    public string Swedish;
    //    public string Greek;
    //    public string Russian;
    //    public string Portuguese;
    //    public string Dutch;
    //    public string Norwegian;
    //    public string Finnish;
    //    public string Danish;
    //    public string Chinese;
    //}

    #endregion
}
