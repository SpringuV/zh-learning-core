import { AlertCircle, RefreshCw } from "lucide-react";
import { Button } from "@/shared/components/ui/button";
import { getErrorMessage } from "@/shared/lib/utils";

interface ErrorAlertProps {
    error: unknown;
    title?: string;
    onRetry?: () => void;
    isLoading?: boolean;
}

export const ErrorAlert = ({
    error,
    title = "Có lỗi xảy ra",
    onRetry,
    isLoading = false,
}: ErrorAlertProps) => {
    const errorMessage = getErrorMessage(error);

    return (
        <div className="rounded-lg border border-destructive/50 bg-destructive/5 p-6">
            <div className="flex gap-4">
                <AlertCircle className="mt-0.5 h-5 w-5 shrink-0 text-destructive" />
                <div className="flex-1">
                    <h3 className="font-semibold text-destructive">{title}</h3>
                    <p className="mt-1 text-sm text-muted-foreground">
                        {errorMessage}
                    </p>
                    {onRetry && (
                        <Button
                            onClick={onRetry}
                            variant="outline"
                            size="sm"
                            className="mt-4 gap-2"
                            disabled={isLoading}
                        >
                            <RefreshCw className="h-4 w-4" />
                            Thử lại
                        </Button>
                    )}
                </div>
            </div>
        </div>
    );
};
