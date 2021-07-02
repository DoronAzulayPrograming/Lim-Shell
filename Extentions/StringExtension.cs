using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace LimOnDotNetCore.Extentions
{
	public static class StringExtension
	{
		[Pure]
		[DebuggerStepThrough]
		public static string CreateLimTag(this string code, string type)
        {
			return $"<lim type=\"{type}\">{code}</lim>";
        }

		public static string RemoveLimTags(this string code)
		{
			int startIndex = -1, endIndex = -1;

            // remove open tags
            while (code.Contains("<lim"))
			{
				startIndex = code.IndexOf("<lim");
				if (startIndex < 0) throw new Exception("missing '<lim' for open tag.!");
				endIndex = code.IndexOf($">", startIndex);
				if (endIndex < 0) throw new Exception("missing '>' for close open tag.!");
				code = code.Replace(code.SmSubstring(startIndex, endIndex),string.Empty);
			}

            // remove close tags
            while (code.Contains("</lim>"))
			{
				startIndex = code.IndexOf("</lim");
				if (startIndex < 0) throw new Exception("missing '</lim' for close tag.!");
				endIndex = code.IndexOf($">", startIndex);
				if (endIndex < 0) throw new Exception("missing '>' for close open tag.!");
				code = code.Replace(code.SmSubstring(startIndex, endIndex), string.Empty);
			}

			return code;
		}

		[Pure]
		[DebuggerStepThrough]
		public static string RemoveTag(this string code, string type = "")
        {
			int startIndex = code.IndexOf($"<lim{(string.IsNullOrWhiteSpace(type) ? string.Empty : $" type=\"{type}\"")}");
			if (startIndex < 0) return code;
			startIndex = code.IndexOf($">");
			if (startIndex < 0) throw new Exception("missing '>' for close open tag.!");
			int endIndex = code.IndexOf("</lim>",startIndex);
			if (endIndex < 0) throw new Exception("closeing tag never found.!");

			return code.SmSubstring(startIndex+1,endIndex-1);
        }
		[Pure]
		[DebuggerStepThrough]
		public static string RemoveComma(this string str, bool deep = false)
        {
            if (deep)
            {
				while (str.Contains(";")) str = str.Replace(";", string.Empty);
				return str;
			}

			return str.Replace(";", string.Empty);
        }
		[Pure]
		[DebuggerStepThrough]
		public static bool IsNumber(this string str)
        {
			string ops = "+-";
			string digits = "0123456789";//+1 123 -1
			int index = 0;
			char toCheck = '\0';
			while (ops.Contains(str[index]) && index < str.Length) toCheck = str[index++];

			return digits.Contains(toCheck);
        }
		[Pure]
		[DebuggerStepThrough]
		public static string SmEndianReverse(this string s)
		{
			char t;
			char[] charArray = s.ToCharArray();
			for (int i = 0; i < charArray.Length - 1; i += 2)
			{
				t = charArray[i];
				charArray[i] = charArray[i + 1];
				charArray[i + 1] = t;
			}
			return new string(charArray);
		}
		[Pure]
		[DebuggerStepThrough]
		public static string SmReverse(this string s)
		{
			char[] charArray = s.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}
		[Pure]
		[DebuggerStepThrough]
		public static bool SmEqualsToOperator(this string text)
		{
			if (text == null) return false;
			if (text.Length <= 0) return false;

			switch (text[0])
			{
				case '+': return true;
				case '-': return true;
				case '*': return true;
				case '/': return true;
				default: return false;
			}
		}
		[Pure]
		[DebuggerStepThrough]
		public static bool SmEqualsToOperator(this string text, char letter)
		{
			if (text == null) return false;
			if (text.Length <= 0) return false;

			bool status = false;
			int i;
			for (i = 0; i < text.Length; i++) if (text[i] == letter) { status = true; break; }

			return status;
		}
		[Pure]
		[DebuggerStepThrough]
		public static string SmSubstring(this string text, int start)
		{
			if (text.Length <= 0) return string.Empty;
			if (start > text.Length - 1) return string.Empty;

			string str = string.Empty;

			for (int i = start; i < text.Length; i++) str += text[i];

			return str;
		}
		[Pure]
		[DebuggerStepThrough]
		public static string SmSubstring(this string text, int start, int end)
		{
			if (text.Length <= 0) return string.Empty;
			if (start > text.Length - 1) return string.Empty;
			if (end > text.Length - 1) return string.Empty;

			int len = text.Length - end;
			string str = string.Empty;

			for (int i = start; i <= end; i++) str += text[i];

			return str;
		}

	}
}
