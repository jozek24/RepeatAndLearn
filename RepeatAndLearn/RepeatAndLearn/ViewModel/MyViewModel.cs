using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using RestSharp;

namespace RepeatAndLearn.ViewModel
{
    class MyViewModel :BindableBase
    {
        const string apiKey = "trnsl.1.1.20190821T170155Z.fafa71ea078787a0.bbbe06c23ca22760e5f05eb512ae2b37f7c110f1";

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
            ApiExecuteCommand = new DelegateCommand(Execute);
        }

        public ICommand ApiExecuteCommand { get; }

        private void Execute()
        {
            var client = new RestClient("https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + apiKey);
            var request = new RestRequest(Method.GET);
            request.AddParameter("text", WordToTranslate);
            request.AddParameter("lang", "en-pl");

            var response = client.Execute<DictionaryResponse>(request);
            if (response.IsSuccessful)
                TranslatedWord = response.Data.Text[0];
        }
    }
}
   
       
   

