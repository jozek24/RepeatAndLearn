using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dapper;
using Prism.Commands;
using Prism.Mvvm;
using RepeatAndLearn.Model;
using RestSharp;

namespace RepeatAndLearn.ViewModel
{
    class MyViewModel : BindableBase
    {
        const string ApiKey = "trnsl.1.1.20190821T170155Z.fafa71ea078787a0.bbbe06c23ca22760e5f05eb512ae2b37f7c110f1";
        private List<Word> actualListOfWords;
        private List<Word> listOfRepeatsToDo;
        private Random random = new Random();
        private int _randomNumber;
        #region all properties



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

        private string _wordToTranslate;
        public string WordToTranslate
        {
            get => _wordToTranslate;
            set => SetProperty(ref _wordToTranslate, value);
        }

        private string _translatedWord;
        public string TranslatedWord
        {
            get => _translatedWord;
            set => SetProperty(ref _translatedWord, value);
        }

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

        private string _languageDirection = "en-pl";
        public string LanguageDirection
        {
            get => _languageDirection;
            set => SetProperty(ref _languageDirection, value);
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
        #endregion

        public MyViewModel()
        {
            UpdateListOfWords();
            actualListOfWords = new List<Word>();
            GetListOfWordsToRepeatAndSetAmount();
            RandomWordToCheck();
            ApiExecuteCommand = new DelegateCommand(Execute);

            CheckAnswerCommand = new DelegateCommand(CheckAnswer);
            RandomWordToCheckCommand = new DelegateCommand(RandomWordToCheck);
            MyAnswerWrongCommand = new DelegateCommand(MyAnswerWrong);
            MyAnswerCorrectCommand = new DelegateCommand(MyAnswerCorrect);

            AddTranslatedWordCommand = new DelegateCommand(AddTranslatedNewWord);
            DeleteTranslatedWordCommand = new DelegateCommand(DeleteTranslatedOldWord);
            LanguageDirectionCommand = new DelegateCommand(ChangeLanguageDirection);

            AddMyWordCommand = new DelegateCommand(AddMyNewWord);
            DeleteTranslatedWordCommand = new DelegateCommand(DeleteMyOldWord);
        }

        public ICommand ApiExecuteCommand { get; }

        public ICommand CheckAnswerCommand { get; }
        public ICommand RandomWordToCheckCommand { get; }
        public ICommand MyAnswerWrongCommand { get; }
        public ICommand MyAnswerCorrectCommand { get; }

        public ICommand AddTranslatedWordCommand { get; }
        public ICommand DeleteTranslatedWordCommand { get; }
        public ICommand LanguageDirectionCommand { get; }

        public ICommand AddMyWordCommand { get; }
        public ICommand DeleteMyWordCommand { get; }

        private void Execute()
        {
            var client = new RestClient("https://translate.yandex.net/api/v1.5/tr.json/translate?key=" + ApiKey);
            var request = new RestRequest(Method.GET);
            request.AddParameter("text", WordToTranslate);
            request.AddParameter("lang", LanguageDirection);

            var response = client.Execute<DictionaryResponse>(request);
            if (response.IsSuccessful)
                TranslatedWord = response.Data.Text[0];

            CheckIfCanAddTranslatedNewWord();
        }

        private void UpdateListOfWords()
        {
            string sqlWordsSelect = "SELECT * FROM Words;";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                actualListOfWords = connection.Query<Word>(sqlWordsSelect).ToList();
            }
        }

        #region translate operations

        private void ChangeLanguageDirection()
        {
            LanguageDirection = (LanguageDirection == "en-pl") ? "pl-en" : "en-pl";
        }

        private void AddTranslatedNewWord()
        {
            string sqlWordInsert = "INSERT INTO Words VALUES(@PlWord,@EnWord,@NowDate,0,0);";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                if (LanguageDirection == "en-pl")
                {
                    var addWord = connection.Execute(
                        sqlWordInsert,
                        new
                        {
                            PlWord = TranslatedWord,
                            EnWord = WordToTranslate,
                            NowDate = DateTime.Now
                        });
                }
                else
                {
                    var addWord = connection.Execute(
                            sqlWordInsert,
                            new
                            {
                                PlWord = WordToTranslate,
                                EnWord = TranslatedWord,
                                NowDate = DateTime.Now
                            });
                }
            }
            UpdateListOfWords();
            CanAddNewWord = false;
            CanDeleteWord = true;
        }

        private void DeleteTranslatedOldWord()
        {
            //do sprawdzenia
            string sqlWordDelete = "DELETE FROM Words WHERE(@PlWord,@EnWord);";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                if (LanguageDirection == "en-pl")
                {
                    var deleteWord = connection.Execute(
                        sqlWordDelete,
                        new
                        {
                            PlWord = TranslatedWord,
                            EnWord = WordToTranslate,
                            NowDate = DateTime.Now
                        });
                }
                else
                {
                    var deleteWord = connection.Execute(
                            sqlWordDelete,
                            new
                            {
                                PlWord = WordToTranslate,
                                EnWord = TranslatedWord,
                                NowDate = DateTime.Now
                            });
                }
            }
            UpdateListOfWords();
            CanAddNewWord = true;
            CanDeleteWord = false;
        }

        private void CheckIfCanAddTranslatedNewWord()
        {
            if (LanguageDirection == "en-pl")
            {
                if (
                    actualListOfWords.Any(x => x.EnWord == WordToTranslate.ToLower().Trim()
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
                   actualListOfWords.Any(x => x.EnWord == TranslatedWord.ToLower().Trim()
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
        #endregion

        #region add my words operations

        private void AddMyNewWord()
        {
            CheckIfCanAddMyNewWord();
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
            UpdateListOfWords();
            CanAddNewWord = false;
            CanDeleteWord = true;
        }

        private void DeleteMyOldWord()
        {
            //do sprawdzenia
            string sqlWordDelete = "DELETE FROM Words WHERE(@PlWord,@EnWord);";

            using (var connection = new SqlConnection(
               "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                var addWord = connection.Execute(
                    sqlWordDelete,
                    new
                    {
                        PlWord = PlWordToAdd,
                        EnWord = EnWordToAdd,
                        NowDate = DateTime.Now
                    });

            }

            UpdateListOfWords();
            CanAddNewWord = true;
            CanDeleteWord = false;
        }

        private void CheckIfCanAddMyNewWord()
        {

            if (
                actualListOfWords.Any(x => x.EnWord == WordToTranslate.ToLower().Trim()
                && x.PlWord == TranslatedWord.ToLower().Trim()))
            {
                CanAddNewWord = false;
                CanDeleteWord = true;
                MessageBox.Show("To słówko już istnieje w twojej liście.");
                return;
            }
            CanAddNewWord = true;
            CanDeleteWord = false;
        }

        #endregion

        #region repeats operations

        private void GetListOfWordsToRepeatAndSetAmount()
        {
            listOfRepeatsToDo = actualListOfWords.Where(x => x.DateOfNextRepeat.Date < DateTime.Now).ToList();
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
            string sqlWordUpdate = "UPDATE Words " +
                   "SET DateOfNextRepeat=@nextRepeatDate, CurrentAmountOfRepeats=0, TotalAmountOfRepeats=@totalRepeatsAmount " +
                   "WHERE IdWord=@idOfWord;";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                var updateWord = connection.Execute(
                          sqlWordUpdate,
                          new
                          {
                              nextRepeatDate = listOfRepeatsToDo[_randomNumber].DateOfNextRepeat
                              .AddDays(DaysToNextRepeat(
                                  listOfRepeatsToDo[_randomNumber].TotalAmountOfRepeats,
                                  listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats)
                                  ),
                              totalRepeatsAmount = listOfRepeatsToDo[_randomNumber].TotalAmountOfRepeats++,
                              idOfWord = listOfRepeatsToDo[_randomNumber].IdWord
                          });
            }
        }

        private void UpdateListOnWrongAnswer()
        {
            string sqlWordUpdate = "UPDATE Words " +
                   "SET CurrentAmountOfRepeats=@currenRepeatsAmount" +
                   "WHERE IdWord=@idOfWord;";

            using (var connection = new SqlConnection(
                "Data Source=LAPTOP-912THUH4;Initial Catalog=RepeatAndLearnDictionary;Integrated Security=true;"))
            {
                var updateWord = connection.Execute(
                          sqlWordUpdate,
                          new
                          {
                              currenRepeatsAmount = listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats++,
                              idOfWord = listOfRepeatsToDo[_randomNumber].IdWord
                          });
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

        #endregion
    }
}