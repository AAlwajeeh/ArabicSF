using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
namespace ArabicSF
{
    /*

Arabic Stemmer: This program stems Arabic words and returns their root.
Copyright (C) 2002 Shereen Khoja

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

Computing Department
Lancaster University
Lancaster
LA1 4YR
s.Khoja@lancaster.ac.uk


Stem class
This is the class that does all the work
It takes in a file containg the text to
be stemmed. The text is then read one line
at a time so large amounts of text can be
efficiently processed.
The class also takes in the vector
containing all the contents of the static
files.
The class now tests for stopwords correctly,
and removes punctuation and diacritics (though
I haven't decided whether to return them to the
word in the final document or not)


Last Modified: 11/6/2001
*/

    public class Stem
    {
        //--------------------------------------------------------------------------

        // boolean variable to check the files
        protected bool couldNotOpenFile = false;

        // one line of text
        private String oneLine;

        // all text to be passed to the text window
        private StringBuilder all = new StringBuilder("");

        // the stemmed Arabic text
        private StringBuilder stemmedText = new StringBuilder();

        // the tokenized line with all punctuation removed
        private List<string> tokenizedLine = new List<string>();

        // the files containing prefixes, suffixes and so on
        private List<string> staticFiles;

        // have the root, pattern, stopword or strange words been found
        private bool rootFound = false;
       // private bool patternFound = false;
        private bool stopwordFound = false;
        private bool strangeWordFound = false;
        private bool rootNotFound = false;
        private bool fromSuffixes = false;
        private String[,] stemmedDocument;
        private String[,] possibleRoots;
        private int wordNumber = 0;
        private int stemmedWordsNumber = 0;
        private int notStemmedWordsNumber = 0;
        private int stopwordNumber = 0;
        private int punctuationWordNumber = 0;
        private int notWordNumber = 0;
        private int[] numberStatistics;
        private List<string> listStemmedWords = new List<string>();
        private List<string> listRootsFound = new List<string>();
        private List<string> listNotStemmedWords = new List<string>();
        private List<string> listStopwordsFound = new List<string>();
        private List<string> listOriginalStopword = new List<string>();
        private List<string> wordsNotStemmed = new List<string>();
        private List<string> wordsWithNoRoots = new List<string>();
        string fileToBeStemmed;
        int number = 0;

        //--------------------------------------------------------------------------

        // constructor
        public Stem(string  fileToBeStemmed, List<string> statFiles)
        {
            staticFiles = statFiles;
            this.fileToBeStemmed =  fileToBeStemmed;
            // create the statistics file array
            stemmedDocument = new String[20000, 3];

            // create the possible roots array
            possibleRoots = new String[10000, 2];

            // read the contents of the file, one line at a time
            // after each line stem the words
            readFromFile(fileToBeStemmed);
        }

        public List <List <string>> GetResults()
        {

            List<List <string>> results = new List<List<string>>();
            results.Add(listRootsFound);
            results.Add(listStemmedWords);
            results.Add(listNotStemmedWords);
            results.Add(listStopwordsFound);
            results.Add(listOriginalStopword);
           


            return results;
            
        }

        //--------------------------------------------------------------------------

        // return the results
        public String displayText()
        {
            return (all.ToString());
        }

        //--------------------------------------------------------------------------

        // return the stemming statistics
        public int[] returnNumberStatistics()
        {
            numberStatistics = new int[6];
            numberStatistics[0] = wordNumber;
            numberStatistics[1] = stemmedWordsNumber;
            numberStatistics[2] = notStemmedWordsNumber;
            numberStatistics[3] = stopwordNumber;
            numberStatistics[4] = punctuationWordNumber;
            numberStatistics[5] = notWordNumber;
            return numberStatistics;
        }

        //--------------------------------------------------------------------------

        // return the stemmed document
        public String[,] returnStatistics()
        {
            return stemmedDocument;
        }

        //--------------------------------------------------------------------------

        // read the contents of the file and tokenize the text
        private void readFromFile(string currentFile)
        {
            String [] br;
            //try
            //{
                // read from the file using FileReader
                //FileInputStream in = new FileInputStream ( currentFile );
            if(currentFile.EndsWith(".txt"))
                 br = File.ReadAllLines(currentFile);//new InputStreamReader ( in, "UTF-16" );
                //StringBuilder  br = new StringBuilder( str );
            else
                br = currentFile.Split(new string []{Environment.NewLine},StringSplitOptions.RemoveEmptyEntries);

                // initialize index
                StringBuilder word = new StringBuilder();
                //Char character;
                String currentWord;
                int lineNumber = 1;
                int x = 0;
                // read in the text a line at a time
                while (x < br.Length)
                {
                    oneLine = br[x];
                    // add spaces at the end of the line
                    oneLine = oneLine + "  ";
                    x++;
                    lineNumber++;

                    // tokenize each line
                    for (int i = 0; i < oneLine.Length; i++)
                    {
                        // if the character is not a space, append it to a word
                        if (!oneLine[i].Equals(' '))
                        {
                            word.Append(oneLine[i]);
                        }
                        // otherwise, if the word contains some characters, add it to the vector
                        else
                        {
                            if (word.Length != 0)
                            {
                                tokenizedLine.Add(word.ToString());
                                word.Length = 0;
                            }
                        }

                    }

                    // now we have tokenized one line, we should stem it
                    for (int i = 0; i < tokenizedLine.Count; i++)
                    {
                        // set the word in a string
                        currentWord = tokenizedLine[i];

                        // store the original word in the array stemmedDocument
                        stemmedDocument[wordNumber, 0] = currentWord;

                        // stem the word
                        currentWord = formatWord(currentWord, i);

                        // if the word wasn't stemmed, indicate this in stemmedDocument
                        if (stemmedDocument[wordNumber, 2] == null)
                        {
                            stemmedDocument[wordNumber, 1] = currentWord;
                            stemmedDocument[wordNumber, 2] = "NOT STEMMED";
                            notStemmedWordsNumber++;
                            listNotStemmedWords.Add(currentWord);
                        }
                        else if (number > 0)
                        {
                            number--;
                            while (number > 0 && possibleRoots[number, 0] == stemmedDocument[wordNumber, 0])
                            {
                                number--;
                            }
                            if (number == 0 && possibleRoots[number, 0] == stemmedDocument[wordNumber, 0])
                                number = 0;
                            else
                                number++;
                        }

                        // increment wordNumber
                        wordNumber++;

                        // re-initialize the variable rootFound
                        rootFound = false;

                        // add the stemmed word to the vector
                        stemmedText.Append(currentWord);
                    }

                    // after adding all the stemmed word on this line, we should add a new line character
                    stemmedText.Append("\n");

                    // after we have finished processing this line we should clear it
                    tokenizedLine.Clear();

                }
            }
        

        

        //--------------------------------------------------------------------------

        // format the word by removing any punctuation, diacritics and non-letter charracters
        private String formatWord(String currentWord, int index)
        {
            StringBuilder modifiedWord = new StringBuilder();

            // remove any diacritics (short vowels)
            //if ( removeDiacritics( currentWord, modifiedWord ) )
            //{
             // tokenizedLine.Insert( index, currentWord = modifiedWord.ToString ());
            //}

            // remove any punctuation from the word
            //if ( removePunctuation( currentWord, modifiedWord ) )
            //{
            //  tokenizedLine.setElementAt ( currentWord = modifiedWord.toString ( ), index );
            //}

            // there could also be characters that aren't letters which should be removed
            //if ( removeNonLetter ( currentWord, modifiedWord ) )
            //{
            //  tokenizedLine.setElementAt ( currentWord = modifiedWord.toString ( ), index );
            //}

            // check for stopwords
            if (!checkStrangeWords(currentWord))
                // check for stopwords
                if( !checkStopwords ( currentWord ) )
                    currentWord = stemWord(currentWord);

            return currentWord;
        }

        //--------------------------------------------------------------------------

        // stem the word
        private String stemWord(String word)
        {
            // check if the word consists of two letters
            // and find it's root
            if (word.Length == 2)
                word = isTwoLetters(word);

            // if the word consists of three letters
            if (word.Length == 3 && !rootFound)
                // check if it's a root
                word = isThreeLetters(word);

            // if the word consists of four letters
            if (word.Length == 4)
                // check if it's a root
                isFourLetters(word);

            // if the root hasn't yet been found
            if (!rootFound)
            {
                // check if the word is a pattern
                word = checkPatterns(word);
            }

            // if the root still hasn't been found
            if (!rootFound)
            {
                // check for a definite article, and remove it
                word = checkDefiniteArticle(word);
            }

            // if the root still hasn't been found
            if (!rootFound && !stopwordFound)
            {
                // check for the prefix waw
                word = checkPrefixWaw(word);
            }

            // if the root STILL hasn't been found
            if (!rootFound && !stopwordFound)
            {
                // check for prefixes
                word = checkForPrefixes(word);
            }

            // if the root STILL hasnt' been found
            if (!rootFound && !stopwordFound)
            {
                // check for suffixes
                word = checkForSuffixes(word);
            }

           
            return word;
        }


        //--------------------------------------------------------------------------

        // check and remove any prefixes
        private String checkForPrefixes(String word)
        {
            String prefix = "";
            String modifiedWord = word;
            string [] prefixes = staticFiles[11].Split(' ').ToArray();

            // for every prefix in the list
            for (int i = 0; i < prefixes.Length; i++)
            {
                prefix = prefixes[i];
                // if the prefix was found
                if (modifiedWord.StartsWith(prefix))
                {
                    modifiedWord = modifiedWord.Substring(prefix.Length);

                     //check to see if the word is a stopword
                     if ( checkStopwords( modifiedWord ) )
                       return modifiedWord;

                    // check to see if the word is a root of three or four letters
                    // if the word has only two letters, test to see if one was removed
                    if (modifiedWord.Length == 2)
                        modifiedWord = isTwoLetters(modifiedWord);
                    else if (modifiedWord.Length == 3 && !rootFound)
                        modifiedWord = isThreeLetters(modifiedWord);
                    else if (modifiedWord.Length == 4)
                        isFourLetters(modifiedWord);

                    // if the root hasn't been found, check for patterns
                    if (!rootFound && modifiedWord.Length > 2)
                        modifiedWord = checkPatterns(modifiedWord);

                    // if the root STILL hasn't been found
                    if (!rootFound && !stopwordFound && !fromSuffixes)
                    {
                        // check for suffixes
                        modifiedWord = checkForSuffixes(modifiedWord);
                    }

                    if (stopwordFound)
                        return modifiedWord;

                    // if the root was found, return the modified word
                    if (rootFound && !stopwordFound)
                    {
                        return modifiedWord;
                    }
                }
            }
            return word;
        }

        //--------------------------------------------------------------------------

        // METHOD CHECKFORSUFFIXES
        private String checkForSuffixes(String word)
        {
            String suffix = "";
            String modifiedWord = word;
            string [] suffixes = staticFiles[16].Split(' ').ToArray();
            fromSuffixes = true;

            // for every suffix in the list
            for (int i = 0; i < suffixes.Length; i++)
            {
                suffix = suffixes[i];

                // if the suffix was found
                if (modifiedWord.EndsWith(suffix))
                {
                    modifiedWord = modifiedWord.Substring(0, modifiedWord.Length - suffix.Length);

                    // check to see if the word is a stopword
                    if ( checkStopwords ( modifiedWord ) )
                    {
                        fromSuffixes = false;
                        return modifiedWord;
                    }

                    // check to see if the word is a root of three or four letters
                    // if the word has only two letters, test to see if one was removed
                    if (modifiedWord.Length == 2)
                    {
                        modifiedWord = isTwoLetters(modifiedWord);
                    }
                    else if (modifiedWord.Length == 3)
                    {
                        modifiedWord = isThreeLetters(modifiedWord);
                    }
                    else if (modifiedWord.Length == 4)
                    {
                        isFourLetters(modifiedWord);
                    }

                    // if the root hasn't been found, check for patterns
                    if (!rootFound && modifiedWord.Length > 2)
                    {
                        modifiedWord = checkPatterns(modifiedWord);
                    }

                    if (stopwordFound)
                    {
                        fromSuffixes = false;
                        return modifiedWord;
                    }

                    // if the root was found, return the modified word
                    if (rootFound)
                    {
                        fromSuffixes = false;
                        return modifiedWord;
                    }
                }
            }
            fromSuffixes = false;
            return word;
        }

        //--------------------------------------------------------------------------

        // check and remove the special prefix (waw)
        private String checkPrefixWaw(String word)
        {
            String modifiedWord = "";

            if (word.Length > 3 && word[0] == '\u0648')
            {
                modifiedWord = word.Substring(1);

                // check to see if the word is a stopword
                if ( checkStopwords ( modifiedWord ) )
                    return modifiedWord;

                // check to see if the word is a root of three or four letters
                // if the word has only two letters, test to see if one was removed
                if (modifiedWord.Length == 2)
                    modifiedWord = isTwoLetters(modifiedWord);
                else if (modifiedWord.Length == 3 && !rootFound)
                    modifiedWord = isThreeLetters(modifiedWord);
                else if (modifiedWord.Length == 4)
                    isFourLetters(modifiedWord);

                // if the root hasn't been found, check for patterns
                if (!rootFound && modifiedWord.Length > 2)
                    modifiedWord = checkPatterns(modifiedWord);

                // iIf the root STILL hasn't been found
                if (!rootFound && !stopwordFound)
                {
                    // check for prefixes
                    modifiedWord = checkForPrefixes(modifiedWord);
                }
                // if the root STILL hasnt' been found
                if (!rootFound && !stopwordFound)
                {
                    // check for suffixes
                    modifiedWord = checkForSuffixes(modifiedWord);
                }

               

                if (stopwordFound)
                    return modifiedWord;

                if (rootFound && !stopwordFound)
                {
                    return modifiedWord;
                }
            }
            return word;
        }

        //--------------------------------------------------------------------------

        // check and remove the definite article
        private String checkDefiniteArticle(String word)
        {
            // looking through the vector of definite articles
            // search through each definite article, and try and
            // find a match
            String definiteArticle = "";
            String modifiedWord = "";
            string [] definiteArticles = staticFiles[0].Split(' ');

            // for every definite article in the list
            for (int i = 0; i < definiteArticles.Length; i++)
            {
                definiteArticle = definiteArticles[i];
                // if the definite article was found
                if ( word.StartsWith(definiteArticle))
                {
                    // remove the definite article
                    modifiedWord = word.Substring(definiteArticle.Length, word.Length - definiteArticle.Length);

                    // check to see if the word is a stopword
                    if ( checkStopwords ( modifiedWord ) )
                        return modifiedWord;

                    // check to see if the word is a root of three or four letters
                    // if the word has only two letters, test to see if one was removed
                    if (modifiedWord.Length == 2)
                        modifiedWord = isTwoLetters(modifiedWord);
                    else if (modifiedWord.Length == 3 && !rootFound)
                        modifiedWord = isThreeLetters(modifiedWord);
                    else if (modifiedWord.Length == 4)
                        isFourLetters(modifiedWord);

                    // if the root hasn't been found, check for patterns
                    if (!rootFound && modifiedWord.Length > 2)
                        modifiedWord = checkPatterns(modifiedWord);

                    if (!rootFound && !stopwordFound)
                    {
                        // check for prefixes
                        modifiedWord = checkForPrefixes(modifiedWord);
                    }
                    // if the root STILL hasnt' been found
                    if (!rootFound && !stopwordFound)
                    {
                        // check for suffixes
                        modifiedWord = checkForSuffixes(modifiedWord);
                    }

                    // if the root STILL hasn't been found
                   


                    if (stopwordFound)
                        return modifiedWord;


                    // if the root was found, return the modified word
                    if (rootFound && !stopwordFound)
                    {
                        return modifiedWord;
                    }
                }
            }
            if (modifiedWord.Length > 3)
                return modifiedWord;
            return word;
        }

        //--------------------------------------------------------------------------

        // if the word consists of two letters
        private String isTwoLetters(String word)
        {
            // if the word consists of two letters, then this could be either
            // - because it is a root consisting of two letters (though I can't think of any!)
            // - because a letter was deleted as it is duplicated or a weak middle or last letter.

            word = duplicate(word);

            // check if the last letter was weak
            if (!rootFound)
                word = lastWeak(word);

            // check if the first letter was weak
            if (!rootFound)
                word = firstWeak(word);

            // check if the middle letter was weak
            if (!rootFound)
                word = middleWeak(word);

            return word;
        }

        //--------------------------------------------------------------------------

        // if the word consists of three letters
        private String isThreeLetters(String word)
        {
            StringBuilder modifiedWord = new StringBuilder(word);
            String root = "";
            // if the first letter is a 'ا', 'ؤ'  or 'ئ'
            // then change it to a 'أ'
            if (word.Length > 0)
            {
                if (word[0] == '\u0627' || word[0] == '\u0624' || word[0] == '\u0626')
                {
                    modifiedWord.Length = 0;
                    modifiedWord.Append('\u0623');
                    modifiedWord.Append(word.Substring(1));
                    root = modifiedWord.ToString();
                }

               
                // if the last letter is a weak letter or a hamza
                // then remove it and check for last weak letters
                if (word[2] == '\u0648' || word[2] == '\u064a' || word[2] == '\u0627' ||
                     word[2] == '\u0649' || word[2] == '\u0621' || word[2] == '\u0626')
                {
                    root = word.Substring(0, 2);
                    root = lastWeak(root);
                    if (rootFound)
                    {
                        return root;
                    }
                }

                // if the second letter is a weak letter or a hamza
                // then remove it
                if (word[1] == '\u0648' || word[1] == '\u064a' || word[1] == '\u0627' || word[1] == '\u0626')
                {
                    root = word.Substring(0, 1);
                    root = root + word.Substring(2);

                    root = middleWeak(root);
                    if (rootFound)
                    {
                        return root;
                    }
                }

                // if the second letter has a hamza, and it's not on a alif
                // then it must be returned to the alif
                if (word[1] == '\u0624' || word[1] == '\u0626')
                {
                    if (word[2] == '\u0645' || word[2] == '\u0632' || word[2] == '\u0631')
                    {
                        root = word.Substring(0, 1);
                        root = root + '\u0627';
                        root = root + word.Substring(2);
                    }
                    else
                    {
                        root = word.Substring(0, 1);
                        root = root + '\u0623';
                        root = root + word.Substring(2);
                    }
                }

                // if the last letter is a shadda, remove it and
                // duplicate the last letter
                if (word[2] == '\u0651')
                {
                    root = word.Substring(0, 1);
                    root = root + word.Substring(1, 2);
                }
            }

            // if word is a root, then rootFound is true
            if (root.Length == 0)
            {
                if (staticFiles[18].Split(' ').Contains(word))
                {
                    rootFound = true;
                    stemmedDocument[wordNumber, 1] = word;
                    stemmedDocument[wordNumber, 2] = "ROOT";
                    stemmedWordsNumber++;
                    listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                    listRootsFound.Add(word);
                    if (rootNotFound)
                    {
                        for (int i = 0; i < number; i++)
                            wordsNotStemmed.RemoveAt(wordsNotStemmed.Count - 1);

                        rootNotFound = false;
                    }
                    return word;
                }
            }
            // check for the root that we just derived
            else if (staticFiles[18].Split(' ').Contains(root))
            {
                rootFound = true;
                stemmedDocument[wordNumber, 1] = root;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(root);
                if (rootNotFound)
                {
                    for (int i = 0; i < number; i++)
                        wordsNotStemmed.RemoveAt(wordsNotStemmed.Count - 1);
                    rootNotFound = false;
                }
                return root;
            }

            if (root.Length == 3)
            {
                possibleRoots[number, 1] = root;
                possibleRoots[number, 0] = stemmedDocument[wordNumber, 0];
                number++;
            }
            else
            {
                possibleRoots[number, 1] = word;
                possibleRoots[number, 0] = stemmedDocument[wordNumber, 0];
                number++;
            }
            return word;
        }

        //--------------------------------------------------------------------------

        // if the word has four letters
        private void isFourLetters(String word)
        {
            // if word is a root, then rootFound is true
            if (staticFiles[13].Split(' ').Contains(word))
            {
                 rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);
            }
        }

        //--------------------------------------------------------------------------

        // check if the word matches any of the patterns
        private String checkPatterns(String word)
        {
            StringBuilder root = new StringBuilder("");
            // if the first letter is a hamza, change it to an alif
            if (word.Length > 0)
                if (word[0] == '\u0623' || word[0] == '\u0625' || word[0] == '\u0622')
                {
                    root.Append("j");
                    root[0] = '\u0627';
                    root.Append(word.Substring(1));
                    word = root.ToString();
                }

            // try and find a pattern that matches the word
            string[] patterns = staticFiles[17].Split(' ');
              
            int numberSameLetters = 0;
            String pattern = "";
            String modifiedWord = "";

            // for every pattern
            for (int i = 0; i < patterns.Length; i++)
            {
                pattern = patterns[i];
                root.Length = 0;
                // if the length of the words are the same
                if (pattern.Length == word.Length)
                {
                    numberSameLetters = 0;
                    // find out how many letters are the same at the same index
                    // so long as they're not a fa, ain, or lam
                    for (int j = 0; j < word.Length; j++)
                        if (pattern[j] == word[j] &&
                             pattern[j] != '\u0641' &&
                             pattern[j] != '\u0639' &&
                             pattern[j] != '\u0644')
                            numberSameLetters++;

                    // test to see if the word matches the pattern افعلا
                    if (word.Length == 6 && word[3] == word[5] && numberSameLetters == 2)
                    {
                        root.Append(word[1]);
                        root.Append(word[2]);
                        root.Append(word[3]);
                        modifiedWord = root.ToString();
                        
                        modifiedWord = isThreeLetters(modifiedWord);
                        if (rootFound)
                            return modifiedWord;
                        else
                            root.Length = 0;
                    }


                    // if the word matches the pattern, get the root
                    if (word.Length - 3 <= numberSameLetters)
                    {
                        // derive the root from the word by matching it with the pattern
                        for (int j = 0; j < word.Length; j++)
                            if (pattern[j] == '\u0641' ||
                                 pattern[j] == '\u0639' ||
                                 pattern[j] == '\u0644')
                                root.Append(word[j]);

                        modifiedWord = root.ToString();
                        modifiedWord = isThreeLetters(modifiedWord);

                        if (rootFound)
                        {
                            word = modifiedWord;
                            return word;
                        }
                    }
                }
            }
            return word;
        }

        //--------------------------------------------------------------------------

        // remove non-letters from the word
        /*private bool removeNonLetter ( String currentWord, StringBuilder modifiedWord )
        {
            bool nonLetterFound = false;
            modifiedWord.Length = 0 ;

            // if any of the word is not a letter then remove it
            for( int i = 0; i < currentWord.Length; i++ )
            {
                if ( Character.isLetter ( currentWord.charAt ( i ) ) )
                {
                    modifiedWord.append ( currentWord.charAt ( i ) );
                    stemmedDocument[wordNumber][2] = null;
                }
                else
                {
                    nonLetterFound = true;
                    if ( modifiedWord.length ( ) == 0 && stemmedDocument[wordNumber][2] == null )
                    {
                        stemmedDocument[wordNumber][2] = "NOT LETTER";
                        notWordNumber ++;
                    }
                }
            }
            return nonLetterFound;
        }
    */
        //--------------------------------------------------------------------------

        // handle duplicate letters in the word
        private String duplicate ( String word )
        {
            // check if a letter was duplicated
            if ( staticFiles[2].Contains ( word ) )
            {
                // if so, then return the deleted duplicate letter
                word = word + word.Substring ( 1 );

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber,1] = word;
                stemmedDocument[wordNumber,2] = "ROOT";
                stemmedWordsNumber ++;
                listStemmedWords.Add( stemmedDocument[wordNumber,0] );
                listRootsFound.Add ( word );

                return word;
            }
            return word;
        }
        
        //--------------------------------------------------------------------------

        // check if the last letter of the word is a weak letter
        private String lastWeak(String word)
        {
            StringBuilder stemmedWord = new StringBuilder("");
            // check if the last letter was an alif
            if (staticFiles[5].Split(' ').Contains(word))
            {
                stemmedWord.Append(word);
                stemmedWord.Append("\u0627");
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            // check if the last letter was an hamza
            else if (staticFiles[6].Split(' ').Contains(word))
            {
                stemmedWord.Append(word);
                stemmedWord.Append("\u0623");
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            // check if the last letter was an maksoura
            else if (staticFiles[7].Split(' ').Contains(word))
            {
                stemmedWord.Append(word);
                stemmedWord.Append("\u0649");
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            // check if the last letter was an yah
            else if (staticFiles[8].Split(' ').Contains(word))
            {
                stemmedWord.Append(word);
                stemmedWord.Append("\u064a");
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            return word;
        }

        //--------------------------------------------------------------------------

        // check if the first letter is a weak letter
        private String firstWeak(String word)
        {
            StringBuilder stemmedWord = new StringBuilder("");
            // check if the first letter was a waw
            if (staticFiles[3].Split(' ').Contains(word))
            {
                stemmedWord.Append("\u0648");
                stemmedWord.Append(word);
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            // check if the first letter was a yah
            else if (staticFiles[4].Split(' ').Contains(word))
            {
                stemmedWord.Append("\u064a");
                stemmedWord.Append(word);
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            return word;
        }

        //--------------------------------------------------------------------------

        // check if the middle letter of the root is weak
        private String middleWeak(String word)
        {
            StringBuilder stemmedWord = new StringBuilder("j");
            // check if the middle letter is a waw
            if (staticFiles[9].Split(' ').Contains(word))
            {
                // return the waw to the word
                stemmedWord[0] = word[0];
                stemmedWord.Append("\u0648");
                stemmedWord.Append(word.Substring(1));
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            // check if the middle letter is a yah
            else if (staticFiles[10].Contains(word))
            {
                // return the waw to the word
                stemmedWord[0] = word[0];
                stemmedWord.Append("\u064a");
                stemmedWord.Append(word.Substring(1));
                word = stemmedWord.ToString();
                stemmedWord.Length = 0;

                // root was found, so set variable
                rootFound = true;

                stemmedDocument[wordNumber, 1] = word;
                stemmedDocument[wordNumber, 2] = "ROOT";
                stemmedWordsNumber++;
                listStemmedWords.Add(stemmedDocument[wordNumber, 0]);
                listRootsFound.Add(word);

                return word;
            }
            return word;
        }

        //--------------------------------------------------------------------------

        // remove punctuation from the word
        /*private boolean removePunctuation ( String currentWord, StringBuffer modifiedWord )
        {
            boolean punctuationFound = false;
            modifiedWord.setLength ( 0 );
            Vector punctuations = ( Vector ) staticFiles.elementAt ( 11 );

            // for every character in the current word, if it is a punctuation then do nothing
            // otherwise, copy this character to the modified word
            for ( int i = 0; i < currentWord.length ( ); i++ )
            {
                if ( ! ( punctuations.contains ( currentWord.substring ( i, i+1 ) ) ) )
                {
                    modifiedWord.append ( currentWord.charAt ( i ) );
                    stemmedDocument[wordNumber][2] = null;
                }
                else
                {
                    punctuationFound = true;
                    if ( modifiedWord.length ( ) == 0 && stemmedDocument[wordNumber][2] == null )
                    {
                        stemmedDocument[wordNumber][2] = "PUNCTUATION";
                        punctuationWordNumber ++;
                    }
                }
            }

            return punctuationFound;
        }
        */
        //--------------------------------------------------------------------------

        // remove diacritics from the word
          private bool removeDiacritics ( String currentWord, StringBuilder modifiedWord )
          {
              bool diacriticFound = false;
              modifiedWord.Length = 0;
              string [] diacritics = staticFiles[1].Split(' ');

              for ( int i = 0; i < currentWord.Length; i++ )
                  // if the character is not a diacritic, append it to modified word
                  if ( !diacritics.Contains(currentWord[i].ToString()))
                      modifiedWord.Append ( currentWord[i].ToString());
                  else
                  {
                      diacriticFound = true;
                  }
              return diacriticFound;
          }
         
        //--------------------------------------------------------------------------

        // check that the word is a stopword
          private bool checkStopwords ( String currentWord )
          {
              List <string> v = staticFiles[14].Split(' ').ToList();

              if ( stopwordFound = v.Contains ( currentWord ) )
              {
                  stemmedDocument[wordNumber,1] = currentWord;
                  stemmedDocument[wordNumber,2] = "STOPWORD";
                  stopwordNumber ++;
                  listStopwordsFound.Add( currentWord );
                  listOriginalStopword.Add( stemmedDocument[wordNumber,0] );

              }
              return stopwordFound;
          }
          
        //--------------------------------------------------------------------------

        // check that the word is a strange word
        private bool checkStrangeWords(String currentWord)
        {
            string v = staticFiles[15];

            if (strangeWordFound = v.Contains(currentWord))
            {
                stemmedDocument[wordNumber, 1] = currentWord;
                stemmedDocument[wordNumber, 2] = "STRANGEWORD";
                stopwordNumber++;
                listStopwordsFound.Add(currentWord);
                listOriginalStopword.Add(stemmedDocument[wordNumber, 0]);

            }
            return strangeWordFound;
        }
    }
}

