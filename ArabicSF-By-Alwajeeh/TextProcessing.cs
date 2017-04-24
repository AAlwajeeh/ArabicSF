using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;
namespace ArabicSF
{
    public class TextProcessing
    {
        private string[] Letters = {"أ","إ","ئ","ى","ء","ؤ","آ","ا", "ب", "ت", "ث", "ج", "ح", "خ", "د", "ذ", "ر", "ز", "س", "ش", "ص", "ض", "ط", "ظ", "ع", "غ", "ف", "ق", "ك", "ل", "م", "ن", "ه","ة", "و", "ي" };
        private int[] SpecialCharacters = {'ّ','ْ','ٌ','ُ','ٍ','ً','ِ','َ','ـ','0','1','2','3','4','5','6','7','8','9', ' ','»', '«','$', '%', '*', '(', ')', '{', '}', '[', ']','^', '\\', '|', '~', '\'', '’', '،', ',', '.', ':', '؛', '؟', '!','=',};
        public string[] removeNonArabicLetters(string [] txt)
        {

            string []ArabicText = new string[txt.Length];
            ArabicText.Initialize();
            int i = 0;
            foreach (string s in txt)
            {
                foreach (char c in s.ToCharArray())
                {
                    
                    
                    if ((SpecialCharacters.Contains(c) || Letters.Contains(c.ToString())))
                        ArabicText[i] += c;
                    

                }
                i++;
                
            }
            return ArabicText;
      }
            

    }
}
