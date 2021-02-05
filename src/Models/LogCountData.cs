using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vschatbot.src.Models
{
    class LogCountData
    {
        private List<string> Get_log_data()
        {
            return new List<string>(System.IO.File.ReadAllLines("log.txt"));
        }            

        public string ShowLast(int last)
        {
            List<string> log_data = new List<string>();
            try
            {
                log_data = Get_log_data();
            }
            catch
            {
                return null;
            }
            int count = last;
            if (log_data.Count < count)
            {
                count = 0;
            }
            string result = "";
            for (int i = log_data.Count - count; i < log_data.Count; i++)
            {
                result += log_data[i] + '\n';
            }
            return result;
        }

        public void Clear_Log()
        {
            System.IO.File.WriteAllText("log.txt", String.Empty);
        }

        public string Find_by_time(string word, out int count)
        {
            List<string> log_data = new List<string>();
            try
            {
                log_data = Get_log_data();
            }
            catch
            {
                count = 0;
                return null;
            }
            List<string> search_results = new List<string>();
            foreach (string str in log_data)
            {
                if (str.Contains(word))
                {
                    search_results.Add(str);
                }
            }
            count = search_results.Count;
            if (search_results.Count > 40)
            {
                int rest_length = search_results.Count - 40;
                    search_results.RemoveRange(0, rest_length);
            }
            string result = "";
            for (int i = 0; i < search_results.Count; i++)
            {
                result += search_results[i] + '\n';
            }
            return result;
        }
    }
}
