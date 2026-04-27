"use client";

import { memo } from "react";
import { LearningExerciseSessionItemDTOResponse } from "@/modules/lesson/types/topic.type";

interface QuestionListProps {
    items: LearningExerciseSessionItemDTOResponse[];
    currentIndex: number;
    submittedAnswers: Record<string, boolean>;
    onNavigate: (index: number, exerciseId: string) => void;
}

const QuestionList = memo(
    ({
        items,
        currentIndex,
        submittedAnswers,
        onNavigate,
    }: QuestionListProps) => {
        return (
            <section className="space-y-3">
                {items.map((item, index) => {
                    const submitted = Boolean(
                        submittedAnswers[item.exerciseId],
                    );
                    const active = index === currentIndex;

                    return (
                        <button
                            key={item.sessionItemId}
                            type="button"
                            onClick={() => onNavigate(index, item.exerciseId)}
                            className={`rounded-xl border px-3 py-2 text-left transition ${
                                active
                                    ? ` bg-emerald-50 ${submitted ? "border-emerald-500" : "border-blue-300"}`
                                    : submitted
                                      ? "border-sky-300 bg-sky-50"
                                      : "border-slate-200 bg-white hover:border-slate-300"
                            }`}
                        >
                            <p className="text-xs font-semibold uppercase tracking-wide text-slate-500">
                                Câu {index + 1}
                            </p>
                        </button>
                    );
                })}
            </section>
        );
    },
);

// Set display name for better debugging and React DevTools identification
QuestionList.displayName = "QuestionList";

export default QuestionList;
