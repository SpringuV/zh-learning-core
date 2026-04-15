"use client";

import { useMemo, useState } from "react";
import { Loader2, Plus } from "lucide-react";
import { toast } from "sonner";
import {
    TopicCreateRequest,
    TopicFormState,
    TopicType,
} from "@/modules/lesson/api/topic.api";
import { useCreateTopic } from "@/modules/lesson/hooks/use.topic.tanstack";
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
type TopicFormErrors = {
    title?: string;
    description?: string;
    estimatedTimeMinutes?: string;
    examYear?: string;
    examCode?: string;
    form?: string;
};

type CreateTopicModalProps = {
    courseId: string;
    onCreated?: (topicId?: string) => void | Promise<void>;
};

export function CreateTopicModal({
    courseId,
    onCreated,
}: CreateTopicModalProps) {
    const createTopicMutation = useCreateTopic();
    const [open, setOpen] = useState(false);
    const [topicForm, setTopicForm] = useState<TopicFormState>({
        title: "",
        description: "",
        topicType: "Learning",
        estimatedTimeMinutes: "30",
        examYear: "",
        examCode: "",
    });
    const [topicErrors, setTopicErrors] = useState<TopicFormErrors>({});

    const topicPayloadPreview = useMemo(() => {
        const estimatedTimeMinutes = Number(topicForm.estimatedTimeMinutes);
        const payload: TopicCreateRequest = {
            CourseId: courseId,
            Title: topicForm.title.trim(),
            Description: topicForm.description.trim(),
            TopicType: topicForm.topicType,
            EstimatedTimeMinutes: Number.isNaN(estimatedTimeMinutes)
                ? 0
                : estimatedTimeMinutes,
        };

        if (topicForm.topicType === "Exam") {
            const examYear = Number(topicForm.examYear);
            payload.ExamYear = Number.isNaN(examYear) ? 0 : examYear;
            payload.ExamCode = topicForm.examCode.trim();
        }

        return payload;
    }, [topicForm, courseId]);

    const resetTopicForm = () => {
        setTopicForm({
            title: "",
            description: "",
            topicType: "Learning",
            estimatedTimeMinutes: "30",
            examYear: "",
            examCode: "",
        });
        setTopicErrors({});
    };

    const handleSubmitCreateTopic = async (
        event: React.SubmitEvent<HTMLFormElement>,
    ) => {
        event.preventDefault();

        const nextErrors: TopicFormErrors = {};
        const title = topicForm.title.trim();
        const description = topicForm.description.trim();
        const estimatedTimeMinutes = Number(topicForm.estimatedTimeMinutes);

        if (!courseId) {
            nextErrors.form = "Không tìm thấy CourseId từ URL.";
        }
        if (!title) {
            nextErrors.title = "Tiêu đề topic không được để trống.";
        }
        if (!description) {
            nextErrors.description = "Mô tả topic không được để trống.";
        }
        if (
            Number.isNaN(estimatedTimeMinutes) ||
            !Number.isInteger(estimatedTimeMinutes) ||
            estimatedTimeMinutes <= 0
        ) {
            nextErrors.estimatedTimeMinutes =
                "EstimatedTimeMinutes phải là số nguyên lớn hơn 0.";
        }

        let examYear: number | undefined;
        let examCode: string | undefined;
        if (topicForm.topicType === "Exam") {
            examYear = Number(topicForm.examYear);
            examCode = topicForm.examCode.trim();

            if (
                Number.isNaN(examYear) ||
                !Number.isInteger(examYear) ||
                examYear <= 0
            ) {
                nextErrors.examYear = "ExamYear phải là số nguyên lớn hơn 0.";
            }
            if (!examCode) {
                nextErrors.examCode =
                    "ExamCode không được để trống với topic Exam.";
            }
        }

        setTopicErrors(nextErrors);
        if (Object.keys(nextErrors).length > 0) {
            return;
        }

        const payload: TopicCreateRequest = {
            CourseId: courseId,
            Title: title,
            Description: description,
            TopicType: topicForm.topicType,
            EstimatedTimeMinutes: estimatedTimeMinutes,
        };

        if (topicForm.topicType === "Exam") {
            payload.ExamYear = examYear;
            payload.ExamCode = examCode;
        }

        try {
            const createdData = await createTopicMutation.mutateAsync(payload);
            const topicId =
                (createdData as { TopicId?: string })?.TopicId ??
                (createdData as { Data?: { TopicId?: string } })?.Data?.TopicId;

            toast.success(
                `Tạo topic thành công${topicId ? ` (#${topicId})` : ""}.`,
            );
            setOpen(false);
            resetTopicForm();
            if (onCreated) {
                await Promise.resolve(onCreated(topicId));
            }
        } catch (error) {
            const message =
                error instanceof Error
                    ? error.message
                    : "Không thể tạo topic. Vui lòng thử lại.";
            setTopicErrors({ form: message });
            toast.error(message);
        }
    };

    return (
        <Dialog
            open={open}
            onOpenChange={(nextOpen) => {
                setOpen(nextOpen);
                if (!nextOpen) {
                    resetTopicForm();
                }
            }}
        >
            <DialogTrigger asChild>
                <Button className="bg-slate-900 text-white hover:bg-slate-800">
                    <Plus className="size-4" />
                    Thêm Topic mới
                </Button>
            </DialogTrigger>

            <DialogContent className="flex max-h-[80vh] flex-col overflow-hidden border sm:max-w-xl">
                <DialogHeader>
                    <DialogTitle>Tạo Topic mới</DialogTitle>
                    <DialogDescription>
                        Điền thông tin để tạo topic mới cho khóa học này.
                    </DialogDescription>
                </DialogHeader>

                <form
                    onSubmit={handleSubmitCreateTopic}
                    className="flex min-h-0 flex-1 flex-col"
                >
                    <div className="min-h-0 space-y-3 overflow-y-auto p-1 pr-2">
                        <div className="space-y-3 rounded-md border border-slate-200 p-3">
                            <div className="space-y-2">
                                <Label htmlFor="topic-course-id">
                                    CourseId
                                </Label>
                                <Input
                                    id="topic-course-id"
                                    value={courseId}
                                    disabled
                                />
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="topic-title">Title *</Label>
                                <Input
                                    id="topic-title"
                                    value={topicForm.title}
                                    onChange={(event) =>
                                        setTopicForm((current) => ({
                                            ...current,
                                            title: event.target.value,
                                        }))
                                    }
                                    placeholder="Ví dụ: Giao tiếp cơ bản HSK4"
                                />
                                {topicErrors.title && (
                                    <p className="text-xs text-red-600">
                                        {topicErrors.title}
                                    </p>
                                )}
                            </div>

                            <div className="space-y-2">
                                <Label htmlFor="topic-description">
                                    Description *
                                </Label>
                                <Textarea
                                    id="topic-description"
                                    value={topicForm.description}
                                    onChange={(event) =>
                                        setTopicForm((current) => ({
                                            ...current,
                                            description: event.target.value,
                                        }))
                                    }
                                    rows={3}
                                    placeholder="Mô tả topic"
                                />
                                {topicErrors.description && (
                                    <p className="text-xs text-red-600">
                                        {topicErrors.description}
                                    </p>
                                )}
                            </div>
                        </div>

                        <div className="space-y-3 rounded-md border border-slate-200 p-3">
                            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                                <div className="space-y-2">
                                    <Label>TopicType *</Label>
                                    <Select
                                        value={topicForm.topicType}
                                        onValueChange={(value: TopicType) =>
                                            setTopicForm((current) => ({
                                                ...current,
                                                topicType: value,
                                            }))
                                        }
                                    >
                                        <SelectTrigger>
                                            <SelectValue placeholder="Chọn loại topic" />
                                        </SelectTrigger>
                                        <SelectContent>
                                            <SelectItem value="Learning">
                                                Learning
                                            </SelectItem>
                                            <SelectItem value="Exam">
                                                Exam
                                            </SelectItem>
                                        </SelectContent>
                                    </Select>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="topic-estimated-time">
                                        EstimatedTimeMinutes *
                                    </Label>
                                    <Input
                                        id="topic-estimated-time"
                                        type="number"
                                        min={1}
                                        step={1}
                                        value={topicForm.estimatedTimeMinutes}
                                        onChange={(event) =>
                                            setTopicForm((current) => ({
                                                ...current,
                                                estimatedTimeMinutes:
                                                    event.target.value,
                                            }))
                                        }
                                        placeholder="Ví dụ: 500"
                                    />
                                    {topicErrors.estimatedTimeMinutes && (
                                        <p className="text-xs text-red-600">
                                            {topicErrors.estimatedTimeMinutes}
                                        </p>
                                    )}
                                </div>
                            </div>
                        </div>

                        {topicForm.topicType === "Exam" && (
                            <div className="grid grid-cols-1 gap-3 rounded-md border border-slate-200 p-3 sm:grid-cols-2">
                                <div className="space-y-2">
                                    <Label htmlFor="topic-exam-year">
                                        ExamYear *
                                    </Label>
                                    <Input
                                        id="topic-exam-year"
                                        type="number"
                                        min={1}
                                        step={1}
                                        value={topicForm.examYear}
                                        onChange={(event) =>
                                            setTopicForm((current) => ({
                                                ...current,
                                                examYear: event.target.value,
                                            }))
                                        }
                                        placeholder="Ví dụ: 2024"
                                    />
                                    {topicErrors.examYear && (
                                        <p className="text-xs text-red-600">
                                            {topicErrors.examYear}
                                        </p>
                                    )}
                                </div>
                                <div className="space-y-2">
                                    <Label htmlFor="topic-exam-code">
                                        ExamCode *
                                    </Label>
                                    <Input
                                        id="topic-exam-code"
                                        value={topicForm.examCode}
                                        onChange={(event) =>
                                            setTopicForm((current) => ({
                                                ...current,
                                                examCode: event.target.value,
                                            }))
                                        }
                                        placeholder="Ví dụ: HSK4-2024-01"
                                    />
                                    {topicErrors.examCode && (
                                        <p className="text-xs text-red-600">
                                            {topicErrors.examCode}
                                        </p>
                                    )}
                                </div>
                            </div>
                        )}

                        <div className="space-y-2 rounded-md border border-slate-200 bg-slate-50 p-3">
                            <p className="text-xs font-medium text-slate-600">
                                JSON Body Preview
                            </p>
                            <pre className="overflow-auto text-xs text-slate-700">
                                {JSON.stringify(topicPayloadPreview, null, 2)}
                            </pre>
                        </div>

                        {topicErrors.form && (
                            <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs text-red-700">
                                {topicErrors.form}
                            </p>
                        )}
                    </div>

                    <DialogFooter className="mt-3 border-t border-slate-200 pt-3">
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => setOpen(false)}
                            disabled={createTopicMutation.isPending}
                        >
                            Hủy
                        </Button>
                        <Button
                            type="submit"
                            disabled={createTopicMutation.isPending}
                        >
                            {createTopicMutation.isPending && (
                                <Loader2 className="size-4 animate-spin" />
                            )}
                            <Plus className="size-4" />
                            Tạo Topic
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
