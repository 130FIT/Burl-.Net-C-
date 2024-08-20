using System.Text.Json;

namespace Helpers
{
    public static class AssertHelper
    {
        public static (string, string) AssertActualTool(string key)
        {
            string assertKey;
            string mode;

            if (key.StartsWith("(count)"))
            {
                mode = "(count)";
            }
            else if (key.StartsWith("(<)"))
            {
                mode = "(<)";
            }
            else if (key.StartsWith("(>)"))
            {
                mode = "(>)";
            }
            else if (key.StartsWith("(<=)"))
            {
                mode = "(<=)";
            }
            else if (key.StartsWith("(>=)"))
            {
                mode = "(>=)";
            }
            else if (key.StartsWith("(!=)"))
            {
                mode = "(!=)";
            }
            else if (key.StartsWith("(==)"))
            {
                mode = "(==)";
            }
            else if (key.StartsWith("(contains)"))
            {
                mode = "(contains)";
            }
            else if (key.StartsWith("(notcontains)"))
            {
                mode = "(notcontains)";
            }
            else
            {
                mode = "(==)";
            }

            assertKey = key.Replace(mode, "").Trim();

            if (mode == "(count)")
            {
                var (newKey, nextMode) = AssertActualTool(assertKey);
                assertKey = newKey;
                mode = mode + "|" + nextMode;
            }

            return (assertKey, mode);
        }
        public static bool Assert(object actualValue, object expectedValue, string mode)
        {
            if (expectedValue?.GetType() == typeof(JsonElement))
            {
                expectedValue = JsonHelper.ConvertToActualType((JsonElement)expectedValue) ?? expectedValue;
            }
            if (actualValue?.GetType() == typeof(JsonElement))
            {
                actualValue = JsonHelper.ConvertToActualType((JsonElement)actualValue) ?? actualValue;
            }

            Console.WriteLine($"\t Expected: ({expectedValue?.GetType()}) {expectedValue} \n\t Actual: ({actualValue?.GetType()}) {actualValue} \n\t Compare with {mode}");

            switch (mode)
            {
                case "(<)":
                case "(>)":
                case "(<=)":
                case "(>=)":
                    return Compare(actualValue!, expectedValue!, mode);

                case "(!=)":
                    return !actualValue!.Equals(expectedValue);

                case "(==)":
                    return actualValue!.Equals(expectedValue);

                case "(contains)":
                    if (actualValue is string actualStr && expectedValue is string expectedStr)
                    {
                        return actualStr.Contains(expectedStr);
                    }
                    throw new ArgumentException("Both actualValue and expectedValue must be strings for '(contains)' mode.");

                case "(notcontains)":
                    if (actualValue is string actualStrNot && expectedValue is string expectedStrNot)
                    {
                        return !actualStrNot.Contains(expectedStrNot);
                    }
                    throw new ArgumentException("Both actualValue and expectedValue must be strings for '(notcontains)' mode.");

                default:
                    return actualValue!.Equals(expectedValue);
            }
        }

        private static bool Compare(object actualValue, object expectedValue, string mode)
        {
            if (actualValue is IComparable actualComparable && expectedValue is IComparable expectedComparable)
            {
                int comparisonResult = actualComparable.CompareTo(expectedComparable);

                return mode switch
                {
                    "(<)" => comparisonResult < 0,
                    "(>)" => comparisonResult > 0,
                    "(<=)" => comparisonResult <= 0,
                    "(>=)" => comparisonResult >= 0,
                    _ => throw new ArgumentException($"Invalid comparison mode: {mode}")
                };
            }
            throw new ArgumentException("Both actualValue and expectedValue must implement IComparable for comparison.");
        }

    }

}