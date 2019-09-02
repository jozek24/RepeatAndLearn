using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace RepeatAndLearn.Model
{
    public static class GlobalSettings
    {
        public const string ApiKey = "trnsl.1.1.20190821T170155Z.fafa71ea078787a0.bbbe06c23ca22760e5f05eb512ae2b37f7c110f1";
        public const string ConnectionString = "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;";
        public const string TranslateHttps = "https://translate.yandex.net/api/v1.5/tr.json/translate?key=";

        public static List<Word> ActualListOfWords;

        public static event EventHandler WordsChange;
        private static void WordsChanged()
            => WordsChange?.Invoke(null, new EventArgs());

        static GlobalSettings()
        {
            UpdateListOfWords();
        }

        public static void UpdateListOfWords()
        {
            string sqlWordsSelect = "SELECT * FROM Words;";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                ActualListOfWords = connection.Query<Word>(sqlWordsSelect).ToList();
            }
            WordsChanged();
        }
    }
}
