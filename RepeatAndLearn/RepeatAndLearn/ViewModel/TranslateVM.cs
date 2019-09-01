using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using RestSharp;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class TranslateVM : BindableBase
    {
        private string _wordToTranslate;
        public string WordToTranslate
        {
            get => _wordToTranslate;
            set
            {
                TranslatedWord = "";
                CanAddNewWord = false;
                CanDeleteWord = false;

                SetProperty(ref _wordToTranslate, value);
                if (_wordToTranslate == "")
                {
                    CanAddNewWord = false;
                    CanDeleteWord = false;
                }
            }
        }

        private string _translatedWord;
        public string TranslatedWord
        {
            get => _translatedWord;
            set
            {
                CanAddNewWord = true;
                SetProperty(ref _translatedWord, value);
            }
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
            LanguageDirectionCommand = new DelegateCommand(ChangeLanguageDirection);
            AddTranslatedWordCommand = new DelegateCommand(AddTranslatedNewWord);
            DeleteTranslatedWordCommand = new DelegateCommand(DeleteTranslatedOldWord);

        }

        public ICommand ApiExecuteCommand { get; }
        public ICommand LanguageDirectionCommand { get; }
        public ICommand AddTranslatedWordCommand { get; }
        public ICommand DeleteTranslatedWordCommand { get; }

        private void Execute()
        {
            var client = new RestClient(
                "https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + GlobalSettings.ApiKey);
            var request = new RestRequest(Method.GET);
            request.AddParameter("text", WordToTranslate);
            request.AddParameter("lang", LanguageDirection);

            var response = client.Execute<DictionaryResponse>(request);
            if (response.IsSuccessful)
                TranslatedWord = response.Data.Text[0];

            CheckIfCanAddTranslatedNewWord();
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
        private void ChangeLanguageDirection()
        {
            CanAddNewWord = false;
            CanDeleteWord = false;
            LanguageDirection = (LanguageDirection == "en-pl") ? "pl-en" : "en-pl";
        }

        private void AddTranslatedNewWord()
        {
            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    if (LanguageDirection == "en-pl")
                    {
                        param.Add("@plWord", TranslatedWord.ToLower().Trim());
                        param.Add("@enWord", WordToTranslate.ToLower().Trim());
                        param.Add("@dateOfNextRepeat", DateTime.Now);
                        param.Add("@currentAmountOfRepeats", 0);
                        param.Add("@totalAmountOfRepeats", 0);

                        connection.Execute("AddNewWord", param, commandType: CommandType.StoredProcedure);
                    }
                    else
                    {
                        param.Add("@plWord", WordToTranslate.ToLower().Trim());
                        param.Add("@enWord", TranslatedWord.ToLower().Trim());
                        param.Add("@dateOfNextRepeat", DateTime.Now);
                        param.Add("@currentAmountOfRepeats", 0);
                        param.Add("@totalAmountOfRepeats", 0);

                        connection.Execute("AddNewWord", param, commandType: CommandType.StoredProcedure);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            GlobalSettings.UpdateListOfWords();
            CanAddNewWord = false;
            CanDeleteWord = true;
        }


        private void DeleteTranslatedOldWord()
        {

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    if (LanguageDirection == "en-pl")
                    {
                        param.Add("@plWord", TranslatedWord);
                        param.Add("@enWord", WordToTranslate);

                        connection.Execute("DeleteOldWord", param, commandType: CommandType.StoredProcedure);
                    }
                    else
                    {
                        param.Add("@plWord", WordToTranslate);
                        param.Add("@enWord", TranslatedWord);

                        connection.Execute("DeleteOldWord", param, commandType: CommandType.StoredProcedure);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            GlobalSettings.UpdateListOfWords();
            CanAddNewWord = true;
            CanDeleteWord = false;
        }


    }
}
