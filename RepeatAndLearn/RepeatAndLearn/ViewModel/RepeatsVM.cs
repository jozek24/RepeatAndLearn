using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Input;

namespace RepeatAndLearn.ViewModel
{
    class RepeatsVM : BindableBase
    {
        private readonly RepeatsM _repeatsM = new RepeatsM();
        private readonly StoredProcedure _storedProcedure = new StoredProcedure();
        private readonly Random _random = new Random();
        private int _randomNumber;
        private List<Word> _listOfRepeatsToDo;

        private int _maxNumberOfRepeats;
        public int MaxNumberOfRepeats
        {
            get => _maxNumberOfRepeats;
            set => SetProperty(ref _maxNumberOfRepeats, value);
        }

        private int _numberOfRepeatsToDoToday;
        public int NumberOfRepeatsToDoToday
        {
            get => _numberOfRepeatsToDoToday;
            set => SetProperty(ref _numberOfRepeatsToDoToday, value);
        }

        private int _numberOfRepeats;
        public int NumberOfRepeats
        {
            get => _numberOfRepeats;
            set => SetProperty(ref _numberOfRepeats, value);
        }

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

        private string _wordToCheck;
        public string WordToCheck
        {
            get => _wordToCheck;
            set => SetProperty(ref _wordToCheck, value);
        }

        private string _correctAnswer;
        public string CorrectAnswer
        {
            get => _correctAnswer;
            set => SetProperty(ref _correctAnswer, value);
        }

        private string _myAnswer = "";
        public string MyAnswer
        {
            get => _myAnswer;
            set => SetProperty(ref _myAnswer, value);
        }

        public RepeatsVM()
        {
            GetListOfWordsToRepeatAndSetAmount();
            GlobalSettings.WordsChange += GlobalSettings_WordsChange;
            RandomWordToCheck();

            CheckAnswerCommand = new DelegateCommand(CheckAnswer);
            MyAnswerWrongCommand = new DelegateCommand(MyAnswerWrong);
            MyAnswerCorrectCommand = new DelegateCommand(MyAnswerCorrect);
            DeleteTranslatedWordCommand = new DelegateCommand(DeleteTranslatedOldWord);
        }

        private void GlobalSettings_WordsChange(object sender, EventArgs e)
        {
            GetListOfWordsToRepeatAndSetAmount();
        }

        public ICommand CheckAnswerCommand { get; }
        public ICommand MyAnswerWrongCommand { get; }
        public ICommand MyAnswerCorrectCommand { get; }
        public ICommand DeleteTranslatedWordCommand { get; }

        private void GetListOfWordsToRepeatAndSetAmount()
        {
            _listOfRepeatsToDo = GlobalSettings.ActualListOfWords.Where(x => x.DateOfNextRepeat.Date < DateTime.Now).ToList();

            NumberOfRepeats = _listOfRepeatsToDo.Count;
            if (_listOfRepeatsToDo.Count > MaxNumberOfRepeats)
                MaxNumberOfRepeats = _listOfRepeatsToDo.Count;
            NumberOfRepeatsToDoToday = MaxNumberOfRepeats - NumberOfRepeats;
        }

        private void RandomWordToCheck()
        {
            _randomNumber = _random.Next(_listOfRepeatsToDo.Count);
            WordToCheck = _listOfRepeatsToDo[_randomNumber].PlWord;
            CorrectAnswer = _listOfRepeatsToDo[_randomNumber].EnWord;
        }

        private void CheckAnswer()
        {
            if (AnswerButtonsVisibility == true)
            {
                if (CorrectAnswer.ToLower().Trim() == MyAnswer.ToLower().Trim())
                {
                    MyAnswerCorrect();
                    return;
                }
                UpdateListOnWrongAnswer();
                RandomWordToCheck();
                MyAnswer = "";
                Colour = "White";
                return;
            }

            AnswerButtonsVisibility = true;
            if (CorrectAnswer.ToLower().Trim() == MyAnswer.ToLower().Trim())
            {
                Colour = "LightGreen";
                return;
            }
            Colour = "IndianRed";
            string newAnswer = "Twoja odpowiedź: " + MyAnswer + "\n" + "Poprawna odpowiedź: " + CorrectAnswer;
            MyAnswer = newAnswer;
        }

        private void MyAnswerCorrect()
        {
            if (AnswerButtonsVisibility == false)
                return;
            UpdateListOnCorrectAnswer();
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
            _storedProcedure.UpdateWordOnCorrect(_listOfRepeatsToDo, _randomNumber, _repeatsM);
            AnswerButtonsVisibility = false;
        }
        private void UpdateListOnWrongAnswer()
        {
            _storedProcedure.UpdateWordOnWrong(_listOfRepeatsToDo, _randomNumber);
            AnswerButtonsVisibility = false;
        }

        private void DeleteTranslatedOldWord()
        {
            _storedProcedure.DeleteOldWord(WordToCheck, CorrectAnswer);
            AnswerButtonsVisibility = false;
            RandomWordToCheck();
            MyAnswer = "";
            Colour = "White";
        }
    }
}
