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

export type CreateCoursePayload = {
    title: string;
    description: string;
    hskLevel: number;
    orderIndex: number;
    slug: string;
};

type CreateCourseModalProps = {
    defaultOrderIndex?: number;
    onCreate?: (payload: CreateCoursePayload) => void | Promise<void>;
};

type FormErrors = {
    title?: string;
    orderIndex?: string;
};

const toSlug = (value: string) => {
    return value
        .toLowerCase()
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/đ/g, "d")
        .replace(/[^a-z0-9]+/g, "-")
        .replace(/^-+|-+$/g, "")
        .replace(/-{2,}/g, "-");
};

export function CreateCourseModal({
    defaultOrderIndex = 1,
    onCreate,
}: CreateCourseModalProps) {
    const [open, setOpen] = useState(false);
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [hskLevel, setHskLevel] = useState("1");
    const [orderIndex, setOrderIndex] = useState(String(defaultOrderIndex));
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [errors, setErrors] = useState<FormErrors>({});

    const slugPreview = useMemo(() => toSlug(title), [title]);

    const resetForm = () => {
        setTitle("");
        setDescription("");
        setHskLevel("1");
        setOrderIndex(String(defaultOrderIndex));
        setErrors({});
    };

    useEffect(() => {
        if (!open) {
            resetForm();
            return;
        }

        setOrderIndex(String(defaultOrderIndex));
    }, [open, defaultOrderIndex]);

    const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
        event.preventDefault();

        const nextErrors: FormErrors = {};
        const normalizedTitle = title.trim();
        const parsedOrderIndex = Number(orderIndex);
        const parsedHskLevel = Number(hskLevel);

        if (!normalizedTitle) {
            nextErrors.title = "Tên khóa học không được để trống.";
        }

        if (!Number.isInteger(parsedOrderIndex) || parsedOrderIndex < 1) {
            nextErrors.orderIndex = "Thứ tự hiển thị phải lớn hơn hoặc bằng 1.";
        }

        setErrors(nextErrors);

        if (Object.keys(nextErrors).length > 0) {
            return;
        }

        const payload: CreateCoursePayload = {
            title: normalizedTitle,
            description: description.trim(),
            hskLevel: parsedHskLevel,
            orderIndex: parsedOrderIndex,
            slug: slugPreview || `khoa-hoc-${Date.now()}`,
        };

        setIsSubmitting(true);

        try {
            await onCreate?.(payload);
            setOpen(false);
        } finally {
            setIsSubmitting(false);
        }
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
                            value={title}
                            onChange={(event) => setTitle(event.target.value)}
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
                                value={hskLevel}
                                onValueChange={setHskLevel}
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

                        <div className="space-y-2">
                            <Label htmlFor="course-order">
                                Thứ tự hiển thị *
                            </Label>
                            <Input
                                id="course-order"
                                type="number"
                                min={1}
                                value={orderIndex}
                                onChange={(event) =>
                                    setOrderIndex(event.target.value)
                                }
                            />
                            {errors.orderIndex && (
                                <p className="text-xs text-red-600">
                                    {errors.orderIndex}
                                </p>
                            )}
                        </div>
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="course-description">Mô tả</Label>
                        <Textarea
                            id="course-description"
                            value={description}
                            onChange={(event) =>
                                setDescription(event.target.value)
                            }
                            rows={4}
                            placeholder="Mô tả ngắn về nội dung và mục tiêu khóa học"
                        />
                    </div>

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
