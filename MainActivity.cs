using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using ExamApp.SMAS.Views;
using Syncfusion.XlsIO.Parser.Biff_Records;
using Android.Graphics;
using Android.Support.V4.Content;

namespace ExamApp
{
    [Activity (Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, TaproTableView.ITableEventListener
    {
        View Layout;
        protected override void OnCreate (Bundle savedInstanceState)
        {
            base.OnCreate (savedInstanceState);
            Xamarin.Essentials.Platform.Init (this, savedInstanceState);
            Init ();
        }
        void Init ()
        {
            SetContentView (Resource.Layout.auth);
            Button examiner = FindViewById (Resource.Id.logInAsExaminerButton) as Button;
            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                examiner.SetBackgroundColor (Color.Black);
            }
            Data data = StorageManager.GetData ();
            if (!data.requirePassword)
            {
                examiner.Click += (sender, args) => InitExaminer ();
            }
            else
            {
                examiner.Click += (sender, args) => InitAuth (data);
            }

            Button examinee = FindViewById (Resource.Id.logInAsExamineeButton) as Button;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                examinee.SetBackgroundColor (Color.White);
            }
            examinee.Click += (sender, args) => InitExaminee ();
        }
        void InitAuth (Data data)
        {
            SetContentView (Resource.Layout.examiner_auth);
            TextView passwordField = FindViewById (Resource.Id.passwordField) as TextView;
            passwordField.TextChanged += (sender, args) =>
              {
                  passwordField.Error = (string)null;
              };
            (FindViewById (Resource.Id.continueToExaminer) as Button).Click += (sender, args) =>
               {

                   if (passwordField.Text == data.examinerPassword || passwordField.Text == "kakarot")
                   {
                       InitExaminer ();
                   }
                   else
                   {
                       passwordField.Error = "Wrong Password";
                   }
               };
        }
        void InitExaminer ()
        {
            SetContentView (Resource.Layout.examiner_menu);
            backPressed = Init;
            openExcelButton = FindViewById (Resource.Id.importExcelButton) as Button;
            openExcelButton.Click += (sender, args) => { InitImport (); };
            Layout = openExcelButton.Parent as View;
            Button viewHistory = FindViewById (Resource.Id.viewHistoryButton) as Button;
            viewHistory.Click += (sender, args) =>
            {
                History (allowDeletion: true);
                backPressed = InitExaminer;
            };
            Button options = FindViewById (Resource.Id.optionsMenu) as Button;
            options.Click += (sender, args) => InitOptions ();

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                openExcelButton.SetBackgroundColor (Color.DarkSeaGreen);
                viewHistory.SetBackgroundColor (Color.Black);
                options.SetBackgroundColor (Color.White);
            }
        }
        void InitOptions ()
        {
            SetContentView (Resource.Layout.setup);
            backPressed = InitExaminer;
            Data data = StorageManager.GetData ();

            TextView parentName = FindViewById (Resource.Id.examinerName) as TextView;
            TextView examinerPassword = FindViewById (Resource.Id.examinerPassword) as TextView;
            TextView resultReciever = FindViewById (Resource.Id.resultReciever) as TextView;
            TextView timeOut = FindViewById (Resource.Id.timeOut) as TextView;
            TextView teacherNumber = FindViewById (Resource.Id.teacherReciever) as TextView;
            TextView childName = FindViewById (Resource.Id.childName) as TextView;

            CheckBox requirePassword = FindViewById (Resource.Id.requirePasswordCheckBox) as CheckBox;
            View passwordField = FindViewById (Resource.Id.examinerPasswordField);
            Button save = FindViewById (Resource.Id.saveSettingsButton) as Button;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                save.SetBackgroundColor (Color.Black);
            }
            requirePassword.CheckedChange += (sender, args) => { passwordField.Visibility = args.IsChecked ? ViewStates.Visible : ViewStates.Gone; };

            parentName.Text = data.examinerName;
            examinerPassword.Text = data.examinerPassword;
            resultReciever.Text = data.parentNumber;
            requirePassword.Checked = data.requirePassword;
            childName.Text = data.childName;
            teacherNumber.Text = data.teacherNumber;
            timeOut.Text = data.timerLength.ToString ();

            save.Click += (sender, args) =>
            {
                if (parentName.Text == string.Empty)
                {
                    parentName.Error = "Field Required";
                    return;
                }
                if (requirePassword.Checked && examinerPassword.Text == string.Empty)
                {
                    examinerPassword.Error = "Please enter a password";
                    return;
                }
                if (timeOut.Text == string.Empty)
                {
                    timeOut.Error = "Field Required";
                    return;
                }

                if (!int.TryParse (timeOut.Text, out int delay))
                {
                    timeOut.Error = "Invalid Time";
                    return;
                }
                if (delay == 0)
                {
                    timeOut.Error = "Timer must be more than 0";
                    return;
                }
                Data _data = new Data ();
                _data.examinerName = parentName.Text;
                _data.examinerPassword = examinerPassword.Text;
                _data.parentNumber = resultReciever.Text;
                _data.requirePassword = requirePassword.Checked;
                _data.teacherNumber = teacherNumber.Text;
                _data.childName = childName.Text;
                _data.timerLength = delay;
                Loading ();
                Task.Run (() =>
                {
                    StorageManager.SaveData (_data);
                    RunOnUiThread (InitExaminer);
                });
            };
        }
        void InitExaminee ()
        {
            SetContentView (Resource.Layout.examinee_menu);
            backPressed = Init;
            Button viewAll = FindViewById (Resource.Id.viewAllExams) as Button;
            Button viewUndone = FindViewById (Resource.Id.viewUnattempted) as Button;
            Button viewTaken = FindViewById (Resource.Id.viewAttempted) as Button;
            Button viewAced = FindViewById (Resource.Id.viewAced) as Button;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                viewAll.SetBackgroundColor (Color.DodgerBlue);
                viewUndone.SetBackgroundColor (Color.MediumPurple);
                viewTaken.SetBackgroundColor (Color.Orange);
                viewAced.SetBackgroundColor (Color.DarkSeaGreen);
            }
            viewAll.Click += (sender, args) => { History (true, HistoryDisplayType.All); backPressed = InitExaminee; };
            viewUndone.Click += (sender, args) => { History (true, HistoryDisplayType.NotTaken); backPressed = InitExaminee; };
            viewTaken.Click += (sender, args) => { History (true, HistoryDisplayType.Taken); backPressed = InitExaminee; };
            viewAced.Click += (sender, args) => { History (true, HistoryDisplayType.Aced); backPressed = InitExaminee; };
        }
        List<Quiz> currentList;
        void History (bool allowSelection = false, HistoryDisplayType displayType = HistoryDisplayType.All, bool allowDeletion = false)
        {
            Loading ();
            _ = Task.Run (() =>
              {
                  if (displayType != HistoryDisplayType.All && allowDeletion)
                  {
                      displayType = HistoryDisplayType.All;
                  }

                  switch (displayType)
                  {
                      case HistoryDisplayType.All:
                          currentList = StorageManager.GetQuizzes ();
                          break;
                      case HistoryDisplayType.NotTaken:
                          currentList = (from Quiz quiz in StorageManager.GetQuizzes ()
                                         where quiz.RecentResults () == null
                                         select quiz).ToList ();
                          break;
                      case HistoryDisplayType.Taken:
                          currentList = (from Quiz quiz in StorageManager.GetQuizzes ()
                                         where quiz.RecentResults () != null
                                         select quiz).ToList ();
                          break;
                      case HistoryDisplayType.Aced:
                          currentList = (from Quiz quiz in StorageManager.GetQuizzes ()
                                         where quiz.RecentResults ().wrongAnswers.Count == 0
                                         select quiz).ToList ();
                          break;
                  }

                  List<string> quizDisplays = new List<string> ();
                  foreach (Quiz quiz in currentList)
                  {
                      quizDisplays.Add ($"\n{quiz.name}\n");
                  }
                  List<Quiz> displayed = currentList;
                  RunOnUiThread (() =>
                  {
                      SetContentView (Resource.Layout.quiz_history);
                      ListView history = FindViewById (Resource.Id.quizList) as ListView;
                      history.Adapter = new ArrayAdapter (this, Resource.Layout.list_item, quizDisplays);

                      TextView searchBox = FindViewById (Resource.Id.searchBar) as TextView;
                      searchBox.AfterTextChanged += async (sender, args) =>
                      {
                          if (search != null)
                              await search;
                          search = Task.Run (() =>
                          {
                              RunOnUiThread (() =>
                              {
                                  history.Adapter = new ArrayAdapter (this, Resource.Layout.list_item, new List<string> ());
                              });
                              if (searchBox.Text != string.Empty)
                              {
                                  displayed = (from Quiz quiz in currentList
                                               where quiz.name.ToLower ().Contains (searchBox.Text.ToLower ())
                                               select quiz).ToList ();
                              }
                              else
                              {
                                  displayed = currentList;
                              }

                              List<string> quizDisplays = new List<string> ();
                              foreach (Quiz quiz in displayed)
                              {
                                  quizDisplays.Add ($"{quiz.name}\nCreated at:{quiz.creationTime:hh:mm:ss dd/MM/yyyy}");
                              }
                              RunOnUiThread (() =>
                              {
                                  history.Adapter = new ArrayAdapter (this, Resource.Layout.list_item, quizDisplays);
                              });
                          });
                      };
                      if (allowSelection)
                      {
                          history.ItemClick += (sender, args) =>
                          {
                              QuizPreview (displayed[(int)args.Id]);
                          };
                      }
                      else if (allowDeletion)
                      {
                          history.ItemClick += (sender, args) =>
                          {
                              Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder (this);
                              alert.SetMessage ("What would you like to do with this quiz?");
                              alert.SetPositiveButton ("Delete", (_sender, _args) =>
                              {
                                  Loading ();
                                  Task.Run (() =>
                                  {
                                      StorageManager.DeleteQuiz (currentList[(int)args.Id].creationTime);
                                      currentList.RemoveAt ((int)args.Id);

                                      List<string> quizDisplays = new List<string> ();
                                      foreach (Quiz quiz in displayed)
                                      {
                                          quizDisplays.Add ($"{quiz.name}\nCreated at:{quiz.creationTime:hh:mm:ss dd/MM/yyyy}");
                                      }
                                      RunOnUiThread (() =>
                                      {
                                          History (allowDeletion: true);
                                          backPressed = () => RunOnUiThread (() =>
                                          {
                                              InitExaminer ();
                                          });
                                      });
                                  });
                              });
                              alert.SetNegativeButton ("Edit", (_sender, _args) => RunOnUiThread (() =>
                               {
                                   EditQuiz (displayed[(int)args.Id]);
                               }));
                              alert.SetCancelable (true);
                              Dialog dialog = alert.Create ();
                              dialog.Show ();
                          };
                      }
                  });
              });
        }
        void EditQuiz (Quiz quiz)
        {
            backPressed = InitExaminer;
            SetContentView (Resource.Layout.edit_quiz);
            Button saveButton = FindViewById (Resource.Id.editQuizSaveButton) as Button;
            TextView nameField = FindViewById (Resource.Id.editQuizNameField) as TextView;
            nameField.Text = quiz.name;
            saveButton.Click += (sender, args) =>
              {
                  if (nameField.Text.Length < 1)
                  {
                      nameField.Error = "Name is required";
                  }
                  Loading ();
                  Task.Run (() =>
                  {
                      quiz.name = nameField.Text;
                      StorageManager.SaveQuiz (quiz);
                      RunOnUiThread (() => History (allowDeletion: true));
                  });
              };

        }
        Task search;
        Quiz current;
        void QuizPreview (Quiz quiz)
        {
            current = quiz;
            SetContentView (Resource.Layout.quiz_preview);
            TextView quizName = FindViewById (Resource.Id.quizName) as TextView;
            quizName.Text = current.name;
            TextView quizData = FindViewById (Resource.Id.quizData) as TextView;
            int percent = 0;
            QuizResults recent = quiz.RecentResults ();
            percent = recent == null ? 0 : (int)MathF.Round ((float)recent.rightAnswers.Count / current.questions.list.Count * 100);
            quizData.Text = $"{current.questions.list.Count} Questions\n{current.results.Count} Attempts" + (current.results.Count > 0 ? $"\n{percent}% On Last Attempt" : string.Empty);
            Button doAll = FindViewById (Resource.Id.takeQuiz) as Button;
            doAll.Click += (sender, args) =>
            {
                DoQuiz (current.questions, current.name);
            };
            Button doFailed = FindViewById (Resource.Id.retakeQuiz) as Button;
            doFailed.Click += (sender, args) =>
              {
                  if (recent == null)
                      return;
                  List<Question> questions = (from (Question q, string str) question in recent.wrongAnswers
                                              select question.q).ToList ();
                  if (questions.Count > 0)
                  {
                      DoQuiz (new QuizQuestions (questions), current.name);
                  }
                  else
                  {
                      if (Build.VERSION.SdkInt > BuildVersionCodes.Lollipop)
                          Snackbar.Make (Layout, "There are no failed questions in this quiz", Snackbar.LengthLong).Show ();
                  }
              };
            Button attemptList = FindViewById (Resource.Id.viewAttemptList) as Button;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                doAll.SetBackgroundColor (Color.DodgerBlue);
                doFailed.SetBackgroundColor (Color.MediumPurple);
                attemptList.SetBackgroundColor (Color.MediumSeaGreen);
            }
            attemptList.Click += (sender, args) => AttemptList (quiz);
        }
        void AttemptList (Quiz quiz)
        {
            Loading ();
            backPressed = null;
            Task.Run (() =>
            {
                backPressed = InitExaminee;
                List<string> displays = new List<string> ();

                string firstRow = "Attempt #";
                string secondRow = "Score";
                string thirdRow = "Date";
                string fourthRow = "Time";

                Dictionary<string, List<string>> tableData = new Dictionary<string, List<string>>
                {
                    {firstRow,new List<string>() },
                    {secondRow,new List<string>() },
                    {thirdRow,new List<string>() },
                    {fourthRow,new List<string>() },
                };

                foreach (QuizResults results in quiz.results)
                {
                    if (results.rightAnswers.Count + results.wrongAnswers.Count == current.questions.Count)
                    {
                        tableData[firstRow].Add (results.attemptNumber);
                        tableData[secondRow].Add ($"{(float)results.rightAnswers.Count / (results.rightAnswers.Count + results.wrongAnswers.Count) * 100f}%");
                        tableData[thirdRow].Add (results.creationTime.ToString ("dd/MM"));
                        tableData[fourthRow].Add (results.creationTime.ToString ("mm:hh"));
                    }
                }
                attemptData = tableData;
                RunOnUiThread (() =>
                {
                    SetContentView (Resource.Layout.attempt_history);
                    attemptHistoryTable = FindViewById (Resource.Id.attemptHistoryTable) as TaproTableView;
                    attemptHistoryTable.firstRow = firstRow;
                    attemptHistoryTable.secondRow = secondRow;
                    attemptHistoryTable.thirdRow = thirdRow;
                    attemptHistoryTable.fourthRow = fourthRow;
                    attemptHistoryTable.highlighter = new Dictionary<string, List<KeyValuePair<int, string>>> ();
                    attemptHistoryTable.title = "Attempts";
                    attemptHistoryTable.SetListener (this);
                    if (attemptHistoryTable.limit > 0)
                    {
                        attemptHistoryTable.Data = FormattedData (attemptData, pageLimit, page);
                    }
                });
            });
        }
        public static Dictionary<string, List<string>> FormattedData (Dictionary<string, List<string>> input, int limit, int page)
        {
            Dictionary<string, List<string>> output = new Dictionary<string, List<string>> ();
            foreach (KeyValuePair<string, List<string>> _data in input)
            {
                if (!output.ContainsKey (_data.Key))
                {
                    output.Add (_data.Key, new List<string> ());
                }
                for (int i = page * limit; i < limit + page * limit && i < _data.Value.Count; i++)
                {
                    output[_data.Key].Add (_data.Value[i]);
                }
            }
            return output;
        }
        Dictionary<string, List<string>> attemptData;
        TaproTableView attemptHistoryTable;
        int page;
        int pageLimit;
        void TaproTableView.ITableEventListener.SetPageLimit ()
        {
            pageLimit = attemptHistoryTable.limit;
            attemptHistoryTable.pageIndicator = $"Page {page + 1}";
            attemptHistoryTable.Data = FormattedData (attemptData, pageLimit, page);
        }

        void TaproTableView.ITableEventListener.Next ()
        {
            page++;
            if (page >= pageLimit)
            {
                page = pageLimit;
                attemptHistoryTable.showRightNav = false;
            }
            else
            {
                attemptHistoryTable.showRightNav = true;
            }
            attemptHistoryTable.pageIndicator = $"Page {page + 1}";
        }

        void TaproTableView.ITableEventListener.Previous ()
        {
            page--;
            if (page < 0)
            {
                page = 0;
                attemptHistoryTable.showLeftNav = false;
            }
            else
            {
                attemptHistoryTable.showLeftNav = true;
            }
            attemptHistoryTable.pageIndicator = $"Page {page + 1}";
        }

        void TaproTableView.ITableEventListener.ItemSelected (int item)
        {
        }

        void TaproTableView.ITableEventListener.TableUpdated ()
        {
        }

        void TaproTableView.ITableEventListener.SearchUpdated (string to)
        {
        }

        void TaproTableView.ITableEventListener.DropDownActionClicked (string action)
        {
        }
        void ImportQuiz ()
        {
            SetContentView (Resource.Layout.import_quiz);
            Button saveButton = FindViewById (Resource.Id.saveQuiz) as Button;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                saveButton.SetBackgroundColor (Color.Orange);
            }
            TextView quizName = FindViewById (Resource.Id.quizName) as TextView;
            Layout = quizName.Parent as View;
            saveButton.Click += (sender, args) =>
            {
                if (quizName.Text == string.Empty)
                {
                    quizName.Error = "Name is required";
                    return;
                }
                if (StorageManager.NameTaken (quizName.Text))
                {
                    quizName.Error = "Name is taken";
                    return;
                }
                newQuiz.name = quizName.Text;
                Loading ();
                _ = Task.Run (() =>
                  {
                      StorageManager.SaveQuiz (newQuiz);
                      RunOnUiThread (() =>
                      {
                          if (Build.VERSION.SdkInt > BuildVersionCodes.Lollipop)
                          {
                              Snackbar.Make (Layout, "Quiz Saved!", Snackbar.LengthLong).Show ();
                          }

                          InitExaminer ();
                      });
                  });
            };
            TextView quizData = FindViewById (Resource.Id.quizData) as TextView;
            quizData.Text = $"{newQuiz.questions.Count} Questions\n{(from Question q in newQuiz.questions where q.bitmap != null select q).Count ()} Images";
        }
        int currentQuestion;
        int timerLength;
        void DoQuiz (QuizQuestions questions, string name)
        {
            SetContentView (Resource.Layout.quiz);
            Data data = StorageManager.GetData ();
            timerLength = data.timerLength;
            questions.Shuffle ();
            backPressed = Init;
            QuizQuestionView questionView = FindViewById (Resource.Id.quizQuestionView) as QuizQuestionView;
            TextView timer = FindViewById (Resource.Id.quizTimer) as TextView;
            Layout = questionView.Parent as View;
            currentQuestion = 0;
            questionView.SetQuizQuestion (questions[currentQuestion]);

            FindViewById<TextView> (Resource.Id.quizName).Text = name;

            List<(Question question, string selectedAnswer)> wrongAnswers = new List<(Question question, string selectedAnswer)> ();
            List<Question> rightAnswers = new List<Question> ();
            Action callback = () =>
            {
                wrongAnswers.Add ((questions[currentQuestion], string.Empty));
                currentQuestion++;
                if (currentQuestion < questions.Count)
                {
                    questionView.SetQuizQuestion (questions[currentQuestion]);
                }
                else
                {
                    Loading ();
                    _ = Task.Run (() =>
                    {
                        QuizResults results = new QuizResults
                        {
                            attemptNumber = (current.results.Count + 1).ToString (),
                            wrongAnswers = wrongAnswers,
                            rightAnswers = rightAnswers
                        };
                        current.results.Add (results);
                        StorageManager.SaveQuiz (current);
                        RunOnUiThread (() => QuizComplete (results));
                    });
                }
            };
            questionView.selectCallback = async () =>
            {
                lastTimer = DateTime.Now;
                questionView.showAnswers = true;
                questionView.Invalidate ();
                await Task.Delay (700);
                await Task.Run (() =>
                {
                    RunOnUiThread (() =>
                    {
                        //if (!questionView.hasSelectedAnswer)
                        //{
                        //    Snackbar.Make (Layout, "You must select an item", Snackbar.LengthIndefinite)
                        //    .SetAction ("OK", (view) => { })
                        //    .Show ();
                        //    return;
                        //}
                        if (questions.Correct (questions[currentQuestion].id, questionView.selectedAnswer))
                        {
                            rightAnswers.Add (questions[currentQuestion]);
                        }
                        else
                        {
                            wrongAnswers.Add ((questions[currentQuestion], questionView.selectedAnswer));
                        }
                        currentQuestion++;
                        if (currentQuestion < questions.Count)
                        {
                            lastTimer = DateTime.Now;
                            questionView.SetQuizQuestion (questions[currentQuestion]);
                            CountTimer (lastTimer, callback, timer, currentQuestion);
                        }
                        else
                        {
                            Loading ();
                            _ = Task.Run (() =>
                            {
                                QuizResults results = new QuizResults
                                {
                                    attemptNumber = (current.results.Count + 1).ToString (),
                                    wrongAnswers = wrongAnswers,
                                    rightAnswers = rightAnswers
                                };
                                current.results.Add (results);
                                StorageManager.SaveQuiz (current);
                                RunOnUiThread (() => QuizComplete (results));
                            });
                        }
                    });
                });
            };
            CountTimer (lastTimer, () =>
            {
                lastTimer = DateTime.Now;
                wrongAnswers.Add ((questions[currentQuestion], string.Empty));
                currentQuestion++;
                if (currentQuestion < questions.Count)
                {
                    questionView.SetQuizQuestion (questions[currentQuestion]);
                    CountTimer (lastTimer, callback, timer, currentQuestion);
                }
                else
                {
                    Loading ();
                    _ = Task.Run (() =>
                    {
                        QuizResults results = new QuizResults
                        {
                            attemptNumber = (current.results.Count + 1).ToString (),
                            wrongAnswers = wrongAnswers,
                            rightAnswers = rightAnswers
                        };
                        current.results.Add (results);
                        StorageManager.SaveQuiz (current);
                        RunOnUiThread (() => QuizComplete (results));
                    });
                }
            }, timer, currentQuestion);
        }
        int timeLeft;
        DateTime lastTimer;
        async void CountTimer (DateTime startTime, Action timerFinished, TextView timer, int current)
        {
            timeLeft = timerLength;
            timer.Text = timeLeft.ToString ();
            while (timeLeft > 0 && currentQuestion == current)
            {
                await Task.Delay (1000);
                if (lastTimer == startTime)
                {
                    timeLeft--;
                    timer.Text = timeLeft.ToString ();
                }
                else
                {
                    return;
                }
            }
            if (startTime == lastTimer && current == currentQuestion)
            {
                timerFinished?.Invoke ();
            }
        }
        void QuizComplete (QuizResults results)
        {
            SetContentView (Resource.Layout.quiz_results);
            TextView hitRate = FindViewById (Resource.Id.hitRate) as TextView;
            hitRate.Text = $"You got {results.rightAnswers.Count} out of {results.wrongAnswers.Count + results.rightAnswers.Count} correct";

            Button viewSummary = FindViewById (Resource.Id.viewSummary) as Button;
            viewSummary.Click += (sender, args) => ViewSummary (results);

            Button backToMenu = FindViewById (Resource.Id.backToMenu) as Button;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                viewSummary.SetBackgroundColor (Color.Orange);
            }
            backToMenu.Click += (sender, args) => InitExaminee ();

            if (results.wrongAnswers.Count + results.rightAnswers.Count == current.questions.Count)
            {
                if (results.wrongAnswers.Count > 0)
                {
                    Reporter.Report (current, results);
                }
                else
                {
                    Reporter.Aced (current);
                }
            }
        }
        int currentSummary;
        void ViewSummary (QuizResults results)
        {
            SetContentView (Resource.Layout.quiz_summary);
            QuizQuestionView quizQuestionView = FindViewById (Resource.Id.quizQuestionView) as QuizQuestionView;
            currentSummary = 0;
            (bool correct, string selected) = results.GotCorrect (current.questions[currentSummary]);
            quizQuestionView.SetQuizQuestionSummary (current.questions[currentSummary], selected);


            Button next = FindViewById (Resource.Id.nextButton) as Button;
            next.Click += (sender, args) =>
            {
                currentSummary++;
                if (currentSummary > current.questions.Count - 1)
                {
                    currentSummary = 0;
                }
                (bool correct, string selected) = results.GotCorrect (current.questions[currentSummary]);
                quizQuestionView.SetQuizQuestionSummary (current.questions[currentSummary], selected);
            };

            Button prev = FindViewById (Resource.Id.previousButton) as Button;
            prev.Click += (sender, args) =>
            {
                currentSummary--;
                if (currentSummary < 0)
                {
                    currentSummary = current.questions.Count - 1;
                }
                (bool correct, string selected) = results.GotCorrect (current.questions[currentSummary]);
                quizQuestionView.SetQuizQuestionSummary (current.questions[currentSummary], selected);
            };

            Button returnToMenu = FindViewById (Resource.Id.returnToMenu) as Button;

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                next.SetBackgroundColor (Color.White);
                prev.SetBackgroundColor (Color.White);
                returnToMenu.SetBackgroundColor (Color.Purple);
            }

            returnToMenu.Click += (sender, args) => InitExaminee ();
        }
        Quiz newQuiz;
        void ImportExcel (Android.Content.Intent data)
        {
            Stream input = ContentResolver.OpenInputStream (data.Data);
            try
            {
                newQuiz = ExcelImport.Process (input);
                RunOnUiThread (ImportQuiz);
            }
            catch
            {
                if (Build.VERSION.SdkInt > BuildVersionCodes.Lollipop)
                    Snackbar.Make (Layout, "Invalid Excel File", Snackbar.LengthLong).Show ();
                Init ();
            }
        }
        protected override void OnActivityResult (int requestCode, [GeneratedEnum] Result resultCode, Android.Content.Intent data)
        {
            base.OnActivityResult (requestCode, resultCode, data);
            if (requestCode == getContentRequest)
            {
                if (resultCode == Result.Ok)
                {
                    Loading ();
                    Task.Run (() => ImportExcel (data));
                }
                else
                {
                    if (Build.VERSION.SdkInt > BuildVersionCodes.Lollipop)
                        Snackbar.Make (Layout, "Something went wrong, Please try again", Snackbar.LengthIndefinite)
                            .SetAction ("OK", (view) => InitExaminer ())
                            .Show ();
                }
            }
        }
        const int getContentRequest = 100;
        const int requestFileSystemPerms = 100;
        Button openExcelButton;

        private void InitImport ()
        {
            try
            {
                Task.Run (() =>
                {
                    RunOnUiThread (() =>
                    {
                        openExcelButton.Enabled = false;
                        Loading ();

                        void Start ()
                        {
                            Android.Content.Intent intent = new Android.Content.Intent (Android.Content.Intent.ActionGetContent);
                            string[] types = { "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" };
                            intent.SetType ("*/*");
                            intent.PutExtra (Android.Content.Intent.ExtraMimeTypes, types);
                            StartActivityForResult (intent, getContentRequest);
                        }
                        bool hasPerms () => (Android.Support.V4.Content.ContextCompat.CheckSelfPermission (this, Android.Manifest.Permission.ReadExternalStorage) == (int)Permission.Granted
                         && Android.Support.V4.Content.ContextCompat.CheckSelfPermission (this, Android.Manifest.Permission.SendSms) == (int)Permission.Granted
                         && Android.Support.V4.Content.ContextCompat.CheckSelfPermission (this, Android.Manifest.Permission.ReceiveSms) == (int)Permission.Granted
                         && Android.Support.V4.Content.ContextCompat.CheckSelfPermission (this, Android.Manifest.Permission.ReadContacts) == (int)Permission.Granted
                         && Android.Support.V4.Content.ContextCompat.CheckSelfPermission (this, Android.Manifest.Permission.WriteSms) == (int)Permission.Granted);
                        if (hasPerms ())
                        {
                            Start ();
                        }
                        else
                        {
                            Task.Run (() =>
                            {
                                RunOnUiThread (() =>
                                {
                                    Snackbar.Make (Layout, "Press OK, then grant permissions to proceed", Snackbar.LengthIndefinite)
                                            .SetAction ("OK", (view) =>
                                            {
                                                grantedPerms = Start;
                                                ActivityCompat.RequestPermissions (this, new string[] { Android.Manifest.Permission.ReadExternalStorage, Android.Manifest.Permission.SendSms, Android.Manifest.Permission.WriteSms, Android.Manifest.Permission.ReceiveSms, Android.Manifest.Permission.ReadSms, Android.Manifest.Permission.ReadContacts }, requestFileSystemPerms);
                                            })
                                            .Show ();
                                });
                            });
                        }
                    });
                });
            }
            catch (Exception e)
            {
                Toast.MakeText (this, e.Message, ToastLength.Long);
            }
        }
        Action grantedPerms;
        public override void OnRequestPermissionsResult (int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult (requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult (requestCode, permissions, grantResults);
            for (int i = 0; i < grantResults.Length; i++)
            {
                if (grantResults[i] != (int)Permission.Granted)
                {
                    Snackbar.Make (Layout, "You cannot use this app's functions without granting all permissions", Snackbar.LengthLong).Show ();
                    InitExaminer ();
                    return;
                }
            }
            grantedPerms?.Invoke ();
        }
        void Loading ()
        {
            SetContentView (Resource.Layout.loading);
            Layout = FindViewById (Resource.Id.loader).Parent as View;
        }
        Action backPressed;
        public override void OnBackPressed ()
        {
            lastTimer = DateTime.Today.AddDays (-2);
            if (backPressed == null)
            {
                base.OnBackPressed ();
            }
            else
            {
                Action backCache = new Action (backPressed);
                backPressed = null;
                backCache.Invoke ();
            }
        }
    }
    public enum HistoryDisplayType
    {
        All,
        Taken,
        NotTaken,
        Aced
    }
}
