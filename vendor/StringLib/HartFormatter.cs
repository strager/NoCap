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

        public static string HartFormat<T>(this string format, T dictionary, FormatterOptions options)
        {
            return new Formatter<T>(format, options).Format(dictionary);
        }

        public static Formatter<T> CompileHartFormatter<T>(this T exemplar, string format)
        {
            return new Formatter<T>(format);
        }

        public static Formatter<T> CompileHartFormatter<T>(this T exemplar, string format, FormatterOptions options)
        {
            return new Formatter<T>(format, options);
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


        public class FormatterOptions
        {
            public string ErrorReplacement { get; set; }
            public bool IgnoreCase { get; set; }

            private IFormatProvider formatProvider;

            public IFormatProvider FormatProvider {
                get
                {
                    if (this.formatProvider == null)
                    {
                        return System.Threading.Thread.CurrentThread.CurrentCulture;
                    }

                    return this.formatProvider;
                }

                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }

                    this.formatProvider = value;
                }
            }

            public static FormatterOptions DefaultOptions
            {
                get
                {
                    return StrictOptions;
                }
            }

            public static FormatterOptions StrictOptions
            {
                get
                {
                    return new FormatterOptions
                    {
                        ErrorReplacement = null,
                        IgnoreCase = false,
                    };
                }
            }

            public static FormatterOptions HumaneOptions
            {
                get
                {
                    return new FormatterOptions
                    {
                        IgnoreCase = true,
                        ErrorReplacement = "(null)",
                    };
                }
            }
        }

        public class Formatter<T>
        {
            // statically, for the type T, generate a dictionary of name -> formatting lambdas
            // This will happen once per type, so multiple format operations on the same type will benefit.
            static Formatter()
            {
                _parameters = new Dictionary<string, Func<T, string, IFormatProvider, string>>();
                foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead))
                {
                    Expression<Func<T, string, IFormatProvider, string>> formatLambda = GetPropertyLambda(prop);
                    _parameters.Add(prop.Name, formatLambda.Compile());
                }
            }

            private static Expression<Func<T, string, IFormatProvider, string>> GetPropertyLambda(PropertyInfo property)
            {
                ParameterExpression t = Expression.Parameter(property.DeclaringType, "t" + property.Name);
                ParameterExpression fmt = Expression.Parameter(typeof(string), "fmt" + property.Name);
                ParameterExpression fmtr = Expression.Parameter(typeof(IFormatProvider), "fmtr" + property.Name);
                Expression<Func<object, string, IFormatProvider, string>> toStringMethod;

                toStringMethod = typeof(IFormattable).IsAssignableFrom(property.PropertyType) ? _iFormattableToString : _objToString;

                return Expression.Lambda<Func<T, string, IFormatProvider, string>>(
                    Expression.Invoke(
                        toStringMethod, 
                        Expression.Convert(Expression.Property(t, property), typeof(object)), 
                        fmt, 
                        fmtr
                    ), 
                    t, 
                    fmt, 
                    fmtr
                );
            }

            private static Dictionary<string, Func<T, string, IFormatProvider, string>> _parameters;

            private IEnumerable<Func<T, IFormatProvider, string>> _generators;

            private FormatterOptions _options;

            public Formatter(string format) : this(format, null)
            {
            }


            public Formatter(string format, FormatterOptions options)
            {
                if (format == null) throw new ArgumentNullException("format");

                options = options ?? FormatterOptions.DefaultOptions;

                _generators = GetTokenGenerators(format, options).ToList();

                // note, GetTokenGenerators() is enumerated now, at construction time, 
                // allowing same format string to be reused without further regex hits

                _options = options;
            }


            public string Format(T dictionary)
            {
                if (dictionary == null) throw new ArgumentNullException("dictionary");
                return _generators.Aggregate(new StringBuilder(), (builder, val) => builder.Append(val(dictionary, _options.FormatProvider)), builder => builder.ToString());
            }

            // parses the format and returns a set of functions that output part of the format result
            private static IEnumerable<Func<T, IFormatProvider, string>> GetTokenGenerators(string format, FormatterOptions options)
            {
                StringComparison comparison = options.IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;

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

                        if (!_parameters.TryGetValue(name, out formatLambda))
                        {
                            formatLambda = _parameters
                                .Where((kvp) => string.Compare(kvp.Key, name, comparison) == 0)
                                .Select((kvp) => kvp.Value)
                                .FirstOrDefault();
                        }

                        string specifiedFormat = null;
                        if (match.Groups["fmt"].Success) specifiedFormat = match.Groups["fmt"].Value;

                        if (formatLambda == null)
                        {
                            // Look for uncached properties (e.g. exposed by a subclass)
                            yield return (t, fmtr) =>
                            {
                                if (t == null) throw new ArgumentNullException("t");

                                PropertyInfo property = t.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | (options.IgnoreCase ? BindingFlags.IgnoreCase : 0));

                                if (property == null)
                                {
                                    if (options.ErrorReplacement == null) throw new FormatException("No such value in dictionary type: " + name + " in format expression at " + match.Groups["name"].Index);

                                    return options.ErrorReplacement;
                                }

                                Expression<Func<object, string, IFormatProvider, string>> toStringLambda = typeof(IFormattable).IsAssignableFrom(property.PropertyType) ? _iFormattableToString : _objToString;

                                return toStringLambda.Compile()(property.GetValue(t, new object[] { }), specifiedFormat, fmtr);
                            };
                        }
                        else if (match.Groups["propertyChain"].Success)
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
