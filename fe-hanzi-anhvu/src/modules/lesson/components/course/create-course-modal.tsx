"use client";

import { useEffect, useMemo, useState } from "react";
import { Loader2, Plus } from "lucide-react";
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
import {
    CourseCreateRequest,
    CourseCreateResponseApi,
} from "@/modules/lesson/types/coure.type";
import { useCreateCourse } from "@/modules/lesson/hooks/use.course.tanstack";
import { createCourseMachine } from "@/modules/lesson/machines/create.course.machine";
import { fromPromise } from "xstate";
import { useMachine } from "@xstate/react";

type CreateCourseModalProps = {
    onCreated?: (response: CourseCreateResponseApi) => void | Promise<void>;
};

type FormErrors = {
    title?: string;
};

type CreateCourseFormState = {
    title: string;
    description: string;
    hskLevel: string;
};

const toSlug = (value: string) => {
    return value
        .toLowerCase()
        .normalize("NFD") // tách chữ và dấu
        .replace(/[\u0300-\u036f]/g, "") // loại bỏ dấu
        .replace(/đ/g, "d") // loại bỏ ký tự đặc biệt
        .replace(/[^a-z0-9]+/g, "-") // thay thế khoảng trắng và ký tự đặc biệt bằng dấu gạch ngang
        .replace(/^-+|-+$/g, "") // loại bỏ dấu gạch ngang ở đầu và cuối
        .replace(/-{2,}/g, "-"); // thay thế nhiều dấu gạch ngang liên tiếp bằng một dấu gạch ngang duy nhất
};

export function CreateCourseModal({ onCreated }: CreateCourseModalProps) {
    const getInitialFormState = (): CreateCourseFormState => ({
        title: "",
        description: "",
        hskLevel: "1",
    });
    // #region State
    // State quản lý form và lỗi
    const [open, setOpen] = useState(false);
    const [form, setForm] =
        useState<CreateCourseFormState>(getInitialFormState);
    const [errors, setErrors] = useState<FormErrors>({});
    // #endregion

    // #region XState machine
    // machine and mutations
    const createMutation = useCreateCourse();
    const machine = useMemo(
        () =>
            createCourseMachine.provide({
                actors: {
                    createCourse: fromPromise(
                        async ({ input }: { input: CourseCreateRequest }) => {
                            return await createMutation.mutateAsync(input);
                        },
                    ),
                },
            }),
        [createMutation],
    );
    const [state, send] = useMachine(machine);

    const isSubmitting = state.matches("submitting");

    useEffect(() => {
        if (!state.matches("success")) {
            return;
        }

        if (state.context.messageResponse && onCreated) {
            void Promise.resolve(onCreated(state.context.messageResponse));
        }

        setOpen(false);
    }, [state, onCreated]);
    // #endregion

    const slugPreview = useMemo(() => toSlug(form.title), [form.title]);

    const resetForm = () => {
        setForm(getInitialFormState());
        setErrors({});
    };

    useEffect(() => {
        if (!open) {
            resetForm();
            send({ type: "RESET" });
        }
    }, [open, send]);

    const handleSubmit = (event: React.SubmitEvent<HTMLFormElement>) => {
        event.preventDefault();

        const nextErrors: FormErrors = {};
        const normalizedTitle = form.title.trim();
        const parsedHskLevel = Number(form.hskLevel);

        if (!normalizedTitle) {
            nextErrors.title = "Tên khóa học không được để trống.";
        }
        setErrors(nextErrors);

        if (Object.keys(nextErrors).length > 0) {
            return;
        }

        const payload: CourseCreateRequest = {
            Title: normalizedTitle,
            Description: form.description.trim(),
            HskLevel: parsedHskLevel,
            Slug: slugPreview || `khoa-hoc-${Date.now()}`,
        };

        send({ type: "SUBMIT", input: payload });
    };

    return (
        <Dialog open={open} onOpenChange={setOpen}>
            <DialogTrigger asChild>
                <Button
                    className="bg-amber-600 text-white hover:bg-amber-700"
                    size="sm"
                >
                    <Plus className="size-4" />
                    Tạo khóa học
                </Button>
            </DialogTrigger>

            <DialogContent className="sm:max-w-lg">
                <DialogHeader>
                    <DialogTitle>Tạo khóa học mới</DialogTitle>
                    <DialogDescription>
                        Nhập thông tin cơ bản theo aggregate `Course`.
                    </DialogDescription>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div className="space-y-2">
                        <Label htmlFor="course-title">Tên khóa học *</Label>
                        <Input
                            id="course-title"
                            value={form.title}
                            onChange={(event) =>
                                setForm((current) => ({
                                    ...current,
                                    title: event.target.value,
                                }))
                            }
                            placeholder="Ví dụ: HSK 4 - Nâng cao"
                        />
                        {errors.title && (
                            <p className="text-xs text-red-600">
                                {errors.title}
                            </p>
                        )}
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="course-slug">Slug xem trước</Label>
                        <Input
                            id="course-slug"
                            value={slugPreview || "(chưa có)"}
                            readOnly
                            className="text-slate-500"
                        />
                    </div>

                    <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
                        <div className="space-y-2">
                            <Label>HSK Level *</Label>
                            <Select
                                value={form.hskLevel}
                                onValueChange={(value) =>
                                    setForm((current) => ({
                                        ...current,
                                        hskLevel: value,
                                    }))
                                }
                            >
                                <SelectTrigger className="w-full">
                                    <SelectValue placeholder="Chọn cấp độ" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="0">HSK 0</SelectItem>
                                    <SelectItem value="1">HSK 1</SelectItem>
                                    <SelectItem value="2">HSK 2</SelectItem>
                                    <SelectItem value="3">HSK 3</SelectItem>
                                    <SelectItem value="4">HSK 4</SelectItem>
                                    <SelectItem value="5">HSK 5</SelectItem>
                                    <SelectItem value="6">HSK 6</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="course-description">Mô tả</Label>
                        <Textarea
                            id="course-description"
                            value={form.description}
                            onChange={(event) =>
                                setForm((current) => ({
                                    ...current,
                                    description: event.target.value,
                                }))
                            }
                            rows={4}
                            placeholder="Mô tả ngắn về nội dung và mục tiêu khóa học"
                        />
                    </div>

                    {state.context.error && (
                        <p className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-xs text-red-700">
                            {state.context.error}
                        </p>
                    )}

                    <p className="rounded-md border border-amber-100 bg-amber-50 px-3 py-2 text-xs text-amber-700">
                        Khóa học mới sẽ ở trạng thái nháp. Bạn có thể xuất bản
                        trong trang chi tiết.
                    </p>

                    <DialogFooter>
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => setOpen(false)}
                            disabled={isSubmitting}
                        >
                            Hủy
                        </Button>
                        <Button type="submit" disabled={isSubmitting}>
                            {isSubmitting && (
                                <Loader2 className="size-4 animate-spin" />
                            )}
                            Tạo khóa học
                        </Button>
                    </DialogFooter>
                </form>
            </DialogContent>
        </Dialog>
    );
}
