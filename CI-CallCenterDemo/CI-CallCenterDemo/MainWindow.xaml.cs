
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Windows;

using Microsoft.ProjectOxford.SpeechRecognition;
using System.ComponentModel;
using System.Threading;
using System.Data.SqlClient;
using System.Windows.Input;
using System.Text;
using System.Windows.Controls;

namespace ContosoInsurance_CallCenterDemo
{


    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        string message;

        string _speechAPIAccountKey = ConfigurationManager.AppSettings["speechAPIAccountKey"];
        string _luisAPIAccountKey = ConfigurationManager.AppSettings["luisAPIAccountKey"];
        string _luisAppID = ConfigurationManager.AppSettings["luisAppID"];
        string _luisAppIDChinese = ConfigurationManager.AppSettings["luisAppIDChinese"];
        string _recoLanguage = "en-US";
        string _connectionString = ConfigurationManager.AppSettings["dbConnectionString"];


        String responseEntity = "";
        String responsePhrase = "";

        private MicrophoneRecognitionClient _micClient;
        private static int _identityAttempts = 1;
        private AutoResetEvent _FinalResponseEvent;

        #region Events

        /// <summary>
        /// Implement INotifyPropertyChanged interface
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        public delegate void ReceiveSpeechToTextResponse(String message);
        public event ReceiveSpeechToTextResponse speechToTextCallBack;

        #endregion Events

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            _FinalResponseEvent = new AutoResetEvent(false);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    this.DragMove();


                }
                catch (Exception ex)
                { }

            }
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    Console.WriteLine(this.Left + this.Width);
                    Console.WriteLine(this.Left);
                    _popup.HorizontalOffset = 500;
                    Console.Write(_popup.HorizontalOffset);
                }
                catch (Exception ex)
                { }

            }
        }
        private void Popup_MouseDown(object sender, DragEventArgs e)
        {
            e.Handled = true;
            e.Effects = DragDropEffects.Copy;
        }

        private void _popup_DragEnter(object sender, DragEventArgs e)
        {


        }

        private Boolean nonNumberEntered = false;
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {

            StringBuilder sb = new StringBuilder();
            var str = e.Key.ToString();

            if (txtNumber.Text.Length >= 5)
            {
                txtNumber.Text = string.Empty;
            }

            Console.WriteLine(e.Key.ToString());

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (nonNumberEntered == false && e.Key != Key.RightShift && e.Key != Key.LeftShift && (e.Key >= Key.D0 || e.Key <= Key.D9 || e.Key >= Key.NumPad0 || e.Key <= Key.NumPad9))
            {
                // Determine whether the keystroke is a number from the keypad.

                // Determine whether the keystroke is a backspace.

                nonNumberEntered = false;

                foreach (char c in str)
                {
                    // Check for numeric characters (hex in this case).  Add "." and "e" if float,
                    // and remove letters.  Include initial space because it is harmless.

                    if ((c >= '0' && c <= '9'))
                    {
                        sb.Append(c);
                    }
                    else if (c == '#')
                    {

                        notifyIdentificationResponse(sb.ToString());
                    }

                }
                txtNumber.Text += sb.ToString();
                //English Path
                if (txtNumber.Text == "132307")
                {
                    _recoLanguage = "en-US";
                    playGreetings();
                }
                //Chinese Path
                else if (txtNumber.Text == "132308")
                {
                    _recoLanguage = "zh-CN";
                    playGreetings();
                }
            }
            else if (e.Key == Key.RightShift || e.Key == Key.LeftShift)
            {
                nonNumberEntered = true;
            }
            else
            {
                if (e.Key.ToString() == "D3")
                {
                    txtNumber.Text += "#";
                    notifyIdentificationResponse(sb.ToString());
                }
            }
        }

        /// <summary>
        //  Raises the System.Windows.Window.Closed event.
        /// </summary>
        /// <param name="e">An System.EventArgs that contains the event data.</param>
        protected override void OnClosed(EventArgs e)
        {
            if (null != _micClient)
            {
                _micClient.Dispose();
            }

            _FinalResponseEvent.Dispose();

            base.OnClosed(e);
        }

        private string callednumber;

        private void hashButton_click(object sender, RoutedEventArgs e)
        {
            callednumber += ((System.Windows.Controls.Button)sender).Content;

            notifyIdentificationResponse(callednumber);
        }

        private void playGreetings()
        {
            System.Media.SoundPlayer myPlayer = new System.Media.SoundPlayer();

            //Getting a filepath for the sound
            String path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "telephonering.wav");
            myPlayer.SoundLocation = path;
            myPlayer.Play();

            if (_recoLanguage == "en-US") // US English
                ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("Hello, Welcome to Contoso Insurance. This is Karen. How may I assist you today?", _recoLanguage);                                                                                                                                                                                                   // OLD: SpeechToTextWPFSample.TextToSpeech.Talk("Hello " + selectedUser + ". Welcome to the Department of Human Services. How may I assist you today?", _recoLanguage); //"English");
            else if (_recoLanguage == "zh-CN") // Chinese
            {
                WriteLine("***************TRANSLATION***************");
                WriteLine("Hello, Welcome to Contoso Insurance. How may I assist you today?");
                WriteLine("*********************************************");
                WriteLine();
                ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("你好 " + ". 欢迎来到 Contoso Insurance。有什么我可以帮您的?", _recoLanguage);
            }
            this.speechToTextCallBack = notifyIntentResponse;
            startListening();
        }

        private void callButton_click(object sender, RoutedEventArgs e)
        {

            callednumber += ((System.Windows.Controls.Button)sender).Content;
            txtNumber.Text = callednumber;
            if (callednumber == "132307")
            {
                _recoLanguage = "en-US";
                playGreetings();
                callednumber = string.Empty;
            }
            else if (callednumber == "132308")
            {
                _recoLanguage = "zh-CN";
                playGreetings();
                callednumber = string.Empty;
            }
            if (txtNumber.Text.Length > 5)
            {
                txtNumber.Text = string.Empty;
                callednumber = string.Empty;
            }

        }
        /// <summary>
        /// Handles the Click event of the _startButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            startListening();
        }

        private void startListening()
        {
            LogRecognitionStart("microphone", _recoLanguage, SpeechRecognitionMode.ShortPhrase);
            if (_micClient == null)
            {
                _micClient = CreateMicrophoneRecoClientWithIntent(_recoLanguage);
            }
            _micClient.StartMicAndRecognition();
            _startButton.IsEnabled = false;
        }

        private void LogRecognitionStart(string recoSource, string recoLanguage, SpeechRecognitionMode recoMode)
        {
            WriteLine("\n--- Start speech recognition using " + recoSource + " with " + recoMode + " mode in " + recoLanguage + " language ----\n\n");
        }

        MicrophoneRecognitionClient CreateMicrophoneRecoClientWithIntent(string recoLanguage)
        {
            WriteLine("--- Start microphone dictation with Intent detection ----");

            MicrophoneRecognitionClientWithIntent intentMicClient =
             SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntent(recoLanguage,
                                                                              _speechAPIAccountKey,
                                                                              _luisAppID,
                                                                              _luisAPIAccountKey);
            if (recoLanguage == "zh-CN")
                intentMicClient =
                    SpeechRecognitionServiceFactory.CreateMicrophoneClientWithIntent(recoLanguage,
                                                                                     _speechAPIAccountKey,
                                                                                     _luisAppIDChinese,
                                                                                     _luisAPIAccountKey);
            intentMicClient.OnIntent += OnIntentHandler;

            // Event handlers for speech recognition results
            intentMicClient.OnMicrophoneStatus += OnMicrophoneStatus;
            intentMicClient.OnPartialResponseReceived += OnPartialResponseReceivedHandler;
            intentMicClient.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            intentMicClient.OnConversationError += OnConversationErrorHandler;

            intentMicClient.StartMicAndRecognition();

            return intentMicClient;

        }


        /// <summary>
        ///     Called when a final response is received;   
        /// </summary>
        void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");

                _FinalResponseEvent.Set();

                // we got the final result, so it we can end the mic reco.  No need to do this
                // for dataReco, since we already called endAudio() on it as soon as we were done
                // sending all the data.
                _micClient.EndMicAndRecognition();

                // BUGBUG: Work around for the issue when cached _micClient cannot be re-used for recognition.
                _micClient.Dispose();
                _micClient = null;

                WriteResponseResult(e);

                _startButton.IsEnabled = true;
            }));
        }

        private void WriteResponseResult(SpeechResponseEventArgs e)
        {
            Console.WriteLine(e.PhraseResponse.RecognitionStatus);
            if (e.PhraseResponse.Results.Length == 0)
            {
                WriteLine("No phrase resonse is available.");
                startListening();
            }
            else
            {
                WriteLine("********* Final n-BEST Results *********");
                for (int i = 0; i < e.PhraseResponse.Results.Length; i++)
                {
                    WriteLine("[{0}] Confidence={1}, Text=\"{2}\"",
                                    i, e.PhraseResponse.Results[i].Confidence,
                                    e.PhraseResponse.Results[i].DisplayText);
                    if (i == 0)
                        message = e.PhraseResponse.Results[i].DisplayText;
                }
                if (this.speechToTextCallBack != null)
                {
                    this.speechToTextCallBack(message);
                }
                WriteLine();
            }
        }

        /// <summary>
        ///     Called when a final response is received and its intent is parsed 
        /// </summary>
        void OnIntentHandler(object sender, SpeechIntentEventArgs e)
        {

            WriteLine("--- Intent received by OnIntentHandler() ---");
            WriteLine("{0}", e.Payload);
            WriteLine();
        }

        /// <summary>
        ///     Called when a partial response is received.
        /// </summary>
        void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            WriteLine("{0}", e.PartialResult);
            WriteLine();
        }

        /// <summary>
        ///     Called when an error is received.
        /// </summary>
        void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _startButton.IsEnabled = true;
            });

            WriteLine("--- Error received by OnConversationErrorHandler() ---");
            WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
            WriteLine("Error text: {0}", e.SpeechErrorText);
            WriteLine();
        }

        /// <summary>
        ///     Called when the microphone status has changed.
        /// </summary>
        void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                WriteLine("--- Microphone status change received by OnMicrophoneStatus() ---");
                WriteLine("********* Microphone status: {0} *********", e.Recording);
                if (e.Recording)
                {
                    WriteLine("Please start speaking.");
                }
                WriteLine();
            });
        }


        void WriteLine()
        {
            WriteLine(string.Empty);
        }


        void WriteLine(string format, params object[] args)
        {
            var formattedStr = string.Format(format, args);
            Trace.WriteLine(formattedStr);
            Dispatcher.Invoke(() =>
            {
                _logText.Text += (formattedStr + "\n");
                _logText.ScrollToEnd();
            });
        }

        private void getLUISIntent(String message)
        {
            if (message != null && message.Length > 0)
            {
                ContosoInsurance_CallCenterDemo.LUISCaller caller = new ContosoInsurance_CallCenterDemo.LUISCaller(_recoLanguage);
                ContosoInsurance_CallCenterDemo.LUISResponse result = caller.Call(message);
                var r = result;
                double score = (double)result.intents[0].score;
                WriteLine((String)result.entities.Length.ToString());
                WriteLine(r.ToString());
                if (result.entities.Length > 0)
                {
                    WriteLine("ENTITY: " + result.entities[0].entity);
                    WriteLine("TYPE: " + result.entities[0].type);
                }
                WriteLine("INTENT: " + result.intents[0].intent);
                WriteLine("INTENT SCORE: " + score);
                if (score > 0.5 && result.entities.Length > 0 && result.entities[0].type != "builtin.number")
                {
                    // Moved above, also printing entity type, intent and score
                    //WriteLine(result.entities[0].entity);
                    responseEntity = (result.entities[0].type);
                    responsePhrase = result.entities[0].entity;
                    if (this._recoLanguage == "zh-CN")
                    {
                        WriteLine("***************TRANSLATION***************");
                        WriteLine("Okay, you are looking for an update on your " + responseEntity + ". Please tell your CRN number and hold on.");
                        WriteLine("*********************************************");
                        WriteLine();
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("好的，您想查询您" + result.entities[0].entity + "的信息" + ". 请您在告诉我您的CRN号码之后耐心等待", "zh-CN");
                    }
                    else
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("Okay, you are looking for an update on your " + responseEntity + ". Can you please let me know your CRN?");
                    this.speechToTextCallBack = notifyIdentificationResponse;
                    startListening();
                }
                else
                {
                    if (this._recoLanguage == "zh-CN")
                    {
                        WriteLine("***************TRANSLATION***************");
                        WriteLine("Sorry, I am having trouble understanding you. Can you please repeat your request?");
                        WriteLine("*********************************************");
                        WriteLine();
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("对不起，不是很清楚您的问题，请重复一遍您的请求", "zh-CN");
                    }
                    else
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("Sorry, I am having trouble understanding you. Can you please repeat your request?");
                    this.speechToTextCallBack = notifyIntentResponse;
                    startListening();
                }
            }
        }
        public void notifyIntentResponse(String message)
        {
            getLUISIntent(message);
        }

        public void notifyIdentificationResponse(String message)
        {

            _identityAttempts++;
            if (this._recoLanguage == "zh-CN")
            {
                WriteLine("***************TRANSLATION***************");
                WriteLine("Thank You. Please hold while I get your details.");
                WriteLine("*********************************************");
                WriteLine();
                ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("谢谢！正在提取详细信息，请稍后。", "zh-CN");
            }
            else
                ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("Thank you. Please hold on until I get your details. Shouldn’t be long.");

            SqlConnection sqlConn = new SqlConnection(_connectionString);
            SqlCommand command;
            try
            {
                sqlConn.Open();
                string user_id = message.Substring(0, message.Length - 1);
                String sql = "SELECT * FROM UserInfo WHERE EntityType = '" + this.responseEntity + "' AND UserId = '" + user_id + "'";
                Console.WriteLine(sql);
                command = new SqlCommand(sql, sqlConn);
                SqlDataReader dataReader = command.ExecuteReader();
                if (dataReader.HasRows)
                {
                    dataReader.Read();
                    String response = "";
                    String firstName = (dataReader.GetString(1));
                    String status = (dataReader.GetString(6));

                    if (this._recoLanguage == "zh-CN")
                    {
                        string statusch = "";
                        if (status == "Approved")
                            statusch = "批准通过";
                        else if (status == "In Review")
                            statusch = "正在审批";
                        response += firstName + ", 多谢等待. " + this.responsePhrase + "查询的结果是: " + statusch;
                        WriteLine("***************TRANSLATION***************");
                        WriteLine(firstName + ", thank you for waiting.The status of your " + this.responseEntity + " is " + status);
                        WriteLine("*********************************************");
                        WriteLine();
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk(response, "zh-CN");
                    }
                    else
                    {
                        response += firstName + ", thank you for waiting. Your " + this.responseEntity + " has been " + status + ". You can also log in to your online account to check for details.";
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk(response);
                    }

                    if (this._recoLanguage == "zh-CN")
                    {
                        WriteLine("***************TRANSLATION***************");
                        WriteLine("Thank you for your call.");
                        WriteLine("*********************************************");
                        WriteLine();
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("多谢您致电", "zh-CN");
                    }
                    else
                        ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("Thank you for your call.");
                    _identityAttempts = 1;
                }
                else
                {
                    if (_identityAttempts < 4)
                    {
                        if (this._recoLanguage == "zh-CN")
                        {
                            WriteLine("***************TRANSLATION***************");
                            WriteLine("Sorry we could not find your information. Please tell your CRN number and hold on.");
                            WriteLine("*********************************************");
                            WriteLine();
                            ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("对不起，找不到您要查询的CRN号. 请您在告诉我您的CRN号码之后耐心等待", "zh-CN");
                        }
                        else
                            ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("Sorry, I could not find your information. Can you please let me know your CRN?");
                        _identityAttempts = 1;
                        this.speechToTextCallBack = notifyIdentificationResponse;
                        startListening();
                    }
                    else
                    {

                        if (this._recoLanguage == "zh-CN")
                        {
                            WriteLine("***************TRANSLATION***************");
                            WriteLine("Sorry, I could not find your information. Can you please repeat your request?");
                            WriteLine("*********************************************");
                            WriteLine();
                            ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("对不起，找不到您要查询的信息。请重复您的请求。", "zh-CN");
                        }
                        else
                            ContosoInsurance_CallCenterDemo.TextToSpeech.Talk("Sorry, I could not find your information. Can you please repeat your request?");
                        this.speechToTextCallBack = notifyIntentResponse;
                        startListening();
                    }
                }
                sqlConn.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        private void _popup_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
    }
}
