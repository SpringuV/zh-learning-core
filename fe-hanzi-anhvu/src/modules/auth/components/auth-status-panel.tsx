import { CheckCircle2, Loader2, MailWarning, XCircle } from "lucide-react";
import { AuthUiStatus } from "@/shared/types/store.type";

type AuthStatusPanelProps = {
    status: AuthUiStatus;
    message: string;
};

// component này sẽ chỉ để hiển thị trạng thái
// sẽ không chứa logic gì khác ngoài việc hiển thị dựa trên props truyền vào
// áp dụng design pattern "Presentational Component" (hay còn gọi là "Dumb Component") để tách biệt phần hiển thị và phần logic xử lý trạng thái
// component này sẽ được sử dụng trong ActivatePage để hiển thị trạng thái kích hoạt tài khoản
// ActivatePage sẽ sử dụng hook useAuthStatus để quản lý trạng thái và truyền xuống AuthStatusPanel thông qua props

export function AuthStatusPanel({ status, message }: AuthStatusPanelProps) {
    if (status === "idle") {
        return null;
    }

    if (status === "loading") {
        return (
            <div className="rounded-xl border border-primary/20 bg-primary/5 p-4 text-center">
                <Loader2 className="mx-auto mb-3 size-8 animate-spin text-primary" />
                <p className="text-sm text-muted-foreground">{message}</p>
            </div>
        );
    }

    if (status === "success") {
        return (
            <div className="rounded-xl border border-green-200 bg-green-50 p-4 text-center">
                <CheckCircle2 className="mx-auto mb-3 size-8 text-green-600" />
                <p className="text-sm text-green-700">{message}</p>
            </div>
        );
    }

    if (status === "warning") {
        return (
            <div className="rounded-xl border border-amber-200 bg-amber-50 p-4 text-center">
                <MailWarning className="mx-auto mb-3 size-8 text-amber-600" />
                <p className="text-sm text-amber-700">{message}</p>
            </div>
        );
    }

    return (
        <div className="rounded-xl border border-red-200 bg-red-50 p-4 text-center">
            <XCircle className="mx-auto mb-3 size-8 text-red-600" />
            <p className="text-sm text-red-700">{message}</p>
        </div>
    );
}
