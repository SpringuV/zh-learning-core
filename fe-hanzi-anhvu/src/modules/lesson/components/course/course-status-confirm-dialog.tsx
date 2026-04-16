"use client";

import { Loader2 } from "lucide-react";
import {
    AlertDialog,
    AlertDialogAction,
    AlertDialogCancel,
    AlertDialogContent,
    AlertDialogDescription,
    AlertDialogFooter,
    AlertDialogHeader,
    AlertDialogTitle,
} from "@/shared/components/ui/alert-dialog";

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
        <AlertDialog open={open} onOpenChange={onOpenChange}>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>{title}</AlertDialogTitle>
                    <AlertDialogDescription>
                        {description}
                    </AlertDialogDescription>
                </AlertDialogHeader>

                <AlertDialogFooter>
                    <AlertDialogCancel disabled={isSubmitting}>
                        Hủy
                    </AlertDialogCancel>
                    <AlertDialogAction
                        disabled={isSubmitting}
                        onClick={(event) => {
                            event.preventDefault();
                            void onConfirm();
                        }}
                    >
                        {isSubmitting && (
                            <Loader2 className="mr-1 size-4 animate-spin" />
                        )}
                        {confirmLabel}
                    </AlertDialogAction>
                </AlertDialogFooter>
            </AlertDialogContent>
        </AlertDialog>
    );
}
