using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class RepeatsVM : BindableBase
    {
        private List<Word> listOfRepeatsToDo;
        private Random random = new Random();
        private int _randomNumber;

        private string _colour = "White";
        public string Colour
        {
            get => _colour;
            set => SetProperty(ref _colour, value);
        }

        private bool _answerButtonsVisibility = false;
        public bool AnswerButtonsVisibility
        {
            get => _answerButtonsVisibility;
            set => SetProperty(ref _answerButtonsVisibility, value);
        }

        private int _numberOfRepeats;
        public int NumberOfRepeats
        {
            get => _numberOfRepeats;
            set => SetProperty(ref _numberOfRepeats, value);
        }

        private string _wordToCheck;
        public string WordToCheck
        {
            get => _wordToCheck;
            set => SetProperty(ref _wordToCheck, value);
        }

        private bool _ifMyAnswerCorrect = false;
        public bool IfMyAnswerCorrect
        {
            get => _ifMyAnswerCorrect;
            set => SetProperty(ref _ifMyAnswerCorrect, value);
        }

        private string _correctAnswer;
        public string CorrectAnswer
        {
            get => _correctAnswer;
            set => SetProperty(ref _correctAnswer, value);
        }

        private string _myAnswer="";
        public string MyAnswer
        {
            get => _myAnswer;
            set => SetProperty(ref _myAnswer, value);
        }


        public RepeatsVM()
        {
            GetListOfWordsToRepeatAndSetAmount();
            RandomWordToCheck();

            CheckAnswerCommand = new DelegateCommand(CheckAnswer);
            MyAnswerWrongCommand = new DelegateCommand(MyAnswerWrong);
            MyAnswerCorrectCommand = new DelegateCommand(MyAnswerCorrect);
            DeleteTranslatedWordCommand = new DelegateCommand(DeleteTranslatedOldWord);

        }
        public ICommand CheckAnswerCommand { get; }
        public ICommand MyAnswerWrongCommand { get; }
        public ICommand MyAnswerCorrectCommand { get; }
        public ICommand DeleteTranslatedWordCommand { get; }



        private void GetListOfWordsToRepeatAndSetAmount()
        {
            listOfRepeatsToDo = GlobalSettings.actualListOfWords.Where(x => x.DateOfNextRepeat.Date < DateTime.Now).ToList();
            NumberOfRepeats = listOfRepeatsToDo.Count;
        }
        private void RandomWordToCheck()
        {
            IfMyAnswerCorrect = false;
            _randomNumber = random.Next(listOfRepeatsToDo.Count);
            WordToCheck = listOfRepeatsToDo[_randomNumber].PlWord;
            CorrectAnswer = listOfRepeatsToDo[_randomNumber].EnWord;
        }

        private void CheckAnswer()
        {
            AnswerButtonsVisibility = true;
            if (CorrectAnswer.ToLower().Trim() == MyAnswer.ToLower().Trim())
            {
                Colour = "LightGreen";
                IfMyAnswerCorrect = true;
                return;
            }
            IfMyAnswerCorrect = false;
            Colour = "IndianRed";
            MyAnswer = CorrectAnswer;
        }

        private void MyAnswerCorrect()
        {
            if (AnswerButtonsVisibility == false)
                return;
            UpdateListOnCorrectAnswer();
            listOfRepeatsToDo.RemoveAt(_randomNumber);
            RandomWordToCheck();
            MyAnswer = "";
            Colour = "White";
        }

        private void MyAnswerWrong()
        {
            if (AnswerButtonsVisibility == false)
                return;
            UpdateListOnWrongAnswer();
            RandomWordToCheck();
            MyAnswer = "";
            Colour = "White";
        }
        private void UpdateListOnCorrectAnswer()
        {
            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@idOfWord", listOfRepeatsToDo[_randomNumber].IdWord);
                    param.Add(
                        "@dateOfNextRepeat",
                    listOfRepeatsToDo[_randomNumber].DateOfNextRepeat
                          .AddDays(DaysToNextRepeat(
                              listOfRepeatsToDo[_randomNumber].TotalAmountOfRepeats,
                              listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats)
                              ));
                    param.Add("@totalAmountOfRepeats", listOfRepeatsToDo[_randomNumber].TotalAmountOfRepeats + 1);
                    connection.Execute("UpdateWordOnCorrect", param, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            GlobalSettings.UpdateListOfWords();
            GetListOfWordsToRepeatAndSetAmount();
            AnswerButtonsVisibility = false;
        }
        private void UpdateListOnWrongAnswer()
        {

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@idOfWord", listOfRepeatsToDo[_randomNumber].IdWord);

                    param.Add(
                        "@dateOfNextRepeat", DateTime.Now);
                    param.Add("@currentAmountOfRepeats", listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats+1);
                    connection.Execute("UpdateWordOnWrong", param, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            GlobalSettings.UpdateListOfWords();
            GetListOfWordsToRepeatAndSetAmount();
            AnswerButtonsVisibility = false;
        }
        private int DaysToNextRepeat(int totalRepeats, int currentRepeats)
        {
            if (totalRepeats <= 1)
            {
                if (currentRepeats <= 2)
                    return 5;

                return 2;
            }
            else if (totalRepeats >= 2 && totalRepeats <= 5)
            {
                if (currentRepeats <= 2)
                    return 14;
                return 10;
            }
            else if (totalRepeats >= 6 && totalRepeats <= 9)
            {
                if (currentRepeats <= 2)
                    return 31;
                return 18;
            }
            else
            {
                if (currentRepeats < 2)
                    return 100;
                return 365;
            }
        }
        private void DeleteTranslatedOldWord()
        {

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@plWord", WordToCheck);
                    param.Add("@enWord", CorrectAnswer);

                    connection.Execute("DeleteOldWord", param, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            GlobalSettings.UpdateListOfWords();
            GetListOfWordsToRepeatAndSetAmount();
            AnswerButtonsVisibility = false;
            RandomWordToCheck();
            MyAnswer = "";
            Colour = "White";
        }
    }
}
