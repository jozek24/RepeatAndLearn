using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class MyWordsVM : BindableBase
    {

        private string _plWordToAdd="";
        public string PlWordToAdd
        {
            get => _plWordToAdd;
            set
            {
                CanAddNewWord = true;
                CanDeleteWord = false;
                SetProperty(ref _plWordToAdd, value);
                if (_plWordToAdd == "" || _enWordToAdd == "")
                {
                    CanAddNewWord = false;
                    CanDeleteWord = false;
                }
            }
        }

        private string _enWordToAdd="";
        public string EnWordToAdd
        {
            get => _enWordToAdd;
            set
            {
                CanAddNewWord = true;
                CanDeleteWord = false;
                SetProperty(ref _enWordToAdd, value);
                if (_plWordToAdd == "" || _enWordToAdd == "")
                {
                    CanAddNewWord = false;
                    CanDeleteWord = false;
                }
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
            AddMyWordCommand = new DelegateCommand(AddMyNewWord).ObservesProperty(() => PlWordToAdd);
            DeleteMyWordCommand = new DelegateCommand(DeleteMyOldWord).ObservesProperty(() => PlWordToAdd);
        }

        public ICommand AddMyWordCommand { get; }
        public ICommand DeleteMyWordCommand { get; }

        private void AddMyNewWord()
        {
            if (!CheckIfCanAddMyNewWord()) return;

            using (var connection = new SqlConnection(
              "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@plWord", PlWordToAdd.ToLower().Trim());
                    param.Add("@enWord", EnWordToAdd.ToLower().Trim());
                    param.Add("@dateOfNextRepeat", DateTime.Now);
                    param.Add("@currentAmountOfRepeats", 0);
                    param.Add("@totalAmountOfRepeats", 0);

                    connection.Execute("AddNewWord", param, commandType: CommandType.StoredProcedure);
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

        private void DeleteMyOldWord()
        {
            using (var connection = new SqlConnection(
               "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@plWord", PlWordToAdd);
                    param.Add("@enWord", EnWordToAdd);

                    connection.Execute("DeleteOldWord", param, commandType: CommandType.StoredProcedure);
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
