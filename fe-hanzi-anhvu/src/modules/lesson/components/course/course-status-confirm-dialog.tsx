"use client";

import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog";

type CourseStatusConfirmDialogProps = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    courseTitle: string;
    nextPublished: boolean;
    isSubmitting?: boolean;
    onConfirm: () => void | Promise<void>;
};

export function CourseStatusConfirmDialog({
    open,
    onOpenChange,
    courseTitle,
    nextPublished,
    isSubmitting = false,
    onConfirm,
}: CourseStatusConfirmDialogProps) {
    const title = nextPublished
        ? "Xác nhận xuất bản khóa học"
        : "Xác nhận hủy xuất bản khóa học";

    const description = nextPublished
        ? `Khóa học \"${courseTitle}\" sẽ được xuất bản và hiển thị cho học viên.`
        : `Khóa học \"${courseTitle}\" sẽ chuyển về trạng thái nháp.`;

    const confirmLabel = nextPublished ? "Xác nhận xuất bản" : "Xác nhận";

    return (
        <ConfirmDialog
            open={open}
            onOpenChange={onOpenChange}
            title={title}
            description={description}
            confirmLabel={confirmLabel}
            confirmClassName={
                nextPublished
                    ? undefined
                    : "bg-amber-600 text-white hover:bg-amber-700 focus-visible:ring-amber-500"
            }
            isSubmitting={isSubmitting}
            onConfirm={onConfirm}
        />
    );
}
