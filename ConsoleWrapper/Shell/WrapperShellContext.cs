using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleWrapper
{
	class WrapperShellContext
	{
		private DirectoryInfo _currentDirectory;
		public DirectoryInfo CurrentDirectory
		{
			get { return _currentDirectory; }
			set { _currentDirectory = value; }
		}

		public WrapperShellContext(DirectoryInfo directory)
		{
			_currentDirectory = directory;
		}

		/// <summary>
		/// Parses an input line into a string array, taking into account
		/// double quotes surrounding parameters with spaces. Quotes are
		/// removed automatically.
		/// </summary>
		/// <param name="line">The input line to process</param>
		/// <returns>A list of arguments</returns>
		public static List<string> GetArguments(string line)
		{
			List<string> arguments = new List<string>();

			string[] args = line.Split(new string[] { " ", "\n", "\n\r", Environment.NewLine }, StringSplitOptions.None);
			StringBuilder parameter = new StringBuilder();
			bool building = false;

			foreach (string arg in args)
			{
				if (building)
				{
					if (arg.EndsWith("\""))
					{
						parameter.Append(" ");
						parameter.Append(arg.Substring(0, arg.Length - 1));
						arguments.Add(parameter.ToString());
						building = false;
					}
					else
					{
						parameter.Append(" ");
						parameter.Append(arg);
					}
				}
				else if (arg.StartsWith("\""))
				{
					if (arg.EndsWith("\""))
					{
						arguments.Add(arg.Substring(1, arg.Length - 2));
					}
					else
					{
						parameter = new StringBuilder(arg.Substring(1));
						building = true;
					}
				}
				else
				{
					arguments.Add(arg);
				}
			}

			return arguments;
		}

		/// <summary>
		/// Parses an input line into a dictionary of param/value
		/// pairs.
		/// </summary>
		/// <param name="line"></param>
		/// <returns></returns>
		public static Dictionary<string, string> GetParameters(string line)
		{
			Dictionary<string, string> parameters = new Dictionary<string, string>();

			Regex splitter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Regex remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			string parameter = null;
			string[] parts;
			string[] args = line.Split(new char[] { ' ' });

			// Valid parameters forms:
			// {-,/,--}param{ ,=,:}((",')value(",'))
			// Examples: 
			// -param1 value1 --param2 /param3:"Test-:-work" 
			//   /param4=happy -param5 '--=nice=--'
			foreach (string arg in args)
			{
				// Look for new parameters (-,/ or --) and a
				// possible enclosed value (=,:)
				parts = splitter.Split(arg, 3);

				switch (parts.Length)
				{

					// Found a value (for the last parameter 
					// found (space separator))
					case 1:
						if (parameter != null)
						{
							if (!parameters.ContainsKey(parameter))
							{
								parts[0] =
									remover.Replace(parts[0], "$1");

								parameters.Add(parameter, parts[0]);
							}
							parameter = null;
						}
						// else Error: no parameter waiting for a value (skipped)
						break;

					// Found just a parameter
					case 2:
						// The last parameter is still waiting. 
						// With no value, set it to true.
						if (parameter != null)
						{
							if (!parameters.ContainsKey(parameter))
								parameters.Add(parameter, "true");
						}
						parameter = parts[1];
						break;

					// Parameter with enclosed value
					case 3:
						// The last parameter is still waiting. 
						// With no value, set it to true.
						if (parameter != null)
						{
							if (!parameters.ContainsKey(parameter))
								parameters.Add(parameter, "true");
						}

						parameter = parts[1];

						// Remove possible enclosing characters (",')
						if (!parameters.ContainsKey(parameter))
						{
							parts[2] = remover.Replace(parts[2], "$1");
							parameters.Add(parameter, parts[2]);
						}

						parameter = null;
						break;
				}
			}

			// In case a parameter is still waiting
			if (parameter != null)
			{
				if (!parameters.ContainsKey(parameter))
					parameters.Add(parameter, "true");
			}

			return parameters;
		}
	}
}
