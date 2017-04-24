using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Threading;

namespace ArabicSF
{
    public partial class MainForm : Form 
    {

        delegate void ChangeMyTextDelegate(Control ctrl, string text);
        public static void ChangeMyText(Control ctrl, string text)
        {
            ctrl.Text = text;
        }
        delegate void ChangeProgressDelegate(ProgressBar ctrl, int precent);
        public static void ChangeProgress(ProgressBar ctrl, int precent)
        {
            ctrl.Value = precent;
        }
        delegate void SetItemsDelegate(ComboBox ctrl, string [] items);
        public static void SetItems(ComboBox ctrl, string[] items)
        {
            ctrl.Items.Clear();
            ctrl.Items.AddRange(items);
        }
       
        delegate void ChangeVisibleDelegate(Control ctrl, int value);
        public static void ChangeVisible(Control ctrl, int value)
        {
            var combobox = ctrl as ComboBox;
            if (combobox != null)
                combobox.SelectedIndex = 0;
            else
            if (value == 1)
                ctrl.Visible = true;
            else if (value == 2)
                ctrl.Visible = false;
            else if (value == 3)
                ctrl.Enabled = true;
            else ctrl.Enabled = false;
        }
        delegate void ChangeHeightDelegate(Form f, int height);
        public static void ChangeHeight(Form f, int height)
        {
            f.Height += height;
        }
        delegate void SetItemDelegate(DataGridView ctrl, string A, string B);
        public static void SetItem(DataGridView ctrl,string A,string B)
        {
                ctrl.Rows.Add(A,B);

        }
        ChangeMyTextDelegate changeText = new ChangeMyTextDelegate(ChangeMyText);
        ChangeProgressDelegate del = new ChangeProgressDelegate(ChangeProgress);
        ChangeVisibleDelegate delVisible = new ChangeVisibleDelegate(ChangeVisible);
        ChangeHeightDelegate changeHeight = new ChangeHeightDelegate(ChangeHeight);
        SetItemsDelegate setItems = new SetItemsDelegate(SetItems);
        SetItemDelegate setItem = new SetItemDelegate(SetItem);
        int count, count1 = 0;
        public string path;
        TextProcessing textObj;
        List<string> staticFiles = new List<string>();
        bool _true;
        private string[] Letters = { "ا", "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر", "ز", "س", "ش", "ص", "ض", "ط", "ظ", "ع", "غ", "ف", "ق", "ك", "ل", "م", "ن", "ه", "و", "ي" };
        private string[] diacritics = { "ّ", "َ", "ً", "ُ", "ِ", "ٍ", "ٌ", "ْ" };
        private string[] separators = {  " ", "!", ".", ",", "،", "؛", "؟" ,":",
                            "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»" };
        private string[] separatorsSentence = { ".", ",","؛", ";", "!", "؟","،" };
        private string[] stopWordsList;
        private int[] countLetter = new int[28];
        string[] prefix = { "م", "ن", "فل", "ولل", "بال", "ب", "فال", "وال", "ال", "لل", "ل", "وس", "ول", "ل", "و" };
        string[] suffix = {"ة", "هما", "تما", "كما", "ان", "ها", "وا", "تم", "كم", "تن", "كن", "نا", "تا", "ته", "ما", "ون", " ين", "هن", "هم", "تي", "ني", "ا", "ي", "ات","ه","ت"};
        string[] stopWords = File.ReadAllText("stopwords.txt").Split(' ');
        string[] sort = new string[28];
        string[] sortInt = new string[28];
        private List<string> temp;
       
        Queue<string> FilePath = new Queue<string>();
        public MainForm()
        {
            InitializeComponent();
            
            _true = false;
            textObj = new TextProcessing();

        }
       
        private void arabicExtract()
        {
            new Thread(new ThreadStart(() =>
            {
                int j = 0;

                foreach (string FileName in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                {

                    string[] txt = File.ReadAllLines(FileName);
                    File.WriteAllLines(FileName.Split('.').First() + ".txt", textObj.removeNonArabicLetters(txt));
              
                    this.setProgressValue(((j + 1) * 100) / _NumberOf_Articles, j + 1, _NumberOf_Articles);
                    j++;
                }

            })).Start();
        
        }

        private void wordsMenuItem_Click(object sender, EventArgs e)
        {

            if (_true)
            {
                toolStripStatusLabel2.Text = "Process has complete successfully";
                toolStripStatusLabel4.Text = "Number of words : " + wordsCounter();
            }
           
            
        }
        private int wordsCounter()
        {
            
            int str=0;
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                count1++;
                statusStrip1.Invoke(del, toolStripProgressBar1, (count1 * 100) / count);
                statusStrip1.Invoke(changeText, toolStripStatusLabel5 , ((count1 * 100) / count).ToString() + " %");
                str += Regex.Replace(File.ReadAllText(file), @"\s+", " ").Trim().Split(separators,StringSplitOptions.RemoveEmptyEntries).Length;
                
            }
            return str;
        }

        private void uniqeWordsMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result;
            List<string> unique = new List<string>();
            if (_true)
            {
                toolStripStatusLabel2.Text = "Process has complete successfully";
                toolStripStatusLabel4.Text = "Number of unique words : " + uniqueWordsCounter();
                result = MessageBox.Show("Number of unique words : " + uniqueWordsCounter() + ".\nDo you want to save unique words?", "Save", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    result = saveFileDialog1.ShowDialog();
                    string pth = saveFileDialog1.FileName;
                    File.WriteAllLines(pth, unique);
                }
            }
            
            
        }
        private int [] uniqueWordsCounter()
        {
            int[] countOT = new int[4];
            string [] str;
            var dict = new Dictionary<string, int>();
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                count1++;
                toolStripProgressBar1.Value = (count1 * 100) / count;
               // statusStrip1.Invoke(changeText, toolStripStatusLabel5, ((count1 * 100) / count).ToString() + " %");
                str = Regex.Replace(File.ReadAllText(file), @"\s+", " ").Split(separators, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var word in str)
                    if (dict.ContainsKey(word))
                    {
                        //if (dict[word] < 2)
                            dict[word] += 1;
                       // else dict.Remove(word);
                    }
                    else
                        dict.Add(word, 1);
            }
            countOT[0] = dict.Values.Count(x=> x == 1);
            countOT[1] = dict.Values.Count(x=> x == 2);
            countOT[2] = dict.Count();
            countOT[3] = dict.Values.Sum();
            return countOT;
        }

        private void rootMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                toolStripStatusLabel2.Text = "Process has complete successfully";
                toolStripStatusLabel4.Text = "Number of Characters : " + charCounter();
            }
           
        }
        private int charCounter()
        {
            int unique = 0;
            //unique.Append("");
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                count1++;
                setProgressValue(((count1 * 100) / count), count1, count);
                unique += File.ReadAllText(file).Length;

            }


            return unique;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            toolStripProgressBar1.ForeColor = Color.Blue;
            foreach (string file in Directory.GetFiles(@"StemmerFiles\", "*.txt"))
                staticFiles.Add(File.ReadAllText(file));
             
        }
       
       
        delegate void setProgressDelgate(int value,int v1,int v2);
        public void setProgressValue(int value, int v1, int v2)
        {
            if (statusStrip1.InvokeRequired)
            {
                setProgressDelgate del = new setProgressDelgate(setProgressValue);
                this.Invoke(del, value, v1, v2);
            }
            else
            {
                if (toolStripProgressBar1.Visible == false)
                {
                    toolStripProgressBar1.Visible = true;
                    toolStripStatusLabel5.Visible = true;
                    toolStripStatusLabel7.Visible = true;
                    if(toolStripStatusLabel1.Text.CompareTo ("Process has successfully Complete.")==0)
                    toolStripStatusLabel1.Text = "";
                }
                
                if (value > toolStripProgressBar1.Value)
                {
                   toolStripProgressBar1.Value = value;
                   toolStripStatusLabel5.Text = value + "%";
                   toolStripStatusLabel7.Text = "(" + v1 + "/" + v2 + ")";
                   
                }
                else if (toolStripProgressBar1.Value == 100)
                {
                    toolStripProgressBar1.Visible = false;
                    toolStripStatusLabel5.Visible = false;
                    toolStripStatusLabel7.Visible = false;
                    toolStripStatusLabel1.Text = "Process has successfully Complete.";
                
                    toolStripProgressBar1.Value = 0;
                    toolStripStatusLabel5.Text = "0%";
                    toolStripStatusLabel7.Text = "(" + 0 + "/" + v2 + ")";
                }
               
            }
        }
       //*****************************
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //ThreadPool.SetMaxThreads(2,2);
            int i = 1;
           
            foreach(string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                try
                {
                    Thread thread = new Thread(() => stem1Fun(file));
                    thread.Start();
                    thread.Join();
                     int percentage = (i * 100) / count;
                     backgroundWorker1.ReportProgress(percentage);
                     i++;
                                       
                }
                catch
                {
                    MessageBox.Show("Error: use Filters tools for solve this problem, don't use stopwords filter\n If the error appear again, make sure that all characters in Arabic.\n You can do that using [Extract Arabic Text].", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }
            
        }
        private void setElapsedTime(double u)
        {
            ChangeMyTextDelegate del = new ChangeMyTextDelegate(ChangeMyText);
            //textBox1.Invoke(del,textBox1, u.ToString());
        }
        int articleCount;
        int _NumberOf_Articles;
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            int i = 0;
         
            articleCount = 0;

            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                for (int s = 0; s < diacritics.Length; s++)
                    File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), diacritics[s], ""));
                File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), @"[^\w\s]", " "));
                File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), "[0-9]", " "));
                File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), @"\s+", " "));
                FilePath.Enqueue(file);
            }
            _NumberOf_Articles = FilePath.Count;
            Thread[] thread = new Thread[10];
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int numberOfTasks = 0;
            for (int a = 0; a < thread.Length; a++)
            {
                Interlocked.Increment(ref numberOfTasks);
                thread[i] = new Thread(new ThreadStart(() =>
                {
                    khojaStemmer();
                    if (Interlocked.Decrement(ref numberOfTasks) == 0)
                        resetEvent.Set();
                }));
                thread[i].IsBackground = true;
                thread[i].Start();

            }
            resetEvent.WaitOne();

                       
        }

        private  Dictionary<string,string> rootIndex = new Dictionary<string,string>();
        private void khojaStemmer()
        {
            int c;
            string FileP;
            List<List<string>> temp;
            Directory.CreateDirectory(@path + "RootIndexing");
            while ((c = Interlocked.Increment(ref articleCount)) <= _NumberOf_Articles)
            {
                int i = c - 1;
                try
                {
                    Monitor.Enter(FilePath);
                    FileP = FilePath.Dequeue().ToString();
                    Monitor.Exit(FilePath);
                    temp = stemFun(FileP);
                  /*  lock (rootIndex)
                    {
                        for (int j = 0; j < temp[0].Count; j++)
                        {
                            temp[0][j] = temp[0][j].Replace('ؤ','أ');
                            if (rootIndex.ContainsKey(temp[1][j]))
                                rootIndex[temp[1][j]] = temp[0][j];
                            else
                                rootIndex.Add(temp[1][j], temp[0][j]);
                        }
                    }*/
                    
                   // _readWriteLock.ExitWriteLock(); 
                    File.WriteAllText(FileP, string.Join(" ", temp[0]));

                    //int percentage = ((i + 1) * 100) / count;
                   
                        setProgressValue(((i + 1) * 100) / _NumberOf_Articles, i+1, _NumberOf_Articles);
                    
                }
                catch
                {
                    MessageBox.Show("Error: use Filters tools for solve this problem, don't use stopwords filter\n If the error appear again, make sure that all characters in Arabic.\n You can do that using [Extract Arabic Text].", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                }
            }

        }
        //List<string> temp1;
        public List<List<string>> stemFun(string file)
        {
            Stem stem;

            stem = new Stem(file, staticFiles);
            List<List<string>> results = stem.GetResults();
            return results;

            
        }
        public void stem1Fun(string file)
        {
            string temp;
            StringBuilder text = new StringBuilder();
            string[] words = File.ReadAllText(file).Replace(@"\s+"," ").Split(new string[]{" "}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in words)
            {
                temp = w;
                
                if(stopWords.Contains(Stylometric.Normalize_Alif(temp)))
                    continue;
              

                temp = Stylometric.RemovePrefixes(prefix, w);
                temp = Stylometric.RemoveSuffixes(suffix, temp);
                if (temp.Length > 1)
                {
                    text.Append(temp);
                    
                }
                text.Append(" ");
            }
            File.WriteAllText(file, text.ToString().Replace(@"\s+"," "));
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            hideControls();
        }
        private void hideControls()
        {
            statusStrip1.Invoke(changeText, toolStripStatusLabel5, "");
            toolStripStatusLabel1.Text = "";
            statusStrip1.Invoke(del, toolStripProgressBar1, 0);
            menuItem26.Enabled = true;
            menuItem27.Enabled = true;
            openFilesMenuItem.Enabled = true;
            //Height -= panel2.Height;
            toolStripProgressBar1.Value = 0;
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel2.Text = "( " + count1.ToString() + " of " + count.ToString() + " ) \t| Process has complete successfully";
           
    
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            foreach(var k in rootIndex)
                File.AppendAllText(@path + "RootIndexing\\" + k.Value + ".txt", k.Key + Environment.NewLine);
            MessageBox.Show("Complete!");
              hideControls();  
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            count1++;
            setProgressValue(e.ProgressPercentage, count1, count);
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            count1++;
            setProgressValue(e.ProgressPercentage, count1, count);
          
        }
       
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(linkLabel1.Text);
        }
        private int [] wordLengthDistribution()
        {
            List<int> wordFreqCount = new List<int>();
            List<string> Unique = new List<string>();
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                count1++;
                stemFun(file);
                foreach (string s in temp)
                    if (!Unique.Contains(s))
                    {
                        Unique.Add(s);
                        wordFreqCount.Add(1);
                    }
                    else
                    {
                        wordFreqCount.Insert(Unique.IndexOf(s), wordFreqCount.ElementAt(Unique.IndexOf(s)) + 1);
                    }
                statusStrip1.Invoke(del, toolStripProgressBar1, (count1 * 100) / count);
                statusStrip1.Invoke(changeText, toolStripStatusLabel5, ((count1 * 100) / count).ToString() + " %");
            }
            int[] x = new int[2];
            x[0] = wordFreqCount.Max();
            x[1] = wordFreqCount.Min();
            return x;
        }

        private int sentencesCounter()
        {
            int sentenceCount = 0;
             string [] sentences;
             foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
             {
                 count1++;
                 setProgressValue(((count1 * 100) / count), count1, count);
            
                 sentences = File.ReadAllText(file).Split(separatorsSentence, StringSplitOptions.RemoveEmptyEntries);
                 sentenceCount += sentences.Length;

             }
             return sentenceCount;
        }
        

        private void Lexical()
        {
            int wordCount;
            int charCount;
            int sentencesCount;
            count1 = 0;
            count = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
            
            int [] uniqueWordsCount = uniqueWordsCounter();
            label25.Invoke(changeText, label25, uniqueWordsCount[0].ToString());
            label23.Invoke(changeText, label23, uniqueWordsCount[1].ToString());
            label11.Invoke(changeText, label11, uniqueWordsCount[2].ToString());
            wordCount = uniqueWordsCount[3];
            label8.Invoke(changeText, label8, wordCount.ToString());
            count1 = 0;
            sentencesCount = sentencesCounter();
            label32.Invoke(changeText, label32, sentencesCount.ToString());
            label10.Invoke(changeText, label10, (wordCount / sentencesCount).ToString());

            count1 = 0; //label20.BackColor = Color.Teal;
            charCount = charCounter();
            label20.Invoke(changeText, label20, charCount.ToString());
            label18.Invoke(changeText, label18, (charCount / sentencesCount).ToString());
            label16.Invoke(changeText, label16, (charCount / wordCount).ToString());

            setProgressValue(((count1 * 100) / count), count1, count);
                                   
        }

        private void FrequencyLetters()
        {
            string txt;
            countLetter.Initialize();
            
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                txt = File.ReadAllText(file);
                txt = txt.Replace("أ", "ا");
                txt = txt.Replace("إ", "ا");
                txt = txt.Replace("آ", "ا");
                txt = txt.Replace("ة", "ه");
                for (int i = 0; i < diacritics.Length; i++)
                    txt = Regex.Replace(txt, diacritics[i], "");
                foreach (char c in txt.ToCharArray())
                    if (Letters.Contains(c.ToString()))
                        countLetter[Letters.ToList().IndexOf(c.ToString())]++;

                count1++;
               statusStrip1.Invoke(del, toolStripProgressBar1, (count1 * 100) / count);
               statusStrip1.Invoke(changeText, toolStripStatusLabel5, ((count1 * 100) / count).ToString() + " %");
                                     
            }
        }

        private void label8_TextChanged(object sender, EventArgs e)
        {
            label8.BackColor = SystemColors.ButtonFace;
        }

        private void label10_TextChanged(object sender, EventArgs e)
        {
            label10.BackColor = SystemColors.ButtonFace;
        }

       

        private void label20_TextChanged(object sender, EventArgs e)
        {
            label20.BackColor = SystemColors.ButtonFace;
        }

        private void label25_TextChanged(object sender, EventArgs e)
        {
            label25.BackColor = SystemColors.ButtonFace;
        }

        private void label23_TextChanged(object sender, EventArgs e)
        {
            label23.BackColor = SystemColors.ButtonFace;
        }

        private void punctuation()
        {
            string str;
            int counter = 0;
            List<string> unique = new List<string>();
            List<int> uCounter = new List<int>();
            count1 = 0;
            count = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                count1++;
                str = Regex.Replace(File.ReadAllText(file), @"[\w\s]", "");
                str = str.Replace(" ", "");
                counter += str.Length;
                foreach (char s in str.ToCharArray())
                    if (s == ' ') continue;
                    else
                    if (!unique.Contains(s.ToString()))
                    {
                        unique.Add(s.ToString());
                        uCounter.Add(1);
                    }
                    else
                    {
                        uCounter.Insert(unique.IndexOf(s.ToString()), uCounter.ElementAt(unique.IndexOf(s.ToString())) + 1);
                    }
                statusStrip1.Invoke(del, toolStripProgressBar1, (count1 * 100) / count);
                statusStrip1.Invoke(changeText, toolStripStatusLabel5 , ((count1 * 100) / count).ToString() + " %");
            }
           
        }

        private void Syntax()
        {
            punctuation();

        }
  
       
        //*************************
        private void breakTextIntoParagraphsMenuItem_Click(object sender, EventArgs e)
        {
            string value = "";
            if (InputBox.Input_Box("Replace", "Insert a value to be replaced :", ref value) == DialogResult.OK && value.Length > 0)
            {
                if (value.Length > 0)
                {
                    string[] t = value.Split(new string[] {" "}, StringSplitOptions .RemoveEmptyEntries );
                    foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                        string temp = "";
                        string[] str = File.ReadAllText(file).Split(t, StringSplitOptions.RemoveEmptyEntries);
                        // if (str.Length == 1)
                        //  continue;
                        for (int i = 0; i < str.Length; i++)
                            if (str[i].Trim().Length != 0)
                            {
                                temp += " " + str[i];
                                if (temp.Length >= 20)//temp.EndsWith("."))// && temp.Length >= 400)
                                {
                                    File.WriteAllText(file.Substring(0, file.Length - 4) + " " + (i + 1) + ".txt", temp);
                                    temp = "";
                                }
                                else
                                    continue;
                            }
                        File.Delete(file);
                    }
                }
            }

        }
        //***********************
        private void styleFeaturesMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {

                bool clear = false;
                foreach (Control c in panel1.Controls)
                {
                    clear = false;
                    var label = c as Label;
                    if (label != null)
                        if (label.Text.CompareTo("") != 0)
                            foreach (char n in label.Text.ToCharArray())
                                if (Char.IsDigit(n))
                                    clear = true;
                                else { clear = false; break; }

                    if (clear) label.Text = "";
                    else continue;


                }
                toolStripStatusLabel1.Text = "Dataset Statistics";
                Thread th = new Thread(new ThreadStart(Lexical));
                th.Start();
            }
        }

        private void removeMultiBlankLinesMenuItem_Click(object sender, EventArgs e)
        {
            new Thread(new ThreadStart(() =>
            {
                List<string> str;
                int j = 0;

                foreach (string FileName in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                {
                    str = File.ReadAllLines(FileName).ToList();
                    str.RemoveAll(x => x.Trim().CompareTo(string.Empty) == 0 || x.Trim().Length < 3);
                    File.WriteAllLines(FileName, str);
                    this.setProgressValue(((j + 1) * 100) / _NumberOf_Articles, j + 1, _NumberOf_Articles);
                    j++;
                }

            })).Start();
          
        }
        string savepath_features = "";
        private void extractionStylometricFeaturesMenuItem_Click(object sender, EventArgs e)
        {
           // int count = 50;
            toolStripStatusLabel2.Text = "";
            ManualResetEvent reset = new ManualResetEvent(false);
            List<string> FileName = new List<string>();
            List<string> articles = new List<string>();
            List<string> className = new List<string>();
           // Dictionary <string,int> countClass = new Dictionary<string,int>();

            if (_true)
            {

                toolStripStatusLabel1.Text = ": Reading Files ...";
                new Thread(new ThreadStart(() =>
                {
                    foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                         //if (countClass.ContainsKey(Directory.GetParent(file).Name))
                             //   countClass[Directory.GetParent(file).Name]  += 1;
                          //  else
                             //   countClass.Add(Directory.GetParent(file).Name, 1);
                        //if (countClass[Directory.GetParent(file).Name] <= count)
                        {
                            //counter++;
                            FileName.Add(file.Substring(file.LastIndexOf('\\') + 1, file.Split('.').First().Length - file.LastIndexOf('\\') - 1));
                            articles.Add(File.ReadAllText(file, Encoding.UTF8));
                            className.Add(Directory.GetParent(file).Name);
                           
                            //setProgressValue((counter * 100) / Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count());

                        }
                    }

                    reset.Set();
                })).Start();
                reset.WaitOne();
                label2.Text = "No. Files = " + FileName.Count() + " Files";
               
                toolStripStatusLabel1.Text = ": Computing Features , Please wait ...";
                timer1.Start();
                Thread th = new Thread(new ThreadStart(() =>
                {
                    Stopwatch t = new Stopwatch();
                    t.Start();
                    Stylometric obj = new Stylometric(ref FileName, ref articles, ref className, this);
                    t.Stop();
                    MessageBox.Show(((t.ElapsedMilliseconds)/1000).ToString() + " seconds.");
                    
                }));
                th.Start();
                Thread.SpinWait(10000);

            }
        }

        public string savePath_f()
        {
            return savepath_features;
        }
        private string savePath()
        {
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
                return saveFileDialog1.FileName;
            return String.Empty;
        }
        private void aboutMenuItem1_Click(object sender, EventArgs e)
        {
           
        }

        private void newMenuItem_Click(object sender, EventArgs e)
        {
            backgroundWorker3.RunWorkerAsync();

        }

        private void duplicateLettersMenuItem_Click(object sender, EventArgs e)
        {
            Regex r = new Regex("(.)(?<=\\1\\1\\1)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            if (_true)
            {
                foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                {
                    File.WriteAllText(file, r.Replace(File.ReadAllText(file), string.Empty), Encoding.UTF8);
                }
                MessageBox.Show("Done.");
            }
        }

        private void aSCIIMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    File.WriteAllText(file, File.ReadAllText(file, Encoding.Default), Encoding.ASCII);
            }
            MessageBox.Show("Done.");
        }

        private void uTF8MenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                {
                    File.WriteAllText(file, File.ReadAllText(file, Encoding.Default), Encoding.UTF8);
                }
            }
            MessageBox.Show("Done.");
        }

        private void uTF32MenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    File.WriteAllText(file, File.ReadAllText(file, Encoding.Default), Encoding.UTF32);
            }
            MessageBox.Show("Done.");
        }

        private void unicodeMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    File.WriteAllText(file, File.ReadAllText(file, Encoding.Default), Encoding.Unicode);
            }
            MessageBox.Show("Done.");
        }

        private void replaceMenuItem_Click(object sender, EventArgs e)
        {
            string value = "";
            string y = "";
            string x = "";
            if (_true)
            {
                if (InputBox.Input_Box("Replace", "Insert a value to be replaced :", ref value) == DialogResult.OK && value.Length > 0)
                {
                    if (value.Length > 0)
                    {
                        x = value;
                        if (InputBox.Input_Box("Replace with", "Insert a replacement value :", ref value) == DialogResult.OK)
                        {
                            y = value;
                            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                                File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), "("+x+")", y));
                        }
                        MessageBox.Show("Done.");
                    }

                    else
                    {
                        MessageBox.Show("You must enter a value to continue.");
                        replaceMenuItem_Click(sender, e);
                    }

                }

            }
           
        }

        private void arabicKhojasStemmerMenuItem_Click(object sender, EventArgs e)
        {
           
        }

        private void defaultMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    File.WriteAllText(file, File.ReadAllText(file, Encoding.Default), Encoding.Default);
            }
            MessageBox.Show("Done.");
        }

        private void arabicLightStemmerMenuItem_Click(object sender, EventArgs e)
        {
            count1 = 0;
            List<string> staticFiles = new List<string>();
            foreach (string file in Directory.GetFiles(@"StemmerFiles\", "*.txt"))
                staticFiles.Add(File.ReadAllText(file));
            if (_true)
            {
                count = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
                if (!backgroundWorker1.IsBusy)
                {
                    menuItem26.Enabled = false;
                    menuItem27.Enabled = false;
                    openFilesMenuItem.Enabled = false;
                  
                    toolStripStatusLabel1.Text = "Light Stemmer Progress";
                    backgroundWorker1.RunWorkerAsync();

                }
                else
                    MessageBox.Show("This tool is busy with another process, please wait to finish.");
            }
        }

        private void extractArabicTextMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                //progressBar1.Style = ProgressBarStyle.Marquee;
                //progressBar1.Visible = true;
                //panel2.Visible = true;
                ////Height += panel2.Height;
                Thread t = new Thread(new ThreadStart(arabicExtract));
                t.Start();
                t.Join();
                toolStripStatusLabel2.Text = "Process has complete successfully";
            }
        }

        private void duplicateSpacesMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                new Thread(new ThreadStart(() =>
                {
                    List <string> str;
                    List <string> str1 = new List<string>();
                    int j = 0;

                    foreach (string FileName in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                        str = File.ReadAllLines(FileName).ToList();
                        foreach (string line in str)
                            str1.Add(line.Split(' ').ToList().RemoveAll(y => y.Trim().CompareTo("") == 0).ToString()); 
                      //  File.WriteAllText(FileName, str.Replace(@"\s+", " "), Encoding.UTF8);
                        this.setProgressValue(((j + 1) * 100) / _NumberOf_Articles, j + 1, _NumberOf_Articles);
                        j++;
                    }

                })).Start();
             
            }
        }

        private void stopWordsMenuItem_Click(object sender, EventArgs e)
        {
            StringBuilder str = new StringBuilder();
            stopWordsList = File.ReadAllText("stopwords.txt").Split(' ');
            if (_true)
            {
                new Thread(new ThreadStart(() =>
                {
                    int j = 0;

                    foreach (string FileName in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                        str.Append(File.ReadAllText(FileName));
                        foreach (string s in stopWordsList)
                            str.Replace(" " + s.Trim() + " ", " ");

                        File.WriteAllText(FileName, str.ToString());
                        str.Clear(); 
                        this.setProgressValue(((j + 1) * 100) / _NumberOf_Articles, j + 1, _NumberOf_Articles);
                        j++;
                    }

                })).Start();
            }
        }

        private void punctuationMarksMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                new Thread(new ThreadStart(() =>
                {
                    int j = 0;

                    foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                        File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), @"[^\w\s]", " ")); 
                        this.setProgressValue(((j + 1) * 100) / _NumberOf_Articles, j + 1, _NumberOf_Articles);
                        j++;
                    }

                })).Start();
             
            }
        }

        private void numbersMenuItem_Click(object sender, EventArgs e)
        {
            if (_true)
            {
                new Thread(new ThreadStart(() =>
                {
                    int j = 0;

                    foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                        File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), "[0-9]", " "));
                        this.setProgressValue(((j + 1) * 100) / _NumberOf_Articles, j + 1, _NumberOf_Articles);
                        j++;
                    }

                })).Start();

            }
            
        }

        private void diacriticsMenuItem_Click(object sender, EventArgs e)
        {

            if (_true)
            {
                new Thread(new ThreadStart(() =>
                {
                    int j = 0;

                    foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                        for (int i = 0; i < diacritics.Length; i++)
                            File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), diacritics[i], ""));
                        this.setProgressValue(((j + 1) * 100) / _NumberOf_Articles,j+1,_NumberOf_Articles );
                        j++;
                    }
                    
                    //toolStripStatusLabel2.Text = "Process has complete successfully";
                 })).Start();
            }
         }
     

        private void openFilesMenuItem_Click(object sender, EventArgs e)
        {
            int c = 0;
            folderBrowserDialog1.Description = "Please, select the input direction.";
            DialogResult result = folderBrowserDialog1.ShowDialog();
           
            if (result == DialogResult.OK)
            {

                try
                {
                    _true = true;
                      path = folderBrowserDialog1.SelectedPath + "\\";
                    label4.Visible = true;
                    c = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
                    label2.Text = "No. Files : " + c.ToString() + " Files";
                    linkLabel1.Text = path;
                    _NumberOf_Articles = c;

                }
                catch (Exception ex)
                {
                    c = 1;
                    _true = false;
                    MessageBox.Show(ex.Message, "Exeption", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    openFilesMenuItem_Click(sender, e);
                }
                finally
                {
                    if (c == 0)
                    {
                        _true = false;
                        MessageBox.Show("Files of txt types are not exist!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        openFilesMenuItem_Click(sender, e);
                    }
                }

              }
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

      
     
        

        private void button2_Click(object sender, EventArgs e)
        {
            int j = 0;
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                j++;
                //string temp = "";
                string[] str = File.ReadAllText(file,Encoding.UTF8).Split(new string[] { "a" }, StringSplitOptions.RemoveEmptyEntries);
                if (str.Length == 1)
                    continue;
                Directory.CreateDirectory(file.Substring(0,file.LastIndexOf("\\")+1) + "A" + j);
                for (int i = 0; i < str.Length; i++)
                    if (str[i].Trim().Length > 0)
                    {
                        File.WriteAllText(file.Substring(0,file.LastIndexOf("\\")+1) + "A" + (j) + "\\" + (i + 1) + ".txt", str[i],Encoding.UTF8);
                    }
                File.Delete(file);
            }
        }

        

        private void menuItem2_Click(object sender, EventArgs e)
        {
            String value="";
            String x="";
            if (InputBox.Input_Box("Replace", "Insert a value to be replaced :", ref value) == DialogResult.OK && value.Length > 0)
            {
                if (value.Length > 0)
                {
                    x = value;
                    foreach (string file in Directory.GetFiles(@path,"*.*", SearchOption.AllDirectories))
                    {
                        File.Move(file, Path.ChangeExtension(file, "."+x));
                        File.Delete(file);
                    }
                }
            }
                 
        }

        

        private void menuItem4_Click(object sender, EventArgs e)
        {
           
            new Thread(() =>
                {
                    int i = 0;
                    int count = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
                    string[] authorNames = File.ReadAllLines("Authors_Names.txt");
                    foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
                    {
                        i++;
                        foreach (string s in authorNames)
                            File.WriteAllText(file, Regex.Replace(File.ReadAllText(file), s, string.Empty));
                        setProgressValue((i * 100) / count, i, count);
                    }
                }).Start();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<string> a = new List<string>();
            foreach (string file in Directory.EnumerateDirectories(@path,"*",SearchOption.AllDirectories))
            {
                a.Add(file.Split('\\').Last());
            }
            File.WriteAllLines("Dataset_Authors_Names.txt", a);
        }

        private void menuItem5_Click(object sender, EventArgs e)
        {
            int threshold;
            string r = "AW";
            int c = 0;
            //folderBrowserDialog1.Description = "Please, select the input direction.";
            DialogResult result;// = folderBrowserDialog1.ShowDialog();

            if (_true)
            {

            /*    try
                {
                    _true = true;
                    //string temp = @System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    //path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+"\\AW-Text Processing Tools OutPut";
                    path = folderBrowserDialog1.SelectedPath + "\\";
                    label4.Visible = true;
                    c = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
                    label2.Text = "No. of Files : " + Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count().ToString() + " Files";
                    linkLabel1.Text = path;
                }
                catch (Exception ex)
                {
                    c = 1;
                    _true = false;
                    MessageBox.Show(ex.Message, "Exeption", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    openFilesMenuItem_Click(sender, e);
                }
                finally
                {
                    if (c == 0)
                    {
                        _true = false;
                        MessageBox.Show("Files of txt types are not exist!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        openFilesMenuItem_Click(sender, e);
                    }
                }*/
            while (r.Trim().ToCharArray().Count(x => !Char.IsDigit(x)) > 0)//  && r.CompareTo(string.Empty)!=0)
                r = Interaction.InputBox("Please enter threshold for number of Files in each class.", "Create Balanced Dataset", String.Empty, -1, -1);
            if (r.CompareTo(String.Empty) != 0)
            {
                threshold = Convert.ToInt32(r);
                

                    folderBrowserDialog1.Description = "Please, select the output direction.";
                    result = folderBrowserDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        string p = folderBrowserDialog1.SelectedPath + "\\";
                        foreach (string dirPath in Directory.GetDirectories(@path, "*", SearchOption.AllDirectories))
                        {
                            if (Directory.GetFiles(dirPath, "*.*").Count() >= threshold)
                            Directory.CreateDirectory(dirPath.Replace(@path, @p));
                        }
                       
                        count = Directory.GetFiles(@path, "*.*", SearchOption.AllDirectories).Count();
                        new Thread(() =>
                            {
                                //Copy all the files & Replaces any files with the same name
                                foreach (string newPath in Directory.GetFiles(@path, "*.*", SearchOption.AllDirectories))
                                {
                                    c++;
                                    if (Directory.GetParent(newPath).GetFiles().Count() >= threshold)
                                        if (Directory.GetParent(newPath.Replace(@path, @p)).GetFiles().Count() < threshold)
                                            File.Copy(newPath, newPath.Replace(@path, @p), true);
                                    setProgressValue((c * 100) / count, c, count);
                                }
                            }).Start();
                        //CopyDirectory(@path, @p, true);
                       // path = p;
                        //linkLabel1.Text = path;

                    }
                }

            }
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            int i = 0;
            Directory.CreateDirectory(path+"\\All_in_one");
            foreach (string newPath in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                //if(File.ReadAllText(newPath).Trim().Length > 600)
                    File.Copy(newPath, path + "\\All_in_one\\"+(++i)+".txt", true);
            }

           
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            //int i = 0;
           // Directory.CreateDirectory(path + "\\All_in_one");
            foreach (string newPath in Directory.GetFiles(@path, "*.*", SearchOption.AllDirectories))
            {
                if (File.ReadAllText(newPath).Trim().Length <= 400)
                    File.Delete(newPath);
            }

        }

       

        private void text_categrization()
        {
            Stylometric obj = new Stylometric();
            string[] prefix = File.ReadAllText(@"StemmerFiles\prefixes.txt").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] suffix = File.ReadAllText(@"StemmerFiles\suffixes.txt").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            string[] Policy = File.ReadAllLines(@"Contents_Features\Policy.txt");
            string[] Economy = File.ReadAllLines(@"Contents_Features\Economy.txt");
            string[] Sport = File.ReadAllLines(@"Contents_Features\Sport.txt");
            string[] Social = File.ReadAllLines(@"Contents_Features\Social.txt");

            string[] positive = File.ReadAllLines(@"Lexicon\positive_words_ar.txt");
            string[] negative = File.ReadAllLines(@"Lexicon\negative_words_ar.txt");

            int sumPolicy = 0;
            int sumEconomy = 0;
            int sumSport = 0;
            int sumSocial = 0;

            string article;
            int i = 0;
            string[] w;
            int count = Directory.GetFiles(@path, "*.*", SearchOption.AllDirectories).Count();
            Dictionary<string,int> max = new Dictionary<string, int>();
            max.Add("Social", 0);
           // max.Add("Sport", 0);
            max.Add("Economy", 0);
            max.Add("Polotical", 0);
            foreach (string file in Directory.GetFiles(@path, "*.*", SearchOption.AllDirectories))
            {
                i++;
                 sumPolicy = 0;
                 sumEconomy = 0;
                 sumSport = 0;
                 sumSocial = 0;

                 article = File.ReadAllText(file);

                 w = article.Split(new string[] {" ", "!", ".", ",", "،", "؛", "؟" ,":",
                "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»" }, StringSplitOptions.RemoveEmptyEntries);

                  sumPolicy = obj._content(Policy, w);    
                  sumEconomy = obj._content(Economy, w);
                  sumSport = obj._content(Sport, w);
                  sumSocial = obj._content(Social, w);


                max["Social"] = sumSocial;
               // max["Sport"] = sumSport;
                max["Economy"] = sumEconomy;
                max["Polotical"] = sumPolicy;
                File.WriteAllText(file, max.FirstOrDefault(x=> x.Value == max.Values.Max()) + "\n" + article);
                setProgressValue(((i) * 100) / count,i,count);
            }

        }

        private void menuItem9_Click(object sender, EventArgs e)
        {
            foreach (var directory in Directory.GetDirectories(@path))
            {
                if (Directory.GetFiles(directory).Length == 0 && Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }

        private void menuItem10_Click(object sender, EventArgs e)
        {
           
           new Thread(()=> MainChild.SaveDatasetToCSV(path,this)).Start();
        }

        private void menuItem11_Click(object sender, EventArgs e)
        {
           
            new Thread(() => MainChild.DatasetStatisticsToCSV(path, this)).Start();
        }

       
        private void menuItem19_Click(object sender, EventArgs e)
        {
            Process.Start("Alwajeeh.exe");
        }

 
        private void menuItem21_Click(object sender, EventArgs e)
        {
            int i = 0;
            foreach (string file in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
            {
                string[] str = File.ReadAllText(file).Split(separatorsSentence, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in str)
                {
                    File.WriteAllText(file.Substring(0, file.Length - 4) + " " + (i + 1) + ".txt", s);
                    i++;
                }

                File.Delete(file);
            }
        }

        private void menuItem26_Click(object sender, EventArgs e)
        {
            count1 = 0;
            if (_true)
            {
                count = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
                if (!backgroundWorker2.IsBusy)
                {
                    menuItem26.Enabled = false;
                    menuItem27.Enabled = false;
                    openFilesMenuItem.Enabled = false;
                  
                    toolStripStatusLabel1.Text = "Khoja Stemmer Progress";
                    backgroundWorker2.RunWorkerAsync();
                }
                else
                    MessageBox.Show("This tool is busy with another process, please wait to finish.");
            }
        }

        private void menuItem27_Click(object sender, EventArgs e)
        {
            count1 = 0;
            List<string> staticFiles = new List<string>();
            foreach (string file in Directory.GetFiles(@"StemmerFiles\", "*.txt"))
                staticFiles.Add(File.ReadAllText(file));
            if (_true)
            {
                count = Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories).Count();
                if (!backgroundWorker1.IsBusy)
                {
                    menuItem26.Enabled = false;
                    menuItem27.Enabled = false;
                    openFilesMenuItem.Enabled = false;
                    
                    toolStripStatusLabel1.Text = "Light Stemmer Progress";
                    backgroundWorker1.RunWorkerAsync();

                }
                else
                    MessageBox.Show("This tool is busy with another process, please wait to finish.");
            }
        }

       

      
       private void menuItem3_Click(object sender, EventArgs e)
        {
            string text = "";
            int i = 1;
            CsvFileReader reader = new CsvFileReader("ReadTest.csv");
            {
                CsvRow row = new CsvRow();
                while (reader.ReadRow(row))
                {
                    text = "";
                    foreach (string s in row)
                    {
                        text += s + " ";
                    }
                    File.WriteAllText("awaw" +i+ ".txt", text,Encoding.UTF8);
                    i++;
                }
                
            }
        }

       private void menuItem1_Click(object sender, EventArgs e)
       {
           foreach (string newPath in Directory.GetFiles(@path, "*.txt", SearchOption.AllDirectories))
           {
               if(File.ReadAllText(newPath).Trim().Length == 0)
                    File.Delete (newPath);
           }
       }

       private void label13_TextChanged(object sender, EventArgs e)
       {

       }

     

    

    

       
       
       
    }
}
