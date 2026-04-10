using System.Text.Json;
using Lesson.Domain.Entities.Events;

namespace Lesson.Domain.Entities;

public enum PhraseType
{
    Word,
    Phrase,
    Idiom,
    Sentence
}

public class FlashcardAggregate : BaseAggregateRoot
{

    public Guid FlashcardId {get; private set; }
    public Guid CourseId { get; private set; }
    public Guid TopicId { get; private set; }
    public string FrontTextChinese { get; private set; } = null!;
    public string Pinyin { get; private set; } = null!;             // pinyin
    public string MeaningVi { get; private set; } = null!;          // meaning_vi
    public string? MeaningEn { get; private set; } = null;          // meaning_en (optional)
    public PhraseType PhraseType { get; private set; } = PhraseType.Word; // e.g. "word", "phrase", "idiom", "sentence"   
    public int OrderIndex { get; private set; }
    public string AudioUrl { get; private set; } = string.Empty; // optional, for pronunciation
    public int? HskLevel { get; private set; } = null; // 1-9 theo chuẩn HSK. Null nếu không thuộc HSK
    public bool IsHskCore { get; private set; } = false; // true nếu là từ vựng HSK core, false nếu là từ phụ hoặc không thuộc HSK
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string? ExampleSentenceChinese { get; private set; } = null;
    public string? ExampleSentencePinyin { get; private set; } = null;
    public string? ExampleSentenceMeaningVi { get; private set; } = null;
    
    // field for word
    public string? Radical { get; private set; } = null; // bộ thủ
    public int? StrokeCount { get; private set; } = null; // số nét
    public string? TraditionalForm { get; private set; } = null; // Hán tự truyền thống (nếu có)
    public JsonDocument? StrokeOrderJson { get; private set; } = null; // SVG path data cho Hanzi Writer

    // Factory method to create a new flashcard
    public static FlashcardAggregate Create(
        Guid courseId, 
        Guid topicId,
        string frontTextChinese, 
        string pinyin, 
        string meaningVi, 
        PhraseType phraseType, 
        int orderIndex,  
        string audioUrl = "",
        string? meaningEn = null, 
        int? hskLevel = null, 
        bool isHskCore = false,
        string? exampleSentenceChinese = null,
        string? exampleSentencePinyin = null,
        string? exampleSentenceMeaningVi = null,
        string? radical = null,
        int? strokeCount = null,
        string? traditionalForm = null,
        JsonDocument? strokeOrderJson = null
        ) 
    {
        if (courseId == Guid.Empty)
            throw new ArgumentException("CourseId cannot be empty.", nameof(courseId));
        if (topicId == Guid.Empty)
            throw new ArgumentException("TopicId cannot be empty.", nameof(topicId));
        if (string.IsNullOrWhiteSpace(frontTextChinese))
            throw new ArgumentException("FrontTextChinese cannot be empty.", nameof(frontTextChinese));
        if (string.IsNullOrWhiteSpace(pinyin))
            throw new ArgumentException("Pinyin cannot be empty.", nameof(pinyin));
        if (string.IsNullOrWhiteSpace(meaningVi))
            throw new ArgumentException("MeaningVi cannot be empty.", nameof(meaningVi));
        if (orderIndex < 0)
            throw new ArgumentException("OrderIndex cannot be negative.", nameof(orderIndex));

        var now = DateTime.UtcNow;
        var flashcard = new FlashcardAggregate
        {
            FlashcardId = Guid.CreateVersion7(),
            CourseId = courseId,
            TopicId = topicId,
            FrontTextChinese = frontTextChinese,
            Pinyin = pinyin,
            MeaningVi = meaningVi,
            MeaningEn = meaningEn,
            PhraseType = phraseType,
            OrderIndex = orderIndex,
            AudioUrl = audioUrl,
            HskLevel = hskLevel,
            IsHskCore = isHskCore,
            CreatedAt = now,
            UpdatedAt = now,
            ExampleSentenceChinese = exampleSentenceChinese,
            ExampleSentencePinyin = exampleSentencePinyin,
            ExampleSentenceMeaningVi = exampleSentenceMeaningVi,
            Radical = radical,
            StrokeCount = strokeCount,
            TraditionalForm = traditionalForm,
            StrokeOrderJson = strokeOrderJson
        };

        // Fire domain event
        flashcard.AddDomainEvent(new FlashcardCreatedEvent(
            flashcard.FlashcardId,
            flashcard.CourseId,
            flashcard.TopicId,
            flashcard.FrontTextChinese,
            flashcard.Pinyin,
            flashcard.MeaningVi,
            flashcard.MeaningEn,
            flashcard.PhraseType.ToString(),
            flashcard.OrderIndex,
            flashcard.AudioUrl,
            flashcard.HskLevel,
            flashcard.IsHskCore,
            flashcard.ExampleSentenceChinese,
            flashcard.ExampleSentencePinyin,
            flashcard.ExampleSentenceMeaningVi,
            flashcard.Radical,
            flashcard.StrokeCount,
            flashcard.TraditionalForm,
            flashcard.StrokeOrderJson?.ToString(),
            flashcard.CreatedAt,
            flashcard.UpdatedAt
        ));

        return flashcard;
    }
    public void UpdateText(string newFrontTextChinese, string newPinyin, string newMeaningVi)
    {
        if (string.IsNullOrWhiteSpace(newFrontTextChinese))
            throw new ArgumentException("FrontTextChinese cannot be empty.", nameof(newFrontTextChinese));
        if (string.IsNullOrWhiteSpace(newPinyin))
            throw new ArgumentException("Pinyin cannot be empty.", nameof(newPinyin));
        if (string.IsNullOrWhiteSpace(newMeaningVi))
            throw new ArgumentException("MeaningVi cannot be empty.", nameof(newMeaningVi));
        
        FrontTextChinese = newFrontTextChinese;
        Pinyin = newPinyin;
        MeaningVi = newMeaningVi;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardTextUpdatedEvent(FlashcardId, newFrontTextChinese, newPinyin, newMeaningVi, UpdatedAt));
    }

    public void UpdateEnglishMeaning(string newMeaningEn)
    {
        if (string.IsNullOrWhiteSpace(newMeaningEn))
            throw new ArgumentException("English meaning cannot be empty.", nameof(newMeaningEn));
        
        MeaningEn = newMeaningEn;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardEnglishMeaningUpdatedEvent(FlashcardId, newMeaningEn, UpdatedAt));
    }

    public void UpdatePhraseType(PhraseType newPhraseType)
    {
        PhraseType = newPhraseType;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardPhraseTypeUpdatedEvent(FlashcardId, newPhraseType.ToString(), UpdatedAt));
    }

    public void UpdateOrderIndex(int newOrderIndex)
    {
        if (newOrderIndex < 0)
            throw new ArgumentException("OrderIndex cannot be negative.", nameof(newOrderIndex));
        
        OrderIndex = newOrderIndex;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardOrderIndexUpdatedEvent(FlashcardId, newOrderIndex, UpdatedAt));
    }

    public void UpdateAudioUrl(string newAudioUrl)
    {
        if (string.IsNullOrWhiteSpace(newAudioUrl))
            throw new ArgumentException("AudioUrl cannot be empty.", nameof(newAudioUrl));
        AudioUrl = newAudioUrl;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardAudioUrlUpdatedEvent(FlashcardId, newAudioUrl, UpdatedAt));
    }

    public void UpdateHskInfo(int newHskLevel, bool newIsHskCore)
    {
        if (newHskLevel <= 0)
            throw new ArgumentOutOfRangeException(nameof(newHskLevel), "HskLevel must be a positive integer.");
        HskLevel = newHskLevel;
        IsHskCore = newIsHskCore;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardHskInfoUpdatedEvent(FlashcardId, newHskLevel, newIsHskCore, UpdatedAt));
    }

    public void UpdateExampleSentence(string? newChineseSentence, string? newPinyinSentence, string? newMeaningViSentence)
    {
        ExampleSentenceChinese = newChineseSentence;
        ExampleSentencePinyin = newPinyinSentence;
        ExampleSentenceMeaningVi = newMeaningViSentence;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardExampleSentenceUpdatedEvent(FlashcardId, newChineseSentence, newPinyinSentence, newMeaningViSentence, UpdatedAt));
    }

    public void UpdateCharacterInfo(string? newRadical, int? newStrokeCount, string? newTraditionalForm, JsonDocument? newStrokeOrderJson)
    {
        Radical = newRadical;
        StrokeCount = newStrokeCount;
        TraditionalForm = newTraditionalForm;
        StrokeOrderJson = newStrokeOrderJson;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new FlashcardCharacterInfoUpdatedEvent(FlashcardId, newRadical, newStrokeCount, newTraditionalForm, newStrokeOrderJson?.ToString(), UpdatedAt));
    }

    /// <summary>
    /// Delete flashcard and fire domain event
    /// </summary>
    public void Delete()
    {
        var deletedAt = DateTime.UtcNow;

        AddDomainEvent(new FlashcardDeletedEvent(
            FlashcardId,
            CourseId,
            TopicId,
            deletedAt
        ));
    }
}