"use client";

import type { ReactNode } from "react";
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

type ConfirmDialogProps = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    title: ReactNode;
    description?: ReactNode;
    confirmLabel?: ReactNode;
    cancelLabel?: ReactNode;
    isSubmitting?: boolean;
    onConfirm: () => void | Promise<void>;
    confirmClassName?: string;
};

export function ConfirmDialog({
    open,
    onOpenChange,
    title,
    description,
    confirmLabel = "Xác nhận",
    cancelLabel = "Hủy",
    isSubmitting = false,
    onConfirm,
    confirmClassName,
}: ConfirmDialogProps) {
    return (
        <AlertDialog open={open} onOpenChange={onOpenChange}>
            <AlertDialogContent>
                <AlertDialogHeader>
                    <AlertDialogTitle>{title}</AlertDialogTitle>
                    {description && (
                        <AlertDialogDescription>
                            {description}
                        </AlertDialogDescription>
                    )}
                </AlertDialogHeader>

                <AlertDialogFooter>
                    <AlertDialogCancel disabled={isSubmitting}>
                        {cancelLabel}
                    </AlertDialogCancel>
                    <AlertDialogAction
                        disabled={isSubmitting}
                        className={confirmClassName}
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
