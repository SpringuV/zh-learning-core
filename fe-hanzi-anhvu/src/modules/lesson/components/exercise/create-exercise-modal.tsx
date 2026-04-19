"use client";

import { useMemo, useState } from "react";
import {
    ExternalLink,
    FileText,
    Image as ImageIcon,
    Loader2,
    Music2,
    Plus,
    Settings2,
    Trash2,
    Upload,
    X,
} from "lucide-react";
import { toast } from "sonner";
import { mediaUploadApi } from "@/modules/lesson/api/media-upload.api";
import { useCreateExercise } from "@/modules/lesson/hooks/use.exercise.tanstack";
import {
    ExerciseContext,
    ExerciseCreateRequest,
    ExerciseDifficulty,
    ExerciseOption,
    ExerciseType,
    SkillType,
} from "@/modules/lesson/types/exercise.type";
import { Button } from "@/shared/components/ui/button";
import {
    Dialog,
    DialogContent,
    DialogDescription,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import {
    Select,
    SelectContent,
    SelectItem,
    SelectTrigger,
    SelectValue,
} from "@/shared/components/ui/select";
import { Textarea } from "@/shared/components/ui/textarea";
import * as z from "zod";

type CreateExerciseModalProps = {
    topicId: string;
    onCreated?: (exerciseId?: string) => void | Promise<void>;
};

type ExerciseFormState = {
    question: string;
    description: string;
    exerciseType: ExerciseType;
    skillType: SkillType;
    difficulty: ExerciseDifficulty;
    exerciseContext: ExerciseContext;
    correctAnswer: string;
    explanation: string;
    audioUrl: string;
    imageUrl: string;
    options: ExerciseOption[];
};

type ExerciseFormErrors = {
    question?: string;
    description?: string;
    correctAnswer?: string;
    explanation?: string;
    audioUrl?: string;
    imageUrl?: string;
    options?: string;
    form?: string;
};

const exerciseTypeOptions: { value: ExerciseType; label: string }[] = [
    { value: "ListenDialogueChoice", label: "Nghe hội thoại" },
    { value: "ListenImageChoice", label: "Nghe chọn hình" },
    { value: "ListenFillBlank", label: "Nghe điền từ" },
    { value: "ListenSentenceJudge", label: "Nghe đúng/sai" },
    { value: "ReadFillBlank", label: "Đọc điền từ" },
    { value: "ReadComprehension", label: "Đọc hiểu" },
    { value: "ReadSentenceOrder", label: "Sắp xếp câu" },
    { value: "ReadMatch", label: "Đọc nối" },
    { value: "WriteHanzi", label: "Viết Hán tự" },
    { value: "WritePinyin", label: "Viết Pinyin" },
    { value: "WriteSentence", label: "Viết câu" },
];

const skillTypeOptions: { value: SkillType; label: string }[] = [
    { value: "Listening", label: "Nghe" },
    { value: "Reading", label: "Đọc" },
    { value: "Writing", label: "Viết" },
    { value: "Speaking", label: "Nói" },
];

const difficultyOptions: { value: ExerciseDifficulty; label: string }[] = [
    { value: "Easy", label: "Dễ" },
    { value: "Medium", label: "Trung bình" },
    { value: "Hard", label: "Khó" },
];

const contextOptions: { value: ExerciseContext; label: string }[] = [
    { value: "Learning", label: "Learning" },
    { value: "Classroom", label: "Classroom" },
    { value: "Mixed", label: "Mixed" },
];

const optionRequiredTypes: ExerciseType[] = [
    "ListenDialogueChoice",
    "ListenSentenceJudge",
    "ReadComprehension",
    "ReadMatch",
    "ReadFillBlank",
    "ListenFillBlank",
    "ListenImageChoice",
    "ReadSentenceOrder",
];

const makeOptionId = () =>
    `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`;

const createDefaultOptions = (): ExerciseOption[] => [
    { id: makeOptionId(), text: "" },
    { id: makeOptionId(), text: "" },
];

const initialFormState: ExerciseFormState = {
    question: "",
    description: "",
    exerciseType: "ListenDialogueChoice",
    skillType: "Listening",
    difficulty: "Medium",
    exerciseContext: "Learning",
    correctAnswer: "",
    explanation: "",
    audioUrl: "",
    imageUrl: "",
    options: createDefaultOptions(),
};

const exerciseTypeValues = [
    "ListenDialogueChoice",
    "ListenImageChoice",
    "ListenFillBlank",
    "ListenSentenceJudge",
    "ReadFillBlank",
    "ReadComprehension",
    "ReadSentenceOrder",
    "ReadMatch",
    "WriteHanzi",
    "WritePinyin",
    "WriteSentence",
] as const;

const skillTypeValues = [
    "Listening",
    "Reading",
    "Writing",
    "Speaking",
] as const;
const difficultyValues = ["Easy", "Medium", "Hard"] as const;
const contextValues = ["Learning", "Classroom", "Mixed"] as const;

const createExerciseSchema = z
    .object({
        question: z.string().trim().min(1, "Câu hỏi không được để trống."),
        description: z.string().trim().min(1, "Mô tả không được để trống."),
        exerciseType: z.enum(exerciseTypeValues),
        skillType: z.enum(skillTypeValues),
        difficulty: z.enum(difficultyValues),
        exerciseContext: z.enum(contextValues),
        correctAnswer: z
            .string()
            .trim()
            .min(1, "Correct answer không được để trống."),
        explanation: z
            .string()
            .trim()
            .min(1, "Explanation không được để trống."),
        audioUrl: z.string().trim(),
        imageUrl: z.string().trim(),
        options: z.array(
            z.object({
                id: z.string().trim().min(1),
                text: z.string().trim(),
            }),
        ),
    })
    .superRefine((value, ctx) => {
        if (value.audioUrl && !URL.canParse(value.audioUrl)) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "Audio URL không hợp lệ.",
                path: ["audioUrl"],
            });
        }

        if (value.imageUrl && !URL.canParse(value.imageUrl)) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "Image URL không hợp lệ.",
                path: ["imageUrl"],
            });
        }

        const isRequiredType = optionRequiredTypes.includes(value.exerciseType);
        if (!isRequiredType) {
            return;
        }

        const nonEmptyOptions = value.options.filter(
            (option) => option.text.length > 0,
        );
        if (nonEmptyOptions.length < 2) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "Cần ít nhất 2 lựa chọn có nội dung.",
                path: ["options"],
            });
        }

        if (
            !value.options.some((option) => option.id === value.correctAnswer)
        ) {
            ctx.addIssue({
                code: z.ZodIssueCode.custom,
                message: "Đáp án đúng phải là một option hợp lệ.",
                path: ["correctAnswer"],
            });
        }
    });

// Lưu ý: Nếu có thêm các ràng buộc phức tạp khác liên quan đến logic giữa các trường, có thể bổ sung vào phần superRefine này để đảm bảo dữ liệu đầu vào luôn hợp lệ trước khi gửi lên API.
const mapExerciseValidationErrors = (
    issues: z.ZodError["issues"],
): ExerciseFormErrors => {
    const nextErrors: ExerciseFormErrors = {};

    for (const issue of issues) {
        const field = issue.path[0];

        if (field === "question" && !nextErrors.question) {
            nextErrors.question = issue.message;
            continue;
        }

        if (field === "description" && !nextErrors.description) {
            nextErrors.description = issue.message;
            continue;
        }

        if (field === "correctAnswer" && !nextErrors.correctAnswer) {
            nextErrors.correctAnswer = issue.message;
            continue;
        }

        if (field === "explanation" && !nextErrors.explanation) {
            nextErrors.explanation = issue.message;
            continue;
        }

        if (field === "audioUrl" && !nextErrors.audioUrl) {
            nextErrors.audioUrl = issue.message;
            continue;
        }

        if (field === "imageUrl" && !nextErrors.imageUrl) {
            nextErrors.imageUrl = issue.message;
            continue;
        }

        if (field === "options" && !nextErrors.options) {
            nextErrors.options = issue.message;
        }
    }

    return nextErrors;
};

const getErrorMessage = (error: unknown): string => {
    if (typeof error === "object" && error !== null) {
        const maybeApiError = error as {
            response?: {
                data?: {
                    detail?: string;
                    title?: string;
                    errors?: Record<string, string[]>;
                    message?: string;
                };
            };
            message?: string;
        };

        const validationErrors = maybeApiError.response?.data?.errors;
        const firstValidationError = validationErrors
            ? Object.values(validationErrors)[0]?.[0]
            : undefined;

        return (
            firstValidationError ??
            maybeApiError.response?.data?.detail ??
            maybeApiError.response?.data?.message ??
            maybeApiError.response?.data?.title ??
            maybeApiError.message ??
            "Không thể tạo bài tập. Vui lòng thử lại."
        );
    }

    return "Không thể tạo bài tập. Vui lòng thử lại.";
};

export function CreateExerciseModal({
    topicId,
    onCreated,
}: CreateExerciseModalProps) {
    const createExerciseMutation = useCreateExercise();
    const [uploadingField, setUploadingField] = useState<
        "audio" | "image" | null
    >(null);
    const [open, setOpen] = useState(false);
    const [formState, setFormState] =
        useState<ExerciseFormState>(initialFormState);
    const [formErrors, setFormErrors] = useState<ExerciseFormErrors>({});

    const isOptionRequired = optionRequiredTypes.includes(
        formState.exerciseType,
    );

    const payloadPreview = useMemo<ExerciseCreateRequest>(
        () => ({
            topicId,
            question: formState.question.trim(),
            description: formState.description.trim(),
            exerciseType: formState.exerciseType,
            skillType: formState.skillType,
            difficulty: formState.difficulty,
            exerciseContext: formState.exerciseContext,
            correctAnswer: formState.correctAnswer.trim(),
            explanation: formState.explanation.trim(),
            audioUrl: formState.audioUrl.trim() || undefined,
            imageUrl: formState.imageUrl.trim() || undefined,
            options: isOptionRequired
                ? formState.options.map((option) => ({
                      id: option.id,
                      text: option.text.trim(),
                  }))
                : undefined,
        }),
        [formState, topicId, isOptionRequired],
    );

    const resetForm = () => {
        setFormState(initialFormState);
        setFormErrors({});
    };

    const handleExerciseTypeChange = (value: ExerciseType) => {
        const nextRequiresOptions = optionRequiredTypes.includes(value);
        setFormState((current) => ({
            ...current,
            exerciseType: value,
            options:
                nextRequiresOptions && current.options.length < 2
                    ? createDefaultOptions()
                    : nextRequiresOptions
                      ? current.options
                      : [],
            correctAnswer: nextRequiresOptions ? current.correctAnswer : "",
        }));
    };

    const handleAddOption = () => {
        setFormState((current) => ({
            ...current,
            options: [...current.options, { id: makeOptionId(), text: "" }],
        }));
    };

    const handleRemoveOption = (id: string) => {
        setFormState((current) => {
            const nextOptions = current.options.filter(
                (option) => option.id !== id,
            );
            const nextCorrectAnswer =
                current.correctAnswer === id ? "" : current.correctAnswer;

            return {
                ...current,
                options: nextOptions,
                correctAnswer: nextCorrectAnswer,
            };
        });
    };

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        if (!topicId) {
            setFormErrors({ form: "Không tìm thấy topicId từ URL." });
            return;
        }

        const validated = createExerciseSchema.safeParse(formState);
        if (!validated.success) {
            setFormErrors(mapExerciseValidationErrors(validated.error.issues));
            return;
        }

        setFormErrors({});

        const shouldIncludeOptions = optionRequiredTypes.includes(
            validated.data.exerciseType,
        );

        const payload: ExerciseCreateRequest = {
            topicId,
            question: validated.data.question,
            description: validated.data.description,
            exerciseType: validated.data.exerciseType,
            skillType: validated.data.skillType,
            difficulty: validated.data.difficulty,
            exerciseContext: validated.data.exerciseContext,
            correctAnswer: validated.data.correctAnswer,
            explanation: validated.data.explanation,
            audioUrl: validated.data.audioUrl || undefined,
            imageUrl: validated.data.imageUrl || undefined,
            options: shouldIncludeOptions
                ? validated.data.options
                      .filter((option) => option.text.length > 0)
                      .map((option) => ({
                          id: option.id,
                          text: option.text,
                      }))
                : undefined,
        };

        try {
            const created = await createExerciseMutation.mutateAsync(payload);
            const exerciseId =
                (created as { exerciseId?: string })?.exerciseId ??
                (created as { ExerciseId?: string })?.ExerciseId ??
                (created as { Data?: { ExerciseId?: string } })?.Data
                    ?.ExerciseId;

            toast.success(
                `Tạo bài tập thành công${exerciseId ? ` (#${exerciseId})` : ""}.`,
            );

            setOpen(false);
            resetForm();

            if (onCreated) {
                await Promise.resolve(onCreated(exerciseId));
            }
        } catch (error) {
            const errorMessage = getErrorMessage(error);
            setFormErrors({ form: errorMessage });
            toast.error(errorMessage);
        }
    };

    const handleUploadMedia = async (file: File, field: "audio" | "image") => {
        const expectedPrefix = field === "audio" ? "audio/" : "image/";
        const allowByExtension =
            field === "audio"
                ? /\.(mp3|wav|ogg|m4a|aac|flac)$/i.test(file.name)
                : /\.(png|jpe?g|webp|gif|bmp|svg)$/i.test(file.name);
        const isAcceptedType =
            file.type.length === 0
                ? allowByExtension
                : file.type.startsWith(expectedPrefix);

        if (!isAcceptedType) {
            const message =
                field === "audio"
                    ? "Vui lòng chọn file audio hợp lệ."
                    : "Vui lòng chọn file image hợp lệ.";
            toast.error(message);
            return;
        }

        setUploadingField(field);

        // #region Api Upload
        // Lưu ý: mediaUploadApi.uploadToR2 sẽ tự động throw error nếu upload thất bại, nên không cần kiểm tra response ở đây nữa.
        try {
            const result = await mediaUploadApi.uploadToR2(
                file,
                field === "audio" ? "audio" : "images",
            );

            setFormState((current) => ({
                ...current,
                audioUrl:
                    field === "audio" ? result.publicUrl : current.audioUrl,
                imageUrl:
                    field === "image" ? result.publicUrl : current.imageUrl,
            }));

            setFormErrors((current) => ({
                ...current,
                audioUrl: field === "audio" ? undefined : current.audioUrl,
                imageUrl: field === "image" ? undefined : current.imageUrl,
            }));

            toast.success(
                field === "audio"
                    ? "Upload audio thành công."
                    : "Upload image thành công.",
            );
        } catch (error) {
            const message = mediaUploadApi.parseErrorMessage(error);

            setFormErrors((current) => ({
                ...current,
                audioUrl: field === "audio" ? message : current.audioUrl,
                imageUrl: field === "image" ? message : current.imageUrl,
            }));

            toast.error(message);
        } finally {
            setUploadingField(null);
        }
    };

    const clearMediaUrl = (field: "audioUrl" | "imageUrl") => {
        setFormState((current) => ({
            ...current,
            [field]: "",
        }));
        setFormErrors((current) => ({
            ...current,
            [field]: undefined,
        }));
    };

    return (
        <Dialog
            open={open}
            onOpenChange={(nextOpen) => {
                setOpen(nextOpen);
                if (!nextOpen) {
                    resetForm();
                }
            }}
        >
            <DialogTrigger asChild>
                <Button className="bg-slate-900 text-white hover:bg-slate-800">
                    <Plus className="size-4" />
                    Thêm bài tập
                </Button>
            </DialogTrigger>

            <DialogContent className="flex max-h-[85vh] flex-col overflow-hidden border sm:max-w-3xl">
                <DialogHeader>
                    <DialogTitle>Tạo bài tập mới</DialogTitle>
                    <DialogDescription>
                        Tạo nhanh bài tập trong topic hiện tại.
                    </DialogDescription>
                </DialogHeader>

                <form
                    onSubmit={handleSubmit}
                    className="flex min-h-0 flex-1 flex-col"
                >
                    <div className="min-h-0 space-y-3 overflow-y-auto p-1 pr-2">
                        <div className="space-y-3 rounded-md border border-slate-200 p-3">
                            <div className="space-y-2">
                                <Label htmlFor="exercise-topic-id">
                                    TopicId
                                </Label>
                                <Input
                                    id="exercise-topic-id"
                                    value={topicId}
                                    disabled
                                />
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="exercise-question">
                                    Câu hỏi *
                                </Label>
                                <Input
                                    id="exercise-question"
                                    value={formState.question}
                                    onChange={(event) =>
                                        setFormState((current) => ({
                                            ...current,
                                            question: event.target.value,
                                        }))
                                    }
                                    placeholder="Nhập câu hỏi"
                                />
                                {formErrors.question && (
                                    <p className="text-xs text-red-600">
                                        {formErrors.question}
                                    </p>
                                )}
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="exercise-description">
                                    Mô tả *
                                </Label>
                                <Textarea
                                    id="exercise-description"
                                    rows={3}
                                    value={formState.description}
                                    onChange={(event) =>
                                        setFormState((current) => ({
                                            ...current,
                                            description: event.target.value,
                                        }))
                                    }
                                    placeholder="Mô tả ngắn về bài tập"
                                />
                                {formErrors.description && (
                                    <p className="text-xs text-red-600">
                                        {formErrors.description}
                                    </p>
                                )}
                            </div>
                        </div>

                        <div className="grid grid-cols-1 gap-3 rounded-md border border-slate-200 p-3 sm:grid-cols-2 lg:grid-cols-4">
                            <div className="space-y-2">
                                <Label>Loại bài *</Label>
                                <Select
                                    value={formState.exerciseType}
                                    onValueChange={(value: ExerciseType) =>
                                        handleExerciseTypeChange(value)
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Loại bài" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {exerciseTypeOptions.map((item) => (
                                            <SelectItem
                                                key={item.value}
                                                value={item.value}
                                            >
                                                {item.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="space-y-2">
                                <Label>Kỹ năng *</Label>
                                <Select
                                    value={formState.skillType}
                                    onValueChange={(value: SkillType) =>
                                        setFormState((current) => ({
                                            ...current,
                                            skillType: value,
                                        }))
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Kỹ năng" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {skillTypeOptions.map((item) => (
                                            <SelectItem
                                                key={item.value}
                                                value={item.value}
                                            >
                                                {item.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="space-y-2">
                                <Label>Độ khó *</Label>
                                <Select
                                    value={formState.difficulty}
                                    onValueChange={(
                                        value: ExerciseDifficulty,
                                    ) =>
                                        setFormState((current) => ({
                                            ...current,
                                            difficulty: value,
                                        }))
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Độ khó" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {difficultyOptions.map((item) => (
                                            <SelectItem
                                                key={item.value}
                                                value={item.value}
                                            >
                                                {item.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="space-y-2">
                                <Label>Context *</Label>
                                <Select
                                    value={formState.exerciseContext}
                                    onValueChange={(value: ExerciseContext) =>
                                        setFormState((current) => ({
                                            ...current,
                                            exerciseContext: value,
                                        }))
                                    }
                                >
                                    <SelectTrigger>
                                        <SelectValue placeholder="Context" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {contextOptions.map((item) => (
                                            <SelectItem
                                                key={item.value}
                                                value={item.value}
                                            >
                                                {item.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>

                        {isOptionRequired && (
                            <div className="space-y-3 rounded-md border border-slate-200 p-3">
                                <div className="flex items-center justify-between">
                                    <Label>Options *</Label>
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        onClick={handleAddOption}
                                    >
                                        <Plus className="size-4" />
                                        Thêm option
                                    </Button>
                                </div>

                                <div className="space-y-2">
                                    {formState.options.map((option, index) => (
                                        <div
                                            key={option.id}
                                            className="flex items-center gap-2"
                                        >
                                            <Input
                                                value={option.id}
                                                disabled
                                                className="w-40"
                                            />
                                            <Input
                                                value={option.text}
                                                onChange={(event) =>
                                                    setFormState((current) => ({
                                                        ...current,
                                                        options:
                                                            current.options.map(
                                                                (item) =>
                                                                    item.id ===
                                                                    option.id
                                                                        ? {
                                                                              ...item,
                                                                              text: event
                                                                                  .target
                                                                                  .value,
                                                                          }
                                                                        : item,
                                                            ),
                                                    }))
                                                }
                                                placeholder={`Nội dung option ${index + 1}`}
                                            />
                                            <Button
                                                type="button"
                                                variant="outline"
                                                size="icon"
                                                onClick={() =>
                                                    handleRemoveOption(
                                                        option.id,
                                                    )
                                                }
                                                disabled={
                                                    formState.options.length <=
                                                    2
                                                }
                                            >
                                                <Trash2 className="size-4" />
                                            </Button>
                                        </div>
                                    ))}
                                </div>

                                {formErrors.options && (
                                    <p className="text-xs text-red-600">
                                        {formErrors.options}
                                    </p>
                                )}

                                <div className="space-y-2">
                                    <Label>
                                        Đáp án đúng (theo option id) *
                                    </Label>
                                    <Select
                                        value={formState.correctAnswer}
                                        onValueChange={(value) =>
                                            setFormState((current) => ({
                                                ...current,
                                                correctAnswer: value,
                                            }))
                                        }
                                    >
                                        <SelectTrigger>
                                            <SelectValue placeholder="Chọn option đúng" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            {formState.options.map((option) => (
                                                <SelectItem
                                                    key={option.id}
                                                    value={option.id}
                                                >
                                                    {option.id}
                                                </SelectItem>
                                            ))}
                                        </SelectContent>
                                    </Select>
                                    {formErrors.correctAnswer && (
                                        <p className="text-xs text-red-600">
                                            {formErrors.correctAnswer}
                                        </p>
                                    )}
                                </div>
                            </div>
                        )}

                        {!isOptionRequired && (
                            <div className="space-y-2 rounded-md border border-slate-200 p-3">
                                <Label htmlFor="exercise-correct-answer">
                                    Đáp án đúng *
                                </Label>
                                <Input
                                    id="exercise-correct-answer"
                                    value={formState.correctAnswer}
                                    onChange={(event) =>
                                        setFormState((current) => ({
                                            ...current,
                                            correctAnswer: event.target.value,
                                        }))
                                    }
                                    placeholder="Nhập đáp án đúng"
                                />
                                {formErrors.correctAnswer && (
                                    <p className="text-xs text-red-600">
                                        {formErrors.correctAnswer}
                                    </p>
                                )}
                            </div>
                        )}

                        <div className="space-y-4 rounded-md border border-slate-200 bg-linear-to-br from-white to-slate-50 p-4">
                            <div className="space-y-1">
                                <p className="text-sm font-semibold text-slate-800">
                                    Media đính kèm (tùy chọn)
                                </p>
                                <p className="text-xs text-slate-500">
                                    Bạn có thể bỏ qua, hoặc tải lên file âm
                                    thanh/hình ảnh.
                                </p>
                            </div>

                            <div className="grid grid-cols-1 gap-6 sm:grid-cols-2">
                                <div className="space-y-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
                                    <div className="flex items-center gap-2">
                                        <Music2 className="size-4 text-blue-500" />
                                        <Label className="text-sm font-bold text-slate-700">
                                            Âm thanh
                                        </Label>
                                    </div>
                                    <label
                                        htmlFor="exercise-audio-file"
                                        className={
                                            "group flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 py-4 transition-colors hover:bg-slate-100 " +
                                            (createExerciseMutation.isPending ||
                                            uploadingField !== null
                                                ? "pointer-events-none opacity-50"
                                                : "")
                                        }
                                    >
                                        <div className="flex flex-col items-center justify-center text-center text-sm text-slate-500 group-hover:text-slate-700">
                                            <Upload className="mb-1 size-5 text-slate-400 group-hover:text-blue-500" />
                                            <span className="font-medium text-blue-600">
                                                Nhấn để tải lên
                                            </span>
                                            <span className="text-xs">
                                                Audio (.mp3, .wav)
                                            </span>
                                        </div>
                                        <Input
                                            id="exercise-audio-file"
                                            className="hidden"
                                            type="file"
                                            accept="audio/*"
                                            disabled={
                                                createExerciseMutation.isPending ||
                                                uploadingField !== null
                                            }
                                            onChange={(event) => {
                                                const file =
                                                    event.currentTarget
                                                        .files?.[0];
                                                event.currentTarget.value = "";
                                                if (!file) return;
                                                void handleUploadMedia(
                                                    file,
                                                    "audio",
                                                );
                                            }}
                                        />
                                    </label>
                                    {uploadingField === "audio" && (
                                        <p className="flex items-center justify-center gap-1 text-xs font-medium text-blue-600">
                                            <Loader2 className="size-3 animate-spin" />
                                            Đang upload...
                                        </p>
                                    )}
                                    <div className="flex items-center gap-2">
                                        <span className="shrink-0 text-[10px] font-semibold uppercase text-slate-400">
                                            Hoặc nhập URL
                                        </span>
                                        <div className="h-px w-full bg-slate-200" />
                                    </div>
                                    <div className="space-y-2">
                                        <div className="flex items-center gap-2">
                                            <Input
                                                id="exercise-audio-url"
                                                value={formState.audioUrl}
                                                onChange={(event) =>
                                                    setFormState((c) => ({
                                                        ...c,
                                                        audioUrl:
                                                            event.target.value,
                                                    }))
                                                }
                                                placeholder="https://..."
                                                className="h-8 text-xs"
                                                disabled={
                                                    createExerciseMutation.isPending
                                                }
                                            />
                                            {formState.audioUrl && (
                                                <Button
                                                    type="button"
                                                    variant="ghost"
                                                    size="icon"
                                                    className="size-8 shrink-0 text-slate-500 hover:bg-red-50 hover:text-red-600"
                                                    onClick={() =>
                                                        clearMediaUrl(
                                                            "audioUrl",
                                                        )
                                                    }
                                                    disabled={
                                                        createExerciseMutation.isPending
                                                    }
                                                >
                                                    <X className="size-4" />
                                                </Button>
                                            )}
                                        </div>
                                        <div className="flex items-center justify-between">
                                            {formState.audioUrl ? (
                                                <a
                                                    className="inline-flex max-w-50 items-center gap-1 truncate text-xs font-medium text-blue-600 hover:underline sm:max-w-62.5"
                                                    href={formState.audioUrl}
                                                    target="_blank"
                                                    rel="noreferrer"
                                                    title={formState.audioUrl}
                                                >
                                                    <Music2 className="size-3 shrink-0" />
                                                    <span className="truncate">
                                                        Nghe thử
                                                    </span>
                                                    <ExternalLink className="size-3 shrink-0" />
                                                </a>
                                            ) : (
                                                <div />
                                            )}
                                            {formErrors.audioUrl && (
                                                <p className="text-xs text-red-600">
                                                    {formErrors.audioUrl}
                                                </p>
                                            )}
                                        </div>
                                    </div>
                                </div>

                                <div className="space-y-3 rounded-lg border border-slate-200 bg-white p-4 shadow-sm">
                                    <div className="flex items-center gap-2">
                                        <ImageIcon className="size-4 text-emerald-500" />
                                        <Label className="text-sm font-bold text-slate-700">
                                            Hình ảnh
                                        </Label>
                                    </div>
                                    <label
                                        htmlFor="exercise-image-file"
                                        className={
                                            "group flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 py-4 transition-colors hover:bg-slate-100 " +
                                            (createExerciseMutation.isPending ||
                                            uploadingField !== null
                                                ? "pointer-events-none opacity-50"
                                                : "")
                                        }
                                    >
                                        <div className="flex flex-col items-center justify-center text-center text-sm text-slate-500 group-hover:text-slate-700">
                                            <Upload className="mb-1 size-5 text-slate-400 group-hover:text-emerald-500" />
                                            <span className="font-medium text-emerald-600">
                                                Nhấn để tải lên
                                            </span>
                                            <span className="text-xs">
                                                Image (.jpg, .png, .webp)
                                            </span>
                                        </div>
                                        <Input
                                            id="exercise-image-file"
                                            className="hidden"
                                            type="file"
                                            accept="image/*"
                                            disabled={
                                                createExerciseMutation.isPending ||
                                                uploadingField !== null
                                            }
                                            onChange={(event) => {
                                                const file =
                                                    event.currentTarget
                                                        .files?.[0];
                                                event.currentTarget.value = "";
                                                if (!file) return;
                                                void handleUploadMedia(
                                                    file,
                                                    "image",
                                                );
                                            }}
                                        />
                                    </label>
                                    {uploadingField === "image" && (
                                        <p className="flex items-center justify-center gap-1 text-xs font-medium text-emerald-600">
                                            <Loader2 className="size-3 animate-spin" />
                                            Đang upload...
                                        </p>
                                    )}
                                    <div className="flex items-center gap-2">
                                        <span className="shrink-0 text-[10px] font-semibold uppercase text-slate-400">
                                            Hoặc nhập URL
                                        </span>
                                        <div className="h-px w-full bg-slate-200" />
                                    </div>
                                    <div className="space-y-2">
                                        <div className="flex items-center gap-2">
                                            <Input
                                                id="exercise-image-url"
                                                value={formState.imageUrl}
                                                onChange={(event) =>
                                                    setFormState((c) => ({
                                                        ...c,
                                                        imageUrl:
                                                            event.target.value,
                                                    }))
                                                }
                                                placeholder="https://..."
                                                className="h-8 text-xs"
                                                disabled={
                                                    createExerciseMutation.isPending
                                                }
                                            />
                                            {formState.imageUrl && (
                                                <Button
                                                    type="button"
                                                    variant="ghost"
                                                    size="icon"
                                                    className="size-8 shrink-0 text-slate-500 hover:bg-red-50 hover:text-red-600"
                                                    onClick={() =>
                                                        clearMediaUrl(
                                                            "imageUrl",
                                                        )
                                                    }
                                                    disabled={
                                                        createExerciseMutation.isPending
                                                    }
                                                >
                                                    <X className="size-4" />
                                                </Button>
                                            )}
                                        </div>
                                        <div className="flex items-center justify-between">
                                            {formState.imageUrl ? (
                                                <a
                                                    className="inline-flex max-w-50 items-center gap-1 truncate text-xs font-medium text-emerald-600 hover:underline sm:max-w-62.5"
                                                    href={formState.imageUrl}
                                                    target="_blank"
                                                    rel="noreferrer"
                                                    title={formState.imageUrl}
                                                >
                                                    <ImageIcon className="size-3 shrink-0" />
                                                    <span className="truncate">
                                                        Xem ảnh
                                                    </span>
                                                    <ExternalLink className="size-3 shrink-0" />
                                                </a>
                                            ) : (
                                                <div />
                                            )}
                                            {formErrors.imageUrl && (
                                                <p className="text-xs text-red-600">
                                                    {formErrors.imageUrl}
                                                </p>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            </div>

                            <div className="space-y-2 sm:col-span-2">
                                <Label htmlFor="exercise-explanation">
                                    Explanation *
                                </Label>
                                <Textarea
                                    id="exercise-explanation"
                                    rows={3}
                                    value={formState.explanation}
                                    onChange={(event) =>
                                        setFormState((current) => ({
                                            ...current,
                                            explanation: event.target.value,
                                        }))
                                    }
                                    placeholder="Giải thích đáp án"
                                />
                                {formErrors.explanation && (
                                    <p className="text-xs text-red-600">
                                        {formErrors.explanation}
                                    </p>
                                )}
                            </div>
                        </div>

                        <div className="space-y-2 rounded-md border border-slate-200 bg-slate-50 p-3">
                            <p className="text-xs font-medium text-slate-600">
                                JSON Body Preview
                            </p>
                            <pre className="overflow-auto text-xs text-slate-700">
                                {JSON.stringify(payloadPreview, null, 2)}
                            </pre>
                        </div>

                        {formErrors.form && (
                            <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs text-red-700">
                                {formErrors.form}
                            </p>
                        )}
                    </div>

                    <DialogFooter className="mt-3 border-t border-slate-200 pt-3">
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => setOpen(false)}
                            disabled={createExerciseMutation.isPending}
                        >
                            Hủy
                        </Button>
                        <Button
                            type="submit"
                            disabled={createExerciseMutation.isPending}
                        >
                            {createExerciseMutation.isPending && (
                                <Loader2 className="size-4 animate-spin" />
                            )}
                            <Plus className="size-4" />
                            Tạo bài tập
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
