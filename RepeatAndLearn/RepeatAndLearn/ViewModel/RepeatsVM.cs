﻿using Dapper;
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
        private RepeatsM _repeatsM = new RepeatsM();
        private StoredProcedure _storedProcedure = new StoredProcedure();
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
            _storedProcedure.UpdateWordOnCorrect(listOfRepeatsToDo, _randomNumber, _repeatsM);

            GlobalSettings.UpdateListOfWords();
            GetListOfWordsToRepeatAndSetAmount();
            AnswerButtonsVisibility = false;
        }
        private void UpdateListOnWrongAnswer()
        {

            _storedProcedure.UpdateWordOnWrong(listOfRepeatsToDo, _randomNumber);

            GlobalSettings.UpdateListOfWords();
            GetListOfWordsToRepeatAndSetAmount();
            AnswerButtonsVisibility = false;
        }
        
        private void DeleteTranslatedOldWord()
        {

            _storedProcedure.DeleteOldWord(WordToCheck, CorrectAnswer);

            GlobalSettings.UpdateListOfWords();
            GetListOfWordsToRepeatAndSetAmount();
            AnswerButtonsVisibility = false;
            RandomWordToCheck();
            MyAnswer = "";
            Colour = "White";
        }
    }
}
