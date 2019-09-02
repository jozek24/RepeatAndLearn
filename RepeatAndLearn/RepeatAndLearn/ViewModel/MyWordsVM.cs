using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class MyWordsVM : BindableBase
    {
        private readonly StoredProcedure _storedProcedure = new StoredProcedure();

        private string _plWordToAdd = "";
        public string PlWordToAdd
        {
            get => _plWordToAdd;
            set
            {
                SetProperty(ref _plWordToAdd, value);
                CheckIfCanAddWord();
            }
        }

        private string _enWordToAdd = "";
        public string EnWordToAdd
        {
            get => _enWordToAdd;
            set
            {
                SetProperty(ref _enWordToAdd, value);
                CheckIfCanAddWord();
            }
        }

        private bool _canAddNewWord = false;
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

        public MyWordsVM()
        {
            AddMyWordCommand = new DelegateCommand(AddMyNewWord);
            DeleteMyWordCommand = new DelegateCommand(DeleteMyOldWord);
        }

        public ICommand AddMyWordCommand { get; }
        public ICommand DeleteMyWordCommand { get; }

        private void CheckIfCanAddWord()
        {
            if (_plWordToAdd == "" || _enWordToAdd == "")
            {
                CanAddNewWord = false;
                CanDeleteWord = false;
                return;
            }
            CanAddNewWord = true;
            CanDeleteWord = false;
        }
        private void AddMyNewWord()
        {
            if (PlWordToAdd == "" || EnWordToAdd == "")
                return;

            CanAddNewWord = false;
            CanDeleteWord = true;
            if (!CheckIfCanAddMyNewWord()) return;
            _storedProcedure.AddNewWord(PlWordToAdd, EnWordToAdd);

        }

        private void DeleteMyOldWord()
        {
            CanAddNewWord = true;
            CanDeleteWord = false;
            _storedProcedure.DeleteOldWord(PlWordToAdd, EnWordToAdd);
        }
        private bool CheckIfCanAddMyNewWord()
        {
            if (
              GlobalSettings.ActualListOfWords
              .Any(x => x.EnWord == EnWordToAdd.ToLower().Trim()
                && x.PlWord == PlWordToAdd.ToLower().Trim())
              )
            {
                MessageBox.Show("To słówko już istnieje w twojej liście.");
                return false;
            }
            return true;
        }

    }
}
