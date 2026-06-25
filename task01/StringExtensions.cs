using System;

public static class StringExtensions
{
    public static bool IsPalindrome(this string input)
    {
        if(string.IsNullOrEmpty(input)){
            return false;
        }
        string low=input.ToLower();
        string clean="";
        foreach(char c in low){
            if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c)){
                clean+=c;
            }
        }
        if(string.IsNullOrEmpty(input)){
            return false;
        }
        string revers="";
        for (int i=clean.Length-1;i>=0;i--){
            revers+=clean[i];
        }
        return clean==revers;

    }

}