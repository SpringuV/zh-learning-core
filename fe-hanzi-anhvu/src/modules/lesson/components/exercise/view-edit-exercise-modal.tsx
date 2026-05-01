"use client";

import { useEffect, useMemo, useRef, useState } from "react";
import {
    ExternalLink,
    Image as ImageIcon,
    Loader2,
    Music2,
    Pencil,
    Plus,
    Settings2,
    Trash2,
    Upload,
    X,
} from "lucide-react";
import { toast } from "sonner";
import { mediaUploadApi } from "@/modules/lesson/api/media-upload.api";
import {
    useGetExerciseDetail,
    useUpdateExercise,
} from "@/modules/lesson/hooks/use.exercise.tanstack";
import {
    ExerciseContext,
    ExerciseDetailResponse,
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
import { useParams, useRouter } from "next/navigation";

// #region Types and constants
type ExerciseEditFormState = {
    description: string;
    question: string;
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

type FormErrors = {
    topicId?: string;
    form?: string;
    question?: string;
    description?: string;
    correctAnswer?: string;
    explanation?: string;
    options?: string;
    audioUrl?: string;
    imageUrl?: string;
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

const createEditExerciseSchema = z
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

const toFormState = (
    exercise: ExerciseDetailResponse,
): ExerciseEditFormState => ({
    description: exercise.description,
    question: exercise.question,
    exerciseType: exercise.exerciseType,
    skillType: exercise.skillType,
    difficulty: exercise.difficulty,
    exerciseContext: exercise.context,
    correctAnswer: exercise.correctAnswer,
    explanation: exercise.explanation ?? "",
    audioUrl: exercise.audioUrl ?? "",
    imageUrl: exercise.imageUrl ?? "",
    options: (exercise.options ?? []).map((option) => ({
        id: option.id,
        text: option.text,
    })),
});

const mapExerciseValidationErrors = (
    issues: z.ZodError["issues"],
): FormErrors => {
    const nextErrors: FormErrors = {};

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

const getErrorMessage = (error: unknown) => {
    if (typeof error === "object" && error !== null) {
        const maybeApiError = error as {
            response?: {
                data?: {
                    detail?: string;
                    title?: string;
                    message?: string;
                    errors?: Record<string, string[]>;
                };
            };
            message?: string;
        };

        const firstValidationError = maybeApiError.response?.data?.errors
            ? Object.values(maybeApiError.response.data.errors)[0]?.[0]
            : undefined;

        return (
            firstValidationError ??
            maybeApiError.response?.data?.detail ??
            maybeApiError.response?.data?.message ??
            maybeApiError.response?.data?.title ??
            maybeApiError.message ??
            "Không thể cập nhật bài tập."
        );
    }

    return "Không thể cập nhật bài tập.";
};

const createOptionId = () =>
    `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`;

const normalizeRouteParam = (value: string | string[] | undefined) => {
    if (Array.isArray(value)) {
        return value[0] ?? "";
    }

    return value ?? "";
};

export function ViewOrEditExerciseModal() {
    const params = useParams();
    const router = useRouter();
    const [isEditMode, setIsEditMode] = useState(false);
    const isClosingHandledRef = useRef(false);

    // Đảm bảo rằng chúng ta luôn có giá trị chuỗi đơn giản cho các param,
    // tránh trường hợp param bị truyền dưới dạng array do lỗi của Next.js dynamic route
    // use memo để tránh normalize lại khi params không thay đổi
    const normalizedCourseId = useMemo(
        // course-id là param cha của topic-id và exercise-id, nên nếu course-id bị lỗi thành array thì cũng có khả năng topic-id và exercise-id cũng bị lỗi tương tự, nên chúng ta normalize tất cả các param này để đảm bảo tính nhất quán
        // tên của folder dynamic route là [course-id], nhưng khi lấy ra bằng useParams thì có thể bị lỗi thành courseId hoặc course_id, nên chúng ta cũng cần normalize để đảm bảo lấy đúng giá trị
        () => normalizeRouteParam(params["course-id"]),
        [params],
    );
    const normalizedTopicId = useMemo(
        () => normalizeRouteParam(params["topic-id"]),
        [params],
    );
    const exerciseId = useMemo(
        () => normalizeRouteParam(params["exercise-id"]),
        [params],
    );

    const topicHref = useMemo(
        () =>
            `/cms/lessons/course/${normalizedCourseId}/topics/${normalizedTopicId}`,
        [normalizedCourseId, normalizedTopicId],
    );

    const viewDetailTanstack = useGetExerciseDetail(exerciseId);
    const updateExerciseMutation = useUpdateExercise();
    const [uploadingField, setUploadingField] = useState<
        "audio" | "image" | null
    >(null);
    const [formState, setFormState] = useState<ExerciseEditFormState | null>(
        null,
    );
    const [errors, setErrors] = useState<FormErrors>({});
    const exercise = viewDetailTanstack.data?.data;

    const handleRequestClose = () => {
        if (isClosingHandledRef.current) {
            return;
        }

        isClosingHandledRef.current = true;
        router.replace(topicHref);
    };

    const resetAndClose = () => {
        setIsEditMode(false);
        setErrors({});
        setFormState(exercise ? toFormState(exercise) : null);
        handleRequestClose();
    };

    const isFieldReadOnly =
        !isEditMode ||
        updateExerciseMutation.isPending ||
        !exercise ||
        uploadingField !== null;

    const isSubmitDisabled =
        !exercise || !isEditMode || updateExerciseMutation.isPending;

    useEffect(() => {
        if (!exercise) {
            return;
        }

        setFormState(toFormState(exercise));
        setErrors({});
        setIsEditMode(false);
    }, [exercise]);

    const isOptionRequired = optionRequiredTypes.includes(
        formState?.exerciseType ?? "ListenDialogueChoice",
    );

    const optionItems = formState?.options ?? [];

    const payloadPreview = useMemo(() => {
        if (!exercise || !formState) {
            return null;
        }

        return {
            exerciseId: exercise.exerciseId,
            description: formState.description.trim(),
            question: formState.question.trim(),
            exerciseType: formState.exerciseType,
            skillType: formState.skillType,
            difficulty: formState.difficulty,
            exerciseContext: formState.exerciseContext,
            correctAnswer: formState.correctAnswer.trim(),
            explanation: formState.explanation.trim(),
            audioUrl: formState.audioUrl.trim(),
            imageUrl: formState.imageUrl.trim(),
            options: isOptionRequired
                ? formState.options.map((option) => ({
                      id: option.id,
                      text: option.text.trim(),
                  }))
                : undefined,
        };
    }, [exercise, formState, isOptionRequired]);

    const handleExerciseTypeChange = (value: ExerciseType) => {
        const nextRequiresOptions = optionRequiredTypes.includes(value);

        setFormState((current) => {
            if (!current) {
                return current;
            }

            return {
                ...current,
                exerciseType: value,
                options:
                    nextRequiresOptions && current.options.length < 2
                        ? [
                              { id: createOptionId(), text: "" },
                              { id: createOptionId(), text: "" },
                          ]
                        : nextRequiresOptions
                          ? current.options
                          : [],
                correctAnswer: nextRequiresOptions ? current.correctAnswer : "",
            };
        });
    };

    const handleSubmit = async () => {
        if (!exercise || !payloadPreview || !formState) {
            return;
        }

        const validated = createEditExerciseSchema.safeParse(formState);
        if (!validated.success) {
            setErrors(mapExerciseValidationErrors(validated.error.issues));
            return;
        }

        setErrors({});

        const shouldIncludeOptions = optionRequiredTypes.includes(
            validated.data.exerciseType,
        );

        try {
            await updateExerciseMutation.mutateAsync({
                exerciseId: payloadPreview.exerciseId,
                description: validated.data.description,
                question: validated.data.question,
                exerciseType: validated.data.exerciseType,
                skillType: validated.data.skillType,
                difficulty: validated.data.difficulty,
                exerciseContext: validated.data.exerciseContext,
                correctAnswer: validated.data.correctAnswer,
                explanation: validated.data.explanation || undefined,
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
            });

            toast.success("Đã cập nhật bài tập.");
            // await viewDetailTanstack.refetch();
            // console.log("Refetched exercise detail do vừa cập nhật");
            // setIsEditMode(false);
        } catch (error) {
            setErrors({ form: getErrorMessage(error) });
            toast.error(getErrorMessage(error));
        }
    };
    // #region Media upload handler

    const handleUploadMedia = async (file: File, field: "audio" | "image") => {
        // Validate file type before uploading to save bandwidth and provide faster feedback to user
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

        try {
            const result = await mediaUploadApi.uploadToR2(
                file,
                field === "audio" ? "audio" : "images",
            );

            setFormState((current) =>
                current
                    ? {
                          ...current,
                          audioUrl:
                              field === "audio"
                                  ? result.publicUrl
                                  : current.audioUrl,
                          imageUrl:
                              field === "image"
                                  ? result.publicUrl
                                  : current.imageUrl,
                      }
                    : current,
            );

            setErrors((current) => ({
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

            setErrors((current) => ({
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
        setFormState((current) =>
            current
                ? {
                      ...current,
                      [field]: "",
                  }
                : current,
        );

        setErrors((current) => ({
            ...current,
            [field]: undefined,
        }));
    };

    return (
        <Dialog
            open
            onOpenChange={(nextOpen) => {
                if (!nextOpen) {
                    resetAndClose();
                }
            }}
        >
            <DialogContent className="flex max-h-[85vh] max-w-[85vw] flex-col overflow-hidden border sm:max-w-4xl animate-out ">
                <DialogHeader>
                    <DialogTitle>Chi tiết bài tập</DialogTitle>
                    <DialogDescription>
                        {isEditMode
                            ? "Đang ở chế độ chỉnh sửa. Bạn có thể cập nhật các trường dữ liệu."
                            : "Đang ở chế độ xem. Nhấn Chỉnh sửa để mở khóa các trường."}
                    </DialogDescription>
                </DialogHeader>

                <div className="min-h-0 space-y-6 overflow-y-auto bg-slate-50/50 p-4 sm:p-6 pr-4">
                    {viewDetailTanstack.isLoading && !formState && (
                        <p className="rounded-md border border-slate-200 bg-slate-50 px-3 py-2 text-xs text-slate-600">
                            Đang tải chi tiết bài tập...
                        </p>
                    )}

                    {!viewDetailTanstack.isLoading && !exercise && (
                        <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs text-red-700">
                            Không thể tải dữ liệu bài tập.
                        </p>
                    )}

                    {errors.form && (
                        <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs text-red-700">
                            {errors.form}
                        </p>
                    )}

                    <div className="space-y-3 rounded-md border border-slate-200 p-3">
                        <div className="flex items-center gap-4 justify-between">
                            <div className="space-y-2 w-full">
                                <Label htmlFor="edit-exercise-id">
                                    ExerciseId
                                </Label>
                                <Input
                                    id="edit-exercise-id"
                                    value={exercise?.exerciseId ?? ""}
                                    disabled
                                />
                            </div>
                        </div>
                        <div className="space-y-2">
                            <Label htmlFor="edit-exercise-question">
                                Câu hỏi *
                            </Label>
                            <Input
                                id="edit-exercise-question"
                                value={formState?.question ?? ""}
                                disabled={isFieldReadOnly}
                                onChange={(event) =>
                                    setFormState((current) =>
                                        current
                                            ? {
                                                  ...current,
                                                  question: event.target.value,
                                              }
                                            : current,
                                    )
                                }
                                placeholder="Nhập câu hỏi"
                            />
                            {errors.question && (
                                <p className="text-xs text-red-600">
                                    {errors.question}
                                </p>
                            )}
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="edit-exercise-description">
                                Mô tả *
                            </Label>
                            <Textarea
                                id="edit-exercise-description"
                                rows={3}
                                value={formState?.description ?? ""}
                                disabled={isFieldReadOnly}
                                onChange={(event) =>
                                    setFormState((current) =>
                                        current
                                            ? {
                                                  ...current,
                                                  description:
                                                      event.target.value,
                                              }
                                            : current,
                                    )
                                }
                                placeholder="Mô tả ngắn về bài tập"
                            />
                            {errors.description && (
                                <p className="text-xs text-red-600">
                                    {errors.description}
                                </p>
                            )}
                        </div>
                    </div>
                    <div className="space-y-5 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
                        <h3 className="flex items-center gap-2 text-sm font-semibold text-slate-800 mb-2">
                            <Settings2 className="size-4 text-purple-500" />
                            Phân loại & Thuộc tính
                        </h3>
                        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
                            <div className="space-y-2">
                                <Label>Loại bài tập</Label>
                                <Select
                                    value={formState?.exerciseType}
                                    onValueChange={(value: ExerciseType) =>
                                        handleExerciseTypeChange(value)
                                    }
                                >
                                    <SelectTrigger disabled={isFieldReadOnly}>
                                        <SelectValue placeholder="Chọn loại" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {exerciseTypeOptions.map((option) => (
                                            <SelectItem
                                                key={option.value}
                                                value={option.value}
                                            >
                                                {option.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="space-y-2">
                                <Label>Kỹ năng</Label>
                                <Select
                                    value={formState?.skillType}
                                    onValueChange={(value: SkillType) =>
                                        setFormState((current) =>
                                            current
                                                ? {
                                                      ...current,
                                                      skillType: value,
                                                  }
                                                : current,
                                        )
                                    }
                                >
                                    <SelectTrigger disabled={isFieldReadOnly}>
                                        <SelectValue placeholder="Chọn kỹ năng" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {skillTypeOptions.map((option) => (
                                            <SelectItem
                                                key={option.value}
                                                value={option.value}
                                            >
                                                {option.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="space-y-2">
                                <Label>Độ khó</Label>
                                <Select
                                    value={formState?.difficulty}
                                    onValueChange={(
                                        value: ExerciseDifficulty,
                                    ) =>
                                        setFormState((current) =>
                                            current
                                                ? {
                                                      ...current,
                                                      difficulty: value,
                                                  }
                                                : current,
                                        )
                                    }
                                >
                                    <SelectTrigger disabled={isFieldReadOnly}>
                                        <SelectValue placeholder="Chọn độ khó" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {difficultyOptions.map((option) => (
                                            <SelectItem
                                                key={option.value}
                                                value={option.value}
                                            >
                                                {option.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>

                            <div className="space-y-2">
                                <Label>Bối cảnh</Label>
                                <Select
                                    value={formState?.exerciseContext}
                                    onValueChange={(value: ExerciseContext) =>
                                        setFormState((current) =>
                                            current
                                                ? {
                                                      ...current,
                                                      exerciseContext: value,
                                                  }
                                                : current,
                                        )
                                    }
                                >
                                    <SelectTrigger disabled={isFieldReadOnly}>
                                        <SelectValue placeholder="Chọn bối cảnh" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {contextOptions.map((option) => (
                                            <SelectItem
                                                key={option.value}
                                                value={option.value}
                                            >
                                                {option.label}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                            </div>
                        </div>
                    </div>

                    {isOptionRequired && (
                        <div className="space-y-3 rounded-md border border-slate-200 p-3">
                            <div className="flex items-center justify-between">
                                <Label>Options *</Label>
                                {isEditMode && (
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        disabled={
                                            updateExerciseMutation.isPending
                                        }
                                        onClick={() =>
                                            setFormState((current) =>
                                                current
                                                    ? {
                                                          ...current,
                                                          options: [
                                                              ...current.options,
                                                              {
                                                                  id: createOptionId(),
                                                                  text: "",
                                                              },
                                                          ],
                                                      }
                                                    : current,
                                            )
                                        }
                                    >
                                        <Plus className="size-4" />
                                        Thêm option
                                    </Button>
                                )}
                            </div>

                            <div className="space-y-2">
                                {optionItems.map((option, index) => (
                                    <div
                                        key={option.id}
                                        className="flex items-center gap-2"
                                    >
                                        <Input
                                            className="w-1/3"
                                            value={option.id}
                                            disabled
                                        />
                                        <Input
                                            value={option.text}
                                            disabled={isFieldReadOnly}
                                            placeholder={`Nội dung option ${index + 1}`}
                                            onChange={(event) =>
                                                setFormState((current) => {
                                                    if (!current) {
                                                        return current;
                                                    }

                                                    return {
                                                        ...current,
                                                        options:
                                                            current.options.map(
                                                                // Khi chỉnh sửa text của option, chỉ update text của option đó, giữ nguyên id để tránh bị mất liên kết với đáp án đúng nếu có
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
                                                    };
                                                })
                                            }
                                        />
                                        <Button
                                            type="button"
                                            variant="outline"
                                            size="icon"
                                            onClick={() =>
                                                setFormState((current) => {
                                                    if (!current) {
                                                        return current;
                                                    }
                                                    // Khi xóa option, nếu option đó đang là đáp án đúng thì reset đáp án đúng về rỗng
                                                    const nextOptions =
                                                        current.options.filter(
                                                            (item) =>
                                                                item.id !==
                                                                option.id,
                                                        );
                                                    const nextCorrectAnswer =
                                                        current.correctAnswer ===
                                                        option.id
                                                            ? ""
                                                            : current.correctAnswer;

                                                    return {
                                                        ...current,
                                                        options: nextOptions,
                                                        correctAnswer:
                                                            nextCorrectAnswer,
                                                    };
                                                })
                                            }
                                            disabled={
                                                isFieldReadOnly ||
                                                optionItems.length <= 2
                                            }
                                        >
                                            <Trash2 className="size-4" />
                                        </Button>
                                    </div>
                                ))}
                            </div>

                            {errors.options && (
                                <p className="text-xs text-red-600">
                                    {errors.options}
                                </p>
                            )}

                            <div className="space-y-2">
                                <Label>Đáp án đúng (theo option id) *</Label>
                                <Select
                                    value={formState?.correctAnswer ?? ""}
                                    onValueChange={(value) =>
                                        setFormState((current) =>
                                            current
                                                ? {
                                                      ...current,
                                                      correctAnswer: value,
                                                  }
                                                : current,
                                        )
                                    }
                                >
                                    <SelectTrigger disabled={isFieldReadOnly}>
                                        <SelectValue placeholder="Chọn option đúng" />
                                    </SelectTrigger>
                                    <SelectContent>
                                        {optionItems.map((option) => (
                                            <SelectItem
                                                key={option.id}
                                                value={option.id}
                                            >
                                                {option.id}
                                            </SelectItem>
                                        ))}
                                    </SelectContent>
                                </Select>
                                {errors.correctAnswer && (
                                    <p className="text-xs text-red-600">
                                        {errors.correctAnswer}
                                    </p>
                                )}
                            </div>
                        </div>
                    )}

                    {!isOptionRequired && (
                        <div className="space-y-2 rounded-md border border-slate-200 p-3">
                            <Label htmlFor="edit-correct-answer">
                                Đáp án đúng *
                            </Label>
                            <Input
                                id="edit-correct-answer"
                                value={formState?.correctAnswer ?? ""}
                                disabled={isFieldReadOnly}
                                onChange={(event) =>
                                    setFormState((current) =>
                                        current
                                            ? {
                                                  ...current,
                                                  correctAnswer:
                                                      event.target.value,
                                              }
                                            : current,
                                    )
                                }
                                placeholder="Nhập đáp án đúng"
                            />
                            {errors.correctAnswer && (
                                <p className="text-xs text-red-600">
                                    {errors.correctAnswer}
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
                                    htmlFor="edit-audio-file"
                                    className={
                                        "group flex flex-col items-center justify-center rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 py-4 transition-colors " +
                                        (isFieldReadOnly ||
                                        uploadingField !== null
                                            ? "pointer-events-none opacity-50"
                                            : "cursor-pointer hover:bg-slate-100")
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
                                        id="edit-audio-file"
                                        className="hidden"
                                        type="file"
                                        accept="audio/*"
                                        disabled={
                                            isFieldReadOnly ||
                                            uploadingField !== null
                                        }
                                        onChange={(event) => {
                                            const file =
                                                event.currentTarget.files?.[0];
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
                                            id="edit-audio-url"
                                            value={formState?.audioUrl ?? ""}
                                            onChange={(event) =>
                                                setFormState((c) =>
                                                    c
                                                        ? {
                                                              ...c,
                                                              audioUrl:
                                                                  event.target
                                                                      .value,
                                                          }
                                                        : c,
                                                )
                                            }
                                            placeholder="https://..."
                                            className="h-8 text-xs"
                                            disabled={isFieldReadOnly}
                                        />
                                        {!isFieldReadOnly &&
                                            formState?.audioUrl && (
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
                                                    disabled={isFieldReadOnly}
                                                >
                                                    <X className="size-4" />
                                                </Button>
                                            )}
                                    </div>
                                    <div className="flex items-center justify-between">
                                        {formState?.audioUrl ? (
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
                                        {errors.audioUrl && (
                                            <p className="text-xs text-red-600">
                                                {errors.audioUrl}
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
                                    htmlFor="edit-image-file"
                                    className={
                                        "group flex flex-col items-center justify-center rounded-lg border-2 border-dashed border-slate-300 bg-slate-50 py-4 transition-colors " +
                                        (isFieldReadOnly ||
                                        uploadingField !== null
                                            ? "pointer-events-none opacity-50"
                                            : "cursor-pointer hover:bg-slate-100")
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
                                        id="edit-image-file"
                                        className="hidden"
                                        type="file"
                                        accept="image/*"
                                        disabled={
                                            isFieldReadOnly ||
                                            uploadingField !== null
                                        }
                                        onChange={(event) => {
                                            const file =
                                                event.currentTarget.files?.[0];
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
                                            id="edit-image-url"
                                            value={formState?.imageUrl ?? ""}
                                            onChange={(event) =>
                                                setFormState((c) =>
                                                    c
                                                        ? {
                                                              ...c,
                                                              imageUrl:
                                                                  event.target
                                                                      .value,
                                                          }
                                                        : c,
                                                )
                                            }
                                            placeholder="https://..."
                                            className="h-8 text-xs"
                                            disabled={isFieldReadOnly}
                                        />
                                        {!isFieldReadOnly &&
                                            formState?.imageUrl && (
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
                                                    disabled={isFieldReadOnly}
                                                >
                                                    <X className="size-4" />
                                                </Button>
                                            )}
                                    </div>
                                    <div className="flex items-center justify-between">
                                        {formState?.imageUrl ? (
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
                                        {errors.imageUrl && (
                                            <p className="text-xs text-red-600">
                                                {errors.imageUrl}
                                            </p>
                                        )}
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="edit-explanation">
                                Explanation *
                            </Label>
                            <Textarea
                                id="edit-explanation"
                                rows={3}
                                value={formState?.explanation ?? ""}
                                disabled={isFieldReadOnly}
                                onChange={(event) =>
                                    setFormState((current) =>
                                        current
                                            ? {
                                                  ...current,
                                                  explanation:
                                                      event.target.value,
                                              }
                                            : current,
                                    )
                                }
                                placeholder="Giải thích đáp án"
                            />
                            {errors.explanation && (
                                <p className="text-xs text-red-600">
                                    {errors.explanation}
                                </p>
                            )}
                        </div>
                    </div>

                    <div className="space-y-2 rounded-xl border border-slate-200 bg-slate-50/50 p-5 shadow-inner">
                        <p className="text-xs font-medium text-slate-600">
                            JSON Body Preview
                        </p>
                        <pre className="overflow-auto text-xs text-slate-700">
                            {JSON.stringify(payloadPreview, null, 2)}
                        </pre>
                    </div>
                </div>

                <DialogFooter className="mt-3 border-t border-slate-200 pt-3">
                    <Button
                        type="button"
                        variant="outline"
                        disabled={updateExerciseMutation.isPending}
                        onClick={resetAndClose}
                    >
                        Hủy
                    </Button>
                    <Button
                        type="button"
                        className="bg-slate-900 text-white hover:bg-slate-800"
                        onClick={() => {
                            if (!isEditMode) {
                                setErrors({});
                                setIsEditMode(true);
                                return;
                            }

                            void handleSubmit();
                        }}
                        disabled={Boolean(
                            !exercise ||
                            updateExerciseMutation.isPending ||
                            (isEditMode && isSubmitDisabled),
                        )}
                    >
                        {isEditMode && updateExerciseMutation.isPending && (
                            <Loader2 className="size-4 animate-spin" />
                        )}
                        <Pencil className="size-4" />
                        {!isEditMode
                            ? "Chỉnh sửa"
                            : updateExerciseMutation.isPending
                              ? "Đang lưu..."
                              : "Lưu thay đổi"}
                    </Button>
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
