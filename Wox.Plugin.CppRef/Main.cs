using HtmlAgilityPack;
using System.Collections.Generic;

namespace Wox.Plugin.CppRef
{
    enum Type
    {
        OPEN_PAGE,
        SHOW_LIST
    };

    public class Main : IPlugin
    {
        public void Init(PluginInitContext context)
        {
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            string[] args = query.RawQuery.Split(' ');
            Type type;
            string url = "https://zh.cppreference.com/mwiki/index.php?search=";
            if (args.Length >= 2)
            {
                if (args[1].CompareTo("d") == 0)
                    type = Type.OPEN_PAGE;
                else
                    type = Type.SHOW_LIST;
            }
            else
            {
                return results;
            }
            int index = (type == Type.OPEN_PAGE ? 2 : 1);
            while (index < args.Length)
            {
                url += args[index];
                if (index != args.Length - 1)
                    url += '+';
                index++;
            }
            if (type == Type.OPEN_PAGE)
            {
                results.Add(new Result
                {
                    Title = "search in cppreference.com",
                    SubTitle = url,
                    IcoPath = "Images\\icon.png",
                    Action = e =>
                    {
                        System.Diagnostics.Process.Start(url);
                        return true;
                    }
                });
            }
            else if (type == Type.SHOW_LIST)
            {
                HtmlWeb html = new HtmlWeb();
                HtmlDocument document = html.Load(url);
                HtmlNode ele = GetElementByClass(document.GetElementbyId("mw-content-text"), "mw-search-results");
                if (ele == null)
                {
                    results.Add(new Result
                    {
                        Title = "no result",
                        IcoPath = "Images\\icon.png",
                        Action = e =>
                        {
                            return false;
                        }
                    });
                }
                else
                {
                    ele = ele.FirstChild.NextSibling;
                    while (ele != null)
                    {
                        HtmlNode node = ele.FirstChild.FirstChild;
                        string direct_url = "https://zh.cppreference.com" + node.Attributes["href"].Value;
                        results.Add(new Result
                        {
                            Title = node.InnerText,
                            IcoPath = "Images\\icon.png",
                            SubTitle = direct_url,
                            Action = e =>
                            {
                                System.Diagnostics.Process.Start(direct_url);
                                return true;
                            }
                        });
                        ele = ele.NextSibling.NextSibling;
                    }
                }
            }
            return results;
        }

        public static HtmlNode GetElementByClass(HtmlNode root, string class_name)
        {
            Queue<HtmlNode> nodes = new Queue<HtmlNode>();
            HtmlNode node = null;
            nodes.Enqueue(root);
            while (nodes.Count != 0)
            {
                node = nodes.Dequeue();
                HtmlNode child = node.FirstChild;
                while (child != null)
                {
                    nodes.Enqueue(child);
                    child = child.NextSibling;
                }
                HtmlAttribute attribute = node.Attributes["class"];
                if (attribute != null && attribute.Value == class_name)
                {
                    return node;
                }
            }
            return null;
        }
    }
}
