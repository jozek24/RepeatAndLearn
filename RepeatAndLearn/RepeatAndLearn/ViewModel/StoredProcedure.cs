using Dapper;
using RepeatAndLearn.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RepeatAndLearn.ViewModel
{
    class StoredProcedure
    {
        public void UpdateWordOnCorrect(List<Word> listOfRepeatsToDo, int _randomNumber, RepeatsM repeatsM)
        {
            using (var connection = new SqlConnection(
                    GlobalSettings.ConnectionString))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@idOfWord", listOfRepeatsToDo[_randomNumber].IdWord);
                    param.Add(
                        "@dateOfNextRepeat",
                    listOfRepeatsToDo[_randomNumber].DateOfNextRepeat
                          .AddDays(repeatsM.DaysToNextRepeat(
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
        }

        public void UpdateWordOnWrong(List<Word> listOfRepeatsToDo, int _randomNumber)
        {
            using (var connection = new SqlConnection(
                GlobalSettings.ConnectionString))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@idOfWord", listOfRepeatsToDo[_randomNumber].IdWord);

                    param.Add(
                        "@dateOfNextRepeat", DateTime.Now);
                    param.Add("@currentAmountOfRepeats", listOfRepeatsToDo[_randomNumber].CurrentAmountOfRepeats + 1);
                    connection.Execute("UpdateWordOnWrong", param, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            GlobalSettings.UpdateListOfWords();
        }

        public void AddNewWord(string plWord, string enWord)
        {
            using (var connection = new SqlConnection(GlobalSettings.ConnectionString))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@plWord", plWord.ToLower().Trim());
                    param.Add("@enWord", enWord.ToLower().Trim());
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
        }

        public void DeleteOldWord(string plWord, string enWord)
        {
            using (var connection = new SqlConnection(
                            GlobalSettings.ConnectionString))
            {
                DynamicParameters param = new DynamicParameters();
                try
                {
                    param.Add("@plWord", plWord);
                    param.Add("@enWord", enWord);

                    connection.Execute("DeleteOldWord", param, commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            GlobalSettings.UpdateListOfWords();
        }
    }
}
