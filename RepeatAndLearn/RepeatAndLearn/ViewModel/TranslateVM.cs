using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using RestSharp;
using System.Linq;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class TranslateVM : BindableBase
    {
        private StoredProcedure _storedProcedure = new StoredProcedure();

        private string _wordToTranslate = "";
        public string WordToTranslate
        {
            get => _wordToTranslate;
            set
            {
                TranslatedWord = "";
                CanAddNewWord = false;
                CanDeleteWord = false;

                SetProperty(ref _wordToTranslate, value);
            }
        }

        private string _translatedWord = "";
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
                GlobalSettings.TranslateHttps + GlobalSettings.ApiKey);

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
            if (WordToTranslate == "")
            {
                return;
            }

            if (LanguageDirection == "en-pl")
            {
                if (CheckIfTheWordAlreadyExist(WordToTranslate, TranslatedWord))
                {
                    CanAddNewWord = false;
                    CanDeleteWord = true;
                    return;
                }
            }
            else
            {
                if (CheckIfTheWordAlreadyExist(TranslatedWord, WordToTranslate))
                {
                    CanAddNewWord = false;
                    CanDeleteWord = true;
                    return;
                }
            }
            CanAddNewWord = true;
            CanDeleteWord = false;
        }
        private bool CheckIfTheWordAlreadyExist(string enWord, string plWord)
        {
            if (GlobalSettings.actualListOfWords.Any(x => x.EnWord == enWord.ToLower().Trim()
                   && x.PlWord == plWord.ToLower().Trim()))
            {
                return true;
            }
            return false;
        }
        private void ChangeLanguageDirection()
        {
            if(TranslatedWord!="")
                WordToTranslate = TranslatedWord;

            LanguageDirection = (LanguageDirection == "en-pl") ? "pl-en" : "en-pl";
        }

        private void AddTranslatedNewWord()
        {
            if (LanguageDirection == "en-pl")
            {
                _storedProcedure.AddNewWord(TranslatedWord, WordToTranslate);
            }
            else
            {
                _storedProcedure.AddNewWord(WordToTranslate, TranslatedWord);
            }
            CanAddNewWord = false;
            CanDeleteWord = true;
        }


        private void DeleteTranslatedOldWord()
        {

            if (LanguageDirection == "en-pl")
            {
                _storedProcedure.DeleteOldWord(TranslatedWord, WordToTranslate);
            }
            else
            {
                _storedProcedure.DeleteOldWord(WordToTranslate, TranslatedWord);
            }
            CanAddNewWord = true;
            CanDeleteWord = false;
        }


    }
}
