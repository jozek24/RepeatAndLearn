using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class RepeatsVM : BindableBase
    {
        private List<Word> listOfRepeatsToDo;
        private Random random = new Random();
        private int _randomNumber;


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

        private string _myAnswer;
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
            RandomWordToCheckCommand = new DelegateCommand(RandomWordToCheck);
            MyAnswerWrongCommand = new DelegateCommand(MyAnswerWrong);
            MyAnswerCorrectCommand = new DelegateCommand(MyAnswerCorrect);

        }
        public ICommand CheckAnswerCommand { get; }
        public ICommand RandomWordToCheckCommand { get; }
        public ICommand MyAnswerWrongCommand { get; }
        public ICommand MyAnswerCorrectCommand { get; }



        private void GetListOfWordsToRepeatAndSetAmount()
        {
            listOfRepeatsToDo = GlobalSettings.actualListOfWords.Where(x => x.DateOfNextRepeat.Date < DateTime.Now).ToList();
            NumberOfRepeats = listOfRepeatsToDo.Count;
        }
        private void RandomWordToCheck()
        {
            IfMyAnswerCorrect = false;
            _randomNumber = random.Next(listOfRepeatsToDo.Count);
            //    WordToCheck = listOfRepeatsToDo[_randomNumber].PlWord;
            // CorrectAnswer = listOfRepeatsToDo[_randomNumber].EnWord;
        }

        private void CheckAnswer()
        {
            if (CorrectAnswer.ToLower().Trim() == MyAnswer.ToLower().Trim())
            {
                IfMyAnswerCorrect = true;
                return;
            }
            IfMyAnswerCorrect = false;
        }

        private void MyAnswerCorrect()
        {
            UpdateListOnCorrectAnswer();
            NumberOfRepeats--;  //powiadomić jeśli wszyskie wykonane
            listOfRepeatsToDo.RemoveAt(_randomNumber);
            RandomWordToCheck();
        }

        private void MyAnswerWrong()
        {
            UpdateListOnWrongAnswer();
            RandomWordToCheck();
        }
        private void UpdateListOnCorrectAnswer()
        {
            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@idOfWord ", listOfRepeatsToDo[_randomNumber].IdWord);
                    param.Add(
                        "@dateOfNextRepeat ",
                        listOfRepeatsToDo[_randomNumber].DateOfNextRepeat
                              .AddDays(DaysToNextRepeat(
                                  listOfRepeatsToDo[_randomNumber].TotalAmountOfRepeats,
                                  listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats)
                                  ));
                    param.Add("@totalAmountOfRepeats ", listOfRepeatsToDo[_randomNumber].TotalAmountOfRepeats++);

                    connection.Execute("UpdateWordOnCorrect", param, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
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
                    param.Add("@currentAmountOfRepeats", listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats++);
                    connection.Execute("UpdateWordOnWrong", param, commandType: CommandType.StoredProcedure);
                    connection.Execute("UpdateWordOnWrong", param, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private int DaysToNextRepeat(int totalRepeats, int currentRepeats)
        {

            // int total = listOfRepeatsToDo[_randomNumber].TotalAmountOfRepeats; //im mniejsze tym szybciej
            //int current = listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats; //im mniejsze tym później

            if (totalRepeats <= 1) //if is new
            {
                if (currentRepeats <= 2) // and easy
                    return 5;

                return 2; // and difficult
            }
            else if (totalRepeats >= 2 && totalRepeats <= 5) //if is average
            {
                if (currentRepeats <= 2) // and easy
                    return 14;
                return 10; // and difficult
            }
            else if (totalRepeats >= 6 && totalRepeats <= 9) //if is old
            {
                if (currentRepeats <= 2) // and easy
                    return 31;
                return 18; //and difficult
            }
            else //when is old
            {
                if (currentRepeats < 2)
                    return 100;
                return 365;
            }
        }
    }
}
