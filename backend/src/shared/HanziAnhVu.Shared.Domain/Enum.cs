namespace HanziAnhVu.Shared.Domain;
public enum ExerciseSessionStatus
{
    NotStarted,
    InProgress,
    Completed,
    Abandoned 
}

public enum ExerciseType
{
    ListenDialogueChoice, // Input: Audio + Multiple Choice Question
    ListenImageChoice, // Input: Audio + Image Choice Question
    ListenFillBlank, // Input: Audio + Fill in the Blank
    ListenSentenceJudge, // Input: Audio + True/False Question
    ReadFillBlank, // Input: Text + Fill in the Blank
    ReadComprehension, // Input: Text + Multiple Choice Question, Read passage, answer questions
    ReadSentenceOrder, // Input: Text + Sentence Ordering Question, Order sentences correctly
    ReadMatch, // Input: Text + Matching Question, Match phrases or definitions
    WriteHanzi, // Input: Text + Hanzi Writing, Write Chinese character (stroke order)
    WritePinyin, // Input: Text + Pinyin Writing,  Write pinyin for character
    WriteSentence, // Input: Text + Sentence Writing,Write complete sentence (AI feedback)
}
public enum SkillType
{
    Reading,
    Writing,
    Listening,
    Speaking
}

public enum ExerciseDifficulty
{
    Easy,
    Medium,
    Hard
}

public enum ExerciseContext
{
    Learning,   // Dành cho topic-based learning (self-paced)
    Classroom,  // Dành cho assignment giao bài trong lớp
    Mixed       // Có thể dùng cho cả hai
}
