using System.Text.Json.Serialization;

namespace HanziAnhVu.Shared.Domain;
// Value Object cho option
// ExerciseOption sẽ được lưu trực tiếp trong ExerciseAggregate dưới dạng JSON, 
// cái option này có tác dụng là để lưu trữ các lựa chọn cho bài tập dạng multiple choice, 
// hoặc các câu trả lời đúng/sai cho bài tập dạng true/false, v.v.
//  ReadSentenceOrder
//  ReadMatch (Matching pairs)
//  ListenSentenceJudge
//  ListenDialogueChoice

/// <summary>
/// Value Object for Exercise Options
/// Used for multiple choice, matching, ordering exercises, etc.
/// IsCorrect is NOT stored here - it's derived from Aggregate's CorrectAnswer field
/// </summary>
public class ExerciseOption : ValueObject
{
    public string Id { get; private set; } = string.Empty; // Id để phân biệt các option, có thể là "A", "B", "C", hoặc "1", "2", v.v. tùy theo loại bài tập
    public string Text { get; private set; } = string.Empty; // Text của option, có thể là câu trả lời, hoặc nội dung của lựa chọn, v.v.
    
    public ExerciseOption() { }

    [JsonConstructor]
    public ExerciseOption(string id, string text)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Option Id cannot be empty", nameof(id));
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Option Text cannot be empty", nameof(text));
        
        Id = id;
        Text = text;
    }
    
    // Equality based on Id and Text
    // This allows us to compare options and ensure correct answer exists in options list
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Id;
        yield return Text;
    }
}
