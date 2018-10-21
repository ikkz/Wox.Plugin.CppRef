using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wox.Plugin;

namespace Wox.Plugin.CppRef
{
    public class Main : IPlugin
    {
        public void Init(PluginInitContext context)
        {
        }

        public List<Result> Query(Query query)
        {
            List<Result> res = new List<Result>();
            string[] args = query.RawQuery.Split(' ');
            if (args.GetLength(0) > 1)
            {
                string url = "https://zh.cppreference.com/mwiki/index.php?search=" + args[1];
                res.Add(new Result
                {
                    Title = "search " + args[1] + " in cppreference.com",
                    IcoPath = "Images\\icon.png",
                    Action = e =>
                    {
                        System.Diagnostics.Process.Start(url);
                        return true;
                    }
                });
            }
            return res;
        }
    }
}
