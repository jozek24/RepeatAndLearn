using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using RestSharp;

namespace RepeatAndLearn.ViewModel
{
    class MyViewModel : BindableBase
    {
        const string ApiKey = "trnsl.1.1.20190821T170155Z.fafa71ea078787a0.bbbe06c23ca22760e5f05eb512ae2b37f7c110f1";
        private List<Word> actualListOfWords;

        private string _wordToTranslate = "House";

        public string WordToTranslate
        {
            get => _wordToTranslate;
            set => SetProperty(ref _wordToTranslate, value);
        }

        private string _translatedWord;

        public string TranslatedWord
        {
            get => _translatedWord;
            set => SetProperty(ref _translatedWord, value);
        }

        public MyViewModel()
        {
            actualListOfWords = new List<Word>();
            ApiExecuteCommand = new DelegateCommand(Execute);
            AddWordCommand = new DelegateCommand(AddNewWord);
        }

        

        public ICommand ApiExecuteCommand { get; }
        public ICommand AddWordCommand { get; }

        private void Execute()
        {
            var client = new RestClient("https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + ApiKey);
            var request = new RestRequest(Method.GET);
            request.AddParameter("text", WordToTranslate);
            request.AddParameter("lang", "en-pl");

            var response = client.Execute<DictionaryResponse>(request);
            if (response.IsSuccessful)
                TranslatedWord = response.Data.Text[0];
        }


        //private void GetAllWordsForToday()
        //{
        //    string sqlWordsSelect = "SELECT * FROM Words;";

        //    using (var connection = new SqlConnection(
        //        "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
        //    {
        //        actualListOfWords = connection.Query<Word>(sqlWordsSelect).ToList();


        //    }
        //}
        private void AddNewWord()
        {
            string sqlWordInsert = "INSERT INTO Words VALUES(@PlWord,@EnWord,@NowDate,0,0);";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
           
                var addWord = connection.Execute(
                    sqlWordInsert,
                    new
                    {
                        PlWord = TranslatedWord,
                        EnWord = WordToTranslate,
                        NowDate =DateTime.Now
                    });
            }
        }

    }
}