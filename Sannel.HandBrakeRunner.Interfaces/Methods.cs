using log4net;
using Sannel.HandBrakeRunner.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sannel.HandBrakeRunner.Interfaces
{
	public static class Methods
	{
		private static ILog log = LogManager.GetLogger(typeof(Methods));

		private static Dictionary<String, MethodInfo> methods;

		public static IReadOnlyDictionary<String, MethodInfo> RegisteredMethods
		{
			get
			{
				return methods;
			}
		}

		public static String FindAndExecuteMethod(String name, ValuesBase valuesBase, String param)
		{
			var normilized = ValuesBase.NormalizeKey(name);
			if (methods != null && methods.ContainsKey(normilized))
			{
				List<String> paramS = new List<String>();
				if (!String.IsNullOrEmpty(param))
				{
					String[] splits = param.Split(',');
					foreach (var p in splits)
					{
						paramS.Add(Uri.UnescapeDataString(p));
					}
				}

				MethodInfo mi = methods[normilized];
				var results = mi.Invoke(null, new object[]
				{
					valuesBase,
					paramS.ToArray()
				}) as String;

				return results;
			}

			return null;
		}

		public static Task LoadVariableMethods()
		{
			if (methods != null)
			{
				return Task.Run(() =>
					{

					});
			}

			methods = new Dictionary<string, MethodInfo>();
			AppDomain.CurrentDomain.AssemblyLoad += assemblyLoaded;
			return Task.Run(() =>
				{
					foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
					{
						loadVariableMethodsFromAssembly(assembly);
					}
				});
		}

		private static void assemblyLoaded(object sender, AssemblyLoadEventArgs args)
		{
			loadVariableMethodsFromAssembly(args.LoadedAssembly);
		}

		private static void loadVariableMethodsFromAssembly(Assembly assembly)
		{
			foreach (var type in assembly.GetTypes())
			{
				foreach (var method in type.GetMethods())
				{
					if (method.IsDefined(typeof(VariableMethodAttribute)))
					{
						var attribute = method.GetCustomAttribute<VariableMethodAttribute>();

						if (log.IsDebugEnabled)
						{
							log.DebugFormat("Method {0}.{1} has VariableMethodAttribute on it.", method.ReflectedType, method.Name);
						}

						if (method.ReturnType != typeof(String))
						{
							if (log.IsErrorEnabled)
							{
								log.ErrorFormat("Method {0}.{1} does not return a string ignoring.", method.ReflectedType, method.Name);
							}

							continue;
						}

						if (!method.IsStatic)
						{
							if (log.IsErrorEnabled)
							{
								log.ErrorFormat("Method {0}.{1} has an invalid signature it should be static {1}(ValuesBase, params String[]) ignoring method for now.", method.ReflectedType, method.Name);
							}
							continue;
						}

						var parameters = method.GetParameters();

						if (parameters.Length != 2)
						{
							if (log.IsErrorEnabled)
							{
								log.ErrorFormat("Method {0}.{1} has an invalid signature it should be static {1}(ValuesBase, params String[]) ignoring method for now.", method.ReflectedType, method.Name);
							}
							continue;
						}

						if (!parameters[0].ParameterType.IsAssignableFrom(typeof(ValuesBase)))
						{

							if (log.IsErrorEnabled)
							{
								log.ErrorFormat("Method {0}.{1} has an invalid signature it should be static {1}(ValuesBase, params String[]) ignoring method for now.", method.ReflectedType, method.Name);
							}
							continue;
						}

						if (parameters[1].ParameterType != typeof(String[]))
						{
							if (log.IsErrorEnabled)
							{
								log.ErrorFormat("Method {0}.{1} has an invalid signature it should be static {1}(ValuesBase, params String[]) ignoring method for now.", method.ReflectedType, method.Name);
							}
							continue;
						}

						String normalizedKey = ValuesBase.NormalizeKey(attribute.Name);
						if (String.IsNullOrWhiteSpace(normalizedKey))
						{
							if (log.IsErrorEnabled)
							{
								log.ErrorFormat("VariableMethod.Name is invalid on method {0}.{1} ignoring method for now.", method.ReflectedType, method.Name);
							}
							continue;
						}

						if (methods.ContainsKey(normalizedKey))
						{
							if (log.IsErrorEnabled)
							{
								log.ErrorFormat("The method name {0} has already been added to known methods ignoring method {1}.{2} for now.", attribute.Name, method.ReflectedType, method.Name);
							}
							continue;
						}
						else
						{
							methods[normalizedKey] = method;
							if (log.IsDebugEnabled)
							{
								log.DebugFormat("Adding Method name {0} pointing to actually method {1}.{2}", attribute.Name, method.ReflectedType, method.Name);
							}
						}
					}
				}
			}
		}

		[VariableMethod("FixFileName", Description =
@"Signature %{FixFileName(<KeyName>)}")]
		public static String FixFileName(ValuesBase valuesBase, params String[] arguments)
		{
			if (valuesBase == null)
			{
				throw new ArgumentNullException("track");
			}

			if (arguments == null || arguments.Length == 0)
			{
				if (log.IsErrorEnabled)
				{
					log.Error("No arguments passed to Method FixFileName.");
				}

				return "";
			}

			if (arguments.Length > 1)
			{
				if (log.IsWarnEnabled)
				{
					log.WarnFormat("{0} arguments passed to FixFileName only '{1}' will be used", arguments.Length, arguments[0]);
				}
			}

			String name = arguments[0];
			String value = valuesBase[name];

			if (value == null)
			{
				if (log.IsDebugEnabled)
				{
					log.DebugFormat("No value was returned for key {0}", name);
				}

				return "";
			}

			StringBuilder fixedName = new StringBuilder();

			var invalidChars = Path.GetInvalidFileNameChars();
			char c;
			for (int i = 0; i < value.Length; i++)
			{
				c = value[i];
				if (!invalidChars.Contains(c))
				{
					fixedName.Append(c);
				}
				else
				{
					fixedName.Append('_');
				}
			}

			return fixedName.ToString();
		}

		public static async Task<int> ExecuteProgramAsync(String command, String args, bool redirectOutput)
		{
			if (log.IsDebugEnabled)
			{
				log.DebugFormat("Executing command {0} {1}", command, args);
			}
			ProcessStartInfo psi = new ProcessStartInfo(command, args);

			if (redirectOutput)
			{
				psi.RedirectStandardOutput = true;
				psi.RedirectStandardError = true;
				psi.UseShellExecute = false;
			}

			Process p = Process.Start(psi);

			if (redirectOutput)
			{
				p.BeginOutputReadLine();
				p.BeginErrorReadLine();
				p.OutputDataReceived += (sender, args2) =>
				{
					Console.WriteLine(args2.Data);
				};
				p.ErrorDataReceived += (sender, args2) =>
				{
					Console.Error.WriteLine(args2.Data);
				};
			}

			while (!p.HasExited)
			{
				await Task.Delay(500);
			}

			return p.ExitCode;
		}

		public static String FixString(String value)
		{
			if (!String.IsNullOrWhiteSpace(value))
			{
				value = value.Replace("\"", "\\\"");
			}

			return value;
		}

		/// <summary>
		/// Changes the directory separator.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="newSeperator">The new separator.</param>
		/// <returns></returns>
		public static String ChangeDirectorySeparator(String path, char newSeparator)
		{
			return ChangeDirectorySeparator(path, newSeparator, Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		}

		/// <summary>
		/// Changes the directory separator.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="newSeparator">The new separator.</param>
		/// <param name="existing">The existing.</param>
		/// <returns></returns>
		public static String ChangeDirectorySeparator(String path, char newSeparator, params char[] existing)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}

			int start = 0;
			int index = path.IndexOfAny(existing);

			StringBuilder builder = new StringBuilder();

			while (index > -1)
			{
				builder.Append(path.Substring(start, index - start));
				builder.Append(newSeparator);
				start = index + 1;
				index = path.IndexOfAny(existing, start);
			}

			builder.Append(path.Substring(start));

			return builder.ToString();
		}

	}
}
