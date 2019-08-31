using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class TranslateVM : BindableBase
    {
        private string _wordToTranslate;
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

        private string _languageDirection = "en-pl";
        public string LanguageDirection
        {
            get => _languageDirection;
            set => SetProperty(ref _languageDirection, value);
        }
        private bool _canAddNewWord;
        public bool CanAddNewWord
        {
            get => _canAddNewWord;
            set => SetProperty(ref _canAddNewWord, value);
        }

        private bool _canDeleteWord = false;

        public bool CanDeleteWord
        {
            get => _canDeleteWord;
            set => SetProperty(ref _canDeleteWord, value);
        }

        public TranslateVM()
        {
            ApiExecuteCommand = new DelegateCommand(Execute);

            AddTranslatedWordCommand = new DelegateCommand(AddTranslatedNewWord);
            DeleteTranslatedWordCommand = new DelegateCommand(DeleteTranslatedOldWord);
            LanguageDirectionCommand = new DelegateCommand(ChangeLanguageDirection);
        }


        public ICommand AddTranslatedWordCommand { get; }
        public ICommand DeleteTranslatedWordCommand { get; }
        public ICommand LanguageDirectionCommand { get; }
        public ICommand ApiExecuteCommand { get; }
        private void Execute()
        {
            var client = new RestClient("https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + GlobalSettings.ApiKey);
            var request = new RestRequest(Method.GET);
            request.AddParameter("text", WordToTranslate);
            request.AddParameter("lang", LanguageDirection);

            var response = client.Execute<DictionaryResponse>(request);
            if (response.IsSuccessful)
                TranslatedWord = response.Data.Text[0];

            CheckIfCanAddTranslatedNewWord();
        }


        private void ChangeLanguageDirection()
        {
            LanguageDirection = (LanguageDirection == "en-pl") ? "pl-en" : "en-pl";
        }

        private void AddTranslatedNewWord()
        {
            string sqlWordInsert = "INSERT INTO Words VALUES(@PlWord,@EnWord,@NowDate,0,0);";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                if (LanguageDirection == "en-pl")
                {
                    var addWord = connection.Execute(
                        sqlWordInsert,
                        new
                        {
                            PlWord = TranslatedWord,
                            EnWord = WordToTranslate,
                            NowDate = DateTime.Now
                        });
                }
                else
                {
                    var addWord = connection.Execute(
                            sqlWordInsert,
                            new
                            {
                                PlWord = WordToTranslate,
                                EnWord = TranslatedWord,
                                NowDate = DateTime.Now
                            });
                }
            }
            GlobalSettings.UpdateListOfWords();
            CanAddNewWord = false;
            CanDeleteWord = true;
        }

        private void DeleteTranslatedOldWord()
        {
            string sqlWordDelete = "DELETE FROM Words WHERE(@PlWord,@EnWord);";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                if (LanguageDirection == "en-pl")
                {
                    var deleteWord = connection.Execute(
                        sqlWordDelete,
                        new
                        {
                            PlWord = TranslatedWord,
                            EnWord = WordToTranslate
                        });
                }
                else
                {
                    var deleteWord = connection.Execute(
                            sqlWordDelete,
                            new
                            {
                                PlWord = WordToTranslate,
                                EnWord = TranslatedWord,
                                NowDate = DateTime.Now
                            });
                }
            }
           GlobalSettings.UpdateListOfWords();
            CanAddNewWord = true;
            CanDeleteWord = false;
        }

        private void CheckIfCanAddTranslatedNewWord()
        {
            if (LanguageDirection == "en-pl")
            {
                if (
                    GlobalSettings.actualListOfWords.Any(x => x.EnWord == WordToTranslate.ToLower().Trim()
                    && x.PlWord == TranslatedWord.ToLower().Trim()))
                {
                    CanAddNewWord = false;
                    CanDeleteWord = true;
                    return;
                }
                CanAddNewWord = true;
                CanDeleteWord = false;
            }
            else
            {
                if (
                   GlobalSettings.actualListOfWords.Any(x => x.EnWord == TranslatedWord.ToLower().Trim()
                   && x.PlWord == WordToTranslate.ToLower().Trim()))
                {
                    CanAddNewWord = false;
                    CanDeleteWord = true;
                    return;
                }
                CanAddNewWord = true;
                CanDeleteWord = false;
            }
        }

    }
}
