using System.Text.Json;

namespace Search.Infrastructure.Queries.Common;

internal static class SearchAfterCursorHelper
{
    public static bool TryParseSearchAfterValues(string raw, out List<FieldValue> fieldValues)
    {
        fieldValues = [];

        try
        {
            var values = JsonSerializer.Deserialize<JsonElement[]>(raw);
            if (values is null || values.Length != 2)
            {
                return false;
            }

            foreach (var value in values)
            {
                if (!TryConvertToFieldValue(value, out var fieldValue))
                {
                    return false;
                }

                fieldValues.Add(fieldValue);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string BuildCursor(object sortValue, object tieBreakerId)
    {
        return JsonSerializer.Serialize(new object[] { sortValue, tieBreakerId });
    }

    private static bool TryConvertToFieldValue(JsonElement value, out FieldValue fieldValue)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.String:
            {
                var stringValue = value.GetString();
                if (stringValue is null)
                {
                    fieldValue = FieldValue.Null;
                    return true;
                }

                fieldValue = stringValue;
                return true;
            }
            case JsonValueKind.Number:
                if (value.TryGetInt64(out var longValue))
                {
                    fieldValue = longValue;
                    return true;
                }

                if (value.TryGetDouble(out var doubleValue))
                {
                    fieldValue = doubleValue;
                    return true;
                }

                break;
            case JsonValueKind.True:
                fieldValue = true;
                return true;
            case JsonValueKind.False:
                fieldValue = false;
                return true;
            case JsonValueKind.Null:
                fieldValue = FieldValue.Null;
                return true;
        }

        fieldValue = FieldValue.Null;
        return false;
    }
}