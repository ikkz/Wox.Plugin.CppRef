using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;

namespace Wox.Plugin.CppRef
{
    public class Main : IPlugin
    {
        private string config_file = "config.json";
        private bool en;
        private bool cpp;

        private void SetEn(bool en)
        {

        }

        private void SetCpp(bool cpp)
        {

        }

        public void Init(PluginInitContext context)
        {
            if (!File.Exists(config_file))
            {
                en = true; cpp = true;
            }
            else
            {
                StreamReader sr = new StreamReader(config_file, System.Text.Encoding.Default);
                string content = sr.ReadToEnd();
                JObject jsonObject = (JObject)JsonConvert.DeserializeObject(content);
                if (jsonObject.ContainsKey("en"))
                    en = (jsonObject.GetValue("en").ToString().ToUpper().CompareTo("TRUE") == 0);
                if (jsonObject.ContainsKey("cpp"))
                    cpp = (jsonObject.GetValue("cpp").ToString().ToUpper().CompareTo("TRUE") == 0);
                sr.Close();
            }
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            string[] args = query.RawQuery.Split(' ');
            bool open_page = false;
            string url = en ? "https://en.cppreference.com/mwiki/index.php?search=" :
                "https://zh.cppreference.com/mwiki/index.php?search=";
            if (args.Length >= 2)
            {
                if (args[1].CompareTo("d") == 0)
                    open_page = true;
            }
            else
                return results;
            int index = (open_page ? 2 : 1);
            while (index < args.Length)
            {
                url += args[index];
                if (index != args.Length - 1)
                    url += '+';
                index++;
            }
            if (open_page)
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
            else
            {
                HtmlWeb html = new HtmlWeb();
                HtmlDocument document = html.Load(url);
                string title = document.DocumentNode.FirstChild.NextSibling.NextSibling.FirstChild.NextSibling.FirstChild.NextSibling.InnerText;
                if (!title.Contains(en ? "Search results" : "搜索结果"))
                {
                    results.Add(new Result
                    {
                        Title = title.Replace(" - cppreference.com", ""),
                        SubTitle = url,
                        IcoPath = "Images\\icon.png",
                        Action = e =>
                        {
                            System.Diagnostics.Process.Start(url);
                            return true;
                        }
                    });
                    return results;
                }
                HtmlNode search_results = GetElementByClass(document.GetElementbyId("mw-content-text"), "mw-search-results");
                if (search_results == null)
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
                    List<HtmlNode> nodes = GetAllSearchResults(search_results);
                    foreach (HtmlNode node in nodes)
                    {
                        string direct_url = "https://" + (en ? "en" : "zh") + ".cppreference.com"
                            + node.Attributes["href"].Value;
                        results.Add(new Result
                        {
                            Title = node.InnerText.Replace("&lt;", "<").Replace("&gt;", ">"),
                            IcoPath = "Images\\icon.png",
                            SubTitle = direct_url,
                            Action = e =>
                            {
                                System.Diagnostics.Process.Start(direct_url);
                                return true;
                            }
                        });
                    }

                }
            }
            return results;
        }

        public static List<HtmlNode> GetAllSearchResults(HtmlNode node)
        {
            List<HtmlNode> nodes = new List<HtmlNode>();
            if (node.Name == "a")
                nodes.Add(node);
            foreach (HtmlNode child in node.ChildNodes)
                nodes.AddRange(GetAllSearchResults(child));
            return nodes;
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
