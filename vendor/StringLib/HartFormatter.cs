// Source: http://blogs.ipona.com/james/archive/2009/01/16/8557.aspx
// Unlicenced / public domain
// Modified by Strager Neds 2010

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Linq.Expressions;
using System.Diagnostics;

namespace StringLib
{
    public static class HartFormatter
    {
        public static string HartFormat<T>(this string format, T dictionary)
        {
            return new Formatter<T>(format).Format(dictionary);
        }

        public static Formatter<T> CompileHartFormatter<T>(this T exemplar, string format)
        {
            return new Formatter<T>(format);
        }

        // these expressions are captured as static fields, shared across all types and instances of Formatter<T>
        static readonly Expression<Func<object, string, IFormatProvider, string>> _objToString = (obj, ignoredFormat, ignoredFormatter) => (obj??"").ToString();
        static readonly Expression<Func<object, string, IFormatProvider, string>> _iFormattableToString = (iformattable, format, formatter) => iformattable == null ? "" : ((IFormattable)iformattable).ToString(format, formatter);


        static readonly Regex _re = new Regex(@"

    (?<escapedOpenBrace>      {{       )
|   (?<escapedCloseBrace>     }}       )
|                             {
                                  (?<name>[A-Za-z_][A-Za-z0-9_]*)
                                  (?<propertyChain>\.(([A-Za-z_][A-Za-z0-9_]*\.)*[A-Za-z_][A-Za-z0-9_]*))?
                                  (:(?<fmt>[^}]+))?
                              }
|   (?<mismatchedBrace>       {     )

", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace);


        public class Formatter<T>
        {
            // statically, for the type T, generate a dictionary of name -> formatting lambdas
            // This will happen once per type, so multiple format operations on the same type will benefit.
            static Formatter()
            {
                _parameters = new Dictionary<string, Func<T, string, IFormatProvider, string>>();
                foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead))
                {
                    ParameterExpression t = Expression.Parameter(typeof(T), "t" + prop.Name);
                    ParameterExpression fmt = Expression.Parameter(typeof(string), "fmt" + prop.Name);
                    ParameterExpression fmtr = Expression.Parameter(typeof(IFormatProvider), "fmtr" + prop.Name);
                    Expression<Func<object, string, IFormatProvider, string>> toStringMethod;

                    toStringMethod = typeof(IFormattable).IsAssignableFrom(prop.PropertyType) ? _iFormattableToString : _objToString;

                    Expression<Func<T, string, IFormatProvider, string>> formatLambda = Expression.Lambda<Func<T, string, IFormatProvider, string>>(
                        Expression.Invoke(
                            toStringMethod, 
                                Expression.Convert(Expression.Property(t, prop), typeof(object)), 
                                fmt, 
                                fmtr
                            ), 
                            t, 
                            fmt, 
                            fmtr
                        );
                    _parameters.Add(prop.Name, formatLambda.Compile());
                }
            }

            private static Dictionary<string, Func<T, string, IFormatProvider, string>> _parameters;

            private IEnumerable<Func<T, IFormatProvider, string>> _generators;

            public Formatter(string format)
            {
                if (format == null) throw new ArgumentNullException("format");
                _generators = GetTokenGenerators(format).ToList(); // note, GetTokenGenerators() is enumerated now, at construction time, 
                                                                   // allowing same format string to be reused without further regex hits
            }


            public string Format(T dictionary, IFormatProvider formatProvider)
            {
                if (dictionary == null) throw new ArgumentNullException("dictionary");
                return _generators.Aggregate(new StringBuilder(), (builder, val) => builder.Append(val(dictionary, formatProvider)), builder => builder.ToString());
            }

            public string Format(T dictionary)
            {
                return Format(dictionary, System.Threading.Thread.CurrentThread.CurrentCulture);
            }

            // parses the format and returns a set of functions that output part of the format result
            private static IEnumerable<Func<T, IFormatProvider, string>> GetTokenGenerators(string format)
            {
                int index = 0;

                foreach (Match match in _re.Matches(format))
                {
                    if (match.Index > index) // don't bother returning for an empty string
                    {
                        string previousText = format.Substring(index, match.Index - index);
                        yield return (t, fmtr) => previousText;
                    }

                    if (match.Groups["mismatchedBrace"].Success) throw new FormatException("Mismatched brace in format expression at " + match.Index);
                    else if (match.Groups["escapedOpenBrace"].Success) yield return OpenBrace;
                    else if (match.Groups["escapedCloseBrace"].Success) yield return CloseBrace;
                    else
                    {
                        Debug.Assert(match.Groups["name"].Success);

                        string name = match.Groups["name"].Value;

                        Func<T, string, IFormatProvider, string> formatLambda;
                        if (!_parameters.TryGetValue(name, out formatLambda)) throw new FormatException("No such value in dictionary type: " + name + " in format expression at " + match.Groups["name"].Index);

                        string specifiedFormat = null;
                        if (match.Groups["fmt"].Success) specifiedFormat = match.Groups["fmt"].Value;

                        if (match.Groups["propertyChain"].Success)
                        {
                            // take a slower path
                            // yield return (t, fmtr) => System.Web.UI.DataBinder.Eval(t, name + match.Groups["propertyChain"].Value, "{0:" + specifiedFormat + "}");
                            throw new NotSupportedException("Property chains not supported");
                        }
                        else
                        {
                            yield return (t, fmtr) => formatLambda(t, specifiedFormat, fmtr);
                        }
                    }


                    index = match.Index + match.Length;
                }

                if (format.Length > index)
                {
                    string previoustext = format.Substring(index, format.Length - index);
                    yield return (t, fmtr) => previoustext;
                }

            }


            // aggressively static implementations for returning these constant strings
            private static string OpenBrace(T t, IFormatProvider fmtr)
            {
                return "{";
            }

            private static string CloseBrace(T t, IFormatProvider fmtr)
            {
                return "}";
            }





        }
    }
}
