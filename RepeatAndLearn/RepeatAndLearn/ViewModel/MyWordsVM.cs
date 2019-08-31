using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class MyWordsVM :BindableBase
    {
        private string _plWordToAdd;
        public string PlWordToAdd
        {
            get => _plWordToAdd;
            set => SetProperty(ref _plWordToAdd, value);
        }

        private string _enWordToAdd;
        public string EnWordToAdd
        {
            get => _enWordToAdd;
            set => SetProperty(ref _enWordToAdd, value);
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

        private void AddMyNewWord()
        {
            if (!(CheckIfCanAddMyNewWord())) return;

            string sqlWordInsert = "INSERT INTO Words VALUES(@PlWord,@EnWord,@NowDate,0,0);";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                var addWord = connection.Execute(
                    sqlWordInsert,
                    new
                    {
                        PlWord = PlWordToAdd,
                        EnWord = EnWordToAdd,
                        NowDate = DateTime.Now
                    });

            }
            GlobalSettings.UpdateListOfWords();
            CanAddNewWord = false;
            CanDeleteWord = true;
        }

        private void DeleteMyOldWord()
        {
            string sqlWordDelete = "DELETE FROM Words WHERE(@PlWord,@EnWord);";

            using (var connection = new SqlConnection(
               "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                var addWord = connection.Execute(
                    sqlWordDelete,
                    new
                    {
                        PlWord = PlWordToAdd,
                        EnWord = EnWordToAdd
                    });

            }
            GlobalSettings.UpdateListOfWords();
            CanAddNewWord = true;
            CanDeleteWord = false;
        }
        private bool CheckIfCanAddMyNewWord()
        {

            if (
              GlobalSettings.actualListOfWords.Any(x => x.EnWord == EnWordToAdd.ToLower().Trim()
                && x.PlWord == PlWordToAdd.ToLower().Trim()))
            {
                CanAddNewWord = false;
                CanDeleteWord = true;
                MessageBox.Show("To słówko już istnieje w twojej liście.");
                return false;
            }
            CanAddNewWord = true;
            CanDeleteWord = false;
            return true;
        }

    }
}
