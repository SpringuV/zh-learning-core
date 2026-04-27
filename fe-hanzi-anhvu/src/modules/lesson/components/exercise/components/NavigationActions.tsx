"use client";

import { memo } from "react";
import { Button } from "@/shared/components/ui/button";

interface NavigationActionsProps {
    currentIndex: number;
    isFinalQuestion: boolean;
    isLastQuestion: boolean;
    isPending: boolean;
    onPrev: () => void;
    onNext: () => void;
    onSubmit: () => void;
}

const NavigationActions = memo(
    ({
        currentIndex,
        isFinalQuestion,
        isLastQuestion,
        isPending,
        onPrev,
        onNext,
        onSubmit,
    }: NavigationActionsProps) => {
        return (
            <div className="flex flex-wrap items-center justify-between gap-2 pt-2">
                <div className="flex gap-2">
                    <Button
                        type="button"
                        variant="outline"
                        disabled={currentIndex === 0}
                        onClick={onPrev}
                    >
                        Câu trước
                    </Button>
                    <Button
                        type="button"
                        variant="outline"
                        disabled={isLastQuestion || isPending}
                        onClick={() => {
                            if (isFinalQuestion) {
                                onSubmit();
                                return;
                            }
                            onNext();
                        }}
                    >
                        {isLastQuestion
                            ? "Nộp bài và hoàn thành"
                            : "Câu tiếp theo"}
                    </Button>
                </div>
            </div>
        );
    },
);

NavigationActions.displayName = "NavigationActions";

export default NavigationActions;
