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
        private RepeatsM _repeatsM = new RepeatsM();
        private StoredProcedure _storedProcedure = new StoredProcedure();
        private List<Word> listOfRepeatsToDo;
        private Random random = new Random();
        private int _randomNumber;

        private int _maxNumberOfRepeats;
        public int MaxNumberOfRepeats
        {
            get => _maxNumberOfRepeats;
            set => SetProperty(ref _maxNumberOfRepeats, value);
        }

        private int _numberOfRepeatsToDo;
        public int NumberOfRepeatsToDo
        {
            get => _numberOfRepeatsToDo;
            set => SetProperty(ref _numberOfRepeatsToDo, value);
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

            MaxNumberOfRepeats = NumberOfRepeats;
            NumberOfRepeatsToDo = MaxNumberOfRepeats - NumberOfRepeats;

            GlobalSettings.WordsChange += GlobalSettings_WordsChange;

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
            listOfRepeatsToDo = GlobalSettings.actualListOfWords.Where(x => x.DateOfNextRepeat.Date < DateTime.Now).ToList();
            NumberOfRepeats = listOfRepeatsToDo.Count;
            if (listOfRepeatsToDo.Count > MaxNumberOfRepeats)
                MaxNumberOfRepeats = listOfRepeatsToDo.Count;
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
           // string newAnswer = "Twoja odpowiedź: " + MyAnswer + "\n" + "Poprawna odpowiedź: " + CorrectAnswer;
            MyAnswer = CorrectAnswer;
        }

        private void MyAnswerCorrect()
        {
            if (AnswerButtonsVisibility == false)
                return;
            UpdateListOnCorrectAnswer();

            RandomWordToCheck();
            MyAnswer = "";
            Colour = "White";
            NumberOfRepeatsToDo = MaxNumberOfRepeats - NumberOfRepeats;
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
            _storedProcedure.UpdateWordOnCorrect(listOfRepeatsToDo, _randomNumber, _repeatsM);
            AnswerButtonsVisibility = false;
        }
        private void UpdateListOnWrongAnswer()
        {

            _storedProcedure.UpdateWordOnWrong(listOfRepeatsToDo, _randomNumber);
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
