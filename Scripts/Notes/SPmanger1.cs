using UnityEngine;


public class SPmanger1 : MonoBehaviour
{
    public TextAsset csvFile;    // ここにCSVファイルをドラッグ＆ドロップ
    public GameObject[] notes; // ノーツ各種
    public Vector3 Line; //レーンの生成座標
    public int CSVstart;
    private float passtime;

    float[] sptime = new float[553];//CSVファイルの生成時間記録
    int[] notestype = new int[553];//CSVファイルのノーツタイプ記録

    void Start()
    {
        string csvText = csvFile.text;
        if (csvFile == null) { Debug.LogError("CSVファイルが設定されていません。"); return; }// CSVファイルが無い時、LOGを残す
        string[] lines = csvText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        for (int i = CSVstart; i >= 0; i--)// CSVファイルを一気に読み取り、分類する
        {
            string[] cells = lines[i].Split(',');
            sptime[i] = float.Parse(cells[0]);// CSVファイルの時間を読み取り
            notestype[i] = int.Parse(cells[1]);// CSVファイルのノーツタイプを読み取り
            Debug.Log("time" + sptime[i]);// CSVファイルの時間確認用LOG
            Debug.Log("notestype" + notestype[i]);// CSVファイルのノーツ確認用LOG
        }

    }
    void Update()
    {
        if (CSVstart >= 0)　　　　//CSVファイルの生成時間を参照（未修正）
        {
            if (notestype[CSVstart] != 0)      //CSVファイルのノーツタイプを参照、0じゃない時対応のノーツ生成
            {
                Instantiate(notes[notestype[CSVstart] - 1], Line, notes[notestype[CSVstart] - 1].transform.rotation);
            }
            CSVstart--;
        }
    }
}


