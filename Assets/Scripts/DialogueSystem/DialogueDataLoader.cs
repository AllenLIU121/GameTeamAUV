using UnityEngine;
using System.Collections.Generic;
using System.IO;

namespace DialogueSystem
{
    /// <summary>
    /// 对话数据加载器（负责CSV解析和数据验证）
    /// </summary>
    public class DialogueDataLoader
    {
        private readonly string _defaultCsvPath;
        private readonly List<DialogueEntry> _fallbackDialogues;

        // 存储每个选择点的原始选择（供返回点逻辑使用）
        public Dictionary<int, List<Choice>> AvailableChoices { get; private set; } = new();

        public DialogueDataLoader(DialogueSystemConfig config)
        {
            _defaultCsvPath = Path.Combine(Application.streamingAssetsPath, config.defaultCSVFileName);
            _fallbackDialogues = config.fallbackDialogues;
        }

        /// <summary>
        /// 加载对话数据（优先CSV，失败则用fallback）
        /// </summary>
        public List<DialogueEntry> LoadDialogues(bool useCSV, string customCsvPath = null)
        {
            if (useCSV)
            {
                var csvPath = string.IsNullOrEmpty(customCsvPath) ? _defaultCsvPath : customCsvPath;
                if (File.Exists(csvPath))
                {
                    return LoadFromCSV(csvPath);
                }
                Debug.LogWarning($"CSV文件不存在：{csvPath}，使用fallback数据");
            }
            return new List<DialogueEntry>(_fallbackDialogues);
        }

        /// <summary>
        /// 重新加载CSV文件
        /// </summary>
        public List<DialogueEntry> ReloadCSV(string csvPath)
        {
            if (File.Exists(csvPath))
            {
                Debug.Log("CSV文件已重新加载");
                return LoadFromCSV(csvPath);
            }
            Debug.LogError($"重新加载失败：CSV文件不存在 {csvPath}");
            return new List<DialogueEntry>();
        }

        /// <summary>
        /// 解析CSV文件为DialogueEntry列表
        /// </summary>
        private List<DialogueEntry> LoadFromCSV(string csvPath)
        {
            var dialogues = new List<DialogueEntry>();
            AvailableChoices.Clear();

            try
            {
                var lines = File.ReadAllLines(csvPath);
                // 跳过表头（第0行），从第1行开始解析
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = SplitCSVLine(lines[i]);
                    if (values.Length < 7)
                    {
                        Debug.LogWarning($"CSV第{i}行数据不完整，跳过");
                        continue;
                    }

                    var entry = ParseDialogueEntry(values, i);
                    if (entry != null)
                    {
                        dialogues.Add(entry);
                        // 存储选择点原始数据
                        if (entry.hasChoices && entry.choices.Count > 0)
                        {
                            AvailableChoices[i] = new List<Choice>(entry.choices);
                        }
                    }
                }
                Debug.Log($"成功加载 {dialogues.Count} 条对话（来自CSV：{csvPath}）");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析CSV失败：{e.Message}");
                dialogues = new List<DialogueEntry>(_fallbackDialogues);
            }

            return dialogues;
        }

        /// <summary>
        /// 将CSV行数据解析为DialogueEntry
        /// </summary>
        private DialogueEntry ParseDialogueEntry(string[] values, int lineIndex)
        {
            var entry = new DialogueEntry();
            try
            {
                entry.characterName = values[0];
                entry.dialogueText = values[1];
                entry.hasChoices = bool.TryParse(values[2], out var hasChoices) ? hasChoices : false;

                // 解析选项
                if (entry.hasChoices)
                {
                    // 解析第一个选项
                    if (!string.IsNullOrEmpty(values[3]) && int.TryParse(values[4], out var choice1Idx))
                    {
                        entry.choices.Add(new Choice
                        {
                            choiceText = values[3],
                            nextDialogueIndex = choice1Idx,
                            isSelected = false
                        });
                    }
                    // 解析第二个选项
                    if (!string.IsNullOrEmpty(values[5]) && int.TryParse(values[6], out var choice2Idx))
                    {
                        entry.choices.Add(new Choice
                        {
                            choiceText = values[5],
                            nextDialogueIndex = choice2Idx,
                            isSelected = false
                        });
                    }
                }

                // 解析表情、返回点、结束点、回调方法
                entry.characterExpression = values.Length >= 8 ? (int.TryParse(values[7], out var exp) ? exp : 0) : 0;
                entry.isReturnPoint = values.Length >= 9 && int.TryParse(values[8], out var isReturn) && isReturn == -1;
                entry.isEndPoint = values.Length >= 10 && int.TryParse(values[9], out var isEnd) && isEnd == 1;
                entry.onDialogueCompleteMethod = values.Length >= 11 ? values[10] : "";
                entry.isDeadly = values.Length >= 12 && int.TryParse(values[11], out var isDeadly) && isDeadly == 1;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"解析CSV第{lineIndex}行失败：{e.Message}");
                return null;
            }
            return entry;
        }

        /// <summary>
        /// 处理CSV行拆分（支持带引号的字段）
        /// </summary>
        private string[] SplitCSVLine(string line)
        {
            var result = new List<string>();
            var inQuotes = false;
            var currentField = "";

            foreach (var c in line)
            {
                if (c == '"' && (currentField.Length == 0 || line[line.IndexOf(c) - 1] != '\\'))
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }
            result.Add(currentField);
            return result.ToArray();
        }
    }
}