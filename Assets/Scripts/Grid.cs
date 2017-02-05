using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Grid : MonoBehaviour {

    public GameObject RowPrefab;
    public GameObject CellPrefab;

    public GameObject Content;

    private List<Column> columns;
    private List<Row> rows;
    

    // Use this for initialization
    void Start () {
        StartCoroutine(LoadGrid());
	}
	
    private IEnumerator LoadGrid()
    {
        var rowshare = RowShare.Instance;
        
        yield return rowshare.LoadTable();

        if(rowshare.Error != null)
        {
            Debug.Log(rowshare.Error);
            yield break;
        }        

        foreach(var rsRow in rowshare.Table.Rows)
        {
            //Create the row
            var row = Instantiate(RowPrefab);
            row.transform.SetParent(Content.transform, false);

            //Create the cells
            var fields = rsRow.Values.GetType().GetFields();
            foreach (var fieldInfo in fields)
            {
                var cell = Instantiate(CellPrefab);
                cell.transform.SetParent(row.transform, false);
                var value = fieldInfo.GetValue(rsRow.Values);
                if (value != null)
                {
                    var cellText = cell.GetComponentInChildren<Text>();
                    cellText.text = value.ToString();
                }
            }
        }
    }
}
