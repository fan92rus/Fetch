using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ReverseMarkdown;

namespace Funny.WebScrape;

public static class ArticleExtractor
{
    public static string ExtractArticle(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        var body = doc.DocumentNode.SelectSingleNode("//body");
        if (body == null) return string.Empty;

        // Собираем все узлы и сразу считаем статистику
        var nodeInfo = new Dictionary<HtmlNode, NodeScoreInfo>();
        var allNodes = new List<HtmlNode>();

        // Обход дерева снизу вверх: сначала обрабатываем дочерние узлы, потом родительские
        ProcessNode(body, nodeInfo, allNodes);

        // Теперь оцениваем каждый узел (уже с готовыми данными)
        var bestNode = allNodes
            .Where(node => nodeInfo[node].Score > 0)
            .OrderByDescending(node => nodeInfo[node].Score)
            .FirstOrDefault();

        // Очищаем лучший узел от дочерних badNodes
        RemoveBadChildNodes(bestNode);

        var converter = new Converter();
        var text = converter.Convert(bestNode.InnerHtml);
        
        return text ?? string.Empty;
    }

    private static void ProcessNode(HtmlNode node, Dictionary<HtmlNode, NodeScoreInfo> nodeInfo,
        List<HtmlNode> allNodes)
    {
        // Добавляем текущий узел
        allNodes.Add(node);

        // Собираем статистику по дочерним узлам
        var childStats = new Dictionary<string, int>();
        var childTextLength = 0;
        var totalChildCount = 0;

        foreach (var child in node.ChildNodes)
        {
            ProcessNode(child, nodeInfo, allNodes); // Рекурсивно обрабатываем дочерние

            // Собираем статистику по дочерним
            if (nodeInfo.TryGetValue(child, out var childInfo))
            {
                // Суммируем статистику от дочерних узлов
                foreach (var kvp in childInfo.TagCount)
                {
                    childStats[kvp.Key] = childStats.GetValueOrDefault(kvp.Key, 0) + kvp.Value;
                }

                childTextLength += childInfo.TextLength;
                totalChildCount += childInfo.ChildCount;
            }
        }
        if (node.Name is "body")
        {
            ;
        }
        // Добавляем текущий узел в статистику
        var tagName = node.Name.ToLower();
        childStats[tagName] = childStats.GetValueOrDefault(tagName, 0) + 1;

        // Общее количество узлов (включая текущий и всех дочерних)
        var totalCount = totalChildCount + 1;

        // Текст текущего узла
        var text = node.InnerText.Trim();
        var normalizedText = Regex.Replace(text, @"\s+", " ").Trim();
        var textLength = normalizedText.Length;

        // Оценка узла (теперь без рекурсивного CollectNodes)
        var score = textLength * 0.1; // Длина текста

        // Паттерны в class/id — не будем проверять здесь, а просто запомним
        var isBadNode = IsBadNode(node) || IsBadNode(node.ParentNode);
        if (isBadNode)
        {
            nodeInfo[node] = new NodeScoreInfo
                { Score = 0, TextLength = textLength, TagCount = childStats, ChildCount = totalCount };
            return;
        }

        // Если текст слишком короткий — штраф
        if (textLength < 50)
            score -= 5;

        // Отношение количества "хороших" тегов к общему
        var totalGoodNodes = childStats.GetValueOrDefault("p", 0) +
                             childStats.GetValueOrDefault("h1", 0) +
                             childStats.GetValueOrDefault("h2", 0) +
                             childStats.GetValueOrDefault("h3", 0) +
                             childStats.GetValueOrDefault("br", 0) +
                             childStats.GetValueOrDefault("hr", 0); // Упрощаем: считаем только "значимые" тексты

        // Отношение количества "хороших" тегов к общему
        var totalBadNodes = childStats.GetValueOrDefault("a", 0) +
                            childStats.GetValueOrDefault("span", 0) +
                            childStats.GetValueOrDefault("li", 0);

        var goodBad = (float)totalBadNodes / (float)totalGoodNodes;

        var totalNodes = totalCount;

        var nodeRate = totalGoodNodes / (double)(totalNodes - totalGoodNodes + 1); // +1 чтобы избежать деления на 0

        if (nodeRate < 1)
        {
            score -= Math.Max(0, score * (1 - nodeRate));
        }

        // Штраф за много дочерних, но мало текста
        if (totalChildCount > 0 && textLength / totalChildCount < 10)
        {
            score -= 10;
        }

        // Сохраняем результат
        nodeInfo[node] = new NodeScoreInfo
        {
            Score = Math.Max(0, score),
            TextLength = textLength,
            TagCount = childStats,
            ChildCount = totalCount
        };
    }

    private static void RemoveBadChildNodes(HtmlNode node)
    {
        if (node == null) return;

        // Работаем с копией списка для безопасного удаления
        var children = node.ChildNodes.ToList();
        
        foreach (var child in children)
        {
            // Рекурсивно очищаем дочерние узлы
            RemoveBadChildNodes(child);
            
            // Проверяем и удаляем badNodes
            if (IsBadNode(child))
            {
                node.RemoveChild(child);
            }
        }
    }

    private static bool IsBadNode(HtmlNode node)
    {
        if (node == null) return false;

        var lowerName = node.Name.ToLower();

        var badPatterns = new[]
        {
            "header", "footer", "nav", "aside", "navbar", "navigation", "menu", "sidebar", "sidebar-right",
            "widget", "social", "share", "related", "subscribe", "button", "style", "script", "link", "aside", "table", "figure"
        };

        if (badPatterns.Contains(lowerName)) return true;

        return false;
    }
}
