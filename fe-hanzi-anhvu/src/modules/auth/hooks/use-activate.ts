import { useCallback } from "react";
import { useAuthActivateAccount } from "@/modules/auth/hooks/use-auth-query";
import { useMachine } from "@xstate/react";
import { AxiosError } from "axios";
import { machineActivate } from "@/modules/auth/machines/activate-account";

export type UseActivateOptions = {
    email: string;
    code: string;
};

// style similar to useResend, but with different logic and messages
// use hook of react instead of orchestrator because the logic is simple and doesn't need to manage multiple steps
//  or complex state transitions like resend flow does
export function useActivateAccount(options: UseActivateOptions) {
    const { email, code } = options;

    const [state, send] = useMachine(machineActivate);

    // mutation api
    const activateMutation = useAuthActivateAccount();

    // can activate if email and code are not empty, but the actual activation logic
    // will be handled in the machine's actions and guards
    const canActivate = Boolean(email.trim()) && Boolean(code.trim());

    const activate = useCallback(async () => {
        send({ type: "ACTIVATE", email, code });

        // becauce machine not prevent the event call from api in hook, so request still can be call
        // althouth params is invalid
        // guard just only control the state transition, not the event call,
        //  so we need to check canActivate here to prevent call api when params is invalid
        if (!canActivate) {
            return;
        }

        try {
            await activateMutation.mutateAsync({
                Email: email,
                Code: code,
            });
            send({ type: "SUCCESS" });
        } catch (error) {
            if (error instanceof AxiosError) {
                send({
                    type: "ERROR",
                    message:
                        error.response?.data?.message ||
                        "Kích hoạt thất bại hoặc liên kết đã hết hạn.",
                });
            }
        }
    }, [canActivate, email, code, activateMutation, send]);

    const reset = useCallback(() => {
        send({ type: "RESET" });
    }, [send]);

    return {
        status: state.context.status,
        message: state.context.message,
        canActivate,
        isLoading: state.matches("activating"),
        activate,
        reset,
    };
}
