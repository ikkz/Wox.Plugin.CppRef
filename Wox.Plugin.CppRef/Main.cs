using HtmlAgilityPack;
using System.Collections.Generic;
using System.IO;

namespace Wox.Plugin.CppRef
{
    public class Main : IPlugin
    {
        private string config_file = "cr_config";
        private bool _en;
        private bool _cpp;

        private void setEnCpp(bool en, bool cpp)
        {
            _en = en; _cpp = cpp;
            using (StreamWriter sw = new StreamWriter(config_file, false))
            {
                sw.Write((en ? "T" : "F") + (cpp ? "T" : "F"));
            }
        }

        private void SetEn(bool en)
        {
            setEnCpp(en, _cpp);
        }

        private void SetCpp(bool cpp)
        {
            setEnCpp(_en, cpp);
        }

        public void Init(PluginInitContext context)
        {
            if (!File.Exists(config_file))
            {
                _en = true; _cpp = true;
            }
            else
            {
                using (StreamReader sr = new StreamReader(config_file))
                {
                    string content = sr.ReadToEnd();
                    _en = content.Length > 0 ? (content[0] == 'T') : true;
                    _cpp = content.Length > 1 ? (content[1] == 'T') : true;
                }
            }
        }

        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            string[] args = query.RawQuery.Split(' ');
            bool open_page = false;
            string url = _en ? "https://en.cppreference.com/mwiki/index.php?search=" :
                "https://zh.cppreference.com/mwiki/index.php?search=";
            if (args.Length >= 2)
            {
                if (args[1].CompareTo("d") == 0)
                    open_page = true;
                if (args[1].CompareTo("config") == 0)
                {
                    results.Add(new Result
                    {
                        Title = "change to " + (_en ? "zh" : "en"),
                        IcoPath = "Images\\icon.png",
                        Action = e =>
                        {
                            SetEn(!_en);
                            return true;
                        }
                    });
                    results.Add(new Result
                    {
                        Title = "change to " + (_cpp ? "c" : "cpp"),
                        IcoPath = "Images\\icon.png",
                        Action = e =>
                        {
                            SetCpp(!_cpp);
                            return true;
                        }
                    });
                    return results;
                }
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
                if (!title.Contains(_en ? "Search results" : "搜索结果"))
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
                List<HtmlNode> search_results = GetElementByClass(document.GetElementbyId("mw-content-text"), "mw-search-results");
                if (search_results.Count == 0 || (search_results.Count == 1 && !_cpp))
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
                    List<HtmlNode> nodes = GetAllSearchResults(search_results[_cpp ? 0 : 1]);
                    foreach (HtmlNode node in nodes)
                    {
                        string direct_url = "https://" + (_en ? "en" : "zh") + ".cppreference.com"
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

        public static List<HtmlNode> GetElementByClass(HtmlNode root, string class_name)
        {
            List<HtmlNode> res = new List<HtmlNode>();
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
                    res.Add(node);
                }
            }
            return res;
        }
    }
}
