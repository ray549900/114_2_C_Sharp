using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Test_Score_List
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void ReadScores(List<int> scoresList)
        {
            string filePath = "TestScores.txt";

            // 清除先前的資料與 ListBox 顯示
            scoresList.Clear();
            testScoresListBox.Items.Clear();

            try
            {
                // 區域函式：嘗試將字串解析為 int，失敗回傳 null
                int? ParseScore(string s)
                {
                    if (string.IsNullOrEmpty(s)) return null;
                    if (int.TryParse(s, out int v)) return v;
                    return null;
                }

                // 讀取所有行並使用 LINQ 查詢語法解析（不使用 lambda）
                string[] lines = File.ReadAllLines(filePath);

                var validRecords =
                    from line in lines
                    let trimmed = (line ?? string.Empty).Trim()
                    where !string.IsNullOrEmpty(trimmed)
                    let parts = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    where parts.Length >= 2
                    let last = parts[parts.Length - 1]
                    let score = ParseScore(last)
                    where score.HasValue
                    select new
                    {
                        Id = string.Join(" ", parts, 0, parts.Length - 1),
                        Score = score.Value,
                        Raw = trimmed
                    };

                // 將解析成功的記錄加入 scoresList 與 ListBox，並記錄已處理的原始行
                var processedRawLines = new List<string>();
                foreach (var rec in validRecords)
                {
                    scoresList.Add(rec.Score);
                    testScoresListBox.Items.Add(rec.Id + " " + rec.Score.ToString());
                    processedRawLines.Add(rec.Raw);
                }

                // 檢查並蒐集解析失敗的行（空行已被忽略）
                var invalidLines = new List<string>();
                int lineNo = 0;
                foreach (var line in lines)
                {
                    lineNo++;
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var t = line.Trim();
                    if (processedRawLines.Contains(t)) continue;

                    // 若不在已處理清單中，視為無法解析的行
                    invalidLines.Add($"第 {lineNo} 行：{t}");
                }

                if (invalidLines.Count > 0)
                {
                    MessageBox.Show("部分資料無法解析：\n" + string.Join("\n", invalidLines),
                        "讀取警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("讀取成績時發生錯誤: " + ex.Message, "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void getScoresButton_Click(object sender, EventArgs e)
        {
            double averageScore;    // 儲存平均成績
            int numAboveAverage;    // 高於平均的成績數量
            int numBelowAverage;    // 低於平均的成績數量

            // 建立用來儲存成績的 List。
            List<int> scoresList = new List<int>();

            // 從檔案讀取成績到 List 中（同時會將 ListBox 顯示成 "學號 分數" 格式）。
            ReadScores(scoresList);

            // 若專案中仍有需要額外顯示的邏輯，請檢查並更新 DisplayScores 實作以免覆寫 ListBox。
            // 這裡保留不呼叫 DisplayScores，因為 ReadScores 已經將顯示填入 ListBox。
            // 如果您希望使用獨立的 DisplayScores 來顯示，請將其修改為顯示 "學號 分數"。

            // 顯示平均成績。
            averageScore = Average(scoresList);
            averageLabel.Text = averageScore.ToString("n1");

            // 顯示高於平均的成績數量。
            numAboveAverage = AboveAverage(scoresList, averageScore);
            aboveAverageLabel.Text = numAboveAverage.ToString();

            // 顯示低於平均的成績數量。
            numBelowAverage = BelowAverage(scoresList);
            belowAverageLabel.Text = numBelowAverage.ToString();
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            // 關閉表單。
            this.Close();
        }
    }
}
