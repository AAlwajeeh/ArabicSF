using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;
using ExportToExcel;
namespace ArabicSF
{
    public class Stylometric
    {
        private int articleCount = 0;
        private Thread[] thread = new Thread[10];
        private Queue articles;
        private List<string> FileName;
        private List<string> className;
        private int _NumberOf_Articles;
        private const int _NumberOf_Features = 339;
        private System.Data.DataTable csv;
        private float[][] Features;
        List<string> staticFiles = new List<string>();
        string[] prefix = File.ReadAllText(@"StemmerFiles\prefixes.txt").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        string[] suffix = File.ReadAllText(@"StemmerFiles\suffixes.txt").Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
        string[] Policy = File.ReadAllLines(@"Lexicon of Contents_Features\Policy.txt");
        string[] Economy = File.ReadAllLines(@"Lexicon of Contents_Features\Economy.txt");
        string[] Sport = File.ReadAllLines(@"Lexicon of Contents_Features\Sport.txt");
        string[] Social = File.ReadAllLines(@"Lexicon of Contents_Features\Social.txt");
        string[] negative = File.ReadAllLines(@"Lexicon of Contents_Features\neg_wordlist.txt").Distinct().ToArray();

        private MainForm form;
        public Stylometric() { }
        public Stylometric(ref List<string> FileName, ref List<string> articles, ref List<string> className, MainForm form)
        {
            foreach (string file in Directory.EnumerateFiles(@"StemmerFiles\", "*.txt"))
                staticFiles.Add(File.ReadAllText(file));
       
            this.articles = new Queue(articles);
            this.form = form;
            csv = new System.Data.DataTable("Stylometric");
            this.FileName = new List<string>(FileName);
            this.className = new List<string>(className);
            _NumberOf_Articles = this.articles.Count;
            Features = new float[_NumberOf_Articles][];
            for (int i = 0; i < _NumberOf_Articles; i++)
            {
                Features[i] = new float[_NumberOf_Features+1];
            }
            
            
            ExtractFeatures();
           
        }

        private void createTable()
        {
            csv.Columns.Add("Article", typeof(string));
            for (int i = 1; i <= _NumberOf_Features; i++)
                csv.Columns.Add("F" + (i), typeof(float));

            csv.Columns.Add("Class", typeof(string));
        }
        private void insertDataToTable()
        {
            for (int i = 0; i <_NumberOf_Articles; i++)
                setRow(i);
        }
        private void setRow(int i)
        {
             DataRow row = csv.NewRow();
             row[0] = FileName[(int)Features[i][_NumberOf_Features]];
             for (int r = 1; r <= _NumberOf_Features; r++)
                row[r] = Features[i][r-1];
            row[_NumberOf_Features + 1] = className[(int)Features[i][_NumberOf_Features]];
           
            csv.Rows.Add(row);
        }
        private double stringtovalue(string x)
        {
            double sum = 0;
            if(x.Length > 0)
            for (int c = 0; c < x.Length;c++)
                sum += Math.Sqrt((int)x[c]) * (c+1);
            return sum;
        }
        private void ExtractFeatures()
        {
            ManualResetEvent resetEvent = new ManualResetEvent(false);
            int numberOfTasks = 0;
            for (int i = 0; i < thread.Length; i++)
            {
                Interlocked.Increment(ref numberOfTasks);
                thread[i] = new Thread(new ThreadStart(() =>
                    {
                        StylometricFeatures();
                        if (Interlocked.Decrement(ref numberOfTasks) == 0)
                            resetEvent.Set();
                    }));
                thread[i].IsBackground = true;
                thread[i].Start();
                
            }
            resetEvent.WaitOne();
            createTable();
            insertDataToTable();
            GenerateExcelFile(csv, @form.path+"Stylometric Features.csv");
 
        }


        private void StylometricFeatures()
        {
            int c;
            while( (c = Interlocked.Increment(ref articleCount)) <= _NumberOf_Articles)
            {
                int i = c-1;
                StylometricFeatures(i);
                //Thread.Sleep(1000);
            }
        }
        private void StylometricFeatures(object z)
        {
            int i = (int)z;
            string article;
            Monitor.Enter(articles);
            article = articles.Dequeue().ToString();
            Monitor.Exit(articles); 
            character_based(i, article);
            word_based(i, article);
            synt(i, article);
            struc(i, article);
            content_Features(i, article);
        }
        // Extract features from F1 to F57.
        private void character_based(int i,string article)
        {
            List<char> Letters = new char[] { 'أ', 'إ', 'آ', 'ء', 'ئ', 'ؤ', 'ى', 'ا', 'ب', 'ت', 'ث', 'ج', 'ح', 'خ', 'د', 'ذ', 'ر', 'ز', 'س', 'ش', 'ص', 'ض', 'ط', 'ظ', 'ع', 'غ', 'ف', 'ق', 'ك', 'ل', 'م', 'ن', 'ه', 'ة', 'و', 'ي' }.ToList();
            List<char> SpecialCharacters = new char []{ '«', '»', '#', '$', '%', '&', '*', '(', ')', '<', '>', '{', '}', '[', ']', '_', '+', '-', '=', '^', '/', '\\', '|', '~' }.ToList();
            int[] countLetter = new int[36];
            countLetter.Initialize();
            string temp;
            string str = "";
           // content_Features_Jam3(i, article);
            //Extract F1 : Total number of characters C
            int TotalNumberofCharacters = article.Length;
            //Extract F2 : Total number of Letters (ا-ي)/C
            int TotalNumberofLetters = article.ToCharArray().Count(L => Char.IsLetter(L));
            //Extract F3 : Total number of Digits
            int TotalNumberofDigits = article.ToCharArray().Count(c=> Char.IsDigit(c));
            //Extract F4 : Total number of White-space characters /C
            int TotalNumberofWhiteSpaces = article.ToCharArray().Count(w=> Char.IsWhiteSpace(w));
            //Extract F5 : Total number of tab space characters/C
            temp = article;
            str = Regex.Replace(temp, " {2,}", @"A");
            int TotalNumberofTabSpaces = str.Length - Regex.Replace(temp, " {2,}", "").Length;
            //Extract F6 : Total number of tab space characters/C
            int EnlogationCounts = article.ToCharArray().Count(c => c == 'ـ');
            //Extract F7 : Total number of tab space characters/C
            temp = article;
            str = Regex.Replace(temp, "[ـ]{2,}", @"A");
            int dulicatesEnlogationCounts = str.Length - Regex.Replace(temp, "[ـ]{2,}", "").Length;
            Monitor.Enter(Features);
            Features[i][0] = TotalNumberofCharacters;
            
            Features[i][1] = TotalNumberofLetters;
            
            Features[i][2] = TotalNumberofDigits;

            Features[i][3] = TotalNumberofWhiteSpaces;

            Features[i][4] = TotalNumberofTabSpaces;
            Features[i][5] = EnlogationCounts;
            Features[i][6] = dulicatesEnlogationCounts;
            Monitor.Exit(Features);
            //Extract F8 to F31 : Number of special characters (%,&,etc.)/C
            int [] SpecialChars_counter = new int[24];
            SpecialChars_counter.Initialize();
            
            foreach (char c in article.ToCharArray())
                if (SpecialCharacters.Contains(c))
                    SpecialChars_counter[SpecialCharacters.IndexOf(c)]++;
            Monitor.Enter(Features);
            for (int t = 0; t < SpecialChars_counter.Length; t++)
                Features[i][7 + t] = SpecialChars_counter[t];
            Monitor.Exit(Features);
               

            //Extract F32 to F67 : Number of Indivisual characters (ا,ب,ت,etc.)/C
            foreach (char c in article.ToCharArray())
                if (Letters.Contains(c))
                    countLetter[Letters.IndexOf(c)]++;
            Monitor.Enter(Features);
            for (int x = 0; x < countLetter.Length; x++)
                Features[i][x+31] = countLetter[x];
            Monitor.Exit(Features);
            //form.setProgressValue((i * 100) / articles.Count);
            
        }

        //Extract features from F67 to F87.
        private void word_based(int i, string article)
        {
            string str = "";
            str = article;
            string[] words = Regex.Replace(str, @"\s+", " ").Split(new string[] { " ", "!", ".", ",", "،", "؛", "؟" ,":",
                "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»" }, StringSplitOptions.RemoveEmptyEntries);

            //Average length per word (in characters)
            int countChar = string.Join("", words).Length;

            //Words longer than 6 characters/N
            int WordLongerThan6_Counter = words.Count(x=>x.Length > 6);

            //Total number of short words (1-3 characters)/N
            int WordShorterThan4_Counter = words.Count(x=> x.Length < 4);

            //Word length frequency distribution/N
            int[] wordDistribution = new int[15];
            wordDistribution.Initialize();
            foreach (string s in words)
                if (s.Length > 0 && s.Length < 16)
                    wordDistribution[s.Length - 1]++;

            //Compute vocabulary richness using hapax legomena and dislegomena,
            //Yules, simpsons D, Sichels S, Honores R and Entropy measures.
            var dict = new Dictionary<string, int>();
            str = article;
            words = Regex.Replace(str, @"\s+", " ").Split(new string[] {" ", "!", ".", ",", "،", "؛", "؟" ,":",
                "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»"}, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in  words)
                if (dict.ContainsKey(word))
                    dict[word]+=1;
                else
                    dict.Add(word,1);
            float Vi;
            float YulesK = 0.0F;
            int countDifferent_words = dict.Count();
            int countWords = words.Length;
            for (int q = 1; q <= countDifferent_words ; q++)
            {
                Vi = dict.Count(x => x.Value == q);
                YulesK += Vi * ((1 / (float)countWords) * (1 / (float)countWords));
            }
            YulesK = -(1 / (float)countWords) + YulesK;
            YulesK = 10000 * YulesK;
           
            float SimpsonsD = 0;
            for (int w = 1; w <= countDifferent_words; w++)
            {
                Vi = dict.Count(x => x.Value == w);
                SimpsonsD += Vi * ((1 / (float)countWords) * ((w - 1) / (float)(countWords - 1)));
            }

            int uniqueOne = dict.Count(x => x.Value == 1);
            int uniqueTwo = dict.Count(x => x.Value == 2);
            float SichelsS = (float)(uniqueTwo) / (float)countWords;
            float HonoresR = (float)(100 * Math.Log10(countWords) / (float)(countWords > 0 ? (1 - (uniqueOne / (float)countWords)) : 1));
            float Entropy = 0.0F;
            for (int y = 1; y <= countWords; y++)
            {
                Vi = dict.Count(x => x.Value == y);
                Entropy += (float)(Vi * (-1*(Math.Log10(y / (float)countWords)) * (y / (float)countWords)));
            }

            //Number of digital words with digit characters
            int DigitWordsCounts = words.Count(x => x.ToCharArray().Count(z=> Char.IsDigit(z)) > 0);

            //Number of words with sequential duplicate letters.
            int WordswithDuplicateLetters = words.Count(x => checkDuplicate(x));
            
            //Save features to "Features" array
            Monitor.Enter(Features);
            Features[i][67] = words.Length;
            if (words.Length > 0)
                Features[i][68] = ((float)countChar / (float)words.Length);
            Features[i][69] = countDifferent_words;
            Features[i][70] = WordLongerThan6_Counter;
            Features[i][71] = WordShorterThan4_Counter;
            Features[i][72] = uniqueOne;
            Features[i][73] = uniqueTwo;
            for (int t = 0; t < wordDistribution.Length; t++)
                Features[i][74 + t] = wordDistribution[t];
            Features[i][89] = DigitWordsCounts;
            Features[i][90] = WordswithDuplicateLetters;
            
            Features[i][91] = YulesK;
            Features[i][92] = SimpsonsD;
            Features[i][93] = SichelsS;
            Features[i][94] = HonoresR;
            Features[i][95] = Entropy;
            
            Monitor.Exit(Features);
            
        }
        private bool checkDuplicate(string x)
        {
            int pervlength = x.Length;
            Regex r = new Regex("(.)(?<=\\1\\1)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            string str = r.Replace(x, string.Empty);
            int afterlength = str.Length;
            if (pervlength != afterlength)
                return true;
            return false;

        }
        private bool checkDigit(string x)
        {
            foreach (char c in x.ToCharArray())
                if (Char.IsDigit(c))
                    return true;
            return false;
        }

        private void synt(int i,string article)
        {
            
            string str;
            int[] syntacticsMarks = { '’','‘', '،',',', '.', ':', '؛', '؟', '!' , '\"' };
            int[] syntacticsMarks_counter = new int[10];
            syntacticsMarks_counter.Initialize();
           
            str = article;
            foreach (int c in str.ToCharArray())
                if (syntacticsMarks.Contains(c))
                    syntacticsMarks_counter[syntacticsMarks.ToList().IndexOf(c)]++;
            string temp = Regex.Replace(str, "؟{2,}", @"A");
            string temp1 = Regex.Replace(str, "!{2,}", @"A");
            string temp2 = Regex.Replace(str, "[.]{2,}", @"A");
            int t1 = temp.Length - Regex.Replace(str, "؟{2,}", string.Empty).Length;
            int t2 = temp1.Length - Regex.Replace(str, "!{2,}", string.Empty).Length;
            int t3 = temp2.Length - Regex.Replace(str, "[.]{2,}", string.Empty).Length;
            Monitor.Enter(Features);
            for (int t = 0; t < syntacticsMarks_counter.Length; t++)
                Features[i][96 + t] = syntacticsMarks_counter[t];

            Features[i][106] = t1;

            Features[i][107] = t2;

            Features[i][108] = t3;
            
            Monitor.Exit(Features);
             
        }

        private void struc(int i,string article)
        {
           
            string paragraph = "";
            string[] Lines;
            List<string> paragraphs = new List<string>();
            Lines = article.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            foreach (string L in Lines)
            {
                paragraph += L.Trim();
                if (paragraph.EndsWith("."))
                {
                    paragraphs.Add(paragraph);
                    paragraph = "";
                }
            }
            string[] separatorsSentence = { ":",",",";",".", "،", "؛", "!", "؟"};
            string[] sentences = article.Split(separatorsSentence, StringSplitOptions.RemoveEmptyEntries);
            string str = article;
            string[] words = Regex.Replace(str, @"\s+", " ").Split(new string[] { " ", "!", ".", ",", "،", "؛", "؟" ,":",
                "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»" }, StringSplitOptions.RemoveEmptyEntries);
            List<string> blankLines = str.Split(new string[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
            blankLines.RemoveAll(x => x.Trim().CompareTo(string.Empty) == 0);
            
            int[] sentencesLevel = new int[11];
            for(int j =0; j<sentencesLevel.Length-1;j++)
                sentencesLevel[j] = sentences.Count(x => x.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length == j+1);
            sentencesLevel[sentencesLevel.Length - 1] = sentences.Count(x => x.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Length >= 30);
            char[] shortphrase = { '<', '(', '{', '[', '«' };
            int shortpraseCount = article.ToCharArray().Count(c => shortphrase.Contains(c));
            int shortPhrasescounts = (article.ToCharArray().Count(c => c == '"' | c == '\'' || c == '-' || c =='_') / 2) + shortpraseCount;
           
            Monitor.Enter(Features);
            Features[i][109] = Lines.Length;
            Features[i][110] = sentences.Length;
            Features[i][111] = (paragraphs.Count > 0 ? paragraphs.Count : 1);
            Features[i][112] = ((float)sentences.Length / (float)(paragraphs.Count > 0 ? paragraphs.Count : 1));
            Features[i][113] = ((float)words.Length / (float)(paragraphs.Count > 0 ? paragraphs.Count : 1));
            Features[i][114] = (article.Length / (paragraphs.Count > 0 ? paragraphs.Count : 1));
            Features[i][115] = ((float)words.Length / (float)sentences.Length);
            Features[i][116] = blankLines.Count();
            Features[i][117] = (float)article.Length / (float)(Lines.Length - blankLines.Count());
            Features[i][118] = shortPhrasescounts;
            for(int j=0;j<sentencesLevel.Length;j++)
                Features[i][j + 119] = sentencesLevel[j];
            Monitor.Exit(Features);
            stopwords_based(i, article);
        }

        private void stopwords_based(int i, string article)
        {
            List<string> stopwords = new List<string>(new string[]{"ان","بعد","ضد","يلي","الى","في","من","حتى","هو","هم",
                "هي","يكون","به","ليس","احد","على","كان","تلك","كذلك","التي","بين","لكن","عن","منذ","الذي","اما","حين",
                "لا","اي","ما","حول","دون","مع","هذا","فقط","ثم","هذه","تكون","قد","جدا","لن","نحو","لم","هؤلاء","ذلك",
                "لو","عند","اللذين","كل","بد","لدي","لدى","فقد","بل","تحت","او","أو","اذ","إذ","علي","عليه","كما","كيف","هنا",
                "لذلك","امام","هناك","قبل","يوم","انت","أنت","هل","حيث","جميع","اذا","و","الي","إلي","مازال","لازال",
                "لايزال","مايزال","اصبح","امسى","اضحى","ظل","مابرح","مافتى","ماانفك","بات","صار","ليت","لعل","لاسيما",
                "الحالي","ضمن","اول","له","ذات","بدلا","الذين","الا","مما","ممن","ابو","يمكن","لدي","ال","آل","هن","حاليا",
                "ممكن","اينما","مهما","لوما","لولا","منذا","ماذا","متى","اين","ام","أم","كي","لما","اذن","إذن","خلا",
                "حاشا","عدا","مذ","كم","صباح","شهر","سنة","اسبوع","بالنسبة","بالمناسبة","لاتزال","ماتزال","زال","مادام",
                "عسى","كاد","شهور","سنوات","اسابيع","اوشك","وبالتالي","وبالاصح","اساسا","الهذا","بالرغم","ايا","أن",
                "أينما","أي","إذا","إن","أنى","أيان","أين","إلى","إلا","أبو","أمام","أول","أصبح","أمسى","أضحى","وبالأصح",
                "أوشك","أساسا","ألهذا","أيا","مساء","كأن","بسبب","سبب","وقت","زمن","عقب","خلف","فوق","بجانب","مناسب",
                "توقيت","لحظة","لحظه","ذو","ذي","حال","احوال","أحوال" });
            string str = article;
            
            string[] words = Regex.Replace(str, @"\s+", " ").Split(new string[] {" ", "!", ".", ",", "،", "؛", "؟" ,":",
                "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»" }, StringSplitOptions.RemoveEmptyEntries);
            int[] stopwordsCounts = new int[stopwords.Count];
            stopwordsCounts.Initialize();
            bool stopwordsfound = false;
            string w;
            
            foreach (string word in words)
            {
                stopwordsfound = false;
                if (stopwords.Contains(word))
                {
                    stopwordsCounts[stopwords.IndexOf(word)]++;
                    continue;
                }
                else
                {
                    w = RemovePrefixes(prefix, word);
                    if (stopwords.Contains(w))
                    {
                        stopwordsCounts[stopwords.IndexOf(w)]++;
                        stopwordsfound = true;
                    }
                    
                }
                
                if (!stopwordsfound)
                {
                    if (word.StartsWith("ال"))
                    {
                        w = word.Substring(2);
                        if (stopwords.Contains(w))
                        {
                            stopwordsCounts[stopwords.IndexOf(w)]++;
                            stopwordsfound = true;
                            continue;
                        }
                    }
                }
                if (!stopwordsfound)
                {
                    w = RemoveSuffixes(suffix, word);
                    if (stopwords.Contains(w))
                    {
                        stopwordsCounts[stopwords.IndexOf(w)]++;
                        stopwordsfound = true;
                    }
                
                }    
            }
           

            Monitor.Enter(Features);
            for (int j = 0; j < stopwordsCounts.Length; j++)
            {
                Features[i][j + 130] = stopwordsCounts[j];
            }
            Monitor.Exit(Features);
           
        }

        public static string RemovePrefixes(string [] prefix, string word)
        {
            foreach (string p in prefix)
                if (word.StartsWith(p))
                    return  word.Substring(p.Length);
            return word;

        }
        public static string RemoveSuffixes(string[] suffix, string word)
        {
            foreach (string s in suffix)
                if (word.EndsWith(s))
                    return word.Substring(0,word.Length - s.Length);
            return word;
        }

        public int _content(string[] p, string[] content)
        {
            string[] phrase;
            int counter = 0;
            string sphrase;
            const int MaxPhraseLength = 3;
            for (int phraseLen = MaxPhraseLength; phraseLen >= 1; phraseLen--)
            {
                for (int j = 0; j < content.Length; j++)
                {
                    if (phraseLen == 1)
                        sphrase = content[j].Trim();
                    else
                    {
                        //get the phrase to match based on phraselen
                        phrase = content.Skip(j).Take(phraseLen).ToArray();
                        sphrase = string.Join(" ", phrase);
                    }
                    if (p.Contains(sphrase))
                        counter++;
                    else
                    {
                        sphrase = RemovePrefixes(this.prefix, sphrase);
                        if (p.Contains(sphrase))
                            counter++;
                        else
                        {
                            sphrase = RemovePrefixes(new string[] { "ال" }, sphrase);
                            if (p.Contains(sphrase))
                                counter++;
                            else
                            {
                                sphrase = RemovePrefixes(this.suffix, sphrase);
                                if (p.Contains(sphrase))
                                    counter++;
                            }
                        }
                    }
                }
            }
            return counter;
        }

        public static string Normalize_Alif(string word)
        {
            word = word.Replace("أ", "ا");
            word = word.Replace("إ", "ا");
            word = word.Replace("آ", "ا");
            return word;
        }

        
        private void content_Features(int i, string article)
        {
            string[] Policy = this.Policy;
            string[] Economy = this.Economy;
            string[] Sport = this.Sport;
            string[] Social = this.Social;
            string[] negative = this.negative;
            string str = article;
            
            int policyCount = 0;
            int economicCount = 0;
            int sportCount = 0;
            int socialCount = 0;
            int negativeCounts = 0;
            str = str.Replace('أ', 'ا');
            str = str.Replace('إ', 'ا');
            str = str.Replace('آ', 'ا');
            str = str.Replace('ة','ه');
            string[] words = Regex.Replace(str, @"\s+", " ").Split(new string[] { " ", "!", ".", ",", "،", "؛", "؟",":",
                "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»" }, StringSplitOptions.RemoveEmptyEntries);

            policyCount = _content(Policy, words);
            economicCount =_content(Economy, words);
            sportCount = _content(Sport, words);
            socialCount = _content(Social, words);
            negativeCounts = _content(negative, words);
            
            Monitor.Enter(Features);
            
            Features[i][323] = policyCount;
            
            Features[i][324] = economicCount;
            
            Features[i][325] = sportCount;
           
            Features[i][326] = socialCount;
            Features[i][327] = negativeCounts;
            Monitor.Exit(Features);
            diacritiesFeatures(i, article);
        
        }
        private void diacritiesFeatures(int i, string article)
        {
            int[] diacrities = { 'َ', 'ِ', 'ً', 'ٍ', 'ُ', 'ٌ', 'ْ', 'ّ' };
            string str = article;
            int[] diacritiesCounts = new int[8];
            diacritiesCounts.Initialize();
            foreach (int c in str)
                if (diacrities.Contains(c))
                    diacritiesCounts[diacrities.ToList().IndexOf(c)]++;
            Monitor.Enter(Features);
            for (int j = 0; j < diacrities.Length; j++)
                Features[i][j + 328] = diacritiesCounts[j];
            
            Monitor.Exit(Features);
            titleFeatures(i, article);

        }
        private void titleFeatures(int i, string article)
        {
            string title = article.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).First();
            int[] SpecialCharacters = {'$', '%', '&', '*', '(', ')', '<', '>', '{', '}', '[', ']', '_', '!','؟','.','«','»','"',':',',','،','؛'};
          
            string str = article;

            Monitor.Enter(Features);
            Features[i][336] = title.Split(new string[] {" ", "!", ".", ",", "،", "؛", "؟" ,":",
                "@","$","&","*", "(", ")", "<", ">", "{", "}", "[", "]", "_","«","»"}, StringSplitOptions.RemoveEmptyEntries).Count();
            Features[i][337] = title.Length;
            Features[i][338] = title.ToCharArray().Count(x => SpecialCharacters.Contains(x));
           
            Monitor.Exit(Features);

            lock (form)
            {
                form.setProgressValue(((i + 1) * 100) / _NumberOf_Articles,i+1,_NumberOf_Articles );
            }
        }

        
     
        public bool _contentCheck(string[] p, string[] content)
        {
            string[] phrase;
            //int counter = 0;
            string sphrase;
            const int MaxPhraseLength = 3;
            for (int phraseLen = MaxPhraseLength; phraseLen >= 1; phraseLen--)
            {
                for (int j = 0; j < content.Length; j++)
                {
                   // if (phraseLen == 1)
                     //   sphrase = content[j].Trim();
                    //else
                    {
                        //get the phrase to match based on phraselen
                        phrase = content.Skip(j).Take(phraseLen).ToArray();
                        sphrase = string.Join(" ", phrase);
                    }
                    if (p.Contains(sphrase))
                        return true;
                    else
                    {
                        sphrase = RemovePrefixes(this.prefix, sphrase);
                        if (p.Contains(sphrase))
                            return true;
                        else
                        {
                            sphrase = RemovePrefixes(new string[] { "ال" }, sphrase);
                            if (p.Contains(sphrase))
                                return true;
                            else
                            {
                                sphrase = RemovePrefixes(this.suffix, sphrase);
                                if (p.Contains(sphrase))
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
        private int _contentIndex(string[] p, string[] content)
        {
            string[] phrase;
            //int counter = 0;
            string sphrase;
            const int MaxPhraseLength = 3;
            for (int phraseLen = MaxPhraseLength; phraseLen >= 1; phraseLen--)
            {
                for (int j = 0; j < content.Length; j++)
                {
                   // if (phraseLen == 1)
                     //   sphrase = content[j].Trim();
                    //else
                    {
                        //get the phrase to match based on phraselen
                        phrase = content.Skip(j).Take(phraseLen).ToArray();
                        sphrase = string.Join(" ", phrase);
                    }
                    if (p.Contains(sphrase))
                        return p.ToList().IndexOf(sphrase);
                    else
                    {
                        sphrase = RemovePrefixes(this.prefix, sphrase);
                        if (p.Contains(sphrase))
                            return p.ToList().IndexOf(sphrase);
                        else
                        {
                            sphrase = RemovePrefixes(new string[] { "ال" }, sphrase);
                            if (p.Contains(sphrase))
                                return p.ToList().IndexOf(sphrase);
                            else
                            {
                                sphrase = RemovePrefixes(this.suffix, sphrase);
                                if (p.Contains(sphrase))
                                    return p.ToList().IndexOf(sphrase);
                            }
                        }
                    }
                }
            }
            return -1;
        }
      
        public void GenerateExcelFile(System.Data.DataTable ds, string paramFileFullPath) 
        {

            CreateExcelFile.CreateExcelDocument(ds, paramFileFullPath);
             
        }
        
    }
}
